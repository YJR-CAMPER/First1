using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 상태 추적 및 진행 관리
/// 대화 시스템, 인벤토리 등 외부 시스템은 이벤트로만 연결
/// </summary>
public class QuestManager : SingletonMonoBehaviour<QuestManager>, ISaveable
{
    [Header("Quest Database")]
    [SerializeField] private List<QuestData> allQuests = new List<QuestData>();

    /// <summary>
    /// 퀘스트 상태 변경 시 발생 (questId, newState)
    /// </summary>
    public event Action<string, QuestState> OnQuestStateChanged;

    /// <summary>
    /// 목표 진행 시 발생 (questId, objectiveId, currentAmount)
    /// </summary>
    public event Action<string, string, int> OnObjectiveProgressed;

    /// <summary>
    /// 퀘스트 완료 시 발생 (questId)
    /// </summary>
    public event Action<string> OnQuestCompleted;

    private readonly Dictionary<string, QuestData> questLookup = new Dictionary<string, QuestData>();
    private readonly Dictionary<string, QuestInstance> activeInstances = new Dictionary<string, QuestInstance>();

    public string SaveId => "quest_manager";

    protected override bool Persist => true;

    protected override void OnSingletonAwake()
    {
        BuildLookup();
    }

    // -- Public API --

    /// <summary>
    /// 퀘스트 시작 (NotStarted -> Active)
    /// </summary>
    public bool StartQuest(string questId)
    {
        var data = GetQuestData(questId);
        if (data == null)
            return false;

        if (activeInstances.ContainsKey(questId))
        {
            var existing = activeInstances[questId];
            if (existing.State == QuestState.Completed && !data.repeatable)
                return false;

            if (existing.State == QuestState.Active)
                return false;
        }

        if (!CheckPrerequisites(data))
            return false;

        var instance = new QuestInstance(data);
        instance.Activate();
        activeInstances[questId] = instance;

        OnQuestStateChanged?.Invoke(questId, QuestState.Active);
        return true;
    }

    /// <summary>
    /// 목표 진행 (수량 기반: Collect, Kill 등)
    /// </summary>
    public void ProgressObjective(string questId, string objectiveId, int amount = 1)
    {
        if (!activeInstances.TryGetValue(questId, out var instance))
            return;

        bool justCompleted = instance.ProgressObjective(objectiveId, amount);
        int current = instance.GetProgress(objectiveId);
        OnObjectiveProgressed?.Invoke(questId, objectiveId, current);

        if (justCompleted)
        {
            TryAutoComplete(instance);
        }
    }

    /// <summary>
    /// 목표 즉시 완료 (Talk, GoTo 등 1회성)
    /// </summary>
    public void CompleteObjective(string questId, string objectiveId)
    {
        if (!activeInstances.TryGetValue(questId, out var instance))
            return;

        bool justCompleted = instance.CompleteObjective(objectiveId);

        if (justCompleted)
        {
            OnObjectiveProgressed?.Invoke(questId, objectiveId, instance.GetProgress(objectiveId));
            TryAutoComplete(instance);
        }
    }

    /// <summary>
    /// 퀘스트 강제 완료
    /// </summary>
    public void ForceCompleteQuest(string questId)
    {
        if (!activeInstances.TryGetValue(questId, out var instance))
            return;

        CompleteQuest(instance);
    }

    /// <summary>
    /// 퀘스트 실패 처리
    /// </summary>
    public void FailQuest(string questId)
    {
        if (!activeInstances.TryGetValue(questId, out var instance))
            return;

        instance.Fail();
        OnQuestStateChanged?.Invoke(questId, QuestState.Failed);
    }

    // -- Event Reporting --

