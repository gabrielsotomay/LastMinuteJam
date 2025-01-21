using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickFunctions : MonoBehaviour
{
    public void OnStartClick()
    {
        SceneManager.LoadScene(1);
    }

    public void OnOptionsClick()
    {
        
    }
    public void OnCustomClick() {
        SceneManager.LoadScene(2);
    }
    
    public void OnBackPressed() {
        
    }
}
