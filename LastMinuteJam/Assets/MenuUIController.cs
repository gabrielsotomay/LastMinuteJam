using Netick.Unity;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEditor.U2D.Animation;
using System.Linq;

public class MenuUIController : MonoBehaviour
{
    public Button createGameButton;
    public Button joinGameButton;
    public Button quickJoinButton;
    public Button startGameButton;
    public Button leaveLobbyButton;
    public TMP_InputField lobbyName;
    public TMP_InputField playerName;
    public LobbyController lobbyController;
    public TMP_Text lobbyCode;

    public GameObject lobbyPanel;
    public List<LobbyPlayerPanelController> playerPanels = new();
    public GameObject playerPanelPrefab;

    public GameObject startContainer;
    
    public List<GameObject> characterContainers;

    private int characterActive = 0;
    private int playersActive = 0;

    public Sprite LandingPageSprite;
    public Sprite LandingPageNoTitleSprite;
    public Image landingPage;

    public List<Button> characterButtons;
    public Button readyButton;

    public AudioSource startGameAudio;
    /*public GameObject playerOneElvira;
    public GameObject playerOneJj;
    public GameObject playerTwoElvira;
    public GameObject playerTwoJj;*/


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        createGameButton.onClick.AddListener(() => CreateGame());
        joinGameButton.onClick.AddListener(() => JoinGame());
        startGameButton.onClick.AddListener(() => StartGame());
        quickJoinButton.onClick.AddListener(() => QuickJoin());
        leaveLobbyButton.onClick.AddListener(() => LeaveLobby());
        landingPage.sprite = LandingPageSprite;
        lobbyPanel.SetActive(false);
        foreach (GameObject character in characterContainers)
        {
            character.SetActive(false);
        }
        foreach (Button button in characterButtons)
        {
            button.onClick.AddListener(() => SwapCharacter());
        }
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
            OnLobbyEnter();
            startGameAudio.Play();
            startGameButton.gameObject.SetActive(true);

            /*playerOneJj.SetActive(true);
            playerTwoJj.SetActive(true);
            playerTwoElvira.SetActive(false);
            playerOneElvira.SetActive(false);*/


        }
    }
    public async void SetReady()
    {
         lobbyController.UpdatePlayerState(LobbyController.KEY_READY);
         startGameAudio.Play();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public async void SwapCharacter()
    {
        characterActive = (characterActive + 1) % 2;
        lobbyController.ChangeCharacter(characterActive);
        ToggleCharButtonsEnable(false);
        startGameAudio.Play();
    }

    public void ToggleCharButtonsEnable(bool enable)
    {
            characterButtons[0].enabled = enable;
            characterButtons[1].enabled = enable;        
    }
    public async void JoinGame()
    {
        if (lobbyName.text == "" || lobbyName.text == null)
        {
            return;
        }
        if (await lobbyController.JoinLobbyByCode(playerName.text, lobbyName.text))
        {
            OnLobbyEnter();
            startGameButton.gameObject.SetActive(false);
        }
    }
    public async void StartGame()
    {
        lobbyController.StartGame();
        startGameAudio.Play();
    }

    public async void QuickJoin()
    {
        if (await lobbyController.QuickJoinLobby(playerName.text))
        {
            OnLobbyEnter();
            startGameButton.gameObject.SetActive(false);
            startGameAudio.Play();
        }
    }


    public async void LeaveLobby()
    {
        lobbyController.LeaveLobby();
        landingPage.sprite = LandingPageSprite;
        lobbyPanel.SetActive(false);
        createGameButton.gameObject.SetActive(true);
        joinGameButton.gameObject.SetActive(true);
        lobbyName.gameObject.SetActive(true);
        startContainer.SetActive(true);
        characterContainers[0].SetActive(false);
        characterButtons[0].onClick.RemoveListener(SwapCharacter);
        characterButtons[1].onClick.RemoveListener(SwapCharacter);
        readyButton.onClick.RemoveListener(SetReady);
        playersActive = 0;
    }
    private void OnLobbyEnter()
    {
        NetworkingController.Instance.myName = playerName.text;
        landingPage.sprite = LandingPageNoTitleSprite;
        lobbyPanel.SetActive(true);
        createGameButton.gameObject.SetActive(false);
        joinGameButton.gameObject.SetActive(false);
        lobbyName.gameObject.SetActive(false);
        startContainer.SetActive(false);
        characterContainers[0].SetActive(true);
        playerPanels[0].Init(new LobbyUIData { name = playerName.text, state = "not ready", character = 0 });
        characterButtons[0].onClick.AddListener(SwapCharacter);
        characterButtons[1].onClick.AddListener(SwapCharacter);
        readyButton.onClick.AddListener(SetReady);
        playersActive = 1;
    }


    public void UpdateUI(List<LobbyUIData> lobbyUIData)
    {
        lobbyCode.text = lobbyController.GetLobbyCode();
        if (lobbyUIData.Count == 2 && playersActive == 1)
        {
            characterContainers[1].SetActive(true);
            if (playerPanels[0].name.text.Equals(lobbyUIData[0].name))
            {
                playerPanels[1].Init(lobbyUIData[1]);
                playersActive++;
                Debug.Log("Added player " + lobbyUIData[1].name);
            }
            else
            {
                playerPanels[1].Init(lobbyUIData[0]);
                playersActive++;
                Debug.Log("Added player " + lobbyUIData[0].name);
            }
            
        }
        foreach (LobbyUIData player in lobbyUIData)
        {
            if (player.name.Equals(playerPanels[0].name.text))
            {
                characterActive = player.character;
                ToggleCharButtonsEnable(true);
            }
            bool foundPanel = false;
            foreach (LobbyPlayerPanelController panel in playerPanels)
            {
                if (panel.name.text.Equals(player.name))
                {
                    foundPanel = true;
                    panel.UpdateInfo(player);
                }
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
