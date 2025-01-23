using UnityEngine;
using Netick;
using Netick.Unity;
using System.Collections.Generic;
using Platformer.Mechanics;
using UnityEngine.InputSystem;
using LastMinuteJam;
using Unity.Cinemachine;
using Platformer.Model;
using static Platformer.Core.SimulationNetick;
using Unity.VisualScripting;
namespace Platformer
{

    public class JamGameEventHandler : NetworkBehaviour
    {
        public List<NetworkedPlayerController> Players = new(4);
        public List<NetworkedPlayerController> AlivePlayers = new(4);

        private GameObject _playerPrefab;
        private GameObject _collectableItem;

        private Vector3[] _spawnPositions = new Vector3[4] { new Vector3(11, 9, 0), new Vector3(11, 1, 0), new Vector3(1, 9, 0), new Vector3(1, 1, 0) };
        private Queue<Vector3> _freePositions = new(4);


        readonly PlatformerModel model = GetModel<PlatformerModel>();

        public ComboController comboController;


        public override void NetworkStart()
        {
            /*
            basicAttackAction.performed += OnLightAttack;
            heavyAttackAction.performed += OnHeavyAttack;
            jumpAction.performed += OnJump;
            moveAction.performed += OnMove;
            */

            _playerPrefab = Sandbox.GetPrefab("NetworkedPlayerPrefab");
            Sandbox.Events.OnConnectRequest += OnConnectRequest;
            Sandbox.Events.OnPlayerConnected += OnPlayerConnected;
            Sandbox.Events.OnPlayerDisconnected += OnPlayerDisconnected;

            // TODO: Make this for the powerups or something Sandbox.InitializePool(Sandbox.GetPrefab("Bomb"), 5);
            Sandbox.InitializePool(_playerPrefab, 4);
            //Sandbox.InitializePool(_collectableItem, 1);
            comboController.Init(model);
            for (int i = 0; i < 4; i++)
            {
                _freePositions.Enqueue(_spawnPositions[i]);
            }
            base.NetworkStart();
            if (IsServer)
                RestartGame();
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
            /*sandbox.NetworkInstantiate(_collectableItem, new Vector3(0, -0.5f, 0),
                Quaternion.identity, null);
            */
            player.PlayerObject = playerObj.gameObject;
            AlivePlayers.Add(playerObj);
            Players.Add(playerObj);
            if (Players.Count == 1)
            {
                playerObj.isPlayer1 = true;
            }
            //SetPlayerInputsRpc();
            Debug.Log("Ran a thing and count is " + Sandbox.ConnectedPlayers.Count);
            foreach (NetworkPlayer networkPlayer in Sandbox.ConnectedPlayers)
            {
                ((GameObject)networkPlayer.PlayerObject).GetComponent<NetworkedPlayerController>().InputSource = networkPlayer;                
            }

            

            AddPlayerToCameraRpc(playerObj.GetComponent<NetworkObject>().Id);


            /*
            foreach (NetworkedPlayerController networkPlayer in Players)
            {
                networkPlayer.InitAttacksRpc(new PlayerAttack.AttackIdsSent(playerObj.GetAttackNetworkIds().ToArray()));
            }
            */
        }

        [Rpc(target: RpcPeers.Everyone, localInvoke: true)]
        public void AddPlayerToCameraRpc(int newPlayerId)
        {
            CinemachineTargetGroup targetGroup = FindFirstObjectByType<CinemachineTargetGroup>();
            NetworkedPlayerController[] foundPlayers = FindObjectsByType<NetworkedPlayerController>(FindObjectsSortMode.InstanceID);
            
            if (targetGroup.IsEmpty) 
            {
                // received by new player, add all players to the camera
                foreach (NetworkedPlayerController player in foundPlayers)
                {
                    FindFirstObjectByType<CinemachineTargetGroup>().AddMember(player.transform, 1, 4);
                    comboController.allPlayers.Add(player);
                    if (player.GetComponent<NetworkObject>().Id == newPlayerId)
                    {
                        comboController.myPlayer = player;
                    }
                }
            }
            else
            {
                // received by existing player, add new player to the camera
                foreach (NetworkedPlayerController player in foundPlayers)
                {
                    if (player.GetComponent<NetworkObject>().Id == newPlayerId)
                    {
                        comboController.allPlayers.Add(player);
                        FindFirstObjectByType<CinemachineTargetGroup>().AddMember(player.transform, 1, 4);
                    }
                }
            }

        }
        /*
        [Rpc(target:RpcPeers.Everyone)]
        private void SetPlayerInputsRpc()
        {
            Find
            Sandbox.FindObjectOfType<NetworkedPlayerController>()
            Debug.Log("Ran a thing and count is " + Sandbox.ConnectedPlayers.Count);
            foreach (NetworkPlayer networkPlayer in Sandbox.ConnectedPlayers)
            {
                ((GameObject)networkPlayer.PlayerObject).GetComponent<NetworkedPlayerController>().InputSource = networkPlayer;
            }
        }
        
        */
        // This is called on the server when a client has disconnected.
        public void OnPlayerDisconnected(NetworkSandbox sandbox, Netick.NetworkPlayer player, TransportDisconnectReason reason)
        {
            _freePositions.Enqueue(((GameObject)player.PlayerObject).GetComponent<NetworkedPlayerController>().spawnPosition);
            Players.Remove(((GameObject)player.PlayerObject).GetComponent<NetworkedPlayerController>());
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
            NetworkedGameController.Instance.TickSimulation();
        }


    }
}
