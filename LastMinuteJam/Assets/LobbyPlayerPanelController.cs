using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class LobbyPlayerPanelController : MonoBehaviour
{

    public Button readyButton;
    public TMP_Text name;
    public List<RectTransform> characterBorders;
    public List<Button> characterButtons;

    LobbyController lobbyController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Init(LobbyController lobbyController_)
    {
        lobbyController = lobbyController_;
        characterButtons[0].onClick.AddListener(SetCharacterToJJ);
        characterButtons[1].onClick.AddListener(SetCharacterToElvira);
        readyButton.onClick.AddListener(SetReady);
    }
    public void UpdateCharacterChoice(int character)
    {
        for (int i = 0; i < characterBorders.Count; i++)
        {
            if (i == character)
            {
                characterBorders[i].gameObject.SetActive(true);
            }
            else
            {
                characterBorders[i].gameObject.SetActive(false);

            }
        }
    }

    public void SetCharacterToJJ()
    {
        lobbyController.ChangeCharacter(0);
    }

    public void SetCharacterToElvira()
    {
        lobbyController.ChangeCharacter(1);
    }
    // Update is called once per frame
    public void UpdateInfo(LobbyUIData info)
    {
        name.text = info.name;
        UpdateCharacterChoice(info.character);
        if (info.state == LobbyController.KEY_READY)
        {
            readyButton.GetComponent<Image>().color = Color.green;
        }
        else
        {
            readyButton.GetComponent<Image>().color = Color.white;

        }
    }

    public async void SetReady()
    {
        lobbyController.UpdatePlayerState(LobbyController.KEY_READY);
    }



}
