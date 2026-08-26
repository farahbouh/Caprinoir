using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

[System.Serializable]
public class Produit
{
    public string nom;
    public Sprite icone;
    public int prix;
}

public class Vendeur : MonoBehaviour
{
    [Header("Le Catalogue du Magasin")]
    public List<Produit> objetsAVendre = new List<Produit>(); 

    [Header("Interface (UI)")]
    public GameObject panelBoutique;       
    public GameObject prefabBoutonArticle; 
    public Transform conteneurBoutons;     

    private bool joueurEstProche = false;
    private bool boutiqueOuverte = false;  

    private void Update()
    {
        if (joueurEstProche && Input.GetKeyDown(KeyCode.T))
        {
            if (boutiqueOuverte) 
                FermerBoutique(); 
            else 
                OuvrirBoutique(); 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) joueurEstProche = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            joueurEstProche = false;
            FermerBoutique(); 
        }
    }

    private void OuvrirBoutique()
    {
        if (panelBoutique == null)
        {
            Transform panel = InventaireGlobal.instance.transform.Find("Canvas/Panel_Boutique");
            
            if (panel != null)
            {
                panelBoutique = panel.gameObject;
                conteneurBoutons = panel; 
            }
            else
            {
                Debug.LogError("Erreur : Le Vendeur ne trouve pas le Panel_Boutique dans l'Inventaire ");
                return; 
            }
        }

        boutiqueOuverte = true;
        panelBoutique.SetActive(true);

        for (int i = conteneurBoutons.childCount - 1; i >= 0; i--)
        {
            Destroy(conteneurBoutons.GetChild(i).gameObject);
        }

        foreach (Produit p in objetsAVendre)
        {
            GameObject nouveauBouton = Instantiate(prefabBoutonArticle, conteneurBoutons);
            
            nouveauBouton.transform.Find("Texte_Infos").GetComponent<TextMeshProUGUI>().text = p.prix + " G";
            nouveauBouton.transform.Find("Icone").GetComponent<Image>().sprite = p.icone;

            Button btn = nouveauBouton.GetComponent<Button>();
            btn.onClick.AddListener(() => {
                if (InventaireGlobal.instance.RetirerPieces(p.prix))
                {
                    InventaireGlobal.instance.AjouterObjet(p.nom, p.icone);
                    Debug.Log("Achat réussi");
                }
            });
        }
    }

    private void FermerBoutique()
    {
        boutiqueOuverte = false;
        if (panelBoutique != null) panelBoutique.SetActive(false); 
    }
}