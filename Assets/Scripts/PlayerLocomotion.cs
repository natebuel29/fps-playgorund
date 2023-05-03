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
        public LayerMask groundLayer;


        [Header("Locomotion Settings")]
        [SerializeField] float walkMulitplier = 90f;
        [SerializeField] float runMultiplier = 120;
        [SerializeField] float maxSpeed = 10f;
        [SerializeField] float mouseXSensitivity;
        [SerializeField] float mouseYSensitivity;
        [SerializeField] float maxYAngle = 90;
        [SerializeField] float minYAngle = -90;
        [SerializeField] float groundedDrag = 5;
        [SerializeField] float airDrag = 1;
        [SerializeField] float airMultiplier = 0.01f;
        [SerializeField] float playerHeight = 2;
        [SerializeField] float jumpMultiplier = 300f;



        [Header("Locomotion Values")]
        [SerializeField] float mouseXAngle;
        [SerializeField] float mouseYAngle;
        [SerializeField] float currentMultiplier;
        [SerializeField] bool isGrounded;
        [SerializeField] bool canJump;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            canJump = true;
        }

        public void HandlePlayerMovement()
        {
            HandleGroundedCheck();
            HandleRigidBodyDrag();
            if (InputHandler.instance.IsSprintInputPressed())
            {
                currentMultiplier = runMultiplier;
            }
            else
            {
                currentMultiplier = walkMulitplier;
            }
            float vertical = InputHandler.instance.verticalInput;
            float horizontal = InputHandler.instance.horizontalInput;
            Vector3 forward = direction.transform.forward * vertical * Time.fixedDeltaTime;
            Vector3 right = direction.transform.right * horizontal * Time.fixedDeltaTime;
            Vector3 inputVelocity = forward + right;
            inputVelocity.Normalize();
            if (isGrounded)
            {
                rb.AddForce(inputVelocity * currentMultiplier, ForceMode.Force);
            }
            else
            {
                rb.AddForce(inputVelocity * currentMultiplier * airMultiplier, ForceMode.Force);
            }

            HandleJump();
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

        public void HandleJump()
        {
            if (isGrounded && InputHandler.instance.IsJumpInputPressed() && canJump)
            {
                rb.AddForce(Vector3.up * jumpMultiplier, ForceMode.Force);
                canJump = false;
                StartCoroutine(JumpDelay());
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

        public void HandleGroundedCheck()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.15f, groundLayer))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }

        public IEnumerator JumpDelay()
        {
            yield return new WaitForSeconds(1);
            canJump = true;

        }

        public void HandleRigidBodyDrag()
        {
            if (!isGrounded)
            {
                rb.drag = airDrag;
            }
            else
            {
                rb.drag = groundedDrag;
            }
        }
    }
}
