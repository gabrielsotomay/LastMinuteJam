using LastMinuteJam;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackTypes", menuName = "Scriptable Objects/PlayerAttackTypes")]
public class PlayerAttackTypes : ScriptableObject
{
    public PlayerAttack basicAttack;
    public PlayerAttack heavyAttack;
    public PlayerAttack specialAttack;

}
