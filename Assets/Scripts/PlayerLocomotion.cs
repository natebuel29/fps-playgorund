using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NB
{
    public class PlayerLocomotion : MonoBehaviour
    {
        Rigidbody rb;

        public Transform direction;


        [Header("Locomotion Settings")]
        [SerializeField] float walkMulitplier = 50f;
        [SerializeField] float maxSpeed = 10f;
        [SerializeField] float mouseXSensitivity;
        [SerializeField] float mouseYSensitivity;


        [Header("Locomotion Values")]
        float mouseXAngle;
        float mouseYAngle;

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
            //Get Mouse input
            //Update the mouseXAngle and mouseYAngle by these values
            //Clamp the Y Angle
            // Create a Euler with Y rotation and rotate camera on X axis
            //Create a Euler with X  and Y rotation and rotate direction on both axis
        }


    }
}
