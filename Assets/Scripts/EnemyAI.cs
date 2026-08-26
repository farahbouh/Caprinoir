using UnityEngine;
using Unity.Netcode;

public class EnemyAI : NetworkBehaviour
{
    public float speed = 2f;
    public Transform[] waypoints;
    public bool isStatic = false; 
    
    private int currentWaypointIndex = 0;
    public Animator anim;
    private SpriteRenderer sprite;
    public float detectionRange = 4f;
    public float attackDistance = 1f;
   
    public Transform player;

    private float nextAttackTime = 0f;
    public float attackRate = 2f; 


    public int degatsDeBase = 2;
    public float ratioAttaque = 0.5f; 
    void Start()
    {

        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }
private float checkTargetTimer = 0f;

void Update()
{
    if (!IsServer) return; 

    checkTargetTimer += Time.deltaTime;
    if (checkTargetTimer >= 0.5f)
    {
        FindNearestPlayer();
        checkTargetTimer = 0f;
    }

    if (player != null && Vector2.Distance(transform.position, player.position) < detectionRange && !isStatic) 
    {
        ChasePlayer();
    } 
    else 
    {
        if (isStatic || waypoints.Length == 0) 
        {
            if (anim != null) anim.SetBool("IsMoving", false);
            return;
        }
        Move(); 
    }
}

    void FindNearestPlayer() {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (GameObject p in players) {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < closestDistance) {
                closestDistance = dist;
                closestPlayer = p.transform;
            }
        }
        player = closestPlayer;
    }


public int GetCalculatedDamage() 
{
    BotHealth bHealth = GetComponent<BotHealth>();
    if (bHealth != null) 
    {
        return degatsDeBase + (bHealth.niveau.Value * 2);
    }

    Health pHealth = GetComponent<Health>();
    if (pHealth != null) 
    {
        return degatsDeBase + (pHealth.niveau.Value * 2);
    }

    return degatsDeBase;
}

    void Move()
    {
        if (waypoints.Length == 0) return;
        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        
        if (anim != null) anim.SetBool("IsMoving", true);

        if (target.position.x > transform.position.x) sprite.flipX = false;
        else if (target.position.x < transform.position.x) sprite.flipX = true;

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

   
void ChasePlayer() {
    if (player == null) return;

    float distance = Vector2.Distance(transform.position, player.position);

    if (distance > attackDistance) 
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        if (anim != null) anim.SetBool("IsMoving", true);
    } 
    else 
    {
        if (anim != null) anim.SetBool("IsMoving", false);
    }

    if (player.position.x > transform.position.x) sprite.flipX = false;
    else if (player.position.x < transform.position.x) sprite.flipX = true;
    
    if (distance <= attackDistance) 
    {
        if (Time.time >= nextAttackTime) 
        {
            Health playerHealth = player.GetComponent<Health>(); 
            if (playerHealth == null) playerHealth = player.GetComponentInParent<Health>(); 
            if (playerHealth == null) playerHealth = player.GetComponentInChildren<Health>(); 

            if (playerHealth != null) 
            {
                playerHealth.TakeDamage(GetCalculatedDamage());
                nextAttackTime = Time.time + attackRate;
                
                Debug.Log("Le fantôme a touché le joueur et lui a retiré des PV ");
            }
            else 
            {
                
                Debug.LogError(" Le fantôme est sur le joueur, mais ne trouve AUCUN script Health");
            }
        }
    }
}

    
    void OnDrawGizmos() {
    Gizmos.color = Color.blue;
    Gizmos.DrawWireSphere(transform.position, detectionRange);
}

}