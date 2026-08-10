using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 상태에 따라 다른 대화를 재생하는 NPC 트리거
/// DialogueTriggerByID의 상위 버전 (상태별 분기 지원)
/// 
/// 동작:
///   1. 플레이어가 범위 내에서 상호작용 키를 누름
///   2. bindings를 위에서부터 검사하여 조건과 일치하는 첫 항목 선택
///   3. 일치 항목이 없으면 defaultDialogueId 재생
///   4. 대화 종료 시 OnNPCTalkedTo 발화 (Talk 목표 자동 매칭)
///   5. 선택된 binding에 startsQuestId가 있으면 대화 종료 시 퀘스트 시작
/// 
/// bindings는 구체적 조건을 위에, 일반적 조건을 아래에 배치할 것.
/// </summary>
public class QuestDialogueTrigger : MonoBehaviour
{
    public static event Action<string> OnNPCTalkedTo;

    [Header("NPC Identity")]
    [Tooltip("이 NPC의 고유 ID (퀘스트 Talk 목표의 targetId와 매칭)")]
    [SerializeField] private string npcId;

    [Header("Quest-aware Dialogues")]
    [Tooltip("위에서부터 첫 매칭을 사용. 구체적 조건을 위로 배치")]
    [SerializeField] private List<QuestDialogueBinding> bindings = new List<QuestDialogueBinding>();

    [Tooltip("어떤 binding과도 매칭되지 않을 때 재생할 기본 대화")]
    [SerializeField] private string defaultDialogueId;

    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRange = 2f;

    [Header("UI (Optional)")]
    [SerializeField] private GameObject interactPrompt;

    private Transform playerTransform;
    private bool playerInRange = false;
    private string pendingStartQuestId;

    public string NpcId => npcId;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (playerTransform == null)
            return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = distance <= interactRange;

        UpdateInteractPrompt();

        if (playerInRange && Input.GetKeyDown(interactKey))
            TriggerDialogue();
    }

    public void TriggerDialogue()
    {
        if (IsDialogueCurrentlyActive())
            return;

        string dialogueId = SelectDialogueId(out string startsQuestId);

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

        pendingStartQuestId = startsQuestId;

        DialogueManager.Instance.OnDialogueEnd.AddListener(HandleDialogueEnd);
        DialogueManager.Instance.StartDialogue(dialogue);
    }

    /// <summary>
    /// 현재 퀘스트 상태에 맞는 대화 ID를 선택
    /// </summary>
    private string SelectDialogueId(out string startsQuestId)
    {
        startsQuestId = null;

        var questManager = QuestManager.Instance;
        if (questManager != null)
        {
            foreach (var binding in bindings)
            {
                if (IsBindingMatch(binding, questManager))
                {
                    startsQuestId = binding.startsQuestId;
                    return binding.dialogueId;
                }
            }
        }

        return defaultDialogueId;
    }

    private bool IsBindingMatch(QuestDialogueBinding binding, QuestManager questManager)
    {
        bool hasQuest = !string.IsNullOrEmpty(binding.questId);

        // 퀘스트 조건은 questId가 지정된 경우에만 검사
        // questId가 비어있으면 플래그 전용 binding으로 취급
        if (hasQuest)
        {
            // 퀘스트 상태 확인
            QuestState state = questManager.GetQuestState(binding.questId);
            if (state != binding.requiredState)
                return false;

            // 목표 완료 조건 (설정된 경우만)
            if (!string.IsNullOrEmpty(binding.requiredCompletedObjective))
            {
                if (!IsObjectiveComplete(questManager, binding.questId, binding.requiredCompletedObjective))
                    return false;
            }

            // 목표 미완료 조건 (설정된 경우만)
            if (!string.IsNullOrEmpty(binding.requiredIncompleteObjective))
            {
                if (IsObjectiveComplete(questManager, binding.questId, binding.requiredIncompleteObjective))
                    return false;
            }
        }

        // 플래그 조건 (설정된 경우만) - 퀘스트 유무와 무관하게 항상 검사
        if (!string.IsNullOrEmpty(binding.requiredFlag))
        {
            bool flagValue = questManager.GetStoryFlag(binding.requiredFlag);
            if (flagValue != binding.requiredFlagValue)
                return false;
        }

        // questId도 없고 플래그도 없는 완전히 빈 binding은 매칭하지 않음
        // (실수로 빈 binding을 넣었을 때 항상 매칭되는 것을 방지)
        if (!hasQuest && string.IsNullOrEmpty(binding.requiredFlag))
            return false;

        return true;
    }

    private bool IsObjectiveComplete(QuestManager questManager, string questId, string objectiveId)
    {
        var instance = questManager.GetQuestInstance(questId);
        if (instance == null)
            return false;

        return instance.IsObjectiveComplete(objectiveId);
    }

    private void HandleDialogueEnd()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueEnd.RemoveListener(HandleDialogueEnd);

        if (!string.IsNullOrEmpty(npcId))
            OnNPCTalkedTo?.Invoke(npcId);

        if (!string.IsNullOrEmpty(pendingStartQuestId) && QuestManager.Instance != null)
            QuestManager.Instance.StartQuest(pendingStartQuestId);

        pendingStartQuestId = null;
    }

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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}