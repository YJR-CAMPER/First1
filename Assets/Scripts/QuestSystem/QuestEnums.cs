/// <summary>
/// 퀘스트 진행 상태
/// </summary>
public enum QuestState
{
    NotStarted = 0,
    Active = 1,
    Completed = 2,
    Failed = 3
}

/// <summary>
/// 퀘스트 목표 유형
/// </summary>
public enum ObjectiveType
{
    Talk,
    Collect,
    Deliver,
    GoTo,
    Kill,
    Custom
}
