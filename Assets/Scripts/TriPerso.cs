using UnityEngine;
using Unity.Netcode;

public class TriPersonnage : NetworkBehaviour
{
    private SpriteRenderer spriteRenderer;
    private int dernierOrder = 0;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        if (spriteRenderer == null) return;
        
        GameObject[] arbres = GameObject.FindGameObjectsWithTag("Arbre");
        
        if (arbres.Length == 0) return;
        
        // Trouve l'arbre le plus proche en distance TOTALE (X + Y)
        GameObject arbreLePlusProche = null;
        float distanceMin = Mathf.Infinity;
        
        foreach (GameObject arbre in arbres)
        {
            // Distance euclidienne (la vraie distance)
            float distanceX = transform.position.x - arbre.transform.position.x;
            float distanceY = transform.position.y - arbre.transform.position.y;
            float distanceTotale = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);
            
            if (distanceTotale < distanceMin)
            {
                distanceMin = distanceTotale;
                arbreLePlusProche = arbre;
            }
        }
        
        if (arbreLePlusProche != null)
        {
            // Le pied de l'arbre est à sa position Y
            float piedArbre = arbreLePlusProche.transform.position.y;
            
            // Ajoute une petite marge pour éviter le clignotement
            float marge = 0.1f;
            int nouvelOrder = dernierOrder;
            
            if (transform.position.y > piedArbre + marge)
            {
                nouvelOrder = 1;
            }
            else if (transform.position.y < piedArbre - marge)
            {
                nouvelOrder = 0;
            }
            
            if (spriteRenderer.sortingOrder != nouvelOrder)
            {
                spriteRenderer.sortingOrder = nouvelOrder;
                dernierOrder = nouvelOrder;
            }
        }
    }
}