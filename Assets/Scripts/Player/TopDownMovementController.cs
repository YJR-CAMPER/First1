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
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int DirectionX = Animator.StringToHash("DirectionX");
    private static readonly int DirectionY = Animator.StringToHash("DirectionY");
    
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.down;
    private bool movementEnabled = true;
    
    private void Awake()
    {
        InitializeComponents();
        ConfigureRigidbody();
    }
    
    /// <summary>
    /// 컴포넌트 초기화 (KISS 원칙)
    /// </summary>
    private void InitializeComponents()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
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
    /// </summary>
    private void UpdateAnimation()
    {
        return;

        if (animator == null)
            return;
        
        float speed = moveInput.magnitude > 0 ? 1f : 0f;
        
        animator.SetFloat(Speed, speed);
        animator.SetFloat(DirectionX, lastDirection.x);
        animator.SetFloat(DirectionY, lastDirection.y);
    }
    
    // ========== IMovementController 구현 ==========
    
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
    
    // ========== IMovementQuery 구현 ==========
    
    public bool IsMoving() => moveInput.magnitude > 0;
    
    public Vector2 GetVelocity() => rb != null ? rb.velocity : Vector2.zero;
    
    // ========== ISpeedController 구현 ==========
    
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }
    
    public float GetMoveSpeed() => moveSpeed;
    
    // ========== Public Getters ==========
    
    public Vector2 GetFacingDirection() => lastDirection;
    
    public void SetUse8Directions(bool use8)
    {
        use8Directions = use8;
    }
}
