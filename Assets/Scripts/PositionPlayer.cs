using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Collections;

public class PositionPlayer : NetworkBehaviour
{
    private NetworkVariable<FixedString32Bytes> spawnIdReseau = new NetworkVariable<FixedString32Bytes>(
        "", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) StartCoroutine(VerificationAutoTeleport());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsServer)
        {
            spawnIdReseau.Value = GameData.prochainSpawnID;
            StartCoroutine(EffacerIDApresDelai());
        }
        if (IsOwner) StartCoroutine(VerificationAutoTeleport());
    }

    private IEnumerator VerificationAutoTeleport()
    {
        yield return new WaitForSeconds(0.3f);

        if (!IsOwner) yield break;

        string id = spawnIdReseau.Value.ToString();
        GameObject targetPoint = null;

        if (!string.IsNullOrEmpty(id))
        {
            targetPoint = GameObject.Find(id);
        }

        if (targetPoint == null)
        {
            GameObject[] joueurs = GameObject.FindGameObjectsWithTag("Player");
            foreach (var j in joueurs)
            {
                var netObj = j.GetComponent<NetworkObject>();
                if (netObj != null && netObj.IsOwnedByServer && j != gameObject)
                {
                    targetPoint = j;
                    break;
                }
            }
        }

        if (targetPoint != null)
        {
            Vector3 finalPos = targetPoint.transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0, 0);
            
            var cnt = GetComponent<ClientNetworkTransform>();
            if (cnt != null)
                cnt.Teleport(finalPos, Quaternion.identity, transform.localScale);
            else
                transform.position = finalPos;

            Debug.Log("<color=green>POSITION:</color> Téléporté vers " + targetPoint.name);
        }
    }

    private IEnumerator EffacerIDApresDelai()
    {
        yield return new WaitForSeconds(2f);
        spawnIdReseau.Value = "";
       
    }
}
