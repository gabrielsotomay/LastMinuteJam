using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class LobbyPlayerPanelController : MonoBehaviour
{

    public Button readyButton;
    public TMP_Text name;
    //public List<RectTransform> characterBorders;
    public List<Button> characterButtons;
    public int NumOfChararacters;
    public List<GameObject> characterAnims;
    private int characterActive = 0;
    LobbyController lobbyController;

    public Sprite readyBarNormal;
    public Sprite readyBarClicked;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Init(LobbyController lobbyController_)
    {
        lobbyController = lobbyController_;
          characterButtons[0].onClick.AddListener(SwapCharacter);
        characterButtons[1].onClick.AddListener(SwapCharacter);
        readyButton.onClick.AddListener(SetReady);
    }
    public void UpdateCharacterChoice(int character)
    {
        for (int i = 0; i < NumOfChararacters; i++)
        {
            if (i == character)
            {
                characterAnims[i].gameObject.SetActive(true);
            }
            else
            {
                characterAnims[i].gameObject.SetActive(false);

            }
        }
    }

    public void SwapCharacter()
    {
        lobbyController.ChangeCharacter((characterActive + 1) % 2);
    }
    
    // Update is called once per frame
    public void UpdateInfo(LobbyUIData info)
    {
        name.text = info.name;
        UpdateCharacterChoice(info.character);
        if (info.state == LobbyController.KEY_READY)
        {   
            readyButton.GetComponent<Image>().sprite = readyBarClicked;
        }
        else
        {
            readyButton.GetComponent<Image>().sprite = readyBarNormal;
        }
    }

    public async void SetReady()
    {
        lobbyController.UpdatePlayerState(LobbyController.KEY_READY);
    }



}
