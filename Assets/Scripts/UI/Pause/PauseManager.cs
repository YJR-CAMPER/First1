using System;
using UnityEngine;

/// <summary>
/// 게임 일시정지 상태를 관리하는 매니저
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }
    
    public event EventHandler<PauseStateChangedEventArgs> OnPauseStateChanged;
    
    public bool IsPaused { get; private set; }
    
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    public void Pause()
    {
        if (IsPaused)
            return;
        
        IsPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        
        OnPauseStateChanged?.Invoke(this, new PauseStateChangedEventArgs(true));
    }

    public void Resume()
    {
        if (!IsPaused)
            return;
        
        IsPaused = false;
        Time.timeScale = previousTimeScale;
        
        OnPauseStateChanged?.Invoke(this, new PauseStateChangedEventArgs(false));
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Time.timeScale = 1f;
            Instance = null;
        }
    }
}
