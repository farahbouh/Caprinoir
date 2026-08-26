using UnityEngine;

public class FarmAnimalAI : MonoBehaviour
{
    public enum State
    {
        Idle,
        Wander,
        FollowPlayer,
        EscapePlayer
    }

    [Header("References")]
    public Transform player;

    [Header("Movement")]
    public float speed = 2f;
    public float followDistance = 3f;
    public float escapeDistance = 1.5f;
    public float reachThreshold = 0.2f;

    [Header("Wander Area")]
    public Transform[] waypoints;

    [Header("Idle")]
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;

    [Header("Food Logic")]
    public string foodItemName = "Carotte";

    [Header("Animation Frames")]
    public Sprite[] sideWalkFrames;
    public Sprite[] frontWalkFrames;
    public Sprite[] backWalkFrames;

    [Header("Animation Settings")]
    public float animationSpeed = 6f;

    private State currentState;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    private Vector2 currentTarget;
    private float idleTimer;

    private float animationTimer = 0f;
    private int currentFrameIndex = 0;
    private Sprite[] currentAnimationFrames;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        PickRandomTarget();
        ChangeState(State.Wander);
    }

    void Update()
    {
        if (player == null)
            return;

        bool playerHasFood = false;

        if (InventaireGlobal.instance != null)
        {
            playerHasFood = InventaireGlobal.instance.objets.Contains(foodItemName);
        }

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer < escapeDistance && !playerHasFood)
        {
            ChangeState(State.EscapePlayer);
        }
        else if (distToPlayer < followDistance && playerHasFood)
        {
            ChangeState(State.FollowPlayer);
        }
        else
        {
            if (currentState == State.FollowPlayer || currentState == State.EscapePlayer)
            {
                PickRandomTarget();
                idleTimer = Random.Range(minIdleTime, maxIdleTime);
                ChangeState(State.Idle);
            }
        }
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;

            case State.Wander:
                HandleWander();
                break;

            case State.FollowPlayer:
                HandleFollowPlayer();
                break;

            case State.EscapePlayer:
                HandleEscapePlayer();
                break;
        }
    }

    void ChangeState(State newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        Debug.Log(gameObject.name + " -> " + currentState);
    }

    Vector2 ClampTargetInsideArea(Vector2 target)
    {
        if (waypoints == null || waypoints.Length < 4)
            return target;

        float minX = Mathf.Min(
            waypoints[0].position.x,
            waypoints[1].position.x,
            waypoints[2].position.x,
            waypoints[3].position.x
        );

        float maxX = Mathf.Max(
            waypoints[0].position.x,
            waypoints[1].position.x,
            waypoints[2].position.x,
            waypoints[3].position.x
        );

        float minY = Mathf.Min(
            waypoints[0].position.y,
            waypoints[1].position.y,
            waypoints[2].position.y,
            waypoints[3].position.y
        );

        float maxY = Mathf.Max(
            waypoints[0].position.y,
            waypoints[1].position.y,
            waypoints[2].position.y,
            waypoints[3].position.y
        );

        float margin = 0.3f;

        target.x = Mathf.Clamp(target.x, minX + margin, maxX - margin);
        target.y = Mathf.Clamp(target.y, minY + margin, maxY - margin);

        return target;
    }

    Vector2 PickFarthestPointFromPlayerInsideArea()
    {
        if (waypoints == null || waypoints.Length < 4)
            return rb.position;

        float minX = Mathf.Min(
            waypoints[0].position.x,
            waypoints[1].position.x,
            waypoints[2].position.x,
            waypoints[3].position.x
        );

        float maxX = Mathf.Max(
            waypoints[0].position.x,
            waypoints[1].position.x,
            waypoints[2].position.x,
            waypoints[3].position.x
        );

        float minY = Mathf.Min(
            waypoints[0].position.y,
            waypoints[1].position.y,
            waypoints[2].position.y,
            waypoints[3].position.y
        );

        float maxY = Mathf.Max(
            waypoints[0].position.y,
            waypoints[1].position.y,
            waypoints[2].position.y,
            waypoints[3].position.y
        );

        float margin = 0.3f;

        Vector2[] candidates = new Vector2[]
        {
            new Vector2(minX + margin, minY + margin),
            new Vector2(minX + margin, maxY - margin),
            new Vector2(maxX - margin, minY + margin),
            new Vector2(maxX - margin, maxY - margin),
            new Vector2((minX + maxX) * 0.5f, minY + margin),
            new Vector2((minX + maxX) * 0.5f, maxY - margin),
            new Vector2(minX + margin, (minY + maxY) * 0.5f),
            new Vector2(maxX - margin, (minY + maxY) * 0.5f)
        };

        Vector2 bestPoint = candidates[0];
        float bestDistance = Vector2.Distance(bestPoint, player.position);

        for (int i = 1; i < candidates.Length; i++)
        {
            float d = Vector2.Distance(candidates[i], player.position);
            if (d > bestDistance)
            {
                bestDistance = d;
                bestPoint = candidates[i];
            }
        }

        return bestPoint;
    }

    void HandleIdle()
    {
        idleTimer -= Time.fixedDeltaTime;

        if (idleTimer <= 0f)
        {
            PickRandomTarget();
            ChangeState(State.Wander);
        }
    }

    void HandleWander()
    {
        Vector2 currentPos = rb.position;
        Vector2 toTarget = currentTarget - currentPos;

        if (toTarget.magnitude <= reachThreshold)
        {
            idleTimer = Random.Range(minIdleTime, maxIdleTime);
            ChangeState(State.Idle);
            return;
        }

        MoveTowards(currentTarget);
    }

    void HandleFollowPlayer()
    {
        if (player == null)
            return;

        MoveTowards(player.position);
    }

    void HandleEscapePlayer()
    {
        if (player == null)
            return;

        Vector2 currentPos = rb.position;
        Vector2 awayDirection = (currentPos - (Vector2)player.position).normalized;

        if (awayDirection.sqrMagnitude < 0.01f)
            awayDirection = Vector2.right;

        Vector2 escapeTarget = currentPos + awayDirection * 2f;
        escapeTarget = ClampTargetInsideArea(escapeTarget);

        if (Vector2.Distance(currentPos, escapeTarget) < 0.2f)
        {
            currentTarget = PickFarthestPointFromPlayerInsideArea();
            MoveTowards(currentTarget);
            return;
        }

        MoveTowards(escapeTarget);
    }

    void MoveTowards(Vector2 target)
    {
        Vector2 currentPos = rb.position;
        Vector2 direction = (target - currentPos).normalized;
        Vector2 newPos = Vector2.MoveTowards(currentPos, target, speed * Time.fixedDeltaTime);

        rb.MovePosition(newPos);

        if (direction.sqrMagnitude > 0.001f)
        {
            UpdateDirectionAnimation(direction);
            AnimateWalk();
        }
    }

    void PickRandomTarget()
    {
        if (waypoints == null || waypoints.Length < 4)
            return;

        float minX = Mathf.Min(
            waypoints[0].position.x,
            waypoints[1].position.x,
            waypoints[2].position.x,
            waypoints[3].position.x
        );

        float maxX = Mathf.Max(
            waypoints[0].position.x,
            waypoints[1].position.x,
            waypoints[2].position.x,
            waypoints[3].position.x
        );

        float minY = Mathf.Min(
            waypoints[0].position.y,
            waypoints[1].position.y,
            waypoints[2].position.y,
            waypoints[3].position.y
        );

        float maxY = Mathf.Max(
            waypoints[0].position.y,
            waypoints[1].position.y,
            waypoints[2].position.y,
            waypoints[3].position.y
        );

        float margin = 0.3f;

        currentTarget = new Vector2(
            Random.Range(minX + margin, maxX - margin),
            Random.Range(minY + margin, maxY - margin)
        );
    }

    void UpdateDirectionAnimation(Vector2 direction)
    {
        if (sprite == null)
            return;

        Sprite[] newFrames = null;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            newFrames = sideWalkFrames;
            sprite.flipX = direction.x < 0;
        }
        else
        {
            sprite.flipX = false;

            if (direction.y > 0)
                newFrames = backWalkFrames;
            else
                newFrames = frontWalkFrames;
        }

        if (currentAnimationFrames != newFrames)
        {
            currentAnimationFrames = newFrames;
            currentFrameIndex = 0;
            animationTimer = 0f;

            if (currentAnimationFrames != null && currentAnimationFrames.Length > 0)
            {
                sprite.sprite = currentAnimationFrames[0];
            }
        }
    }

    void AnimateWalk()
    {
        if (currentAnimationFrames == null || currentAnimationFrames.Length == 0)
            return;

        animationTimer += Time.deltaTime;

        if (animationTimer >= 1f / animationSpeed)
        {
            animationTimer = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % currentAnimationFrames.Length;
            sprite.sprite = currentAnimationFrames[currentFrameIndex];
        }
    }
}