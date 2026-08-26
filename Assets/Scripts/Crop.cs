using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Crop : NetworkBehaviour 
{
    [Header("Identité de la plante")]
    public string nomDuFruit;
    public Sprite iconeInventaire;

    [Header("Growth")]
    public List<Sprite> growthStages = new List<Sprite>();
    
        public NetworkVariable<int> currentStage = new NetworkVariable<int>(0);

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    
    public override void OnNetworkSpawn()
    {
        
        UpdateSprite(0, currentStage.Value);

        currentStage.OnValueChanged += UpdateSprite;
    }

    public override void OnNetworkDespawn()
    {
        
        currentStage.OnValueChanged -= UpdateSprite;
    }

    public void Grow()
    {
        if (!IsServer) return;

        if (growthStages == null || growthStages.Count == 0) return;

        if (currentStage.Value < growthStages.Count - 1)
        {
            currentStage.Value++; 
        }
        else
        {
            Debug.Log($"{name}: already ready (stage {currentStage.Value}).");
        }
    }

    public bool IsReady()
    {
        return growthStages != null && growthStages.Count > 0 && currentStage.Value >= growthStages.Count - 1;
    }

    public void Harvest()
    {
        if (IsServer) Destroy(gameObject);
    }

    private void UpdateSprite(int ancienneValeur, int nouvelleValeur)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (growthStages != null && growthStages.Count > 0)
        {
            sr.sprite = growthStages[Mathf.Clamp(nouvelleValeur, 0, growthStages.Count - 1)];
        }
    }
}