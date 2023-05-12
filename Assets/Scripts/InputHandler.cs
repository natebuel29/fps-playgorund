using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NB
{
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler instance;
        PlayerManager playerManager;
        public float horizontalInput;
        public float verticalInput;

        public float mouseVerticalInput;
        public float mouseHorizontalInput;

        public float moveAmount;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            playerManager = FindObjectOfType<PlayerManager>();
        }

        private void Update()
        {
            HandleHorizontalAndVerticalInput();
            HandleMouseInput();
            HandleCrouchInput();
        }

        public bool IsHorizontalInputPressed()
        {
            return Input.GetAxisRaw("Horizontal") > 0.1f || Input.GetAxisRaw("Horizontal") < -0.1f;
        }

        public bool IsVerticalInputPressed()
        {
            return Input.GetAxisRaw("Vertical") > 0.1f || Input.GetAxisRaw("Vertical") < -0.1f;
        }

        public bool IsSprintInputPressed()
        {
            return Input.GetKey(KeyCode.LeftShift);
        }

        public bool IsJumpInputPressed()
        {
            return Input.GetKey(KeyCode.Space);
        }

        private void HandleCrouchInput()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                playerManager.playerLocomotion.HandlePlayerCrouch();
            }
        }

        private void HandleHorizontalAndVerticalInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            moveAmount = Mathf.Clamp01(Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput));

        }

        private void HandleMouseInput()
        {
            mouseHorizontalInput = Input.GetAxis("Mouse X");
            mouseVerticalInput = Input.GetAxis("Mouse Y");
        }
    }
}