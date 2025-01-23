using UnityEngine;
using System.Collections.Generic;
using Netick.Unity;
using Platformer.Model;
using static Platformer.Core.SimulationNetick;
using Netick;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using System;
using Platformer.Mechanics;


public class ComboController : NetworkBehaviour
{

    List<CollectableItem> comboItems = new();

    GameObject pickupPrefab;
    PlatformerModel model;

    bool comboIsActive = true;
    [SerializeField] ComboData comboData;
    int playerId;

    Combo activeCombo;
    Combo.Input lastInput;
    Combo inputCombo;
    List<Combo> activeCombos = new();

    int comboProgressIndex = 0;
    public void Init(PlatformerModel model_)
    {
        model = model_;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void NetworkStart()
    {
        pickupPrefab = Sandbox.GetPrefab("Square");
    }




    // TODO: Spawn pickups periodically
    public void SpawnPickup()
    {
        GameObject newPickup = Sandbox.NetworkInstantiate(pickupPrefab, GetPickupSkySpawn(), Quaternion.identity).gameObject;

        comboItems.Add(newPickup.GetComponent<CollectableItem>());
    }

    public void OnComboCollected(CollectableItem item, NetworkPlayer player)
    {
        // this runs on server
        int comboSeed = UnityEngine.Random.Range(0, 1000);

        GetComboRpc(player.PlayerId, comboSeed, item.difficulty, false);
        UnityEngine.Random.InitState(comboSeed);
        activeCombos.Add(GenerateRandomCombo(item.difficulty));
    }

    [Rpc(target: RpcPeers.Everyone, localInvoke:true)]
    public void GetComboRpc(int playerId, int seed, int difficulty, bool isGlobal)
    {
        
        if (Sandbox.LocalPlayer.PlayerId == playerId)
        {
            UnityEngine.Random.InitState(seed);
            activeCombo = GenerateRandomCombo(difficulty);
            activeCombo.isGlobal = isGlobal;
            comboIsActive = true;
            model.comboUIcontroller.DisplayCombo(activeCombo);
            inputCombo = new();
            comboProgressIndex = 0;
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
            activeCombo.sequence.Add((Combo.Input)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(Combo.Input)).Length));
        }
        return combo;
    }

    public override void NetworkFixedUpdate()
    {
        if (comboIsActive)
        {
            ProcessInput();
        }
    }
        public void ProcessInput()
        {
        if (!FetchInput(out FighterInput frameInput))
        {
            return;
        }
        Combo.Input newInput = FighterInputToComboInput(frameInput);
        if (newInput != lastInput)
        {
            lastInput = newInput;
        } 
        else
        {
            return;
        }
        inputCombo.sequence.Add(newInput);

        if (newInput == activeCombo.sequence[comboProgressIndex] && comboProgressIndex < activeCombo.sequence.Count)
        {
            model.comboUIcontroller.OnComboHit(comboProgressIndex++);
        }
        else
        {
            OnComboFail();
        }

        if (comboProgressIndex == activeCombo.sequence.Count)
        {
            OnComboComplete();            

        }
    }

    void OnComboComplete()
    {
        model.comboUIcontroller.OnComboCompleted();
    }
    void OnComboFail()
    {
        model.comboUIcontroller.OnComboFail(comboProgressIndex);

    }

    Combo.Input FighterInputToComboInput(FighterInput input)
    {
        NetworkedPlayerController.Direction dir = NetworkedPlayerController.UpdateDirection(input.movement, NetworkedPlayerController.Direction.None);
        if (input.lightAttack)
        {
            return Combo.Input.LightAttack;
        }
        else if(input.heavyAttack)
        {
            return Combo.Input.HeavyAttack;
        }
        else if(dir != NetworkedPlayerController.Direction.None)
        {
            return DirectiontoComboInput(dir);
        }
        else
        {
            return Combo.Input.None;
        }
            
    }

    public Combo.Input DirectiontoComboInput(NetworkedPlayerController.Direction dir)
    {
        Combo.Input input = Combo.Input.None;
        switch (dir)
        {
            case NetworkedPlayerController.Direction.Up:
                input = Combo.Input.Up;
                break;
            case NetworkedPlayerController.Direction.Left:
                input = Combo.Input.Left;
                break;
            case NetworkedPlayerController.Direction.Down:
                input = Combo.Input.Down;
                break;
            case NetworkedPlayerController.Direction.Right:
                input = Combo.Input.Right;
                break;
            default:
            case NetworkedPlayerController.Direction.None:
                break;
        }
        return input;
    }

    Vector3 GetPickupSkySpawn()
    {
        return Vector3.Lerp(model.topLeft.position, model.topRight.position, UnityEngine.Random.Range(0, 1f));
    }


}
