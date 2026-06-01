using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 정의 데이터 (ScriptableObject)
/// 각 퀘스트당 하나의 에셋으로 관리
/// </summary>
[CreateAssetMenu(fileName = "NewQuest", menuName = "Game/Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("퀘스트 고유 ID")]
    public string questId;

    [Tooltip("퀘스트 이름 (UI 표시용)")]
    public string questName;

    [TextArea(3, 8)]
    [Tooltip("퀘스트 설명")]
    public string description;

    [Header("Objectives")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Prerequisites")]
    [Tooltip("이 퀘스트를 받기 위해 완료해야 하는 퀘스트 ID 목록")]
    public List<string> prerequisiteQuestIds = new List<string>();

    [Tooltip("이 퀘스트를 받기 위해 필요한 스토리 플래그")]
    public List<string> prerequisiteFlags = new List<string>();

    [Header("Rewards")]
    [Tooltip("완료 시 설정할 스토리 플래그")]
    public string completionFlag;

    [Tooltip("완료 시 시작할 후속 대화 ID")]
    public string completionDialogueId;

    [Header("Settings")]
    [Tooltip("자동 완료 (모든 목표 달성 시 즉시 완료 처리)")]
    public bool autoComplete = true;

    [Tooltip("반복 가능 여부")]
    public bool repeatable = false;
}
