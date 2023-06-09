using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NB
{
    public class PlayerLocomotion : MonoBehaviour
    {
        public Rigidbody rb;
        public Transform direction;
        public Camera mainCamera;
        public Transform cameraHolder;
        public LayerMask groundLayer;


        [Header("Locomotion Settings")]
        [SerializeField] float maxSpeed = 7f;
        [SerializeField] float mouseXSensitivity;
        [SerializeField] float mouseYSensitivity;
        [SerializeField] float maxYAngle = 90;
        [SerializeField] float minYAngle = -90;
        [SerializeField] float groundedDrag = 5;
        [SerializeField] float playerHeight = 2;
        [SerializeField] float normalScale = 1f;

        [Header("Locomotion Settings_Walk")]
        [SerializeField] float walkMulitplier = 120f;
        [SerializeField] float maxWalkSpeed = 7f;

        [Header("Locomotion Settings_Run")]
        [SerializeField] float runMultiplier = 140f;
        [SerializeField] float maxRunSpeed = 10f;
        [Header("Locomotion Settings_Crouch")]
        [SerializeField] float crouchMultiplier = 80f;
        [SerializeField] float maxCrouchSpeed = 5f;
        [SerializeField] float crouchedScale = 0.75f;
        [Header("Locomotion Settings_Slope")]
        [SerializeField] float slopeMulitplier = 125f;
        [SerializeField] float maxSlopeAngle = 40f;
        [Header("Locomotion Settings_Slide")]
        [SerializeField] float maxSlideTime = 3f;
        [SerializeField] float slideDelay = 1f;
        [SerializeField] float slideMultiplier = 200f;
        [SerializeField] float maxSlideSpeed = 15f;
        [Header("Locomotion Settings_Air")]
        [SerializeField] float maxAirSpeed = 15f;
        [SerializeField] float jumpMultiplier = 300f;
        [SerializeField] float airDrag = 1;
        [SerializeField] float airMultiplier = 0.01f;
        [Header("Locomotion Settings_Wallrun")]
        [SerializeField] float wallRunSpeed = 140;
        [SerializeField] float maxWallRunSpeed = 10;




        [Header("Locomotion Values")]
        [SerializeField] float desiredMoveSpeed;
        [SerializeField] float lastDesiredMoveSpeed;
        [SerializeField] float mouseXAngle;
        [SerializeField] float mouseYAngle;
        [SerializeField] float speed;
        [SerializeField] bool isGrounded;
        [SerializeField] bool isOnSlope;
        [SerializeField] bool canJump;
        [SerializeField] bool canSlide;
        [SerializeField] float currentSlideTime;
        [SerializeField] bool isCrouching = false;
        [SerializeField] bool isSliding = false;
        [SerializeField] public bool isWallRunning = false;

        public MovementState state;

        public enum MovementState
        {
            walking,
            sprinting,
            sliding,
            wallrunning,
            crouching,
            air
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            canJump = true;
            canSlide = true;
        }

        // Handles PlayerLocomotion state and sets movement variables for each state
        public void StateHandler()
        {
            if (isWallRunning)
            {
                state = MovementState.wallrunning;
                speed = wallRunSpeed;
                maxSpeed = maxWallRunSpeed;
            }
            else if (isGrounded && InputHandler.instance.IsSprintInputPressed() && !isCrouching && !isSliding)
            {
                state = MovementState.sprinting;
                maxSpeed = maxRunSpeed;
                speed = runMultiplier;
            }
            else if (isGrounded && !isCrouching && !isSliding)
            {
                state = MovementState.walking;
                maxSpeed = maxWalkSpeed;
                speed = walkMulitplier;
            }
            else if (isGrounded && isSliding && !isCrouching)
            {
                state = MovementState.sliding;
                maxSpeed = maxSlideSpeed;
            }
            else if (isGrounded && isCrouching && !isSliding)
            {
                state = MovementState.crouching;
                maxSpeed = maxCrouchSpeed;
                speed = crouchMultiplier;
            }
            else
            {
                state = MovementState.air;
                maxSpeed = maxAirSpeed;
                speed = walkMulitplier;
            }
        }

        public void HandlePlayerMovement()
        {
            HandleGroundedCheck();
            HandleRigidBodyDrag();
            isOnSlope = CheckForSlope();
            float vertical = InputHandler.instance.verticalInput;
            float horizontal = InputHandler.instance.horizontalInput;
            //Calculate target forward direction by multiplying the forward vector of the direction transfom by the vertical input
            Vector3 forward = direction.transform.forward * vertical * Time.fixedDeltaTime;
            //Calculate target right direction by multiplying the right vector of the direction transfom by the horizontal input
            Vector3 right = direction.transform.right * horizontal * Time.fixedDeltaTime;
            // Get the inputDirection by adding the two input vectors together
            Vector3 inputDirection = forward + right;
            //Normalize the inputDirection vector to ensure magnitude is 1 and maintain consistent movement speed regardless of combination of inputs
            inputDirection.Normalize();

            //If is on slope, project inputDirection onto slope normal (see GetSlopeDirection)
            if (isOnSlope)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.25f, groundLayer))
                {
                    Vector3 slopeDirection = GetSlopeDirection(inputDirection);
                    rb.AddForce(slopeDirection * speed, ForceMode.Force);
                }
            }
            // Apply force to grounded character
            else if (isGrounded)
            {
                rb.AddForce(inputDirection * speed, ForceMode.Force);
            }
            // Apply force to character but multiply by air multipier
            else
            {
                rb.AddForce(inputDirection * speed * airMultiplier, ForceMode.Force);
            }
            HandleJump();
        }

        public void LimitSpeed()
        {
            float mag = rb.velocity.magnitude;
            // if magnitude (rb speed) is greater than maxSpeed, then normalize velocity and multiply by maxSpeed
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
                //Reset y velocity to 0 so we get consistent jump heights
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                //Apply force using ForceMode.Impulse which indicates the force should be applied instantaneously and with a large magnitude
                rb.AddForce(transform.up * jumpMultiplier, ForceMode.Impulse);
                canJump = false;
                StartCoroutine(JumpDelay());
            }
        }

        public void HandleSlide()
        {
            //START SLIDE
            if (!isSliding && InputHandler.instance.IsSlideInputPressed() && canSlide)
            {
                StartSlide();
            }

            //CONTINUE SLIDE 
            if (isSliding && InputHandler.instance.IsSlideInputPressed())
            {
                HandleSlideMovement();
            }
            //END SLIDE 
            if (isSliding && (!InputHandler.instance.IsSlideInputPressed() || currentSlideTime <= 0))
            {
                EndSlide();
            }
        }


        public void HandlePlayerRotation()
        {
            float newMouseX = InputHandler.instance.mouseHorizontalInput;
            float newMouseY = InputHandler.instance.mouseVerticalInput;

            //Add newMouseX to mouseXAngle. This will either increase or decrease the angle and is represented in degrees.
            //Explanation: When the game starts, mouseXAngle is at 0. When we move the mouse to the right and get an input of 50,
            // the mouseXAngle will be at a value of 50 and the character will be rotated horizontally by 50 degrees (look right)
            mouseXAngle += newMouseX;
            //Subtract newMouseY to mouseYAngle. This will either increase or decrease the angle and is represented in degrees.
            //Explanation: When the game starts, mouseYAngle is at 0. When we move the mouse to the up and get an input of 50,
            // the mouseYAngle will be at a value of -50 and the character will be rotated vertically by -50 degrees (look up)
            mouseYAngle -= newMouseY;

            //Clamp the mouseYAngle between a min and max value so player won't rotate all around the X axis (you can't look all the way behind you by rotating your head vertically, right?)
            mouseYAngle = Mathf.Clamp(mouseYAngle, minYAngle, maxYAngle);

            //Create a Quaternion using Euler rotating the Y angle on the x axis and the x angle on the y axis
            Quaternion targetRotation = Quaternion.Euler(mouseYAngle, mouseXAngle, 0);
            //rotate the cameraHolder transform in both input rotations
            cameraHolder.transform.rotation = targetRotation;

            //create a Quaternion using Euler rotating the Xangle on the y axis
            targetRotation = Quaternion.Euler(0, mouseXAngle, 0);
            //rotate the direction transform in just the x rotation
            direction.transform.rotation = targetRotation;
        }

        private void HandleGroundedCheck()
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

        private bool CheckForSlope()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.25f, groundLayer))
            {
                // Calculate the angle between the surface normal and the downward direction
                float slopeAngle = Vector3.Angle(hit.normal, -Vector3.down);
                // Check if the slope angle is within the acceptable range and not completely flat
                return slopeAngle < maxSlopeAngle && slopeAngle != 0;
            }
            else
            {
                return false;
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
                //Apply down force so because character scales in to center of bean
                rb.AddForce(Vector3.down * 30, ForceMode.Impulse);
                isCrouching = true;
            }
        }
        private void StartSlide()
        {
            isSliding = true;
            canSlide = false;
            currentSlideTime = maxSlideTime;
            transform.localScale = new Vector3(transform.localScale.x, crouchedScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 30, ForceMode.Impulse);
        }

        private void HandleSlideMovement()
        {
            // Slide in any direction character is moving
            float vertical = InputHandler.instance.verticalInput;
            float horizontal = InputHandler.instance.horizontalInput;
            Vector3 forward = direction.transform.forward * vertical * Time.fixedDeltaTime;
            Vector3 right = direction.transform.right * horizontal * Time.fixedDeltaTime;
            Vector3 inputDirection = forward + right;
            inputDirection.Normalize();
            if (CheckForSlope() || rb.velocity.y > -0.1)
            {
                rb.AddForce(inputDirection * slideMultiplier, ForceMode.Force);

                currentSlideTime -= Time.deltaTime;
            }
            else
            {
                Vector3 slopeDirection = GetSlopeDirection(inputDirection);
                rb.AddForce(slopeDirection * slideMultiplier, ForceMode.Force);
            }
        }

        private void EndSlide()
        {
            isSliding = false;
            transform.localScale = new Vector3(transform.localScale.x, normalScale, transform.localScale.z);
            StartCoroutine(SlideDelay());
        }

        private Vector3 GetSlopeDirection(Vector3 inputDirection)
        {
            RaycastHit hit;
            Vector3 slopeDirection;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, playerHeight * 0.5f + 0.25f, groundLayer))
            {
                // If the raycast hit the ground layer, calculate the slope direction
                // by projecting the input direction onto the plane defined by the surface normal.
                slopeDirection = Vector3.ProjectOnPlane(inputDirection, hit.normal);
            }
            else
            {
                // If not ground detection, then set slopeDirection to 0
                slopeDirection = Vector3.zero;
            }

            return slopeDirection;
        }

        private bool CanStand()
        {
            bool canStand;
            if (Physics.Raycast(transform.position, Vector3.up, 2f))
            {
                canStand = false;
            }
            else
            {
                canStand = true;
            }

            return canStand;
        }

        private IEnumerator JumpDelay()
        {
            yield return new WaitForSeconds(1);
            canJump = true;
        }

        private IEnumerator SlideDelay()
        {
            yield return new WaitForSeconds(slideDelay);
            canSlide = true;
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

    //ALTERNATE WAY TO LIMIT SPEED USING COUNTER MOVMENT
    // public void SpeedLimiter()
    // { // Check if the rigidbody is exceeding the speed limit
    //     Vector3 velocityWithoutY = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    //     if (velocityWithoutY.magnitude > maxSpeed)
    //     {
    //         // Calculate the force to apply
    //         float forceMagnitude = (velocityWithoutY.magnitude - maxSpeed) * 4;

    //         // Apply the force in the opposite direction of the rigidbody's velocity without Y component
    //         Vector3 force = -velocityWithoutY.normalized * forceMagnitude;
    //         rb.AddForce(force, ForceMode.Impulse);
    //     }
    // }
}
