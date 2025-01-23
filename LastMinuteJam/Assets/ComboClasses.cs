using UnityEngine;


using System.Collections.Generic;
using System;
public class Combo
{
    public enum Input
    {
        Left,
        Right,
        Up,
        Down,
        LightAttack,
        HeavyAttack,
        None,
    }

    public List<Input> sequence = new();
    public bool isGlobal = false;
    public ComboEffect comboEffect;

}
[Serializable]
public struct ComboEffect
{

    public enum Type
    {
        Speed,
        Damage,
        AttackSpeed,
        AttackSize
    }
    public Type type;

    public float value;

    public ComboEffect(Type type_, float value_)
    {
        type = type_;
        value = value_;
    }


}
