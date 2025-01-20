using System;
using UnityEditor.UI;
using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [SerializeField] private ComboInput comboInput;
    [SerializeField] private int comboDifficulty = 3;
    private void OnTriggerEnter2D(Collider2D other)
    {
        comboInput._playComboEvent = true;
        comboInput.GenerateRandomCombo(comboDifficulty);
        comboInput.DisplayCombo();
        Destroy(this.gameObject);
    }
}
