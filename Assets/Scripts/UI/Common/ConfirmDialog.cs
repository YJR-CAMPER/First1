using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 범용 확인 팝업 (콜백 기반)
/// 호출자가 "예를 누르면 할 일"을 콜백으로 넘기므로,
/// 팝업은 어떤 상황에서 불렸는지 알 필요가 없다.
/// 
/// 자동 타임아웃을 설정하면 지정 시간 후 자동으로 취소 콜백을 실행한다.
/// (잘못된 해상도로 화면이 깨져 확인 버튼을 못 누르는 상황 방어)
/// 
/// Setup:
///   Canvas 아래에 배치, 시작 시 비활성.
///   messageText / yesButton / noButton / panel 할당.
///   여러 곳에서 공유하므로 씬에 하나만 두고 Instance로 접근.
/// </summary>
public class ConfirmDialog : SingletonMonoBehaviour<ConfirmDialog>
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Tooltip("타임아웃 카운트다운을 표시할 텍스트 (선택)")]
    [SerializeField] private TextMeshProUGUI countdownText;

    private Action onConfirm;
    private Action onCancel;
    private Coroutine timeoutRoutine;

    protected override void OnSingletonAwake()
    {
        if (yesButton != null)
            yesButton.onClick.AddListener(HandleConfirm);

        if (noButton != null)
            noButton.onClick.AddListener(HandleCancel);

        Hide();
    }

    /// <summary>
    /// 확인 팝업 표시
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="onConfirm">예를 누르면 실행할 동작</param>
    /// <param name="onCancel">아니오 또는 타임아웃 시 실행할 동작 (선택)</param>
    /// <param name="autoTimeoutSeconds">0보다 크면 해당 시간 후 자동 취소 (선택)</param>
    public void Show(string message, Action onConfirm, Action onCancel = null, float autoTimeoutSeconds = 0f)
    {
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;

        if (messageText != null)
            messageText.text = message;

        if (panel != null)
            panel.SetActive(true);

        StopTimeoutRoutine();

        if (autoTimeoutSeconds > 0f)
        {
            timeoutRoutine = StartCoroutine(TimeoutRoutine(autoTimeoutSeconds));
        }
        else if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    public void Hide()
    {
        StopTimeoutRoutine();

        if (panel != null)
            panel.SetActive(false);
    }

    // -- Button Handlers --

    private void HandleConfirm()
    {
        var callback = onConfirm;
        ClearAndHide();
        callback?.Invoke();
    }

    private void HandleCancel()
    {
        var callback = onCancel;
        ClearAndHide();
        callback?.Invoke();
    }

    private void ClearAndHide()
    {
        onConfirm = null;
        onCancel = null;
        Hide();
    }

    // -- Timeout --

    private IEnumerator TimeoutRoutine(float seconds)
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        float remaining = seconds;
        while (remaining > 0f)
        {
            if (countdownText != null)
                countdownText.text = $"{Mathf.CeilToInt(remaining)}";

            // Time.timeScale=0(일시정지) 상황에서도 동작하도록 unscaled 사용
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        timeoutRoutine = null;
        HandleCancel();
    }

    private void StopTimeoutRoutine()
    {
        if (timeoutRoutine != null)
        {
            StopCoroutine(timeoutRoutine);
            timeoutRoutine = null;
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }
}
