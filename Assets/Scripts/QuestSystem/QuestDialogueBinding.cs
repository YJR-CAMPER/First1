using UnityEngine;

/// <summary>
/// 퀘스트 상태(및 목표/플래그 조건)와 대화 ID를 묶는 데이터
/// QuestDialogueTrigger의 Inspector에서 리스트로 설정
/// 위에서부터 첫 매칭을 사용하므로 순서가 우선순위가 된다
/// </summary>
[System.Serializable]
public class QuestDialogueBinding
{
    [Tooltip("이 대화가 매칭될 퀘스트 ID")]
    public string questId;

    [Tooltip("매칭할 퀘스트 상태")]
    public QuestState requiredState = QuestState.NotStarted;

    [Header("목표(Objective) 단계 조건 (선택)")]
    [Tooltip("이 목표가 '완료' 상태여야 매칭. 비워두면 무시")]
    public string requiredCompletedObjective;

    [Tooltip("이 목표가 '미완료' 상태여야 매칭. 비워두면 무시")]
    public string requiredIncompleteObjective;

    [Header("플래그 조건 (선택)")]
    [Tooltip("이 플래그가 아래 값과 같아야 매칭. 비워두면 무시")]
    public string requiredFlag;

    [Tooltip("requiredFlag가 이 값과 같아야 매칭")]
    public bool requiredFlagValue = true;

    [Header("대화")]
    [Tooltip("이 조건에서 재생할 대화 ID")]
    public string dialogueId;

    [Tooltip("이 대화를 보면 시작시킬 퀘스트 ID (수주 권유용, 선택)")]
    public string startsQuestId;
}
