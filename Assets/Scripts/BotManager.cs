using UnityEngine;
using Unity.Netcode;

public class BotManager : NetworkBehaviour
{
    [Header("Configuration")]
    public GameObject botPrefab;
    private GameObject botInstance;
    private float timerVerification = 0f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) 
        {
            this.enabled = false;
            return;
        }
        
        NettoyerVieuxBots();

        NetworkManager.Singleton.OnClientConnectedCallback += (id) => ActualiserLeBot();
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) => ActualiserLeBot();
        
        ActualiserLeBot();
    }

    void Update()
    {
       
        if (!IsServer) return;

        timerVerification += Time.deltaTime;
        if (timerVerification >= 1.5f) 
        {
            timerVerification = 0f;
            ActualiserLeBot();
        }
    }

    private void ActualiserLeBot()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;

        int humains = NetworkManager.Singleton.ConnectedClientsList.Count;

        if (humains == 1 && botInstance == null)
        {
            SpawnBot();
        }
        
        else if (humains > 1 && botInstance != null)
        {
            if (botInstance.GetComponent<NetworkObject>().IsSpawned)
            {
                botInstance.GetComponent<NetworkObject>().Despawn();
            }
            botInstance = null;
        }
    }

    private void SpawnBot()
    {
        
        if (botInstance != null) return;

        GameObject hostPlayer = null;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.ClientId == NetworkManager.ServerClientId)
            {
                hostPlayer = client.PlayerObject.gameObject;
                break;
            }
        }

        if (hostPlayer != null)
        {
            botInstance = Instantiate(botPrefab, hostPlayer.transform.position + new Vector3(1, 1, 0), Quaternion.identity);
            
            NetworkObject nObj = botInstance.GetComponent<NetworkObject>();
            if (nObj != null)
            {
                nObj.Spawn();
                Debug.Log("<color=cyan>[BotManager]</color> Bot créé pour le mode solo.");
            }
        }
    }

    private void NettoyerVieuxBots()
    {
        GameObject[] vieuxBots = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject g in vieuxBots)
        {
            if (g.name.Contains(botPrefab.name) && g != botInstance)
            {
                NetworkObject n = g.GetComponent<NetworkObject>();
                if (n != null && n.IsSpawned) n.Despawn();
                else Destroy(g);
            }
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= (id) => ActualiserLeBot();
            NetworkManager.Singleton.OnClientDisconnectCallback -= (id) => ActualiserLeBot();
        }
    }
}
