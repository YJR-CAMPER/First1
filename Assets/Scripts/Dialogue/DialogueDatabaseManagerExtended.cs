using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 대화 데이터베이스의 로드 및 ID 기반 조회만 담당
/// 대화 시작 로직은 호출자가 직접 DialogueManager에 전달
/// </summary>
public class DialogueDatabaseManagerExtended : MonoBehaviour
{
    public static DialogueDatabaseManagerExtended Instance { get; private set; }

    [SerializeField] private DialogueDatabaseExtended database;

    private Dictionary<string, DialogueNodeExtended> dialogueLookup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        BuildLookup();
    }

    public DialogueNodeExtended GetDialogue(string dialogueId)
    {
        if (dialogueLookup != null && dialogueLookup.TryGetValue(dialogueId, out var dialogue))
        {
            return dialogue;
        }

        Debug.LogWarning($"Dialogue not found: {dialogueId}");
        return null;
    }

    public List<string> GetAllDialogueIds()
    {
        if (dialogueLookup == null)
            return new List<string>();

        return dialogueLookup.Keys.ToList();
    }

    public void ReloadDatabase()
    {
        BuildLookup();
    }

    public DialogueDatabaseExtended GetDatabase()
    {
        return database;
    }

    private void BuildLookup()
    {
        dialogueLookup = new Dictionary<string, DialogueNodeExtended>();

        if (database == null || database.dialogues == null)
            return;

        foreach (var dialogue in database.dialogues)
        {
            if (!dialogueLookup.ContainsKey(dialogue.id))
            {
                dialogueLookup[dialogue.id] = dialogue;
            }
            else
            {
                Debug.LogWarning($"Duplicate dialogue ID: {dialogue.id}");
            }
        }
    }
}
