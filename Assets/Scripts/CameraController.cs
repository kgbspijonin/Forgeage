using Forgeage.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Forgeage
{
    public class CameraController : Singleton<CameraController>
    {
        public static int Player = 1;

        public static KeyCode forward = KeyCode.W;
        public static KeyCode backward = KeyCode.S;
        public static KeyCode left = KeyCode.A;
        public static KeyCode right = KeyCode.D;
        public static KeyCode toggle = KeyCode.Space;

        private float minScrollSpeed = 5;
        private float maxScrollSpeed = 40;
        private float currentZScrollSpeed = 0;
        private float currentXScrollSpeed = 0;
        private float scrollSpeedStep = 1.075f;
        private float minHeight = 20;
        private float maxHeight = 150;
        private float upDownSpeed = 1f;

        private bool isActive = true;

        private bool movedThisFrame = false;
        private bool movedLastFrame = false;

        private new Camera camera;

        void Start()
        {
            camera = GetComponent<Camera>();
            InputHandler.Instance.OnKeybindsPrepared += new EventHandler(SetCameraInputs);
        }

        void Update()
        {
            HandleMouseMovement();
            HandleScrollMovement();
        }

        protected void SetCameraInputs(object sender, EventArgs e)
        {
            InputHandler.Instance.Keybinds.GetOrAddValue(forward, new List<KeyPressDelegate>()).Add(MoveForward);
            InputHandler.Instance.Keybinds.GetOrAddValue(backward, new List<KeyPressDelegate>()).Add(MoveBackward);
            InputHandler.Instance.Keybinds.GetOrAddValue(left, new List<KeyPressDelegate>()).Add(MoveLeft);
            InputHandler.Instance.Keybinds.GetOrAddValue(right, new List<KeyPressDelegate>()).Add(MoveRight);
            InputHandler.Instance.Keybinds.GetOrAddValue(toggle, new List<KeyPressDelegate>()).Add(Toggle);
        }

        void HandleScrollMovement()
        {
            float scroll = Input.mouseScrollDelta.y;
            if(scroll != 0)
            {
                float newY = Mathf.Clamp(transform.position.y - scroll * upDownSpeed, minHeight, maxHeight);
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        void HandleMouseMovement()
        {
            movedThisFrame = false;
            if (Input.mousePosition.y >= Screen.height * 0.99f)
            {
                movedThisFrame = true;
                if (!movedLastFrame)
                {
                    MoveForward(ActionStatus.PRESSED);
                }
                else
                {
                    MoveForward(ActionStatus.HELD);
                }
            }
            else if (Input.mousePosition.y <= Screen.height * 0.01f)
            {
                movedThisFrame = true;
                if (!movedLastFrame)
                {
                    MoveBackward(ActionStatus.PRESSED);
                }
                else
                {
                    MoveBackward(ActionStatus.HELD);
                }
            }
            if (Input.mousePosition.x <= Screen.width * 0.01f)
            {
                movedThisFrame = true;
                if (!movedLastFrame)
                {
                    MoveLeft(ActionStatus.PRESSED);
                }
                else
                {
                    MoveLeft(ActionStatus.HELD);
                }
            }
            else if (Input.mousePosition.x >= Screen.width * 0.99f)
            {
                movedThisFrame = true;
                if (!movedLastFrame)
                {
                    MoveRight(ActionStatus.PRESSED);
                }
                else
                {
                    MoveRight(ActionStatus.HELD);
                }
            }
            movedLastFrame = movedThisFrame;
        }

        void Toggle(ActionStatus status)
        {
            if(status == ActionStatus.PRESSED)
            {
                isActive = !isActive;
            }
        }

        void MoveForward(ActionStatus status)
        {
            if(isActive)
            {
                if (status == ActionStatus.PRESSED)
                {
                    currentZScrollSpeed = minScrollSpeed;
                }
                transform.position += new Vector3(0, 0, currentZScrollSpeed) * Time.deltaTime;
                currentZScrollSpeed = Mathf.Clamp(currentZScrollSpeed * scrollSpeedStep, minScrollSpeed, maxScrollSpeed);
            }
        }

        void MoveBackward(ActionStatus status)
        {
            if (isActive)
            {
                if (status == ActionStatus.PRESSED)
                {
                    currentZScrollSpeed = minScrollSpeed;
                }
                transform.position += new Vector3(0, 0, -currentZScrollSpeed) * Time.deltaTime;
                currentZScrollSpeed = Mathf.Clamp(currentZScrollSpeed * scrollSpeedStep, minScrollSpeed, maxScrollSpeed);
            }
        }

        void MoveLeft(ActionStatus status)
        {
            if (isActive)
            {
                if (status == ActionStatus.PRESSED)
                {
                    currentXScrollSpeed = minScrollSpeed;
                }
                transform.position += new Vector3(-currentXScrollSpeed, 0, 0) * Time.deltaTime;
                currentXScrollSpeed = Mathf.Clamp(currentXScrollSpeed * scrollSpeedStep, minScrollSpeed, maxScrollSpeed);
            }
        }

        void MoveRight(ActionStatus status)
        {
            if (isActive)
            {
                if (status == ActionStatus.PRESSED)
                {
                    currentXScrollSpeed = minScrollSpeed;
                }
                transform.position += new Vector3(currentXScrollSpeed, 0, 0) * Time.deltaTime;
                currentXScrollSpeed = Mathf.Clamp(currentXScrollSpeed * scrollSpeedStep, minScrollSpeed, maxScrollSpeed);
            }
        }
    }
}