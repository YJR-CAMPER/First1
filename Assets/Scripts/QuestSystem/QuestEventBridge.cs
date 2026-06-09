using UnityEngine;

/// <summary>
/// DialogueManager.OnDialogueEvent를 QuestManager 명령으로 변환하는 브릿지
/// 
/// 이벤트 문자열 규약:
///   "quest_start:{questId}"              - 퀘스트 시작
///   "quest_complete:{questId}"           - 퀘스트 강제 완료
///   "quest_fail:{questId}"              - 퀘스트 실패
///   "quest_progress:{questId}:{objId}:{amount}" - 목표 진행 (Collect 등)
///   "flag_set:{flagName}"               - 스토리 플래그 설정
///   "flag_clear:{flagName}"             - 스토리 플래그 해제
/// 
/// Talk/GoTo 목표는 자동 매칭이므로 이벤트 문자열 불필요.
/// </summary>
public class QuestEventBridge : MonoBehaviour
{
    private const char SEPARATOR = ':';

    /// <summary>
    /// DialogueManager.OnDialogueEvent에 연결할 public 메서드
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

            case "quest_fail":
                HandleQuestFail(parts);
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

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StartQuest(parts[1]);
        }
    }

    private void HandleQuestComplete(string[] parts)
    {
        if (parts.Length < 2) return;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ForceCompleteQuest(parts[1]);
        }
    }

    private void HandleQuestFail(string[] parts)
    {
        if (parts.Length < 2) return;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.FailQuest(parts[1]);
        }
    }

    private void HandleObjectiveProgress(string[] parts)
    {
        if (parts.Length < 4) return;

        if (!int.TryParse(parts[3], out int amount))
            amount = 1;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ReportEvent(ObjectiveType.Custom, parts[2], amount);
        }
    }

    private void HandleFlagSet(string[] parts, bool value)
    {
        if (parts.Length < 2) return;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SetStoryFlag(parts[1], value);
        }
    }
}
