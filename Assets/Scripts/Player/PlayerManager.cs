using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NB
{
    public class PlayerManager : MonoBehaviour
    {
        PlayerLocomotion playerLocomotion;

        private void Start()
        {
            playerLocomotion = GetComponent<PlayerLocomotion>();
        }
        private void Update()
        {
            playerLocomotion.HandlePlayerRotation();
            //playerLocomotion.LimitSpeed();
        }

        private void FixedUpdate()
        {
            playerLocomotion.HandlePlayerMovement();
        }
    }
}