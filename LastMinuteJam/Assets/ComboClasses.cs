using UnityEngine;


using System.Collections.Generic;
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


}