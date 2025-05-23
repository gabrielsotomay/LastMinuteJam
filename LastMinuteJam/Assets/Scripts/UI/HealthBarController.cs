using UnityEngine;
using System.Collections.Generic;
using Netick;
using Platformer.Mechanics;
using NUnit.Framework.Constraints;

public class HealthBarController : MonoBehaviour
{ 
    public List<Transform> healthBarPositions;

    public List<HealthBarControllerUI> healthBars = new();
    public GameObject healthBarPrefab;

    public List<Sprite> JJIcons;
    public List<Sprite> ElviraIcons;
    public List<Sprite> emptyHealthBars;
    public List<Sprite> fullHealthBars;



    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void AddNew(NetworkedPlayerController player)
    {
        HealthBarControllerUI newHealthBar = Instantiate(healthBarPrefab, healthBarPositions[healthBars.Count]).GetComponent<HealthBarControllerUI>();
        Sprite newIcon = JJIcons[0];
        Sprite newEmptySprite;
        Sprite newFullSprite;
        bool isLeft = healthBars.Count == 0; // second character is on right (!isLeft)
        switch (player.character)
        {
            case PlayerData.Character.JJ:
                newIcon = JJIcons[healthBars.Count];
                break;
            case PlayerData.Character.Elvira:
                newIcon = ElviraIcons[healthBars.Count];
                break;
            default:
                break;
        }
        newEmptySprite = emptyHealthBars[healthBars.Count];
        newFullSprite = fullHealthBars[healthBars.Count];
        newHealthBar.Init(isLeft, newIcon, newEmptySprite, newFullSprite, newFullSprite);

        player.health.OnPlayerHurt += newHealthBar.ShowDamage;
        player.health.OnPlayerDamaged += newHealthBar.ShowHealth;


        healthBars.Add(newHealthBar);

        
    }

    /*
    public void ShowDamage(NetworkPlayer networkPlayer, float damage, float health)
    {
        foreach (HealthBarControllerUI bar in healthBars)
        {
            if (bar.player == networkPlayer)
            {
                bar.ShowDamage(damage, health);
            }
        }
    }
    public void UpdateHealth(NetworkPlayer networkPlayer, float health)
    {
        foreach (HealthBarControllerUI bar in healthBars)
        {
            if (bar.player == networkPlayer)
            {
                bar.ShowHealth(health);
            }
        }
    }
    */
}
