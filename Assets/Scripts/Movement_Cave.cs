using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class Movement_Cave : NetworkBehaviour
{
    public float speed = 5f;
    private Animator animator;
    private Rigidbody2D rd;
    private bool peutBouger = true;
    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rd = GetComponent<Rigidbody2D>();
    }

   
    public override void OnNetworkSpawn()
    {
        if (IsOwner) StartCoroutine(WaitAfterSpawn());
    }

    private IEnumerator WaitAfterSpawn()
    {
       
        if (!string.IsNullOrEmpty(Teleporteur.prochainSpawnID))
        {
            peutBouger = false; 
            yield return new WaitForSeconds(0.3f); 
            peutBouger = true;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner || !peutBouger) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector2 direction = new Vector2(horizontal, vertical).normalized;

        if (animator != null)
        {
            bool isMoving = direction.magnitude > 0.1f;
            animator.SetBool("isMoving", isMoving);
            if (isMoving)
            {
                animator.SetFloat("horizontal", direction.x);
                animator.SetFloat("vertical", direction.y);
            }
        }

        if (rd != null) {
            rd.linearVelocity = direction * speed;
        }
    }
}