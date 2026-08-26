using System.Collections; 
using System.Collections.Generic; 
using UnityEngine;
using Unity.Netcode;

public class Health : NetworkBehaviour 
{
    [Header("Stats de base")]
    public int pvDeBase = 15;
    public int pvParNiveau = 5;
    public bool isPlayer = false;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(15, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(15, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> niveau = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> xpActuelle = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Experience")]
    public int xpPourNiveauSuivant = 45;
    private UIHealthBar uiBar;

    public override void OnNetworkSpawn()
    {
        uiBar = GetComponent<UIHealthBar>();

        if (IsServer) 
        {
            if (isPlayer && GameData.estInitialise)
            {
                niveau.Value = GameData.niveauSauvegarde;
                xpActuelle.Value = GameData.xpSauvegarde;
                xpPourNiveauSuivant = GameData.palierSauvegarde;
                
                int calculPv = pvDeBase + (niveau.Value * pvParNiveau);
                maxHealth.Value = calculPv;
                currentHealth.Value = (GameData.pvActuelsSauvegarde != -1) ? GameData.pvActuelsSauvegarde : calculPv;
            }
            else
            {
                ActualiserStatsSelonNiveau(); 
            }
        }

        currentHealth.OnValueChanged += (oldVal, newVal) => { if (uiBar != null) uiBar.UpdateUI(); };
        if (uiBar != null) uiBar.UpdateUI();
    }

    public void ActualiserStatsSelonNiveau()
    {
        if (!IsServer) return;
        int nouveauxPvMax = pvDeBase + (niveau.Value * pvParNiveau);
        maxHealth.Value = nouveauxPvMax;
        currentHealth.Value = nouveauxPvMax; 
    }

    public int GetCurrentHealth() { return currentHealth.Value; }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        currentHealth.Value -= damage;
        if (isPlayer) GameData.pvActuelsSauvegarde = currentHealth.Value;
        if (currentHealth.Value <= 0) Die();
    }

    public void AjouterXP(int montant)
    {
        if (!IsServer) return;
        xpActuelle.Value += montant;
        if (isPlayer) GameData.xpSauvegarde = xpActuelle.Value;
        while (xpActuelle.Value >= xpPourNiveauSuivant) LevelUp();
    }

    void LevelUp()
    {
        niveau.Value++;
        xpActuelle.Value -= xpPourNiveauSuivant;
        xpPourNiveauSuivant = Mathf.RoundToInt(xpPourNiveauSuivant * 1.3f) + 20;
        maxHealth.Value = pvDeBase + (niveau.Value * pvParNiveau);
        currentHealth.Value = maxHealth.Value; 
        
        if (isPlayer)
        {
            GameData.niveauSauvegarde = niveau.Value;
            GameData.palierSauvegarde = xpPourNiveauSuivant;
        }
        if (uiBar != null) uiBar.UpdateUI();
    }

    void Die()
    {
        if (!IsServer) return;

        if (isPlayer) 
        {
            currentHealth.Value = maxHealth.Value;
            RespawnPlayer();
        }
        else
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players)
            {
                Health h = p.GetComponent<Health>();
                if (h != null && h.isPlayer) 
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

    void RespawnPlayer()
    {
        string nomDuPoint = Teleporteur.prochainSpawnID;
        GameObject spawnTarget = GameObject.Find(string.IsNullOrEmpty(nomDuPoint) ? "DefaultSpawn" : nomDuPoint);

        if (spawnTarget != null) 
        {
            transform.position = spawnTarget.transform.position;
        }
        if (uiBar != null) uiBar.UpdateUI();
    }
}