using UnityEngine;
using Netick.Unity;
using Netick;
using Platformer.Mechanics;
using UnityEngine.EventSystems;
using Platformer.Model;
using System.Collections;
using static Platformer.Core.SimulationNetick;

public class PlayerComboController : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] ComboData comboData;
    readonly PlatformerModel model = GetModel<PlatformerModel>();
    NetworkedPlayerController thisPlayer;
    public Combo.Input lastInput = Combo.Input.None;
    public Combo.Input requiredInput = Combo.Input.None;
    public Combo activeCombo;
    bool comboIsActive = false;
    int comboProgressIndex = 0;
    public bool failable = false;

    private void Awake()
    {
        thisPlayer = GetComponent<NetworkedPlayerController>();
    }
    // 
    public override void NetworkFixedUpdate()
    {
        if (IsServer && comboIsActive)
        {
            ProcessInput();
        }
    }

    public void ReceiveCombo(Combo newCombo, bool isGlobal)
    {
        activeCombo = newCombo;
        activeCombo.isGlobal = isGlobal;
        requiredInput = activeCombo.sequence[0];
        comboIsActive = true;
        comboProgressIndex = 0;
        StopAllCoroutines();
        StartCoroutine(DelayFailable(1f));
        StartCoroutine(FailComboOnTime(comboData.timeToComplete));
    }
    [Rpc(target: RpcPeers.InputSource)]
    public void DisplayComboLocally()
    {
        model.comboUIcontroller.DisplayCombo(activeCombo);
    }

    public void ProcessInput()
    {
        if (FetchInput(out FighterInput frameInput))
        {
            Combo.Input newInput = FighterInputToComboInput(frameInput);
            
            if (newInput == Combo.Input.None || lastInput == newInput)
            {
                lastInput = newInput;
                return;
            }
            lastInput = newInput;

            if (newInput == activeCombo.sequence[comboProgressIndex] && comboProgressIndex < activeCombo.sequence.Count)
            {
                OnComboHit(comboProgressIndex++);
                StartCoroutine(DelayFailable(0.2f));
            }
            else if (failable)
            {
                OnComboFail();
            }

            if (comboProgressIndex == activeCombo.sequence.Count)
            {
                OnComboComplete();

            }
        }        
    }

    private void OnComboHit(int index)
    {
        UpdateComboHitRpc(index);
        if (index + 1 < activeCombo.sequence.Count)
        {
            requiredInput = activeCombo.sequence[index + 1];
        }
        else
        {
            requiredInput = Combo.Input.None;
        }
    }
    public void OnComboComplete()
    { 
        comboIsActive = false;
        StopAllCoroutines();
        ApplyComboEffectRpc(activeCombo.comboEffect);
        LocalComboCompleteRpc();
    }


    [Rpc(target: RpcPeers.InputSource)]
    public void UpdateComboHitRpc (int index)
    {
        model.comboUIcontroller.OnComboHit(index);
    }
    [Rpc(target: RpcPeers.InputSource)]
    public void UpdateComboFailRpc()
    {
        model.comboUIcontroller.OnComboFail(comboProgressIndex);
    }

    void OnComboFail()
    {
        UpdateComboFailRpc();
        comboIsActive = false;
    }
    [Rpc(target: RpcPeers.InputSource)]
    public void LocalComboCompleteRpc()
    {
        model.comboUIcontroller.OnComboCompleted();
    }
    [Rpc(target: RpcPeers.Everyone, localInvoke: true)]
    public void ApplyComboEffectRpc(ComboEffect comboEffect)
    {
        thisPlayer.ApplyComboEffect(comboEffect);         
    }


    Combo.Input FighterInputToComboInput(FighterInput input)
    {
        NetworkedPlayerController.Direction dir = NetworkedPlayerController.UpdateDirection(input.movement, NetworkedPlayerController.Direction.None);
        if (input.lightAttack)
        {
            return Combo.Input.LightAttack;
        }
        else if (input.heavyAttack)
        {
            return Combo.Input.HeavyAttack;
        }
        else if (dir != NetworkedPlayerController.Direction.None)
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
