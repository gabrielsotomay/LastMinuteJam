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

public class LobbyController : MonoBehaviour
{

    public Lobby hostLobby;
    public Lobby joinedLobby;
    public RelayController relayController;
    public static LobbyController Instance;

    public event EventHandler<EventArgs> OnGameStarted;
    public event EventHandler<EventArgs> OnLobbyUpdate;

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

    // Lobby return states
    public const string KEY_PLAY_AGAIN = "PlayAgain";
    public const string KEY_RUNNING = "Running";

    public const string KEY_PLAYER_STATE = "PlayerState";
    public const string KEY_PLAYER_CHARACTER = "Character";
    public const string KEY_READY = "Ready";
    public const string KEY_IDLE = "Idle";
    public const string KEY_ALIVE = "Alive";

    public const string KEY_JJ = "JJ";
    public const string KEY_ELVIRA = "Elvira";

    string oldMapSent = "";
    public bool gameRunning;

    public List<LobbyUIData> playerData = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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
                    await Lobbies.Instance.SendHeartbeatPingAsync(hostLobby.Id);
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
        SceneManager.LoadScene("MultiGame");
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

            joinedLobby = await Lobbies.Instance.GetLobbyAsync(joinedLobby.Id);
            foreach(Player player in joinedLobby.Players)
            {
                bool foundPlayer = false; ;
                foreach (LobbyUIData data in playerData)
                {
                    if (data.name.Equals(player.Data[KEY_PLAYER_NAME].Value))
                    {
                        SetPlayerUIData(player, data);
                    }
                }
                if (!foundPlayer)
                {                    
                    playerData.Add(GetPlayerUIData(player));
                }
            }
            OnLobbyUpdate?.Invoke(this, EventArgs.Empty);

            if (!gameRunning && !IsLobbyHost())
            {
                if (joinedLobby.Data[KEY_GAME_STATE].Value.Equals(KEY_GAME_START))
                {
                    StartClientGame();
                }
                else if (IsHostInLobby() && joinedLobby.Players[0].Data[KEY_PLAYER_STATE].Value.Equals(KEY_PLAY_AGAIN))
                {
                    //ExitToLobby();
                }
            }
            else if (gameRunning && joinedLobby.Data[KEY_GAME_STATE].Value.Equals(KEY_GAME_START))
            {
                if (IsLobbyHost() && (joinedLobby.Players.Count == 1 || joinedLobby.Players[1].Data[KEY_PLAYER_STATE].Value.Equals(KEY_ALIVE)))
                {
                    UpdateLobbyGameState(KEY_RUNNING);
                }
            }

        }
    }

    private static LobbyUIData GetPlayerUIData(Player player)
    {
        LobbyUIData newPlayer = new LobbyUIData();
        newPlayer.name = player.Data[KEY_PLAYER_NAME].Value;
        switch (player.Data[KEY_PLAYER_CHARACTER].Value)
        {
            case KEY_JJ:
                newPlayer.character = 0;
                break;
            case KEY_ELVIRA:
                newPlayer.character = 1;
                break;
            default:
                break;
        }
        newPlayer.state = player.Data[KEY_PLAYER_STATE].Value;
        return newPlayer;
    }

    private static void SetPlayerUIData(Player player, LobbyUIData data)
    {

        data.name = player.Data[KEY_PLAYER_NAME].Value;
        switch (player.Data[KEY_PLAYER_CHARACTER].Value)
        {
            case KEY_JJ:
                data.character = 0;
                break;
            case KEY_ELVIRA:
                data.character = 1;
                break;
            default:
                break;
        }
        data.state = player.Data[KEY_PLAYER_STATE].Value;
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



    public async Task<bool> CreateLobby(string playerName_)
    {
        try
        {
            playerName = playerName_;
            string lobbyName = playerName + "'s Lobby";
            int maxPlayers = 4;
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
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            joinedLobby = hostLobby;
            Debug.Log("Created Lobby! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            PrintPlayers(hostLobby);
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return false;
        }


    }
    public async Task<bool> JoinLobbyByCode(string playerName_, string lobbyCode)
    {
        try
        {
            playerName = playerName_;
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

    public async Task<bool> QuickJoinLobby(string playerName_)
    {
        try
        {
            playerName = playerName_;
            // Quick-join a random lobby with a maximum capacity of 10 or more players.
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync(options);
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
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                {KEY_MAP_NAME, new DataObject(DataObject.VisibilityOptions.Public,newMapName )}
            }
            });
            joinedLobby = hostLobby;

            oldMapSent = newMapName;


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
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
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
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
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
        if (joinedLobby.Players.Count < 1)
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

    public void ChangeCharacter(int characterId)
    {
        switch(characterId)
        {
            case 0:
                UpdatePlayerCharacter(KEY_JJ);
                break;
            case 1:
                UpdatePlayerCharacter(KEY_ELVIRA);
                break;
            default:
                break;
        }
    }
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                { KEY_PLAYER_STATE, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, KEY_IDLE)  },
                { KEY_PLAYER_CHARACTER,  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, KEY_JJ)  }
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



    public async void UpdatePlayerState(string state)
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {KEY_PLAYER_STATE, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, state) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    public async void UpdatePlayerCharacter(string state)
    {
        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, playerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {KEY_PLAYER_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, state) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }
}
