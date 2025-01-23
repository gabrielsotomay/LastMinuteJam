using UnityEngine;

[CreateAssetMenu(fileName = "ComboEffectData", menuName = "Scriptable Objects/ComboEffectData")]
public class ComboEffectData : ScriptableObject
{
    public ComboEffect speedEasy;
    public ComboEffect speedMedium;
    public ComboEffect speedHard;
    public ComboEffect damageEasy;
    public ComboEffect damageMedium;
    public ComboEffect damageHard;
    public ComboEffect attackSpeedEasy;
    public ComboEffect attackSpeedMedium;
    public ComboEffect attackSpeedHard;
    public ComboEffect attackSizeEasy;
    public ComboEffect attackSizeMedium;
    public ComboEffect attackSizeHard;

    public float comboEffectTime;
}
