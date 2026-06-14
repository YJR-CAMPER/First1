using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// NPC에 부착하여 ID 기반으로 대화를 시작하는 트리거
/// QuestManager를 직접 참조하지 않고 정적 이벤트로 통보
/// </summary>
public class DialogueTriggerByID : MonoBehaviour
{
    /// <summary>
    /// NPC 대화 완료 시 발화. QuestManager 등 외부 시스템이 구독.
    /// </summary>
    public static event Action<string> OnNPCTalkedTo;

    [SerializeField] private string dialogueId;

    [Header("NPC Identity")]
    [Tooltip("이 NPC의 고유 ID (퀘스트 Talk 목표의 targetId와 매칭)")]
    [SerializeField] private string npcId;

    [Header("Repeat Settings")]
    [SerializeField] private bool canRepeat = true;
    private bool hasTriggered = false;

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRange = 2f;

    [Header("UI (Optional)")]
    [SerializeField] private GameObject interactPrompt;

    private Transform playerTransform;
    private bool playerInRange = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (playerTransform == null || string.IsNullOrEmpty(dialogueId))
            return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = distance <= interactRange;

        UpdateInteractPrompt();

        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TriggerDialogue();
        }
    }

    public void TriggerDialogue()
    {
        if (IsDialogueCurrentlyActive())
            return;

        if (!canRepeat && hasTriggered)
            return;

        if (string.IsNullOrEmpty(dialogueId))
            return;

        var dbManager = DialogueDatabaseManagerExtended.Instance;
        if (dbManager == null)
        {
            Debug.LogError("DialogueDatabaseManager not found.");
            return;
        }

        var dialogue = dbManager.GetDialogue(dialogueId);
        if (dialogue == null)
            return;

        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager not found.");
            return;
        }

        // 대화 완료 시점에 통보하도록 예약 (시작 시점이 아님)
        if (!string.IsNullOrEmpty(npcId))
            RegisterTalkReport();

        DialogueManager.Instance.StartDialogue(dialogue);
        hasTriggered = true;
    }

    /// <summary>
    /// 대화 종료 시 OnNPCTalkedTo를 1회만 발화한 뒤 자기 리스너를 해제한다.
    /// </summary>
    private void RegisterTalkReport()
    {
        UnityAction handler = null;
        handler = () =>
        {
            OnNPCTalkedTo?.Invoke(npcId);
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.OnDialogueEnd.RemoveListener(handler);
        };
        DialogueManager.Instance.OnDialogueEnd.AddListener(handler);
    }

    public void SetDialogueId(string newDialogueId)
    {
        dialogueId = newDialogueId;
        hasTriggered = false;
    }

    public void SetInteractPrompt(GameObject prompt)
    {
        interactPrompt = prompt;
    }

    public void ResetDialogue()
    {
        hasTriggered = false;
    }

    public string NpcId => npcId;

    private void UpdateInteractPrompt()
    {
        if (interactPrompt == null)
            return;

        bool shouldShow = playerInRange && !IsDialogueCurrentlyActive();
        interactPrompt.SetActive(shouldShow);
    }

    private bool IsDialogueCurrentlyActive()
    {
        return DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
