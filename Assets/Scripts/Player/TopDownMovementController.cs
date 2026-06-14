using UnityEngine;

/// <summary>
/// 탑뷰 이동 컨트롤러 (SOLID 원칙 준수)
/// 책임: 플레이어 입력 처리 및 이동만 담당
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TopDownMovementController : MonoBehaviour,
    IMovementController,
    IMovementQuery,
    ISpeedController
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private bool use8Directions = false;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    // 애니메이터 파라미터 해시 (성능 최적화)
    private static readonly int AnimSpeed = Animator.StringToHash("Speed");
    private static readonly int AnimDirectionX = Animator.StringToHash("DirectionX");
    private static readonly int AnimDirectionY = Animator.StringToHash("DirectionY");

    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.down;
    private bool movementEnabled = true;
    private bool hasAnimatorParams = false;

    private void Awake()
    {
        InitializeComponents();
        ConfigureRigidbody();
    }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        ValidateAnimatorParams();
    }

    /// <summary>
    /// Animator Controller에 필요한 파라미터가 있는지 1회 검증
    /// 파라미터 미존재 시 SetFloat 경고를 방지
    /// </summary>
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

    /// <summary>
    /// Rigidbody2D 설정
    /// </summary>
    private void ConfigureRigidbody()
    {
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        ProcessInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    /// <summary>
    /// 입력 처리 (단일 책임)
    /// </summary>
    private void ProcessInput()
    {
        if (!movementEnabled)
        {
            moveInput = Vector2.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(horizontal, vertical);

        // 4방향 제한
        if (!use8Directions && moveInput.x != 0 && moveInput.y != 0)
        {
            moveInput = Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y)
                ? new Vector2(moveInput.x, 0)
                : new Vector2(0, moveInput.y);
        }

        // 방향 업데이트
        if (moveInput != Vector2.zero)
        {
            lastDirection = moveInput.normalized;
        }
    }

    /// <summary>
    /// 물리 기반 이동 적용
    /// </summary>
    private void ApplyMovement()
    {
        if (rb == null)
            return;

        Vector2 velocity = moveInput.normalized * moveSpeed;
        rb.velocity = velocity;
    }

    /// <summary>
    /// 애니메이션 업데이트
    /// Animator가 할당되지 않았거나 애니메이터 컨트롤러가 없으면 스킵
    /// </summary>
    private void UpdateAnimation()
    {
        if (!hasAnimatorParams)
            return;

        float speed = moveInput.magnitude > 0 ? 1f : 0f;

        animator.SetFloat(AnimSpeed, speed);
        animator.SetFloat(AnimDirectionX, lastDirection.x);
        animator.SetFloat(AnimDirectionY, lastDirection.y);
    }

    // ========== IMovementController ==========

    public void EnableMovement(bool enabled)
    {
        movementEnabled = enabled;

        if (!enabled)
        {
            moveInput = Vector2.zero;
            if (rb != null)
                rb.velocity = Vector2.zero;
        }
    }

    public bool IsMovementEnabled() => movementEnabled;

    // ========== IMovementQuery ==========

    public bool IsMoving() => moveInput.magnitude > 0;

    public Vector2 GetVelocity() => rb != null ? rb.velocity : Vector2.zero;

    // ========== ISpeedController ==========

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    public float GetMoveSpeed() => moveSpeed;

    // ========== Public Getters ==========

    public Vector2 GetFacingDirection() => lastDirection;

    /// <summary>
    /// 바라보는 방향을 직접 설정한다 (세이브 복원 등).
    /// 다음 UpdateAnimation에서 Animator에 반영된다.
    /// </summary>
    public void SetFacingDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
            lastDirection = direction.normalized;
    }

    public void SetUse8Directions(bool use8)
    {
        use8Directions = use8;
    }
}
