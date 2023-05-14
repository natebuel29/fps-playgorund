using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NB
{
    public class PlayerManager : MonoBehaviour
    {
        public PlayerLocomotion playerLocomotion;

        private void Start()
        {
            playerLocomotion = GetComponent<PlayerLocomotion>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            InputHandler.instance.lockMouse_flag = true;
        }

        private void Update()
        {
            playerLocomotion.HandlePlayerRotation();
            playerLocomotion.StateHandler();
            playerLocomotion.LimitSpeed();
            UIManager.instance.SetSpeedText(Mathf.Round(playerLocomotion.rb.velocity.magnitude).ToString());
        }

        private void FixedUpdate()
        {
            playerLocomotion.HandlePlayerMovement();
            playerLocomotion.HandleSlide();
        }

        public void ShouldLockMouse(bool lockMouse_flag)
        {
            if (lockMouse_flag)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}