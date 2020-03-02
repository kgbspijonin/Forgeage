using Forgeage.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Forgeage
{
	public class ActionController : Singleton<ActionController>
	{
        public static KeyCode mousePrimaryButton = KeyCode.Mouse0;
        public static KeyCode mouseSecondaryButton = KeyCode.Mouse1;
        public static KeyCode mouseMiddleButton = KeyCode.Mouse2;

        public static int terrainLayerMask;
        public static int uiLayerMask;
        public static int resourcesLayerMask;
        public static int buildingsLayerMask;
        public static int unitsLayerMask;
        public static int waterTerrainLayerMask;

        public Vector3 initialClickPosition;
        public Vector3 currentClickPosition;

        private bool isInBuilderMode = false;

        private Entities.Entity? BuildingPreviewEntity;
        private GameObject BuildingPreview;

        private new Camera camera;

        private bool isDock = false;

        void Start()
		{
            terrainLayerMask = (1 << LayerMask.NameToLayer("Terrain"));
            uiLayerMask = (1 << LayerMask.NameToLayer("UI"));
            resourcesLayerMask = (1 << LayerMask.NameToLayer("Natural Resources"));
            buildingsLayerMask = (1 << LayerMask.NameToLayer("Buildings"));
            unitsLayerMask = (1 << LayerMask.NameToLayer("Units"));
            waterTerrainLayerMask = (1 << LayerMask.NameToLayer("Water Terrain"));

            camera = GetComponent<Camera>();
            InputHandler.Instance.OnKeybindsPrepared += new EventHandler(SetMouseActionInputs);
        }

        protected void SetMouseActionInputs(object sender, EventArgs e)
        {
            InputHandler.Instance.Keybinds.GetOrAddValue(mousePrimaryButton, new List<KeyPressDelegate>()).Add(MousePrimaryClicked);
            InputHandler.Instance.Keybinds.GetOrAddValue(mouseSecondaryButton, new List<KeyPressDelegate>()).Add(MouseSecondaryClicked);
            InputHandler.Instance.Keybinds.GetOrAddValue(mouseMiddleButton, new List<KeyPressDelegate>()).Add(MouseMiddleClicked);
        }

        void Update()
        {
            if(BuildingPreviewEntity != null)
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask | waterTerrainLayerMask))
                {
                    if ((isDock == true && hit.collider.gameObject.layer == LayerMask.NameToLayer("Water Terrain")) ||
                        (isDock == false && hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))) {
                        BuildingPreview.transform.position = hit.point;
                        if (Input.GetMouseButtonDown(1))
                        {
                            if (!isDock)
                            {
                                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                                {
                                    Spawner.Instance.Entities.Spawn(BuildingPreviewEntity.Value, BuildingPreview.transform.position, CameraController.Player);
                                    Destroy(BuildingPreview);
                                    EndBuilding();
                                }
                            }
                            else
                            {
                                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Water Terrain"))
                                {
                                    Spawner.Instance.Entities.Spawn(BuildingPreviewEntity.Value, BuildingPreview.transform.position, CameraController.Player);
                                    Destroy(BuildingPreview);
                                    EndBuilding();
                                }
                            }
                        }
                    }
                }
            }
        }

        public void InitBuilding(Entities.Entity preview)
        {
            if(BuildingPreview != null)
            {
                Destroy(BuildingPreview);
            }
            isDock = false;
            isInBuilderMode = true;
            BuildingPreviewEntity = preview;
            BuildingPreview = Instantiate(Spawner.Instance.Entities.Spawnables[preview]);
            if(BuildingPreview.GetComponent<DockController>() != null)
            {
                isDock = true;
            }
            Destroy(BuildingPreview.GetComponent<BuildingController>());
            Destroy(BuildingPreview.GetComponent<EntityController>());
            Destroy(BuildingPreview.GetComponent<Collider>());
            Destroy(BuildingPreview.GetComponent<NavMeshObstacle>());
            foreach(Transform t in BuildingPreview.transform)
            {
                if(t.name == "Canvas")
                {
                    Destroy(t.gameObject);
                }
            }
        }

        public void EndBuilding()
        {
            isDock = false;
            isInBuilderMode = false;
            BuildingPreviewEntity = null;
            BuildingPreview = null;
        }

        void MousePrimaryClicked(ActionStatus status) // SELECTS ENTITIES
        {
            if(!isInBuilderMode)
            {
                if (status == ActionStatus.PRESSED)
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {

                    }
                    else
                    {
                        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask | waterTerrainLayerMask))
                        {
                            ActionHandler.Instance.Selected.Clear();
                            initialClickPosition = hit.point;
                        }
                    }
                }
                else if (status == ActionStatus.HELD)
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {

                    }
                    else
                    {
                        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask | waterTerrainLayerMask))
                        {
                            currentClickPosition = hit.point;
                        }
                        foreach (Collider collider in Physics.OverlapBox(
                            (initialClickPosition + currentClickPosition) / 2,
                            new Vector3(
                                Mathf.Abs(initialClickPosition.x - currentClickPosition.x) / 2,
                                10,
                                Mathf.Abs(initialClickPosition.z - currentClickPosition.z) / 2
                                ))
                        )
                        {
                            if (collider.GetComponent<EntityController>() != null && collider.GetComponent<EntityController>().Affiliation == CameraController.Player)
                            {
                                if (!ActionHandler.Instance.Selected.Contains(collider.gameObject.GetComponent<EntityController>()))
                                {
                                    ActionHandler.Instance.Selected.Add(collider.gameObject.GetComponent<EntityController>());
                                }
                            }
                        }
                    }
                }
                else // if(status == ActionStatus.RELEASED)
                {

                }
            }
        }

        void MouseSecondaryClicked(ActionStatus status) // DELEGATES ACTIONS WITH ENTITIES
        {
            if (!isInBuilderMode)
            {
                if (status == ActionStatus.PRESSED)
                {
                    Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask | resourcesLayerMask | buildingsLayerMask | unitsLayerMask | waterTerrainLayerMask))
                    {
                        initialClickPosition = hit.point;
                    }
                    if (hit.collider.gameObject.GetComponent<Terrain>() != null) // hit terrain
                    {
                        foreach (EntityController ec in ActionHandler.Instance.Selected)
                        {
                            if (ec.gameObject.GetComponent<GalleyController>() != null)
                            {
                                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Water Terrain"))
                                {
                                    ec.gameObject.GetComponent<Controller>().MoveTo(hit.point);
                                }
                            }
                            else if (ec.gameObject.GetComponent<NavMeshAgent>() != null)
                            {
                                ec.gameObject.GetComponent<Controller>().MoveTo(hit.point);
                            }
                        }
                    }
                    if (hit.collider.gameObject.GetComponent<ResourceController>() != null || 
                        (hit.collider.gameObject.GetComponent<BuildingController>() != null && 
                        hit.collider.gameObject.GetComponent<EntityController>().Affiliation == CameraController.Player))
                    {
                        foreach (EntityController ec in ActionHandler.Instance.Selected)
                        {
                            if (ec.gameObject.GetComponent<VillagerController>() != null)
                            {
                                ec.gameObject.GetComponent<VillagerController>().GoWork(hit.collider.gameObject);
                            }
                        }
                    }
                    if (hit.collider.gameObject.GetComponent<EntityController>() != null && 
                        hit.collider.gameObject.GetComponent<EntityController>().Affiliation != CameraController.Player)
                    {
                        foreach (EntityController ec in ActionHandler.Instance.Selected)
                        {
                            if(ec.gameObject.GetComponent<MilitaryController>() != null)
                            {
                                ec.gameObject.GetComponent<MilitaryController>().GoAttack(hit.collider.gameObject);
                            }
                        }
                    }
                }
                else if (status == ActionStatus.HELD)
                {

                }
                else // if(status == ActionStatus.RELEASED)
                {

                }
            }
        }

        void MouseMiddleClicked(ActionStatus status)
        {
            if (!isInBuilderMode)
            {
                if (status == ActionStatus.PRESSED)
                {

                }
                else if (status == ActionStatus.HELD)
                {
                    Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayerMask))
                    {
                        initialClickPosition = hit.point;
                    }
                }
                else // if(status == ActionStatus.RELEASED)
                {

                }
            } 
        }
    }
}
