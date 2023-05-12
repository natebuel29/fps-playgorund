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
        public Transform cameraHolder;
        public LayerMask groundLayer;


        [Header("Locomotion Settings")]
        [SerializeField] float crouchMultiplier = 80f;
        [SerializeField] float walkMulitplier = 120f;
        [SerializeField] float slopeMulitplier = 125f;
        [SerializeField] float runMultiplier = 140f;
        [SerializeField] float maxCrouchSpeed = 5f;
        [SerializeField] float maxWalkSpeed = 7f;
        [SerializeField] float maxRunSpeed = 10f;
        [SerializeField] float maxSpeed = 7f;
        [SerializeField] float mouseXSensitivity;
        [SerializeField] float mouseYSensitivity;
        [SerializeField] float maxYAngle = 90;
        [SerializeField] float minYAngle = -90;
        [SerializeField] float groundedDrag = 5;
        [SerializeField] float airDrag = 1;
        [SerializeField] float airMultiplier = 0.01f;
        [SerializeField] float playerHeight = 2;
        [SerializeField] float jumpMultiplier = 300f;
        [SerializeField] float maxSlopeAngle = 40f;
        [SerializeField] float normalScale = 1f;
        [SerializeField] float crouchedScale = 0.75f;



        [Header("Locomotion Values")]
        [SerializeField] float mouseXAngle;
        [SerializeField] float mouseYAngle;
        [SerializeField] float currentMultiplier;
        [SerializeField] bool isGrounded;
        [SerializeField] bool isOnSlope;
        [SerializeField] bool canJump;
        [SerializeField] bool isCrouching = false;

        public MovementState state;

        public enum MovementState
        {
            walking,
            sprinting,
            crouching,
            air
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            canJump = true;
        }

        public void StateHandler()
        {
            if (isGrounded && InputHandler.instance.IsSprintInputPressed() && !isCrouching)
            {
                state = MovementState.sprinting;
                maxSpeed = maxRunSpeed;
                currentMultiplier = runMultiplier;
            }
            else if (isGrounded && !isCrouching)
            {
                state = MovementState.walking;
                maxSpeed = maxWalkSpeed;
                currentMultiplier = walkMulitplier;
            }
            else if (isGrounded && isCrouching)
            {
                state = MovementState.crouching;
                maxSpeed = maxCrouchSpeed;
                currentMultiplier = crouchMultiplier;
            }
            else
            {
                state = MovementState.air;
                currentMultiplier = walkMulitplier * airMultiplier;
            }
        }

        public void HandlePlayerMovement()
        {
            HandleGroundedCheck();
            HandleRigidBodyDrag();
            CheckForSlope();
            float vertical = InputHandler.instance.verticalInput;
            float horizontal = InputHandler.instance.horizontalInput;
            Vector3 forward = direction.transform.forward * vertical * Time.fixedDeltaTime;
            Vector3 right = direction.transform.right * horizontal * Time.fixedDeltaTime;
            Vector3 inputVelocity = forward + right;
            inputVelocity.Normalize();
            if (isOnSlope)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.25f, groundLayer))
                {
                    Vector3 slopeVelocity = Vector3.ProjectOnPlane(inputVelocity, hit.normal);
                    rb.AddForce(slopeVelocity * currentMultiplier, ForceMode.Force);
                }
            }
            else if (isGrounded)
            {
                rb.AddForce(inputVelocity * currentMultiplier, ForceMode.Force);
            }
            else
            {
                rb.AddForce(inputVelocity * currentMultiplier, ForceMode.Force);
            }
            HandleJump();
            SpeedLimiter();
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

        public void SpeedLimiter()
        { // Check if the rigidbody is exceeding the speed limit
            Vector3 velocityWithoutY = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (velocityWithoutY.magnitude > maxSpeed)
            {
                // Calculate the force to apply
                float forceMagnitude = (velocityWithoutY.magnitude - maxSpeed) * 2;

                // Apply the force in the opposite direction of the rigidbody's velocity without Y component
                Vector3 force = -velocityWithoutY.normalized * forceMagnitude;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }

        public void HandleJump()
        {
            if (isGrounded && InputHandler.instance.IsJumpInputPressed() && canJump)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(transform.up * jumpMultiplier, ForceMode.Impulse);
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
            cameraHolder.transform.rotation = targetRotation;

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

        public void CheckForSlope()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.25f, groundLayer))
            {
                float slopeAngle = Vector3.Angle(hit.normal, -Vector3.down);
                isOnSlope = slopeAngle < maxSlopeAngle && slopeAngle != 0;
            }
            else
            {
                isOnSlope = false;
            }
        }

        public void HandlePlayerCrouch()
        {
            if (isCrouching && CanStand())
            {
                transform.localScale = new Vector3(transform.localScale.x, normalScale, transform.localScale.z);
                isCrouching = false;
            }
            else if (isGrounded)
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchedScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 30, ForceMode.Impulse);
                isCrouching = true;
            }
        }

        public bool CanStand()
        {
            bool canStand;
            if (Physics.Raycast(transform.position, Vector3.up, 2f))
            {
                return false;
            }
            else
            {
                canStand = true;
            }

            return canStand;
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
