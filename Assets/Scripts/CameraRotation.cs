using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NB
{
    public class CameraRotation : MonoBehaviour
    {

        public IEnumerator RotateCameraUsingLerpCoroutine(Quaternion targetRotation, float rotationSpeed, float rotationDuration)
        {
            Quaternion currentRotation = transform.localRotation;
            float elapsedTime = 0f;

            while (elapsedTime < rotationDuration)
            {
                // increase the elapsed time
                elapsedTime += Time.deltaTime;
                // Calculate a normalized interpolation parameter. 
                // Clamp01 will ensure the value is between 0 and 1
                // The interpolation parameter determines the blending ration during the interpolation process
                // For example: t = 0, then new rotation = current rotation - t = 0.5, the new rotation is 50% between curentRotation and targetRotation
                // t = 1, then new rotation = targetRotation
                float t = Mathf.Clamp01(elapsedTime / rotationDuration);
                // Interpolate between the current rotation and the target rotation using a lerp
                Quaternion newRotation = Quaternion.Lerp(currentRotation, targetRotation, t);
                transform.localRotation = newRotation;
                // Yield control and wait for the next frame
                yield return null;
            }
        }
    }
}