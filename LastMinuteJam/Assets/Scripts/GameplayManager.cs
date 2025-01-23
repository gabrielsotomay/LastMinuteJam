using Netick;
using Netick.Unity;
using Platformer.Model;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayManager : NetworkEventsListener
{

    // Movement/attaacks
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction basicAttackAction;
    private InputAction heavyAttackAction;

    private Vector2 moveVector;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool lightAttacked;
    private bool heavyAttacked;


    // Combo related



    private void Start()
    {
        basicAttackAction = InputSystem.actions["BasicAttack"];
        heavyAttackAction = InputSystem.actions["HeavyAttack"];
        jumpAction = InputSystem.actions["Jump"];
        moveAction = InputSystem.actions["Movement"];
        basicAttackAction.Enable();
        heavyAttackAction.Enable();
        jumpAction.Enable();
        moveAction.Enable();
    }

    // This is called to read inputs.
    public override void OnInput(NetworkSandbox sandbox)
    {
        FighterInput input = sandbox.GetInput<FighterInput>();
        moveVector = moveAction.ReadValue<Vector2>();
        input.movement = moveVector;
        /*
        input.lightAttack = lightAttacked;
        input.heavyAttack = heavyAttacked;
        input.jumpPress = jumpPressed;
        input.jumpRelease = jumpReleased;
        */
        input.lightAttack = basicAttackAction.ReadValue<float>() > 0f;
        input.heavyAttack = heavyAttackAction.ReadValue<float>() > 0f;
        input.jumpPress = jumpAction.ReadValue<float>() > 0f;
        input.jumpRelease = jumpAction.ReadValue<float>() <= 0f;

        sandbox.SetInput(input);
    }

}
