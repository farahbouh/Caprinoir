using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventaireGlobal : MonoBehaviour
{
    public static InventaireGlobal instance;

    [Header("Le contenu de mon sac")]
    public List<string> objets = new List<string>();
    public List<Sprite> icones = new List<Sprite>();
    public List<int> quantites = new List<int>(); 

    [Header("Économie")]
    public PrixDeVente[] cataloguePrix; 

    [Header("Interface Visuelle (UI)")]
    public GameObject panelGrandInventaire;
    public Image[] casesHotbar;
    public TextMeshProUGUI[] textesQuantite;

    [Header("Argent")]
    public int pieces = 100; 
    public TextMeshProUGUI textePieces;

    [Header("Sélection")]
    public int indexSelectionne = 0;
    public Image[] borduresSelection;

    [System.Serializable]
    public struct PrixDeVente
    {
        public string nomObjet;
        public int prix;
    }

    private bool inventaireOuvert = false;

    private void Awake()
    {
        if (instance != null) { Destroy(this.gameObject); return; }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        MettreAJourUI(); 
        ActualiserArgentUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventaireOuvert = !inventaireOuvert;
            if (panelGrandInventaire != null) panelGrandInventaire.SetActive(inventaireOuvert);
        }

        for (int i = 0; i < casesHotbar.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ChangerSelection(i);
            }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (objets.Count > 0 && indexSelectionne < objets.Count)
            {
                string nomObjetEnMain = objets[indexSelectionne];
                int prixGagne = 0;

       
                foreach (PrixDeVente pv in cataloguePrix)
                {
                    if (pv.nomObjet == nomObjetEnMain)
                    {
                        prixGagne = pv.prix;
                        break; 
                    }
                }

                if (prixGagne > 0)
                {
                    AjouterPieces(prixGagne); 
                    Debug.Log("Tu as vendu [" + nomObjetEnMain + "] pour " + prixGagne + " G !");
                    
                    ConsommerObjet(indexSelectionne);
                }
                else
                {
                    Debug.Log("Le marchand ne veut pas acheter [" + nomObjetEnMain + "].");
                }
            }
        }
    }

    void ChangerSelection(int nouvelIndex)
    {
        
        if (nouvelIndex < 0) nouvelIndex = casesHotbar.Length - 1;
        if (nouvelIndex >= casesHotbar.Length) nouvelIndex = 0;

        indexSelectionne = nouvelIndex;
        ActualiserVisuelSelection();
        
        if (indexSelectionne < objets.Count) {
            Debug.Log("Maintenant en main : " + objets[indexSelectionne]);
        } else {
            Debug.Log("Main vide");
        }
    }

    [Header("Sélection Visuelle")]
    public GameObject cadreSelection; 

    void ActualiserVisuelSelection()
    {
        if (indexSelectionne < casesHotbar.Length)
        {
            cadreSelection.transform.position = casesHotbar[indexSelectionne].transform.position;
            
            cadreSelection.SetActive(true);
            
            for (int i = 0; i < casesHotbar.Length; i++)
            {
                casesHotbar[i].transform.localScale = (i == indexSelectionne) ? new Vector3(1.1f, 1.1f, 1) : Vector3.one;
            }
        }
    }

    public void AjouterObjet(string nomObjet, Sprite imageObjet)
    {
        if (objets.Contains(nomObjet))
        {
            int index = objets.IndexOf(nomObjet);
            quantites[index]++;
            Debug.Log("+1 " + nomObjet + " (Total : " + quantites[index] + ")");
        }
        else
        {
            
            objets.Add(nomObjet);
            icones.Add(imageObjet);
            quantites.Add(1); 
            Debug.Log("Nouvelle case pour : " + nomObjet);
        }

        MettreAJourUI();
    }

    public void MettreAJourUI()
    {
        for (int i = 0; i < casesHotbar.Length; i++)
        {
            if (i < objets.Count)
            {
                casesHotbar[i].sprite = icones[i];
                casesHotbar[i].color = new Color(1, 1, 1, 1);
                
                if (textesQuantite.Length > i && textesQuantite[i] != null)
                {
                    textesQuantite[i].text = quantites[i].ToString();
                    textesQuantite[i].enabled = true; 
                }
            }
            else
            {
                casesHotbar[i].sprite = null;
                casesHotbar[i].color = new Color(1, 1, 1, 0);

                if (textesQuantite.Length > i && textesQuantite[i] != null)
                {
                    textesQuantite[i].enabled = false; 
                }
            }
        }
    }

    public bool RetirerPieces(int montant)
    {
        if (pieces >= montant)
        {
            pieces -= montant;
            ActualiserArgentUI();
            return true;
        }
        Debug.Log("Pas assez de pièces !");
        return false; 
    }

    public void AjouterPieces(int montant)
    {
        pieces += montant;
        ActualiserArgentUI();
    }

    void ActualiserArgentUI()
    {
        if (textePieces != null) textePieces.text = pieces.ToString() + " G";
    }

    public void ConsommerObjet(int index)
    {
        if (index >= 0 && index < objets.Count)
        {
            quantites[index]--;

            if (quantites[index] <= 0)
            {
                objets.RemoveAt(index);
                icones.RemoveAt(index);
                quantites.RemoveAt(index);
                
                if (indexSelectionne >= objets.Count)
                {
                    indexSelectionne = Mathf.Max(0, objets.Count - 1);
                }
            }

            MettreAJourUI();
        }
    }
}