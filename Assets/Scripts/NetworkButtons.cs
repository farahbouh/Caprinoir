using Unity.Netcode;
using UnityEngine;

public class NetworkButtons : MonoBehaviour
{
    private void OnGUI()
    {
        if (NetworkManager.Singleton == null) return;

        float width = 300;
        float height = 150;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;

        GUILayout.BeginArea(new Rect(x, y, width, height));
        
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            GUI.Box(new Rect(0, 0, width, height), "Menu Multijoueur");
            GUILayout.BeginVertical();
            GUILayout.Space(40);

            if (GUILayout.Button("HÉBERGER (Host)", GUILayout.Height(40))) 
            {
                if (NetworkManager.Singleton.StartHost()) 
                {
                    if (NetworkManager.Singleton.SceneManager != null) 
                    {
                        NetworkManager.Singleton.SceneManager.LoadScene(
                            "Multiplayer_Test", 
                            UnityEngine.SceneManagement.LoadSceneMode.Single
                        );
                    }
                    else 
                    {
                        Debug.LogWarning("Le SceneManager n'est pas encore prêt");
                    }
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Rejoindre (Client)", GUILayout.Height(40))) 
            {
                NetworkManager.Singleton.StartClient();
            }
            GUILayout.EndVertical();
        }
        
        GUILayout.EndArea();
    }
}