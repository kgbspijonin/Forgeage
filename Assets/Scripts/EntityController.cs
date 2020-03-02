using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;

namespace Forgeage
{
	public class EntityController : MonoBehaviour, IPunObservable
	{
        public string Name;
        public Sprite UITexture;
        private int affiliation;
        public int Affiliation {
            get {
                return affiliation;
            } set {
                affiliation = value;
                UpdateHealthbar(this, null);
            }
        }
        public float MaxHP;
        public float currentHP;
        public Controller Controller;
        public GameObject HealthBackground;
        public GameObject HealthForeground;
        public event EventHandler OnInitialized;

        public void InitFinished()
        {
            OnInitialized?.Invoke(this, null);
        }

        public EntityController()
        {
            OnHealthChanged += UpdateHealthbar;
        }

        public float CurrentHP {
            get {
                return currentHP;
            }
            set {
                currentHP = value;
                if(currentHP < 0)
                {
                    KillUnit();
                }
                if(currentHP > MaxHP)
                {
                    currentHP = MaxHP;
                }
                OnHealthChanged.Invoke(this, null);
            }
        }
        protected event EventHandler OnHealthChanged;

        public float maxHealthbarOffset = 0.25f;
        private Vector3 initialHealthbarScale;
        private Vector3 initialHealtbarPosition;

        void Start()
		{
            initialHealthbarScale = HealthForeground.transform.localScale;
            initialHealtbarPosition = new Vector3(0, HealthForeground.transform.localPosition.y, 0);
            UpdateHealthbar(this, null);
        }

        void UpdateHealthbar(object sender, EventArgs args)
        {
            if(Affiliation == 0)
            {
                HealthForeground.GetComponent<Image>().color = Color.black;
            }
            if(Affiliation == 1)
            {
                HealthForeground.GetComponent<Image>().color = Color.blue;
            }
            if (Affiliation == 2)
            {
                HealthForeground.GetComponent<Image>().color = Color.red;
            }
            HealthForeground.transform.localPosition = new Vector3(
                initialHealtbarPosition.x - maxHealthbarOffset * (1 - CurrentHP / MaxHP), 
                HealthForeground.transform.localPosition.y, 
                HealthForeground.transform.localPosition.z);
            if(initialHealthbarScale != new Vector3())
            {
                HealthForeground.transform.localScale = new Vector3(
                initialHealthbarScale.x * (CurrentHP / MaxHP),
                HealthForeground.transform.localScale.y,
                HealthForeground.transform.localScale.z);
            }
        }

        protected void KillUnit()
        {
            Controller.Die();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(stream.IsWriting)
            {
                stream.SendNext(Affiliation);
                stream.SendNext(CurrentHP);
            }
            else
            {
                Affiliation = (int) stream.ReceiveNext();
                CurrentHP = (float) stream.ReceiveNext();
            }
        }
    }
}
