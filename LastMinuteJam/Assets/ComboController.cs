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

    bool comboIsActive = false;
    [SerializeField] ComboData comboData;
    [SerializeField] ComboEffectData comboEffectData;
    int playerId;

    public Combo activeCombo;
    public Combo.Input lastInput = Combo.Input.None;
    public Combo.Input requiredInput = Combo.Input.None;
    Combo inputCombo;
    List<Combo> activeCombos = new();
    public NetworkedPlayerController myPlayer;
    public List<NetworkedPlayerController> allPlayers = new();

    int comboProgressIndex = 0;
    public bool failable = false;
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
        activeCombos.Add(GenerateRandomCombo(item.difficulty));
        comboItems.Remove(item);
        Sandbox.Destroy(item.GetComponent<NetworkObject>());
    }

    [Rpc(target: RpcPeers.Everyone, localInvoke:true)]
    public void GetComboRpc(int playerId, int seed, int difficulty, bool isGlobal)
    {
        
        if (Sandbox.LocalPlayer.PlayerId == playerId)
        {
            //InputSource = Sandbox.LocalPlayer;
            UnityEngine.Random.InitState(seed);
            activeCombo = GenerateRandomCombo(difficulty);
            requiredInput = activeCombo.sequence[0];
            activeCombo.isGlobal = isGlobal;
            comboIsActive = true;
            model.comboUIcontroller.DisplayCombo(activeCombo);
            inputCombo = new();
            comboProgressIndex = 0;
            StopAllCoroutines();
            StartCoroutine(DelayFailable(1f));
            StartCoroutine(FailComboOnTime(comboData.timeToComplete));
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
        if (comboIsActive)
        {
            ProcessInput();
        }
        

    }
        public void ProcessInput()
        {
        /*
        if (!FetchInput(out FighterInput frameInput))
        {
            return;
        }
        */
        Combo.Input newInput = FighterInputToComboInput(myPlayer.latestInput);
        if (newInput == Combo.Input.None || lastInput == newInput)
        {
            lastInput = newInput;
            return;
        }
        lastInput = newInput;
        inputCombo.sequence.Add(newInput);

        if (newInput == activeCombo.sequence[comboProgressIndex] && comboProgressIndex < activeCombo.sequence.Count)
        {
            Debug.Log("Hit combo " + requiredInput);
            model.comboUIcontroller.OnComboHit(comboProgressIndex++);
            if (comboProgressIndex < activeCombo.sequence.Count)
            {
                requiredInput = activeCombo.sequence[comboProgressIndex];
            }
            else
            {
                requiredInput = Combo.Input.None;
            }
            StartCoroutine(DelayFailable(0.2f));
        }
        else if(failable)
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
        comboIsActive = false;
        StopAllCoroutines();
        if (IsServer)
        {

            ApplyComboEffectRpc(myPlayer.GetComponent<NetworkedKinematicObject>().Id, activeCombo.comboEffect);
        }
        else
        {
            SendComboToServerRpc(myPlayer.GetComponent<NetworkedKinematicObject>().Id, activeCombo.comboEffect);
        }
    }
    [Rpc(source: RpcPeers.Everyone, target: RpcPeers.Owner)]
    public void SendComboToServerRpc(int playerId, ComboEffect comboEffect)
    {
        ApplyComboEffectRpc(playerId, comboEffect);
    }

    [Rpc(target: RpcPeers.Everyone, localInvoke: true)]
    public void ApplyComboEffectRpc(int playerId, ComboEffect comboEffect)
    {
        foreach (NetworkedPlayerController player in allPlayers)
        {
            if (playerId == player.GetComponent<NetworkedKinematicObject>().Id)
            {
                player.ApplyComboEffect(comboEffect);
            }
        }
    }


    void OnComboFail()
    {
        model.comboUIcontroller.OnComboFail(comboProgressIndex);
        comboIsActive = false;

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

    IEnumerator DelayFailable(float delay)
    {
        failable = false;
        float time = 0;
        while (time < delay)
        {
            time += Time.deltaTime;
            yield return null;
        }
        failable = true;
    }


    IEnumerator FailComboOnTime(float maxTime)
    {
        float time = 0;
        while (time < maxTime)
        {
            time += Time.deltaTime;
            yield return null;
        }
        OnComboFail();
    }
}
