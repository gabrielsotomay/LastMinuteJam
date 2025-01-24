using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class LobbyPlayerPanelController : MonoBehaviour
{

    public TMP_Text name;
    //public List<RectTransform> characterBorders;
    public int NumOfChararacters;
    public List<GameObject> characterAnims;
    public GameObject readyIndicator;
    public Sprite readyBarNormal;
    public Sprite readyBarClicked;
    public TMP_Text readyText;
    int characterActive = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    
    private void Start()
    {
        //characterAnims[0].gameObject.SetActive(false);
        //characterAnims[1].gameObject.SetActive(false);
    }
    public void UpdateCharacterChoice(int character)
    {

        for (int i = 0; i < NumOfChararacters; i++)
        {
            if (i == character)
            {
                Debug.Log("Changed to " + i);
                characterAnims[i].SetActive(true);
                characterAnims[(i+1)%2].SetActive(false);
                
            }
        }
    }
    public void Init(LobbyUIData info)
    {
        name.text = info.name;
        UpdateCharacterChoice(info.character);
    }
    
    // Update is called once per frame
    public void UpdateInfo(LobbyUIData info)
    {
        name.text = info.name;
        if (characterActive != info.character)
        {
            UpdateCharacterChoice(info.character);
            characterActive = info.character;
        }
        if (info.state == LobbyController.KEY_READY)
        {
            readyText.text = "Ready!";
        }
        else
        {
            readyText.text = "Not ready";
        }
    }



}
