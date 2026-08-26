using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class Teleporteur : NetworkBehaviour
{
    [Header("Où aller ?")]
    public string sceneDestination; 
    public string nomDuSpawnArrivee; 
    public static string prochainSpawnID; 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            NetworkObject netObj = collision.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsLocalPlayer)
            {
                GameData.prochainSpawnID = nomDuSpawnArrivee; 
                Debug.Log("<color=magenta><b>[SAVE]</b></color> ID enregistré dans GameData : " + GameData.prochainSpawnID);

                if (NetworkManager.Singleton.IsServer)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene(sceneDestination, LoadSceneMode.Single);
                }
            }
        }
    }
    
}