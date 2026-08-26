using UnityEngine;
using UnityEngine.SceneManagement;

public class Menuemanager : MonoBehaviour
{
    public void ChargerScene()
    {
        SceneManager.LoadScene("FarmScene");
    }
    public void QuitterJeu()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }

    public void Reseau()
    {
       
    }
    
    public void Devine()
    {
       
    }
}