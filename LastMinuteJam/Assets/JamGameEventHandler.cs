using UnityEngine;
using Netick;
using Netick.Unity;
using System.Collections.Generic;
using Platformer.Mechanics;
using UnityEngine.InputSystem;

namespace Platformer
{

    public class JamGameEventHandler : NetworkBehaviour
    {
        public List<NetworkedPlayerController> Players = new(4);
        public List<NetworkedPlayerController> AlivePlayers = new(4);

        private GameObject _playerPrefab;
        private GameObject _colletableItem;

        private Vector3[] _spawnPositions = new Vector3[4] { new Vector3(11, 9, 0), new Vector3(11, 1, 0), new Vector3(1, 9, 0), new Vector3(1, 1, 0) };
        private Queue<Vector3> _freePositions = new(4);


        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction basicAttackAction;
        private InputAction heavyAttackAction;

        private Vector2 moveVector;
        private bool jumpPressed;
        private bool jumpReleased;
        private bool lightAttacked;
        private bool heavyAttacked;



        public override void NetworkStart()
        {

            basicAttackAction = InputSystem.actions["BasicAttack"];
            heavyAttackAction = InputSystem.actions["HeavyAttack"];
            jumpAction = InputSystem.actions["Jump"];
            moveAction = InputSystem.actions["Movement"];
            basicAttackAction.Enable();
            heavyAttackAction.Enable();
            jumpAction.Enable();
            moveAction.Enable();
            /*
            basicAttackAction.performed += OnLightAttack;
            heavyAttackAction.performed += OnHeavyAttack;
            jumpAction.performed += OnJump;
            moveAction.performed += OnMove;
            */

            _playerPrefab = Sandbox.GetPrefab("NetworkedPlayerPrefab");
            _colletableItem = Sandbox.GetPrefab("Square");
            Sandbox.Events.OnInputRead += OnInput;
            Sandbox.Events.OnConnectRequest += OnConnectRequest;
            Sandbox.Events.OnPlayerConnected += OnPlayerConnected;
            Sandbox.Events.OnPlayerDisconnected += OnPlayerDisconnected;

            // TODO: Make this for the powerups or something Sandbox.InitializePool(Sandbox.GetPrefab("Bomb"), 5);
            Sandbox.InitializePool(_playerPrefab, 4);
            Sandbox.InitializePool(_colletableItem, 1);

            for (int i = 0; i < 4; i++)
            {
                _freePositions.Enqueue(_spawnPositions[i]);
            }
            base.NetworkStart();

            if (IsServer)
                RestartGame();
        }

        public void OnDestroy()
        {
            basicAttackAction.performed -= OnLightAttack;
            heavyAttackAction.performed -= OnHeavyAttack;
            jumpAction.performed -= OnJump;
            moveAction.performed -= OnMove;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            moveVector = context.ReadValue<Vector2>();
        }
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                jumpPressed = true;
                jumpReleased = false;
            }
            else
            {
                jumpPressed = false;
                jumpReleased = true;
            }

        }
        public void OnLightAttack(InputAction.CallbackContext context)
        {
            Debug.Log("Called OnLightAttack");
            if (context.action.triggered)
            {
                lightAttacked = true;
            }
            else
            {
                lightAttacked = false;
            }
        }
        public void OnHeavyAttack(InputAction.CallbackContext context)
        {
            if (context.action.triggered)
            {
                heavyAttacked = true;
            }
            else
            {
                heavyAttacked = false;
            }
        }

        public void OnConnectRequest(NetworkSandbox sandbox, NetworkConnectionRequest request)
        {
            if (Sandbox.ConnectedPlayers.Count >= 4)
                request.Refuse();
        }
        // This is called on the server when a playerObj has connected.
        public void OnPlayerConnected(NetworkSandbox sandbox, NetworkPlayer player)
        {
            var playerObj = sandbox.NetworkInstantiate(_playerPrefab, _spawnPositions[Sandbox.ConnectedPlayers.Count], Quaternion.identity, player).GetComponent<NetworkedPlayerController>();
            //sandbox.NetworkInstantiate(_colletableItem, new Vector3(0, -0.5f, 0),
            //    Quaternion.identity, null);
            
            player.PlayerObject = playerObj.gameObject;
            AlivePlayers.Add(playerObj);
            Players.Add(playerObj);
        }

        // This is called on the server when a client has disconnected.
        public void OnPlayerDisconnected(NetworkSandbox sandbox, Netick.NetworkPlayer player, TransportDisconnectReason reason)
        {
            _freePositions.Enqueue(((GameObject)player.PlayerObject).GetComponent<NetworkedPlayerController>().spawnPosition);
            Players.Remove(((GameObject)player.PlayerObject).GetComponent<NetworkedPlayerController>());
        }

        // This is called to read inputs.
        public void OnInput(NetworkSandbox sandbox)
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




        public void RestartGame()
        {
            

            // reset players.
            foreach (var player in Players)
                player.Respawn();
        }

        public void KillPlayer(NetworkedPlayerController fighter)
        {
            AlivePlayers.Remove(fighter);

            if (AlivePlayers.Count == 1)
            {
                // TODO : Give player points/take away lives or whatever
                RestartGame();
            }

            else if (AlivePlayers.Count < 1)
                RestartGame();
        }
        public void RespawnPlayer(NetworkedPlayerController fighter)
        {
            if (!AlivePlayers.Contains(fighter))
                AlivePlayers.Add(fighter);
        }


        public override void NetworkFixedUpdate()
        {
            base.NetworkFixedUpdate();
            GameController.Instance.TickSimulation();
        }


    }
}
