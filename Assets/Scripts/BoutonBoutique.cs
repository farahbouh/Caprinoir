using UnityEngine;
using Unity.Netcode; // Ajoute ça

public class BoutonBoutique : MonoBehaviour
{
    public string nomArticle;
    public int prixArticle;

    public void TenterAchat()
    {
        
        if (InventaireGlobal.instance.RetirerPieces(prixArticle))
        {
            Debug.Log("Achat réussi : " + nomArticle);
            
        }
    }
}
