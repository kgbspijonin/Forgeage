using MathNet.Numerics.Distributions;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Forgeage
{
    public class TerrainGenerator : Singleton<TerrainGenerator>
    {
        public int TerrainLength { get; private set; } = 512; // MUST BE A POWER OF 2 {32, 64, 128}
        public float WaterLevel { get; private set; } = 0;

        public float AveragePlainLevel { get; private set; } = 5;
        public float MaxPlainLevel { get; private set; } = 10;

        public float AverageHillLevel { get; private set; } = 15;
        public float MaxHillLevel { get; private set; } = 50;

        public GameObject globalTrees;
        public GameObject globalGolds;
        public List<GameObject> trees;
        public List<GameObject> goldPiles;

        public GameObject water;

        private ulong seed;
        public ulong Seed {
            get {
                return seed;
            }
            private set {
                seed = value;
                var bytes = BitConverter.GetBytes(seed);
                SeedX = BitConverter.ToUInt32(bytes, 0);
                SeedY = BitConverter.ToUInt32(bytes, 4);
            }
        }
        public uint SeedX { get; private set; }
        public uint SeedY { get; private set; }

        private float Scale = 5f;

        private Terrain terrain;
        private System.Random random;
        private Normal gaussianRandom;

        private int waterPos;
        private int seaLevel = 24;

        private int outerEdgeWidth;
        private int coastalWaterWidth;
        private int minRiverWaterWidth;
        private int maxRiverWaterWidth;
        private int minSpawnDistanceFromEdge;

        private float Player1XPos;
        private float Player1ZPos;
        private float Player2XPos;
        private float Player2ZPos;

        void CalculateLandscapeSizes()
        {
            water.transform.localScale = new Vector3(TerrainLength / 100, 1, TerrainLength / 100);
            water.transform.position = transform.position + new Vector3(TerrainLength / 2, seaLevel, TerrainLength / 2);
            outerEdgeWidth = TerrainLength / 50;
            coastalWaterWidth = TerrainLength / 8;
            minRiverWaterWidth = TerrainLength / 5;
            maxRiverWaterWidth = TerrainLength / 4;
            minSpawnDistanceFromEdge = TerrainLength / 20;
        }

        void Start()
        {
            terrain = GetComponent<Terrain>();
            CalculateLandscapeSizes();
            EnsureDimensionsAreCorrect();
            if (PhotonNetwork.IsMasterClient) {
                PrepareSeed();
                photonView.RPC("SendSeed", RpcTarget.All, unchecked((int)SeedX), unchecked((int)SeedY));
                CameraController.Player = 1;
            }
            else if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
            {
                CameraController.Player = 2;
            }
            else if (!PhotonNetwork.IsConnected)
            {
                PrepareSeed();
                InitGeneration();
                GeneratePlayers();
            }
        }

        [PunRPC]
        void SendSeed(int x, int y)
        {
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            Seed = (((ulong)x) << 32) | ((ulong)y);
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
            PrepareRandom();
            InitGeneration();
            if(PhotonNetwork.IsMasterClient)
            {
                GeneratePlayers();
            }
        }

        void PrepareSeed()
        {
            random = new System.Random();
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            Seed = (((ulong)random.Next()) << 32) | ((ulong)random.Next());
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand
        }

        void InitGeneration()
        {
            PrepareTerrain();
            GenerateTerrain();
            GenerateSplatmap();
            CalculatePlayerPositions();
            GenerateOuterEdge();
            GenerateResources();
            GenerateAnimals();
            BakeNavMesh();
            if(PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
            {
                CameraController.Player = 1;
            } 
            else if(!PhotonNetwork.IsMasterClient)
            {
                CameraController.Player = 2;
            }
        }

        void PrepareRandom()
        {
            random = new System.Random((int)SeedX);
            gaussianRandom = new Normal(random);
            UnityEngine.Random.InitState((int)Seed);
            InitGeneration();
        }

        void EnsureDimensionsAreCorrect()
        {
            TerrainLength = (int)Mathf.Pow(2, Mathf.Ceil(Mathf.Log(TerrainLength) / Mathf.Log(2)));
        }

        void PrepareTerrain()
        {
            terrain.terrainData.heightmapResolution = TerrainLength + 1;
            terrain.terrainData.size = new Vector3(
                TerrainLength,
                MaxHillLevel,
                TerrainLength
                );
        }

        void GenerateTerrain()
        {
            terrain.terrainData.SetHeights(0, 0, GeneratePlains());
        }

        float[,] GeneratePlains()
        {
            waterPos = random.Next() % 3; // 0 for "coastal", 1 for left river, 2 for right river
            float[,] heights = new float[TerrainLength, TerrainLength];
            for (int x = 0; x < TerrainLength; x++)
            {
                for (int z = 0; z < TerrainLength; z++)
                {
                    heights[x, z] = CalculateHeight(x, z) + 0.5f;
                }
            }
            if (waterPos == 0)
            {
                for (int k = outerEdgeWidth; k < outerEdgeWidth + coastalWaterWidth; k++)
                {
                    for (int i = outerEdgeWidth + coastalWaterWidth; i < TerrainLength - outerEdgeWidth - coastalWaterWidth; i++)
                    {
                        if (k < outerEdgeWidth + coastalWaterWidth / 2)
                        {
                            heights[i, k] -= 0.5f;
                            heights[TerrainLength - i - 1, TerrainLength - k] -= 0.5f;
                            heights[k, i] -= 0.5f;
                            heights[TerrainLength - k - 1, TerrainLength - i] -= 0.5f;
                        }
                        else
                        {
                            heights[i, k] -= 0.5f - 0.5f * ((((float)k - outerEdgeWidth) / coastalWaterWidth) - 0.5f) * 2;
                            heights[TerrainLength - i - 1, TerrainLength - k] -= 0.5f - 0.5f * ((((float)k - outerEdgeWidth) / coastalWaterWidth) - 0.5f) * 2;
                            heights[k, i] -= 0.5f - 0.5f * ((((float)k - outerEdgeWidth) / coastalWaterWidth) - 0.5f) * 2;
                            heights[TerrainLength - k - 1, TerrainLength - i] -= 0.5f - 0.5f * ((((float)k - outerEdgeWidth) / coastalWaterWidth) - 0.5f) * 2;
                        }
                    }
                    for (int i = outerEdgeWidth; i < outerEdgeWidth + coastalWaterWidth; i++)
                    {
                        if (k > outerEdgeWidth + coastalWaterWidth / 2 && i > outerEdgeWidth + coastalWaterWidth / 2)
                        {
                            float bigger = Mathf.Min(i, k);
                            heights[i, k] -= 0.5f - 0.5f * (((bigger - outerEdgeWidth) / coastalWaterWidth) - 0.5f) * 2;
                            heights[TerrainLength - i - 1, k] -= 0.5f - 0.5f * (((bigger - outerEdgeWidth) / coastalWaterWidth) - 0.5f) * 2;
                            heights[i, TerrainLength - k] -= 0.5f - 0.5f * (((bigger - outerEdgeWidth) / coastalWaterWidth) - 0.5f) * 2;
                            heights[TerrainLength - i - 1, TerrainLength - k] -= 0.5f - 0.5f * (((bigger - outerEdgeWidth) / coastalWaterWidth) - 0.5f) * 2;

                        }
                        else
                        {
                            heights[i, k] -= 0.5f;
                            heights[TerrainLength - i - 1, k] -= 0.5f;
                            heights[i, TerrainLength - k] -= 0.5f;
                            heights[TerrainLength - i - 1, TerrainLength - k] -= 0.5f;
                        }
                    }
                    if (k > outerEdgeWidth + coastalWaterWidth / 2)
                    {
                        heights[k, TerrainLength - outerEdgeWidth - coastalWaterWidth] -= 0.5f - 0.5f * ((((float)k - outerEdgeWidth) / coastalWaterWidth) - 0.5f) * 2;
                        heights[TerrainLength - k, outerEdgeWidth + coastalWaterWidth] -= 0.5f - 0.5f * ((((float)k - outerEdgeWidth) / coastalWaterWidth) - 0.5f) * 2;
                    }
                    else
                    {
                        heights[k, TerrainLength - outerEdgeWidth - coastalWaterWidth] -= 0.5f;
                        heights[TerrainLength - k - 1, outerEdgeWidth + coastalWaterWidth] -= 0.5f;
                    }
                }
            }
            if (waterPos == 1)
            {
                float[] riverLine = new float[TerrainLength];
                for (int x = outerEdgeWidth; x < TerrainLength - outerEdgeWidth; x++)
                {
                    riverLine[x] = Mathf.PerlinNoise((((float)x) / TerrainLength) * Scale + ((short)SeedX), 100);
                    var start = outerEdgeWidth + minRiverWaterWidth + riverLine[x] * (maxRiverWaterWidth - minRiverWaterWidth);
                    for (int k = outerEdgeWidth; k < outerEdgeWidth + minRiverWaterWidth; k++)
                    {
                        heights[x, k] -= 0.5f;
                    }
                    for (int i = 0; i < riverLine[x] * (maxRiverWaterWidth - minRiverWaterWidth); i++)
                    {
                        heights[x, outerEdgeWidth + minRiverWaterWidth + i] -= 0.5f;
                    }
                    for (int i = 0; i < 25; i++)
                    {
                        heights[x, Mathf.FloorToInt(start - i)] += 0.5f - i * 0.02f;
                    }
                }
            }
            if (waterPos == 2)
            {
                float[] riverLine = new float[TerrainLength];
                for (int x = outerEdgeWidth; x < TerrainLength - outerEdgeWidth; x++)
                {
                    riverLine[x] = Mathf.PerlinNoise((((float)x) / TerrainLength) * Scale + ((short)SeedX), 100);
                    var start = outerEdgeWidth + minRiverWaterWidth + riverLine[x] * (maxRiverWaterWidth - minRiverWaterWidth);
                    for (int k = outerEdgeWidth; k < outerEdgeWidth + minRiverWaterWidth; k++)
                    {
                        heights[x, TerrainLength - k] -= 0.5f;
                    }
                    for (int i = 0; i < riverLine[x] * (maxRiverWaterWidth - minRiverWaterWidth); i++)
                    {
                        heights[x, TerrainLength - outerEdgeWidth - minRiverWaterWidth - i] -= 0.5f;
                    }
                    for (int i = 0; i < 25; i++)
                    {
                        heights[x, TerrainLength - Mathf.FloorToInt(start - i)] += 0.5f - i * 0.02f;
                    }
                }
            }
            return heights;
        }

        float CalculateHeight(int x, int y)
        {
            float xCoord = (((float)x) / TerrainLength) * Scale + ((short)SeedX);
            float yCoord = (((float)y) / TerrainLength) * Scale + ((short)SeedY);
            return ((Mathf.PerlinNoise(xCoord, yCoord) / 4));
        }

        void GenerateSplatmap()
        {
            TerrainData terrainData = terrain.terrainData;
            float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];
            for (int y = 0; y < terrainData.alphamapHeight; y++)
            {
                for (int x = 0; x < terrainData.alphamapWidth; x++)
                {
                    // Normalise x/y coordinates to range 0-1 
                    float y_01 = (float)y / (float)terrainData.alphamapHeight;
                    float x_01 = (float)x / (float)terrainData.alphamapWidth;

                    float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth)); // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
                    Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01); // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
                    float steepness = terrainData.GetSteepness(y_01, x_01); // Calculate the steepness of the terrain
                    float[] splatWeights = new float[terrainData.alphamapLayers]; // Setup an array to record the mix of texture weights at this point

                    splatWeights[0] = Mathf.Sin(height / MaxPlainLevel);
                    splatWeights[3] = 1 - Mathf.Sin(height / MaxPlainLevel);

                    float z = splatWeights.Sum(); // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                    for (int i = 0; i < terrainData.alphamapLayers; i++) // Loop through each terrain texture
                    {
                        splatWeights[i] /= z; // Normalize so that sum of all texture weights = 1
                        splatmapData[x, y, i] = splatWeights[i]; // Assign this point to the splatmap array
                    }
                }
            }
            terrainData.SetAlphamaps(0, 0, splatmapData);
        }

        void GenerateResources()
        {
            float[,] heights = new float[TerrainLength, TerrainLength];
            for (int x = outerEdgeWidth; x < TerrainLength - outerEdgeWidth; x++)
            {
                for (int z = outerEdgeWidth; z < TerrainLength - outerEdgeWidth; z++)
                {
                    heights[x, z] = CalculateTreeChance(x, z);
                }
            }
            for (int x = outerEdgeWidth; x < TerrainLength - outerEdgeWidth; x += 4)
            {
                for (int z = outerEdgeWidth; z < TerrainLength - outerEdgeWidth; z += 4)
                {
                    var p1xmin = Player1XPos - 20 - transform.position.x;
                    var p1xmax = Player1XPos + 20 - transform.position.x;
                    var p1zmin = Player1ZPos - 20 - transform.position.z;
                    var p1zmax = Player1ZPos + 20 - transform.position.z;
                    var p2xmin = Player2XPos - 20 - transform.position.x;
                    var p2xmax = Player2XPos + 20 - transform.position.x;
                    var p2zmin = Player2ZPos - 20 - transform.position.z;
                    var p2zmax = Player2ZPos + 20 - transform.position.z;

                    if (!(((x > p1xmin && x < p1xmax) && (z > p1zmin && z < p1zmax)) || ((x > p2xmin && x < p2xmax) && (z > p2zmin && z < p2zmax))))
                    {
                        if (heights[x, z] > 0.67f && GetHeight(x, z) > seaLevel)
                        {
                            CreateTree(new Vector3(x + transform.position.x, terrain.terrainData.GetHeight(x, z), z + transform.position.z));
                        }
                        if (heights[x, z] < 0.2f && GetHeight(x, z) > seaLevel && x % 3 == 0 && z % 3 == 0)
                        {
                            CreateGoldPile(new Vector3(x + transform.position.x, terrain.terrainData.GetHeight(x, z), z + transform.position.z));
                        }
                    }
                }
            }
        }

        void CalculatePlayerPositions()
        {
            Player1XPos = UnityEngine.Random.Range(outerEdgeWidth + maxRiverWaterWidth + minSpawnDistanceFromEdge,
                TerrainLength - (outerEdgeWidth + maxRiverWaterWidth + minSpawnDistanceFromEdge)) + transform.position.x;
            Player1ZPos = outerEdgeWidth + coastalWaterWidth + minSpawnDistanceFromEdge + transform.position.z;

            Player2XPos = UnityEngine.Random.Range(outerEdgeWidth + coastalWaterWidth + minSpawnDistanceFromEdge,
                TerrainLength - (outerEdgeWidth + maxRiverWaterWidth + minSpawnDistanceFromEdge)) + transform.position.x;
            Player2ZPos = TerrainLength - (outerEdgeWidth + maxRiverWaterWidth + minSpawnDistanceFromEdge) + transform.position.z;
        }

        void GeneratePlayer1Stuff()
        {
            Spawner.Instance.Entities.Spawn(Entities.Entity.TownCenter, new Vector3(
                Player1XPos,
                terrain.terrainData.GetHeight(Mathf.RoundToInt(Player1XPos - transform.position.x), Mathf.RoundToInt(Player1ZPos - transform.position.z)),
                Player1ZPos
                ), 1, true);
            Spawner.Instance.Entities.Spawn(Entities.Entity.Hero, new Vector3(
                Player1XPos,
                terrain.terrainData.GetHeight(Mathf.RoundToInt(Player1XPos - transform.position.x), Mathf.RoundToInt(Player1ZPos - transform.position.z - 10)),
                Player1ZPos - 10), 1);
        }

        void GeneratePlayer2Stuff() 
        {
            Spawner.Instance.Entities.Spawn(Entities.Entity.TownCenter, new Vector3(
                Player2XPos,
                terrain.terrainData.GetHeight(Mathf.RoundToInt(Player2XPos - transform.position.x), Mathf.RoundToInt(Player2ZPos - transform.position.z)),
                Player2ZPos
                ), 2, true);
            Spawner.Instance.Entities.Spawn(Entities.Entity.Hero, new Vector3(
                Player2XPos,
                terrain.terrainData.GetHeight(Mathf.RoundToInt(Player2XPos - transform.position.x), Mathf.RoundToInt(Player2ZPos - transform.position.z - 10)),
                Player2ZPos - 10), 2);
        }

        void CreateGoldPile(Vector3 pos, int pileNum = -1)
        {
            var goldPile = Instantiate(goldPiles[pileNum == -1 ? random.Next() % 2 : pileNum], pos, Quaternion.Euler(0, random.Next() % 360, 0));
            goldPile.transform.parent = globalGolds.transform;
        }

        float CalculateTreeChance(int x, int y)
        {
            float xCoord = (((float)x) / TerrainLength) * 20 + ((short)SeedX);
            float yCoord = (((float)y) / TerrainLength) * 20 + ((short)SeedY);
            return Mathf.PerlinNoise(xCoord, yCoord);
        }

        void GenerateOuterEdge()
        {
            for (int i = 0; i < TerrainLength; i += 8)
            {
                for (int k = 0; k < outerEdgeWidth; k += 8)
                {
                    CreateTree(new Vector3(i + transform.position.x, GetHeight(i, k), k + transform.position.z));
                    CreateTree(new Vector3(i + transform.position.x, GetHeight(i, TerrainLength - k - 1), TerrainLength - k - 1 + transform.position.z));
                    CreateTree(new Vector3(k + transform.position.x, GetHeight(k, i), i + transform.position.z));
                    CreateTree(new Vector3(TerrainLength - k + transform.position.x, GetHeight(k, TerrainLength - i), TerrainLength - i + transform.position.z));
                }
            }
        }

        float GetHeight(int x, int y)
        {
            return terrain.terrainData.GetHeight(x, y);
        }

        GameObject CreateTree(Vector3 pos, int treeNum = -1)
        {
            var tree = Instantiate(trees[treeNum == -1 ? random.Next() % 2 : treeNum], pos, Quaternion.Euler(0, random.Next() % 360, 0));
            tree.transform.parent = globalTrees.transform;
            return tree;
        }

        void GenerateAnimals()
        {
            float[,] heights = new float[TerrainLength, TerrainLength];
            var p1xmin = Player1XPos - 20 - transform.position.x;
            var p1xmax = Player1XPos + 20 - transform.position.x;
            var p1zmin = Player1ZPos - 20 - transform.position.z;
            var p1zmax = Player1ZPos + 20 - transform.position.z;
            var p2xmin = Player2XPos - 20 - transform.position.x;
            var p2xmax = Player2XPos + 20 - transform.position.x;
            var p2zmin = Player2ZPos - 20 - transform.position.z;
            var p2zmax = Player2ZPos + 20 - transform.position.z;
            for (int x = outerEdgeWidth; x < TerrainLength - outerEdgeWidth; x++)
            {
                for (int z = outerEdgeWidth; z < TerrainLength - outerEdgeWidth; z++)
                {
                    heights[x, z] = CalculateWolfChance(x, z);
                }
            }
            
            for (int x = outerEdgeWidth; x < TerrainLength - outerEdgeWidth; x += 20)
            {
                for (int z = outerEdgeWidth; z < TerrainLength - outerEdgeWidth; z += 20)
                {
                    if (!(((x > p1xmin && x < p1xmax) && (z > p1zmin && z < p1zmax)) || ((x > p2xmin && x < p2xmax) && (z > p2zmin && z < p2zmax))))
                    {
                        if (heights[x, z] > 0.75f && GetHeight(x, z) > seaLevel)
                        {
                            CreateCow(new Vector3(x + transform.position.x, terrain.terrainData.GetHeight(x, z), z + transform.position.z));
                        }
                    }
                    if (!(((x > p1xmin - 20 && x < p1xmax + 20) && (z > p1zmin - 20 && z < p1zmax + 20)) || ((x > p2xmin - 20 && x < p2xmax + 20) && (z > p2zmin - 20 && z < p2zmax + 20))))
                    {
                        if (heights[x, z] < 0.25f && GetHeight(x, z) > seaLevel)
                        {
                            CreateWolf(new Vector3(x + transform.position.x, terrain.terrainData.GetHeight(x, z), z + transform.position.z));
                        }
                    }
                }
            }
            CreateCow(new Vector3(Player1XPos - 15, GetHeight(Mathf.FloorToInt(Player1XPos - transform.position.x - 15), Mathf.FloorToInt(Player1ZPos)), Player1ZPos));
            CreateCow(new Vector3(Player1XPos - 17, GetHeight(Mathf.FloorToInt(Player1XPos - transform.position.x - 17), Mathf.FloorToInt(Player1ZPos)), Player1ZPos));
        }

        private GameObject CreateWolf(Vector3 pos)
        {
            var wolf = Spawner.Instance.Entities.Spawn(Entities.Entity.Wolf, pos, 0, rotation:Quaternion.Euler(0, random.Next() % 360, 0));
            wolf.transform.parent = Spawner.Instance.Entities.gaiaparent.transform;
            Spawner.Instance.Entities.gaiaeintities.Add(wolf);
            return wolf;
        }

        private GameObject CreateCow(Vector3 pos)
        {
            var cow = Spawner.Instance.Entities.Spawn(Entities.Entity.Cow, pos, 0, rotation: Quaternion.Euler(0, random.Next() % 360, 0));
            cow.transform.parent = Spawner.Instance.Entities.gaiaparent.transform;
            Spawner.Instance.Entities.gaiaeintities.Add(cow);
            return cow;
        }

        float CalculateWolfChance(int x, int y)
        {
            float xCoord = (((float)x) / TerrainLength) * 15 - ((short)SeedX);
            float yCoord = (((float)y) / TerrainLength) * 15 - ((short)SeedY);
            return Mathf.PerlinNoise(xCoord, yCoord);
        }

        void BakeNavMesh()
        {
            GetComponent<NavMeshSurface>().BuildNavMesh();
        }

        void GeneratePlayers()
        {
            GeneratePlayer1Stuff();
            GeneratePlayer2Stuff();
        }
    }
}
