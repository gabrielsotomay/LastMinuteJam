using Netick.Unity;
using Platformer.Mechanics;
using UnityEngine;
using UnityEngine.UI;

public class DamageTestController : MonoBehaviour
{
    public Slider healthSlider;
    public Slider oldHealthSlider;
    public HealthBarControllerUI healthBarPrefab;
    public GameObject leftHealthBarContainer;
    public GameObject rightHealthBarContainer;
    public HealthBarController healthBarController;
    public Health health;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        healthSlider.onValueChanged.AddListener(x => health.Hurt(x));
        healthBarController.TestNewBar(PlayerData.Character.JJ, health);
        healthBarController.TestNewBar(PlayerData.Character.Elvira, health);

    }

    /*
    public void DoDamage(float health)
    {
        healthBar.ShowDamage(health, oldHealthSlider.value);

    }


    public void SetHealth(float health)
    {
        healthBar.ShowHealth(health);
    }
    */
}
