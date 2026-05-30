using UnityEngine;

/// <summary>
/// 인터페이스 분리 원칙(ISP)을 따른 세분화된 인터페이스
/// </summary>

// ============ 체력 시스템 인터페이스 ============

/// <summary>
/// 데미지를 받을 수 있는 최소 인터페이스
/// </summary>
public interface IDamageable
{
    void TakeDamage(float damage);
}

/// <summary>
/// 죽음 처리 인터페이스 (데미지와 분리)
/// </summary>
public interface IMortal
{
    bool IsDead();
    event System.Action OnDeath;
}

/// <summary>
/// 즉사 처리 인터페이스 (필요한 경우만 구현)
/// </summary>
public interface IInstantKillable
{
    void InstantKill();
}

/// <summary>
/// 체력 회복 인터페이스
/// </summary>
public interface IHealable
{
    void Heal(float amount);
}

/// <summary>
/// 체력 조회 인터페이스 (읽기 전용)
/// </summary>
public interface IHealthQuery
{
    float GetCurrentHealth();
    float GetMaxHealth();
    float GetHealthPercentage();
}

/// <summary>
/// 무적 상태 조회 인터페이스
/// </summary>
public interface IInvincible
{
    bool IsInvincible();
}

// ============ 이동 시스템 인터페이스 ============

/// <summary>
/// 이동 제어 인터페이스
/// </summary>
public interface IMovementController
{
    void EnableMovement(bool enabled);
    bool IsMovementEnabled();
}

/// <summary>
/// 이동 상태 조회 인터페이스
/// </summary>
public interface IMovementQuery
{
    bool IsMoving();
    Vector2 GetVelocity();
}

/// <summary>
/// 속도 제어 인터페이스
/// </summary>
public interface ISpeedController
{
    void SetMoveSpeed(float speed);
    float GetMoveSpeed();
}

// ============ 물리 시스템 인터페이스 ============

/// <summary>
/// 넉백을 받을 수 있는 인터페이스
/// </summary>
public interface IKnockbackable
{
    void ApplyKnockback(Vector2 direction, float force);
}

// ============ 상호작용 시스템 인터페이스 ============

/// <summary>
/// 상호작용 가능한 객체 인터페이스
/// </summary>
public interface IInteractable
{
    void Interact(GameObject interactor);
    bool CanInteract();
}

/// <summary>
/// 상호작용 정보 제공 인터페이스
/// </summary>
public interface IInteractableInfo
{
    string GetInteractPrompt();
    float GetInteractRange();
}

// ============ 이벤트 인터페이스 ============

/// <summary>
/// 체력 변경 이벤트
/// </summary>
public interface IHealthEventSource
{
    event System.Action<float, float> OnHealthChanged; // current, max
}

/// <summary>
/// 데미지 이벤트
/// </summary>
public interface IDamageEventSource
{
    event System.Action<float> OnDamageTaken; // damage amount
}
