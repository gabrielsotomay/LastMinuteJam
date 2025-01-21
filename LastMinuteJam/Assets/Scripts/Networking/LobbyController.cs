using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LobbyController
{

    public Lobby hostLobby;
    public Lobby joinedLobby;
    RelayController relayController;


    private float hearbeatTimer;
    private float lobbyUpdateTimer;
    private const float lobbyUpdateTimerMax = 1.1f;
    private string playerName;


    public const string KEY_GAME_START = "GameStart";
    public const string KEY_GAME_STATE = "GameState";
    public const string KEY_RELAY_CODE = "RelayCode";
    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_MAP_NAME = "MapName";
    public const string KEY_LOBBY = "Lobby";
    public const string KEY_NULL = "Null";
    private string playerId; // TODO: CHANGE ALL THESE TO AuthenticationService.Instance.PlayerId

    public const string KEY_PLAYER_STATE = "PlayerState";
    public const string KEY_READY = "Ready";
    public const string KEY_IDLE = "Idle";
    string oldMapSent = "";
    public bool gameRunning;
    public async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            PlayerSetupAsync();
            gameRunning = false;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

    }


    private async void PlayerSetupAsync()
    {
        playerName = "Goose " + UnityEngine.Random.Range(10, 9999);
        //await AuthenticationService.Instance.SignInAnonymouslyAsync();
        if (true)
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
                playerId = AuthenticationService.Instance.PlayerId;
            };
            AuthenticationService.Instance.ClearSessionToken(); // TODO: REMOVE THIS SO PLAYER DATA IS NOT LOST

        }

        await AuthenticationService.Instance.SignInAnonymouslyAsync();


        Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        playerId = AuthenticationService.Instance.PlayerId;

        AuthenticationService.Instance.SignInFailed += (err) => {
            Debug.LogError(err);
        };
        Debug.Log(playerName);
    }


    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            hearbeatTimer -= Time.deltaTime;
            if (hearbeatTimer < 0f)
            {
                float heartBeatTimerMax = 15;
                hearbeatTimer = heartBeatTimerMax;
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log(e);
                }
            }
        }
    }

    private void StartClientGame()
    {

        relayController.JoinRelay(joinedLobby.Data[KEY_RELAY_CODE].Value);
        gameRunning = true;
        UpdatePlayerState(KEY_ALIVE);
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }
    private async void HandleLobbyPollForUpdates()
    {
        if (IsLobbyPollTime())
        {
            lobbyUpdateTimer = lobbyUpdateTimerMax;

            joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);


            if (!gameRunning && !IsLobbyHost())
            {
                if (joinedLobby.Data[KEY_GAME_STATE].Value.Equals(KEY_GAME_START))
                {
                    StartClientGame();
                }
                else if (IsHostInLobby() && joinedLobby.Players[playerIndex].Data[KEY_PLAYER_STATE].Value.Equals(KEY_PLAY_AGAIN))
                {
                    ExitToLobby();
                }
            }
            else if (gameRunning && joinedLobby.Data[KEY_GAME_STATE].Value.Equals(KEY_GAME_START))
            {
                if (IsLobbyHost() && (joinedLobby.Players.Count == 1 || joinedLobby.Players[1].Data[KEY_PLAYER_STATE].Value.Equals(KEY_ALIVE)))
                {
                    UpdateLobbyGameState(KEY_RUNNING);
                }
            }
            else if (gameRunning && joinedLobby.Players.Count > 1)
            {
                string otherPlayerKey = joinedLobby.Players[(playerIndex + 1) % 2].Data[KEY_PLAYER_STATE].Value;
                string myKey = joinedLobby.Players[playerIndex].Data[KEY_PLAYER_STATE].Value;
                GameEnd endData = GameEnd.CheckEndConditions(m_GameManager.Mode, otherPlayerKey, myKey);
                if (endData != null)
                {
                    Debug.Log(endData);
                    EndGame(endData);
                }
            }

        }
    }
    public void ResetLobby()
    {
        UpdateLobbyGameState(KEY_LOBBY);
    }
    public bool IsHostInLobby()
    {
        if (joinedLobby.Data[KEY_GAME_STATE].Value.Equals(KEY_LOBBY))
        {
            return true;
        }
        return false;
    }

    private bool IsLobbyPollTime()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                return true;
            }
        }
        return false;
    }



    private void EndGame()
    {
        
        gameRunning = false;
        if (IsLobbyHost())
        {
            UpdateLobbyGameState(KEY_ENDED);
        }
    }
    public async Task<bool> CreateLobby()
    {
        try
        {
            string lobbyName = playerName + "'s Lobby";
            int maxPlayers = 2;
            oldMapSent = "map0";
            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_GAME_STATE, new DataObject(DataObject.VisibilityOptions.Public, KEY_LOBBY) },
                    {KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, KEY_NULL) },
                    {KEY_MAP_NAME, new DataObject(DataObject.VisibilityOptions.Public, oldMapSent) },
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            UpdateMap(oldMapSent);
            hostLobby = lobby;
            joinedLobby = hostLobby;
            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            PrintPlayers(hostLobby);
            lobbyData = new(playerName, "", GameManager.Difficulty.Medium, KEY_IDLE, KEY_NULL);
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }


    }
    public async Task<bool> JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            JoinLobby(lobby);

            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }

    public async Task<bool> QuickJoinLobby()
    {
        try
        {
            // Quick-join a random lobby with a maximum capacity of 10 or more players.
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            JoinLobby(lobby);
            return true;
            // ...
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }
    }

    private void JoinLobby(Lobby lobby)
    {
        joinedLobby = lobby;
        SetModeOnJoining(lobby);
        lobbyData = InitLobbyData(joinedLobby);
        SetTaskInfo(lobby.Data[KEY_TASK_INFO].Value);
        UpdateMap(lobby.Data[KEY_MAP_NAME].Value);
        Debug.Log("Joined Lobby with code " + lobby.Id);
        PrintPlayers(joinedLobby);
    }

    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name + " " + lobby.Data[KEY_MAP_NAME].Value);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    public async void UpdateLobbyMapName(string mapName, int taskIndex, int mapIndex)
    {
        try
        {
            string newMapName = "map0";
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                {KEY_MAP_NAME, new DataObject(DataObject.VisibilityOptions.Public,newMapName )}
            }
            });
            joinedLobby = hostLobby;
            UpdateMap(newMapName);

            oldMapSent = mapCode;


            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdateLobbyGameState(string gameState)
    {
        try
        {
            hostLobby = await Lobb.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { KEY_GAME_STATE, new DataObject(DataObject.VisibilityOptions.Public, gameState)
                }
            }
            });
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    public async void LeaveLobby()
    {
        SceneManager.LoadScene("MainMenu");
        if (joinedLobby == null)
        {
            return;
        }
        try
        {

            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            joinedLobby = null;
            hostLobby = null;
            return;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }
    }

    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void StartGame()
    {

        if (!CheckLobbyReady())
        {
            return;
        }

        try
        {
            SceneManager.LoadScene("MultiGame");
            
            string relayCode = await relayController.CreateRelay();

            Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_MAP_NAME, new DataObject(DataObject.VisibilityOptions.Public, oldMapSent) },
                    {KEY_RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                    {KEY_GAME_STATE, new DataObject(DataObject.VisibilityOptions.Public, KEY_GAME_START) },
                }
            });
            joinedLobby = lobby;
            gameRunning = true;
            UpdatePlayerState(KEY_ALIVE);

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public bool CheckLobbyReady()
    {
        if (joinedLobby.Players.Count < 2)
        {
            return false;
        }
        foreach (Player player in joinedLobby.Players)
        {
            if (!player.Data[KEY_PLAYER_STATE].Value.Equals(KEY_READY))
            {
                return false;
            }
        }
        return true;
    }


    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                { KEY_PLAYER_STATE, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, KEY_IDLE)  }
            }
        };
    }
    public bool IsLobbyHost()
    {
        return joinedLobby?.Players[0].Id.Equals(playerId) ?? false;
    }

    public string GetLobbyCode()
    {
        return joinedLobby?.LobbyCode ?? "";
    }

}
