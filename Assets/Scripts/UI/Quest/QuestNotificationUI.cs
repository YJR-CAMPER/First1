using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 퀘스트 상태 변경 시 화면에 알림 배너를 표시
/// 시작/완료/실패에 따라 다른 색상과 아이콘 표시
/// 
/// Setup:
///   1. Canvas 아래에 QuestNotificationUI 오브젝트 배치
///   2. notificationPanel 안에 statusText, questNameText 할당
///   3. CanvasGroup을 notificationPanel에 추가 (자동 페이드용)
/// </summary>
public class QuestNotificationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private Image backgroundImage;

    [Header("Colors")]
    [SerializeField] private Color startColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    [SerializeField] private Color completeColor = new Color(0.2f, 0.8f, 0.4f, 1f);
    [SerializeField] private Color failColor = new Color(0.9f, 0.3f, 0.3f, 1f);

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private readonly Queue<NotificationRequest> pendingQueue = new Queue<NotificationRequest>();
    private Coroutine activeCoroutine;
    private bool isShowing;

    private void Awake()
    {
        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        if (QuestManager.Instance == null)
            return;

        QuestManager.Instance.OnQuestStateChanged += HandleQuestStateChanged;
    }

    private void Start()
    {
        if (QuestManager.Instance == null)
            return;

        QuestManager.Instance.OnQuestStateChanged -= HandleQuestStateChanged;
        QuestManager.Instance.OnQuestStateChanged += HandleQuestStateChanged;
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestStateChanged -= HandleQuestStateChanged;
        }
    }

    private void HandleQuestStateChanged(string questId, QuestState newState)
    {
        var data = QuestManager.Instance.GetQuestData(questId);
        if (data == null)
            return;

        string status;
        Color color;

        switch (newState)
        {
            case QuestState.Active:
                status = "Quest Started";
                color = startColor;
                break;
            case QuestState.Completed:
                status = "Quest Complete";
                color = completeColor;
                break;
            case QuestState.Failed:
                status = "Quest Failed";
                color = failColor;
                break;
            default:
                return;
        }

        EnqueueNotification(status, data.questName, color);
    }

    private void EnqueueNotification(string status, string questName, Color color)
    {
        pendingQueue.Enqueue(new NotificationRequest
        {
            status = status,
            questName = questName,
            color = color
        });

        if (!isShowing)
        {
            ShowNext();
        }
    }

    private void ShowNext()
    {
        if (pendingQueue.Count == 0)
        {
            isShowing = false;
            return;
        }

        isShowing = true;
        var request = pendingQueue.Dequeue();

        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(ShowNotificationRoutine(request));
    }

    private IEnumerator ShowNotificationRoutine(NotificationRequest request)
    {
        if (statusText != null)
            statusText.text = request.status;

        if (questNameText != null)
            questNameText.text = request.questName;

        if (backgroundImage != null)
            backgroundImage.color = request.color;

        notificationPanel.SetActive(true);

        // Fade in
        yield return FadeRoutine(0f, 1f, fadeInDuration);

        // Display
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade out
        yield return FadeRoutine(1f, 0f, fadeOutDuration);

        notificationPanel.SetActive(false);
        activeCoroutine = null;

        ShowNext();
    }

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private struct NotificationRequest
    {
        public string status;
        public string questName;
        public Color color;
    }
}
