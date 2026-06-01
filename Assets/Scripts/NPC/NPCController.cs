using UnityEngine;

/// <summary>
/// NPC 행동을 관리하는 FSM 오케스트레이터
/// Idle, Patrol, Interact 상태를 전환
/// 
/// Setup:
///   1. NPC 오브젝트에 NPCController + Rigidbody2D 추가
///   2. WaypointPath를 별도 오브젝트에 추가하고 waypointPath에 할당
///   3. DialogueTriggerByID가 있으면 대화 시 자동으로 Interact 상태 전환
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class NPCController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float idleWaitTime = 2f;

    [Header("Patrol")]
    [SerializeField] private WaypointPath waypointPath;
    [SerializeField] private bool startWithPatrol = true;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimDirectionX = Animator.StringToHash("DirectionX");
    private static readonly int AnimDirectionY = Animator.StringToHash("DirectionY");

    private INPCState currentState;
    private Vector2 facingDirection = Vector2.down;
    private int currentWaypointIndex;
    private bool hasAnimatorParams;

    // -- State Instances (재사용) --
    public NPCIdleState IdleState { get; private set; }
    public NPCPatrolState PatrolState { get; private set; }
    public NPCInteractState InteractState { get; private set; }

    // -- Public Properties --
    public float MoveSpeed => moveSpeed;
    public float IdleWaitTime => idleWaitTime;
    public Vector2 Position => transform.position;
    public bool HasWaypoints => waypointPath != null && waypointPath.Count > 0;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        ConfigureRigidbody();
        ValidateAnimatorParams();

        IdleState = new NPCIdleState();
        PatrolState = new NPCPatrolState();
        InteractState = new NPCInteractState();
    }

    private void Start()
    {
        INPCState initialState = (startWithPatrol && HasWaypoints)
            ? (INPCState)PatrolState
            : IdleState;

        TransitionTo(initialState);
    }

    private void Update()
    {
        currentState?.Update(this);
        UpdateAnimation();
    }

    private void OnEnable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart.AddListener(HandleDialogueStart);
        }
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart.RemoveListener(HandleDialogueStart);
        }
    }

    // -- State Machine --

    public void TransitionTo(INPCState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    // -- Movement --

    public void SetVelocity(Vector2 velocity)
    {
        if (rb != null)
        {
            rb.velocity = velocity;
        }
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            facingDirection = direction;
        }
    }

    // -- Waypoint --

    public Vector2 GetCurrentWaypoint()
    {
        if (waypointPath == null)
            return Position;

        return waypointPath.GetPosition(currentWaypointIndex);
    }

    public void AdvanceWaypoint()
    {
        if (waypointPath == null)
            return;

        currentWaypointIndex = waypointPath.GetNextIndex(currentWaypointIndex);
    }

    // -- Dialogue Integration --

    private void HandleDialogueStart()
    {
        if (!IsPlayerNearby())
            return;

        if (currentState != InteractState)
        {
            TransitionTo(InteractState);
        }
    }

    private bool IsPlayerNearby()
    {
        var trigger = GetComponent<DialogueTriggerByID>();
        if (trigger == null)
            return false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return false;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        return distance <= 3f;
    }

    // -- Animation --

    private void UpdateAnimation()
    {
        if (!hasAnimatorParams)
            return;

        float speed = rb != null && rb.velocity.magnitude > 0.01f ? 1f : 0f;

        animator.SetFloat(AnimSpeed, speed);
        animator.SetFloat(AnimDirectionX, facingDirection.x);
        animator.SetFloat(AnimDirectionY, facingDirection.y);
    }

    private void ValidateAnimatorParams()
    {
        hasAnimatorParams = false;

        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        bool hasSpeed = false;
        bool hasDirX = false;
        bool hasDirY = false;

        foreach (var param in animator.parameters)
        {
            if (param.nameHash == AnimSpeed) hasSpeed = true;
            else if (param.nameHash == AnimDirectionX) hasDirX = true;
            else if (param.nameHash == AnimDirectionY) hasDirY = true;
        }

        hasAnimatorParams = hasSpeed && hasDirX && hasDirY;
    }

    // -- Setup --

    private void ConfigureRigidbody()
    {
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}
