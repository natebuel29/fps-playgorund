using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NB
{

    //This was cppied and modified from Dave / Game Development tutorial series:https://www.youtube.com/watch?v=gNt9wBOrQO4
    //I wasn't sure how to code wall running and wanted to know how
    public class WallRunning : MonoBehaviour
    {
        public CameraRotation cameraRotation;

        [Header("Wallrunning")]
        public LayerMask whatIsWall;
        public LayerMask whatIsGround;
        public float wallRunForce;
        public float maxWallRunTime;
        public float wallClimbSpeed = 3f;
        public float wallRunTimer;
        public float wallJumpUpForce;
        public float wallJumpSideForce;

        [Header("Exiting")]
        public bool exitingWall;
        public float exitWallTime;
        public float exitWallTimer;

        [Header("Detection")]
        public float wallCheckDistance;
        public float minJumpHeight;
        private RaycastHit leftWallHit;
        private RaycastHit rightWallHit;
        public bool wallLeft;
        public bool wallRight;
        private bool upwardsRunning;
        private bool downwardsRunning;

        [Header("References")]
        public Transform orientation;
        private PlayerLocomotion playerLocomotion;
        private Rigidbody rb;

        private void Start()
        {
            playerLocomotion = GetComponent<PlayerLocomotion>();
            rb = GetComponent<Rigidbody>();
            orientation = playerLocomotion.direction;
            cameraRotation = FindObjectOfType<CameraRotation>();
        }

        void Update()
        {
            CheckForWall();
            StateMachine();
        }

        private void FixedUpdate()
        {
            if (playerLocomotion.isWallRunning)
            {
                WallRunMovement();
            }
        }

        private void CheckForWall()
        {
            wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
            wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
        }

        private bool AboveGround()
        {
            return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
        }

        private void StateMachine()
        {

            upwardsRunning = InputHandler.instance.IsClimbWallPressed();
            downwardsRunning = InputHandler.instance.IsDescendWallPressed();

            if ((wallLeft || wallRight) && InputHandler.instance.IsVerticalInputPressed() && AboveGround() && !exitingWall)
            {
                if (!playerLocomotion.isWallRunning)
                {
                    StartWallRun();
                }

                if (wallRunTimer > 0)
                    wallRunTimer -= Time.deltaTime;

                if (wallRunTimer <= 0 && playerLocomotion.isWallRunning)
                {
                    exitingWall = true;
                    exitWallTimer = exitWallTime;
                }

                if (InputHandler.instance.IsJumpInputPressed())
                    WallJump();
            }

            else if (exitingWall)
            {
                if (playerLocomotion.isWallRunning)
                {
                    StopWallRun();
                }

                if (exitWallTimer > 0)
                    exitWallTimer -= Time.deltaTime;

                if (exitWallTimer <= 0)
                    exitingWall = false;
            }
            else
            {
                StopWallRun();
            }
        }

        private void StartWallRun()
        {
            playerLocomotion.isWallRunning = true;
            wallRunTimer = maxWallRunTime;
            if (wallLeft)
            {
                Quaternion targetRotation = Quaternion.Euler(0, 0, -10f);
                StartCoroutine(cameraRotation.RotateCameraUsingLerpCoroutine(targetRotation, 4, 0.3f));
            }
            else if (wallRight)
            {
                Quaternion targetRotation = Quaternion.Euler(0, 0, 10f);
                StartCoroutine(cameraRotation.RotateCameraUsingLerpCoroutine(targetRotation, 4, 0.3f));
            }


        }

        private void WallRunMovement()
        {
            float vertical = InputHandler.instance.verticalInput;
            float horizontal = InputHandler.instance.horizontalInput;
            rb.useGravity = false;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

            Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

            if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            {
                wallForward = -wallForward;
            }

            rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

            // upwards/downwards force
            if (upwardsRunning)
                rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
            if (downwardsRunning)
                rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

            // push to wall force
            if (!(wallLeft && horizontal > 0) && !(wallRight && vertical < 0))
                rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }

        private void WallJump()
        {
            exitingWall = true;
            exitWallTimer = exitWallTime;
            Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

            Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(forceToApply, ForceMode.Impulse);
        }

        private void StopWallRun()
        {
            if (playerLocomotion.isWallRunning == false)
                return;
            playerLocomotion.isWallRunning = false;
            rb.useGravity = true;
            StopAllCoroutines();
            Quaternion targetRotation = Quaternion.Euler(0, 0, 0f);
            StartCoroutine(cameraRotation.RotateCameraUsingLerpCoroutine(targetRotation, 5, 0.3f));
        }
    }
}