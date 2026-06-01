/// <summary>
/// 저장/로드가 필요한 컴포넌트가 구현하는 인터페이스
/// SaveManager가 씬 내 모든 ISaveable을 수집하여 처리
/// </summary>
public interface ISaveable
{
    /// <summary>
    /// 고유 식별자 (같은 타입이 여러 개일 때 구분)
    /// </summary>
    string SaveId { get; }

    /// <summary>
    /// 현재 상태를 SaveData에 기록
    /// </summary>
    void CaptureState(SaveData saveData);

    /// <summary>
    /// SaveData에서 상태를 복원
    /// </summary>
    void RestoreState(SaveData saveData);
}
