using System;
using Netick.Unity;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;


public class CollectableItem : NetworkBehaviour
    {
        [SerializeField] ComboInput comboInput;
        [SerializeField] private int comboDifficulty;

        public void CollectItem()
        {

            if (Sandbox.LocalPlayer == InputSource)
            {
                comboInput._playComboEvent = true;
                comboInput.GenerateRandomCombo(comboDifficulty);
                comboInput.DisplayCombo();
                gameObject.SetActive(false);
                Debug.Log("destroyed");
            }
            
        }
    }
