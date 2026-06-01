using UnityEngine;

/// <summary>
/// DialogueManager.OnDialogueEvent를 QuestManager 명령으로 변환하는 브릿지
/// 
/// 이벤트 문자열 규약:
///   "quest_start:{questId}"              - 퀘스트 시작
///   "quest_complete:{questId}"           - 퀘스트 강제 완료
///   "quest_objective:{questId}:{objId}"  - 목표 완료 (Talk, GoTo 등)
///   "quest_progress:{questId}:{objId}:{amount}" - 목표 진행 (Collect 등)
///   "flag_set:{flagName}"               - 스토리 플래그 설정
///   "flag_clear:{flagName}"             - 스토리 플래그 해제
/// 
/// Setup:
///   DialogueManager의 OnDialogueEvent에 HandleDialogueEvent를 연결
/// </summary>
public class QuestEventBridge : MonoBehaviour
{
    private const char SEPARATOR = ':';

    /// <summary>
    /// DialogueManager.OnDialogueEvent에 연결할 public 메서드
    /// Inspector에서 UnityEvent 리스너로 할당
    /// </summary>
    public void HandleDialogueEvent(string eventString)
    {
        if (string.IsNullOrEmpty(eventString))
            return;

        string[] parts = eventString.Split(SEPARATOR);
        if (parts.Length < 2)
            return;

        string command = parts[0];

        switch (command)
        {
            case "quest_start":
                HandleQuestStart(parts);
                break;

            case "quest_complete":
                HandleQuestComplete(parts);
                break;

            case "quest_objective":
                HandleObjectiveComplete(parts);
                break;

            case "quest_progress":
                HandleObjectiveProgress(parts);
                break;

            case "flag_set":
                HandleFlagSet(parts, true);
                break;

            case "flag_clear":
                HandleFlagSet(parts, false);
                break;
        }
    }

    private void HandleQuestStart(string[] parts)
    {
        if (parts.Length < 2) return;

        string questId = parts[1];
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StartQuest(questId);
        }
    }

    private void HandleQuestComplete(string[] parts)
    {
        if (parts.Length < 2) return;

        string questId = parts[1];
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ForceCompleteQuest(questId);
        }
    }

    private void HandleObjectiveComplete(string[] parts)
    {
        if (parts.Length < 3) return;

        string questId = parts[1];
        string objectiveId = parts[2];

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteObjective(questId, objectiveId);
        }
    }

    private void HandleObjectiveProgress(string[] parts)
    {
        if (parts.Length < 4) return;

        string questId = parts[1];
        string objectiveId = parts[2];

        if (!int.TryParse(parts[3], out int amount))
            amount = 1;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ProgressObjective(questId, objectiveId, amount);
        }
    }

    private void HandleFlagSet(string[] parts, bool value)
    {
        if (parts.Length < 2) return;

        string flagName = parts[1];

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CurrentSaveData.questData.SetStoryFlag(flagName, value);
        }
    }
}
