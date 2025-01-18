using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    public float knockbackModifier;
    public float moveSpeed;
    public float maxSpeed;
    public float jumpSpeed;

}
