using System;
using Netick;
using Netick.Unity;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;


public class CollectableItem : NetworkBehaviour
    {
       
        public int difficulty;
        ComboController comboController;
    
        public void CollectItem(NetworkPlayer player)
        {
        // Only happens in server
            comboController.OnComboCollected(this, player);
            
            Destroy(this);
            Debug.Log("destroyed");
        }

        public void Init(ComboController comboController_, int difficulty_)
        {
            comboController = comboController_;
            difficulty = difficulty_;
        }
    }
