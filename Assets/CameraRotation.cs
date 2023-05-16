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
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / rotationDuration);
                Quaternion newRotation = Quaternion.Lerp(currentRotation, targetRotation, t);
                transform.localRotation = newRotation;
                yield return null;
            }
        }
    }
}