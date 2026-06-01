using UnityEngine;

/// <summary>
/// 퀘스트 개별 목표 정의
/// QuestData 내에 리스트로 포함
/// </summary>
[System.Serializable]
public class QuestObjective
{
    [Tooltip("목표 고유 ID")]
    public string objectiveId;

    [Tooltip("목표 유형")]
    public ObjectiveType type;

    [Tooltip("UI에 표시할 목표 설명")]
    public string description;

    [Tooltip("대상 ID (아이템 ID, NPC ID, 맵 ID 등)")]
    public string targetId;

    [Tooltip("필요 수량 (Collect, Kill 등)")]
    public int requiredAmount = 1;

    [Tooltip("순서 강제 여부 (true면 이전 목표 완료 후에만 진행)")]
    public bool requirePreviousComplete = false;
}
