using UnityEngine;

/// <summary>
/// Singleton MonoBehaviour 공용 베이스 클래스
/// 모든 싱글턴의 생명주기와 중복 방지 로직을 통일
/// Persist를 override하면 DontDestroyOnLoad 적용
/// </summary>
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    /// <summary>
    /// true를 반환하면 씬 전환 시에도 유지 (DontDestroyOnLoad)
    /// </summary>
    protected virtual bool Persist => false;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;

        if (Persist)
        {
            DontDestroyOnLoad(gameObject);
        }

        OnSingletonAwake();
    }

    /// <summary>
    /// Singleton 초기화 후 호출되는 콜백
    /// Awake 대신 이 메서드를 override하여 초기화 로직을 구현
    /// </summary>
    protected virtual void OnSingletonAwake() { }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
