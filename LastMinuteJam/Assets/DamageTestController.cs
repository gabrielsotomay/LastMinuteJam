using Netick.Unity;
using UnityEngine;
using UnityEngine.UI;

public class DamageTestController : MonoBehaviour
{
    public Slider healthSlider;
    public Slider oldHealthSlider;
    public HealthBarControllerUI healthBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //healthSlider.onValueChanged.AddListener(x => DoDamage(x));
        //oldHealthSlider.onValueChanged.AddListener(x => SetHealth(x));
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
