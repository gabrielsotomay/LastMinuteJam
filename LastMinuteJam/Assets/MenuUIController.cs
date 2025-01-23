using Netick.Unity;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEditor.U2D.Animation;

public class MenuUIController : MonoBehaviour
{
    public Button createGameButton;
    public Button joinGameButton;
    public Button quickJoinButton;
    public Button startGameButton;
    public TMP_InputField lobbyName;
    public TMP_InputField playerName;
    public LobbyController lobbyController;
    public TMP_Text lobbyCode;

    public GameObject lobbyPanel;
    List<LobbyPlayerPanelController> lobbyPlayers = new();
    public GameObject playerPanelPrefab;

    public GameObject playerContainer;
    public GameObject startContainer;

    public GameObject charactersContainer;
    public GameObject playerOneElvira;
    public GameObject playerOneJj;
    public GameObject playerTwoElvira;
    public GameObject playerTwoJj;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createGameButton.onClick.AddListener(() => CreateGame());
        joinGameButton.onClick.AddListener(() => JoinGame());
        startGameButton.onClick.AddListener(() => StartGame());
        quickJoinButton.onClick.AddListener(() => QuickJoin());
        lobbyPanel.SetActive(false);
        charactersContainer.SetActive(false);
        lobbyController.OnLobbyUpdate += QueryLobby;
    }
    private void OnDestroy()
    {

        lobbyController.OnLobbyUpdate -= QueryLobby;
    }
    public async void CreateGame()
    {
        if (await lobbyController.CreateLobby(playerName.text))
        {
            lobbyPanel.SetActive(true);
            createGameButton.gameObject.SetActive(false);
            joinGameButton.gameObject.SetActive(false);
            lobbyName.gameObject.SetActive(false);
            startGameButton.gameObject.SetActive(true);
            startContainer.SetActive(false);
            charactersContainer.SetActive(true);
            playerOneJj.SetActive(true);
            playerTwoJj.SetActive(true);
            playerTwoElvira.SetActive(false);
            playerOneElvira.SetActive(false);
            
            
        }

    }
    public async void JoinGame()
    {
        if (await lobbyController.JoinLobbyByCode(playerName.text, lobbyName.text))
        {
            lobbyPanel.SetActive(true);
            createGameButton.gameObject.SetActive(false);
            joinGameButton.gameObject.SetActive(false);
            lobbyName.gameObject.SetActive(false);
            startGameButton.gameObject.SetActive(false);
            startContainer.SetActive(false);
        }
    }
    public async void StartGame()
    {
        lobbyController.StartGame();
    }

    public async void QuickJoin()
    {
        if (await lobbyController.QuickJoinLobby(playerName.text))
        {
            lobbyPanel.SetActive(true);
            createGameButton.gameObject.SetActive(false);
            joinGameButton.gameObject.SetActive(false);
            lobbyName.gameObject.SetActive(false);
            startGameButton.gameObject.SetActive(false);
            startContainer.SetActive(false);
        }
    }


    public void UpdateUI(List<LobbyUIData> lobbyUIData)
    {
        lobbyCode.text = lobbyController.GetLobbyCode();
        foreach (LobbyUIData player in lobbyUIData)
        {
            bool foundPanel = false;
            foreach (LobbyPlayerPanelController panel in lobbyPlayers)
            {
                if (panel.name.text == player.name)
                {
                    foundPanel = true;
                    panel.UpdateInfo(player);
                }
            }
            if (!foundPanel)
            {
                lobbyPlayers.Add(Instantiate(playerPanelPrefab, playerContainer.transform).GetComponent<LobbyPlayerPanelController>());
                lobbyPlayers[^1].Init(lobbyController);
                lobbyPlayers[^1].UpdateInfo(player);
            }
        }




    }


    public void QueryLobby(object o, EventArgs args)
    {
        UpdateUI(lobbyController.playerData);
    }
    // Update is called once per frame
    void Update()
    {
        if (lobbyPanel.activeInHierarchy)
        {
            lobbyCode.text = lobbyController.GetLobbyCode();

        }
    }
}
