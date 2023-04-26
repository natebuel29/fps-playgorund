using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NB
{
    public class PlayerLocomotion : MonoBehaviour
    {
        Rigidbody rb;

        public Transform direction;
        public Camera mainCamera;


        [Header("Locomotion Settings")]
        [SerializeField] float walkMulitplier = 50f;
        [SerializeField] float maxSpeed = 10f;
        [SerializeField] float mouseXSensitivity;
        [SerializeField] float mouseYSensitivity;
        [SerializeField] float maxYAngle = 90;
        [SerializeField] float minYAngle = -90;



        [Header("Locomotion Values")]
        [SerializeField] float mouseXAngle;
        [SerializeField] float mouseYAngle;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void HandlePlayerMovement()
        {
            float vertical = InputHandler.instance.verticalInput;
            float horizontal = InputHandler.instance.horizontalInput;
            Vector3 forward = direction.transform.forward * vertical * Time.fixedDeltaTime;
            Vector3 right = direction.transform.right * horizontal * Time.fixedDeltaTime;
            Vector3 inputVelocity = forward + right;
            inputVelocity.Normalize();
            rb.AddForce(inputVelocity * walkMulitplier);
            LimitSpeed();
        }

        public void LimitSpeed()
        {
            float mag = rb.velocity.magnitude;
            if (mag > maxSpeed)
            {
                Vector3 targetVelocity = rb.velocity.normalized * maxSpeed;
                rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            }
        }

        public void HandlePlayerRotation()
        {
            float newMouseX = InputHandler.instance.mouseHorizontalInput;
            float newMouseY = InputHandler.instance.mouseVerticalInput;

            mouseXAngle += newMouseX;
            mouseYAngle -= newMouseY;

            mouseYAngle = Mathf.Clamp(mouseYAngle, minYAngle, maxYAngle);

            Quaternion targetRotation = Quaternion.Euler(mouseYAngle, mouseXAngle, 0);
            mainCamera.transform.rotation = targetRotation;

            targetRotation = Quaternion.Euler(0, mouseXAngle, 0);
            direction.transform.rotation = targetRotation;
        }


    }
}
