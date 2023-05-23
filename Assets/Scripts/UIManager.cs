using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NB
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
        public Text speedText;

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

        public void SetSpeedText(string speed)
        {
            speedText.text = speed;
        }
    }
}