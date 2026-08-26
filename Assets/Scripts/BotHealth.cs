using UnityEngine;
using Unity.Netcode;

public class BotHealth : NetworkBehaviour 
{
    [Header("Stats du Bot")]
    public int pvDeBase = 10;
    public int pvParNiveau = 5;
    public bool isPlayer = false;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(10, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> niveau = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private UIHealthBar uiBar;

    public override void OnNetworkSpawn()
    {
        uiBar = GetComponent<UIHealthBar>();
        if (IsServer) 
        {
            int calculPv = pvDeBase + (niveau.Value * pvParNiveau);
            maxHealth.Value = calculPv;
            currentHealth.Value = calculPv;
        }
        currentHealth.OnValueChanged += (oldVal, newVal) => { if (uiBar != null) uiBar.UpdateUI(); };
        if (uiBar != null) uiBar.UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        currentHealth.Value -= damage;
        if (currentHealth.Value <= 0) Die();
    }

    void Die()
    {
        if (!IsServer) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            Health h = p.GetComponent<Health>();
            if (h != null) 
            {
                int xpGagnee = Mathf.RoundToInt(20 * Mathf.Pow(niveau.Value, 1.2f));
                h.AjouterXP(xpGagnee);
            }
        }

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
    }
}