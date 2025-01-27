using UnityEngine;
using System.Collections.Generic;
using Netick.Unity;
using Platformer.Model;
using Netick;
using Platformer.Mechanics;
using System.Collections;


public class ComboController : NetworkBehaviour
{

    List<CollectableItem> comboItems = new();

    [SerializeField] GameObject pickupPrefab;
    PlatformerModel model;

    [SerializeField] ComboData comboData;
    [SerializeField] ComboEffectData comboEffectData;
    int playerId;

    List<Combo> activeCombos = new();
    public NetworkedPlayerController myPlayer;
    public List<NetworkedPlayerController> allPlayers = new();

    public bool globalComboActive = false;
    public void Init(PlatformerModel model_)
    {
        model = model_;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void NetworkStart()
    {
        
    }




    // TODO: Spawn pickups periodically
    public void SpawnPickup()
    {        
        CollectableItem newPickup = Sandbox.NetworkInstantiate(pickupPrefab, GetPickupSkySpawn(), Quaternion.identity).GetComponent<CollectableItem>();
        newPickup.Init(this, UnityEngine.Random.Range(1, 4));
        comboItems.Add(newPickup);
    }

    public void OnComboCollected(CollectableItem item, NetworkPlayer player)
    {
        // this runs on server
        int comboSeed = (int)Time.time;

        GetComboRpc(player.PlayerId, comboSeed, item.difficulty, false);
        UnityEngine.Random.InitState(comboSeed);
        Combo newCombo = GenerateRandomCombo(item.difficulty);
        activeCombos.Add(newCombo);
        comboItems.Remove(item);
        Sandbox.Destroy(item.GetComponent<NetworkObject>());

        foreach (NetworkedPlayerController playerController in allPlayers)
        {
            if (playerController.InputSource.PlayerId == player.PlayerId)
            {
                playerController.comboController.ReceiveCombo(newCombo, false);
            }
        }
    }

    [Rpc(target: RpcPeers.Everyone, localInvoke:true)]
    public void GetComboRpc(int playerId, int seed, int difficulty, bool isGlobal)
    {
        UnityEngine.Random.InitState(seed);
        Combo newCombo = GenerateRandomCombo(difficulty);

        if (Sandbox.LocalPlayer.PlayerId == playerId)
        {
            model.comboUIcontroller.DisplayCombo(newCombo);
            //InputSource = Sandbox.LocalPlayer;
            if (isGlobal)
            {
                globalComboActive = true;
            }
        }


    }



    public Combo GenerateRandomCombo(int difficulty)
    {
        Combo combo = new Combo();
        int comboLength = 0;
        switch (difficulty)
        {
            case 1:
                comboLength = comboData.easyLength;
                break;
            case 2:
                comboLength = comboData.mediumLength;
                break;
            case 3:
                comboLength = comboData.hardLength;
                break;
        }
        for (int i = 0; i < comboLength; i++)
        {
            combo.sequence.Add((Combo.Input)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(Combo.Input)).Length - 1));
        }
        ComboEffect.Type effectType = (ComboEffect.Type)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(ComboEffect.Type)).Length);
        combo.comboEffect = new ComboEffect(effectType, GetDifficultyValue(difficulty, effectType));

        return combo;
    }


    public float GetDifficultyValue(int difficulty, ComboEffect.Type effectType)
    {
        switch (effectType)
        {
            case ComboEffect.Type.Speed:
                if (difficulty == 1)
                {
                    return comboEffectData.speedEasy.value;
                }
                else if (difficulty == 2)
                {
                    return comboEffectData.speedMedium.value;

                }
                else if (difficulty == 3)
                {
                    return comboEffectData.speedHard.value;
                }
                return 0;
            case ComboEffect.Type.Damage:
                if (difficulty == 1)
                {
                    return comboEffectData.damageEasy.value;
                }
                else if (difficulty == 2)
                {
                    return comboEffectData.damageMedium.value;
                }
                else if (difficulty == 3)
                {
                    return comboEffectData.damageHard.value;
                }
                return 0;
            case ComboEffect.Type.AttackSpeed:
                if (difficulty == 1)
                { 
                    return comboEffectData.attackSpeedEasy.value;
                }
                else if (difficulty == 2)
                {
                    return comboEffectData.attackSpeedMedium.value;
                }
                else if (difficulty == 3)
                {
                    return comboEffectData.attackSpeedHard.value;
                }
                return 0;
            case ComboEffect.Type.AttackSize:
                if (difficulty == 1)
                {
                    return comboEffectData.attackSizeEasy.value;

                }
                else if (difficulty == 2)
                {
                    return comboEffectData.attackSizeMedium.value;
                }
                else if (difficulty == 3)
                {
                    return comboEffectData.attackSizeHard.value;
                }
                return 0;
            default:
                return 0;
        }
    }

    public override void NetworkFixedUpdate()
    {
        if (IsServer)
        {
            if (comboItems.Count == 0)
            {
                SpawnPickup();
            }
        }       

    }
    


    Vector3 GetPickupSkySpawn()
    {
        return Vector3.Lerp(model.topLeft.position, model.topRight.position, UnityEngine.Random.Range(0, 1f));
    }

}
