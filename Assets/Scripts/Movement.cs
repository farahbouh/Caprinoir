using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed = 5f;
    public Animator animator; 
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 direction = new Vector3(horizontal, vertical, 0);

        if (direction.magnitude > 1) direction = direction.normalized;

        AnimateMovement(direction);
        transform.position += direction * speed * Time.deltaTime;
    }

    void AnimateMovement(Vector3 direction)
    {
        if (direction.magnitude > 0.1f) 
        {
            animator.SetBool("isMoving", true);
            animator.SetFloat("horizontal", direction.x);
            animator.SetFloat("vertical", direction.y);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }
}