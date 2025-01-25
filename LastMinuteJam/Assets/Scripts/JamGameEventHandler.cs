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

        private GameObject JJPrefab;
        private GameObject ElviraPrefab;
        private GameObject _collectableItem;
        //private GameObject _healthBarPrefab;

        private List<Transform> _spawnPositions = new();
        private Queue<Vector3> _freePositions = new(4);


        readonly PlatformerModel model = GetModel<PlatformerModel>();

        public ComboController comboController;

        int initIndex = 0;


        public override void NetworkStart()
        {
            /*
            basicAttackAction.performed += OnLightAttack;
            heavyAttackAction.performed += OnHeavyAttack;
            jumpAction.performed += OnJump;
            moveAction.performed += OnMove;
            */
            SetMap(SelectMap(NetworkingController.Instance.mapName));

            JJPrefab = Sandbox.GetPrefab("JJPrefab");
            ElviraPrefab = Sandbox.GetPrefab("ElviraPrefab");
            Sandbox.Events.OnConnectRequest += OnConnectRequest;
            Sandbox.Events.OnPlayerConnected += OnPlayerConnected;
            Sandbox.Events.OnPlayerDisconnected += OnPlayerDisconnected;

            // TODO: Make this for the powerups or something Sandbox.InitializePool(Sandbox.GetPrefab("Bomb"), 5);
            Sandbox.InitializePool(JJPrefab, 2);
            Sandbox.InitializePool(ElviraPrefab, 2);
            //Sandbox.InitializePool(_collectableItem, 1);
            comboController.Init(model);
            /*
            for (int i = 0; i < 4; i++)
            {
                _freePositions.Enqueue(_spawnPositions[i].position);
            }
            */
            base.NetworkStart();
            if (IsServer)
                RestartGame();
        }
        static int SelectMap(string mapName)
        {
            Debug.Log("Selected " + mapName);
            switch (mapName)
            {
                case LobbyController.KEY_MAP_CLASSIC:
                    return 0;
                case LobbyController.KEY_MAP_OLDMAP:
                    return 1;
            }
            return -1;
        }

        public void SetMap(int mapSelected)
        {
            Transform spawnPointContainer = model.spawnPointsContainers[0];
            _spawnPositions = new();
            for (int i = 0; i < model.maps.Count; i++)
            {
                if (mapSelected == i)
                {
                    model.maps[mapSelected].SetActive(true);
                    spawnPointContainer = model.spawnPointsContainers[mapSelected];
                    model.topLeft = model.mapMarkers[mapSelected].GetChild(0);
                    model.topRight = model.mapMarkers[mapSelected].GetChild(1);
                }
                else
                {
                    model.maps[i].SetActive(false);
                }
            }
            for (int i = 0; i < spawnPointContainer.childCount; i++)
            {
                _spawnPositions.Add(spawnPointContainer.GetChild(i));
                Debug.Log("Added spawn position" + i);
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
            NetworkedPlayerController playerObj = new NetworkedPlayerController( );
            
            if (player.PlayerId == Sandbox.LocalPlayer.PlayerId)
            {
                foreach (PlayerData data in NetworkingController.Instance.playerData)
                {
                    if (data.name.Equals(NetworkingController.Instance.myName))
                    {
                        if (data.character == PlayerData.Character.Elvira)
                        {
                            playerObj = sandbox.NetworkInstantiate(ElviraPrefab, _spawnPositions[Sandbox.ConnectedPlayers.Count-1].position, Quaternion.identity, player).GetComponent<NetworkedPlayerController>();
                        }
                        else
                        {
                            playerObj = sandbox.NetworkInstantiate(JJPrefab, _spawnPositions[Sandbox.ConnectedPlayers.Count - 1].position, Quaternion.identity, player).GetComponent<NetworkedPlayerController>();
                        }
                    }
                }
            }            
            else
            {
                foreach (PlayerData data in NetworkingController.Instance.playerData)
                {
                    if (!data.name.Equals(NetworkingController.Instance.myName))
                    {
                        if (data.character == PlayerData.Character.Elvira)
                        {
                            playerObj = sandbox.NetworkInstantiate(ElviraPrefab, _spawnPositions[Sandbox.ConnectedPlayers.Count - 1].position, Quaternion.identity, player).GetComponent<NetworkedPlayerController>();
                        }
                        else
                        {
                            playerObj = sandbox.NetworkInstantiate(JJPrefab, _spawnPositions[Sandbox.ConnectedPlayers.Count - 1].position, Quaternion.identity, player).GetComponent<NetworkedPlayerController>();
                        }
                    }
                }
            }



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
            /*
            List<NetworkedPlayerController> foundPlayers = new();
            foreach (NetworkedPlayerController player in allFoundPlayers)
            {
                if (player.gameObject.active)
                {
                    foundPlayers.Add(player);
                }
            }
            */
            if (targetGroup.IsEmpty) 
            {
                int i = 0;
                // received by new player, add self, then other playerall players to the camera
                foreach (NetworkedPlayerController player in foundPlayers)
                {
                    if (player.GetComponent<NetworkObject>().Id == newPlayerId)
                    {
                        foreach (PlayerData data in NetworkingController.Instance.playerData)
                        {
                            if (data.name.Equals(NetworkingController.Instance.myName))
                            {
                                player.Init(data);
                                model.healthBarController.AddNew(player);
                                comboController.myPlayer = player;
                            }
                        }
                    }                    
                }
                foreach (NetworkedPlayerController player in foundPlayers)
                {
                    // If own player
                    if (player.GetComponent<NetworkObject>().Id != newPlayerId)
                    {
                        foreach (PlayerData data in NetworkingController.Instance.playerData)
                        {
                            if (!data.name.Equals(NetworkingController.Instance.myName))
                            {
                                player.Init(data);
                                model.healthBarController.AddNew(player);
                            }
                        }
                    }
                    FindFirstObjectByType<CinemachineTargetGroup>().AddMember(player.transform, 1, 4);
                    comboController.allPlayers.Add(player);
                }
            }
            else
            {
                // received by existing player, add new player to the camera
                foreach (NetworkedPlayerController player in foundPlayers)
                {
                    if (player.GetComponent<NetworkObject>().Id == newPlayerId)
                    {
                        foreach (PlayerData data in NetworkingController.Instance.playerData)
                        {
                            if (!data.name.Equals(NetworkingController.Instance.myName))
                            {
                                player.Init(data);
                                model.healthBarController.AddNew(player); 
                                comboController.allPlayers.Add(player);
                                FindFirstObjectByType<CinemachineTargetGroup>().AddMember(player.transform, 1, 4);
                            }
                        }
                        
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
            //_freePositions.Enqueue(((GameObject)player.PlayerObject).GetComponent<NetworkedPlayerController>().spawnPosition);
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
