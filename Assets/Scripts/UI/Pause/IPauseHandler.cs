using System;

/// <summary>
/// 일시정지 상태 변경에 반응해야 하는 객체용 인터페이스
/// </summary>
public interface IPauseHandler
{
    void OnPause();
    void OnResume();
}

/// <summary>
/// 일시정지 상태 변경 이벤트 인자
/// </summary>
public class PauseStateChangedEventArgs : EventArgs
{
    public bool IsPaused { get; }
    
    public PauseStateChangedEventArgs(bool isPaused)
    {
        IsPaused = isPaused;
    }
}
