using System;
using Netick.Unity;
using UnityEditor.UI;
using UnityEngine;


    public class CollectableItem : MonoBehaviour
    {
        [SerializeField] ComboInput comboInput;
        [SerializeField] private int comboDifficulty;

        public void CollectItem()
        {
            Debug.Log("trigger is working");
            comboInput._playComboEvent = true;
            comboInput.GenerateRandomCombo(comboDifficulty);
            comboInput.DisplayCombo();
            Destroy(gameObject);
        }
    }
