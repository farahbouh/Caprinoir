using UnityEngine;
using Unity.Cinemachine;
using Unity.Netcode;

public class CameraFollowFix : NetworkBehaviour
{
    private CinemachineCamera vcam;

    void Awake() {
        vcam = GetComponent<CinemachineCamera>();
    }

    void LateUpdate() {
        if (vcam == null) return;

        
        if (vcam.Target.TrackingTarget == null) {
            
            
            NetworkObject[] players = GameObject.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None);

            foreach (NetworkObject p in players)
            {
               
                if (p.IsLocalPlayer)
                {
                    vcam.Target.TrackingTarget = p.transform;
                    
                    vcam.ForceCameraPosition(p.transform.position, Quaternion.identity);
                    
                    Debug.Log("Caméra verrouillée sur MON Player local");
                    break; 
                }
            }
        }
    }

}