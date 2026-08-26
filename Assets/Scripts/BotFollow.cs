using UnityEngine;
using Unity.Netcode;

public class BotFollow : NetworkBehaviour
{
    [Header("Réglages du Bot")]
    public float distanceSuivi = 1.5f;
    public float vitesse = 3.5f;
    
    private Transform cible;
    private Animator anim; 

    void Start()
    {
        
        anim = GetComponentInChildren<Animator>();
        
        if (anim == null)
        {
            Debug.LogError("Le script BotFollow ne trouve pas d'Animator sur " + gameObject.name + " ou ses enfants");
        }
    }

    void Update()
    {
        if (!IsServer) return;

        if (cible == null) TrouverLeHost();

        if (cible != null)
        {
            CalculerMouvement();
        }
    }

    void CalculerMouvement()
    {
        float distanceActuelle = Vector2.Distance(transform.position, cible.position);

        if (distanceActuelle > distanceSuivi)
        {
            
            transform.position = Vector2.MoveTowards(transform.position, cible.position, vitesse * Time.deltaTime);
            
            if (anim != null)
            {
                anim.SetBool("isMoving", true); 
                anim.SetFloat("horizontal", 1f); 
                anim.SetFloat("vertical", 0f);
            }

            if (cible.position.x < transform.position.x)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            if (anim != null)
            {
                anim.SetBool("isMoving", false);
                anim.SetFloat("horizontal", 0f);
                anim.SetFloat("vertical", 0f);
            }
        }
    }

    void TrouverLeHost()
    {
        GameObject[] joueurs = GameObject.FindGameObjectsWithTag("Player");
        foreach (var j in joueurs)
        {
            var netObj = j.GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsOwnedByServer && j != gameObject)
            {
                cible = j.transform;
                break;
            }
        }
    }
}