    /// <summary>
    /// 게임 내 행동 발생 시 호출. 모든 활성 퀘스트의 목표와 자동 대조.
    /// 
    /// 사용 예시:
    ///   NPC 대화 완료:  ReportEvent(ObjectiveType.Talk, "npc_herbalist")
    ///   맵 도착:        ReportEvent(ObjectiveType.GoTo, "cave_entrance")
    ///   아이템 획득:    ReportEvent(ObjectiveType.Collect, "herb_red", 1)
    ///   아이템 전달:    ReportEvent(ObjectiveType.Deliver, "npc_herbalist:herb_bundle")
    ///   적 처치:        ReportEvent(ObjectiveType.Kill, "slime", 1)
    ///   커스텀:         ReportEvent(ObjectiveType.Custom, "lever_pulled")
    /// </summary>
    public void ReportEvent(ObjectiveType type, string targetId, int amount = 1)
    {
        if (string.IsNullOrEmpty(targetId))
            return;

        foreach (var kvp in activeInstances)
        {
            var instance = kvp.Value;
            if (instance.State != QuestState.Active)
                continue;

            var matched = instance.TryMatchEvent(type, targetId, amount);

            foreach (var objectiveId in matched)
            {
                OnObjectiveProgressed?.Invoke(kvp.Key, objectiveId, instance.GetProgress(objectiveId));
            }

            if (matched.Count > 0)
            {
                TryAutoComplete(instance);
            }
        }
    }

    // -- Query --

    public QuestState GetQuestState(string questId)
    {
        if (activeInstances.TryGetValue(questId, out var instance))
            return instance.State;

        return QuestState.NotStarted;
    }

    public QuestInstance GetQuestInstance(string questId)
    {
        return activeInstances.TryGetValue(questId, out var instance) ? instance : null;
    }

    public bool IsQuestActive(string questId)
    {
        return GetQuestState(questId) == QuestState.Active;
    }

    public bool IsQuestCompleted(string questId)
    {
        return GetQuestState(questId) == QuestState.Completed;
    }

    public List<QuestInstance> GetActiveQuests()
    {
        var result = new List<QuestInstance>();
        foreach (var kvp in activeInstances)
        {
            if (kvp.Value.State == QuestState.Active)
                result.Add(kvp.Value);
        }

        return result;
    }

    public QuestData GetQuestData(string questId)
    {
        return questLookup.TryGetValue(questId, out var data) ? data : null;
    }

    // -- ISaveable --

    public void CaptureState(SaveData saveData)
    {
        saveData.questData.questEntries.Clear();

        foreach (var kvp in activeInstances)
        {
            saveData.questData.questEntries.Add(kvp.Value.ToSaveEntry());
        }
    }

    public void RestoreState(SaveData saveData)
    {
        activeInstances.Clear();

        foreach (var entry in saveData.questData.questEntries)
        {
            var data = GetQuestData(entry.questId);
            if (data == null)
                continue;

            var instance = new QuestInstance(data);
            instance.RestoreFromSaveEntry(entry);
            activeInstances[entry.questId] = instance;
        }
    }

    // -- Private --

    private void BuildLookup()
    {
        questLookup.Clear();

        foreach (var quest in allQuests)
        {
            if (quest == null)
                continue;

            if (!questLookup.ContainsKey(quest.questId))
            {
                questLookup[quest.questId] = quest;
            }
            else
            {
                Debug.LogWarning($"[QuestManager] Duplicate quest ID: {quest.questId}");
            }
        }
    }

    private bool CheckPrerequisites(QuestData data)
    {
        foreach (var prereqId in data.prerequisiteQuestIds)
        {
            if (!IsQuestCompleted(prereqId))
                return false;
        }

        if (SaveManager.Instance != null)
        {
            foreach (var flag in data.prerequisiteFlags)
            {
                if (!SaveManager.Instance.CurrentSaveData.questData.GetStoryFlag(flag))
                    return false;
            }
        }

        return true;
    }

    private void TryAutoComplete(QuestInstance instance)
    {
        if (!instance.Data.autoComplete)
            return;

        if (instance.AreAllObjectivesComplete())
        {
            CompleteQuest(instance);
        }
    }

    private void CompleteQuest(QuestInstance instance)
    {
        instance.Complete();

        if (!string.IsNullOrEmpty(instance.Data.completionFlag) && SaveManager.Instance != null)
        {
            SaveManager.Instance.CurrentSaveData.questData.SetStoryFlag(instance.Data.completionFlag, true);
        }

        OnQuestStateChanged?.Invoke(instance.Data.questId, QuestState.Completed);
        OnQuestCompleted?.Invoke(instance.Data.questId);
    }
}
