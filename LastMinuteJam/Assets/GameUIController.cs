
using UnityEngine;
using UnityEngine.UI;


public class GameUIController : MonoBehaviour
{
    public GameObject loadingPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is createdw
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PrepareGame()
    {
        loadingPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }


}
