using UnityEngine;

/// <summary>
/// NPC에 부착하여 ID 기반으로 대화를 시작하는 트리거
/// </summary>
public class DialogueTriggerByID : MonoBehaviour
{
    [SerializeField] private string dialogueId;

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

        DialogueManager.Instance.StartDialogue(dialogue);
        hasTriggered = true;
    }

    public void SetDialogueId(string newDialogueId)
    {
        dialogueId = newDialogueId;
        hasTriggered = false;
    }

    public void ResetDialogue()
    {
        hasTriggered = false;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
