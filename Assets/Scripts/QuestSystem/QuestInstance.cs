using System.Collections.Generic;

/// <summary>
/// 런타임에서 개별 퀘스트의 진행 상태를 추적
/// QuestData(정의)와 분리된 런타임 인스턴스
/// </summary>
public class QuestInstance
{
    public QuestData Data { get; private set; }
    public QuestState State { get; private set; }

    private readonly Dictionary<string, int> objectiveProgress;
    private readonly HashSet<string> completedObjectives;

    public QuestInstance(QuestData data)
    {
        Data = data;
        State = QuestState.NotStarted;
        objectiveProgress = new Dictionary<string, int>();
        completedObjectives = new HashSet<string>();

        foreach (var obj in data.objectives)
        {
            objectiveProgress[obj.objectiveId] = 0;
        }
    }

    public void Activate()
    {
        if (State == QuestState.NotStarted)
        {
            State = QuestState.Active;
        }
    }

    /// <summary>
    /// 목표 진행도 증가. 완료 시 true 반환.
    /// </summary>
    public bool ProgressObjective(string objectiveId, int amount = 1)
    {
        if (State != QuestState.Active)
            return false;

        if (completedObjectives.Contains(objectiveId))
            return false;

        var objective = FindObjective(objectiveId);
        if (objective == null)
            return false;

        if (objective.requirePreviousComplete && !IsPreviousObjectiveComplete(objectiveId))
            return false;

        if (!objectiveProgress.ContainsKey(objectiveId))
            objectiveProgress[objectiveId] = 0;

        objectiveProgress[objectiveId] += amount;

        if (objectiveProgress[objectiveId] >= objective.requiredAmount)
        {
            objectiveProgress[objectiveId] = objective.requiredAmount;
            completedObjectives.Add(objectiveId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 특정 목표 즉시 완료 처리 (Talk, GoTo 등 1회성)
    /// </summary>
    public bool CompleteObjective(string objectiveId)
    {
        if (State != QuestState.Active)
            return false;

        if (completedObjectives.Contains(objectiveId))
            return false;

        var objective = FindObjective(objectiveId);
        if (objective == null)
            return false;

        objectiveProgress[objectiveId] = objective.requiredAmount;
        completedObjectives.Add(objectiveId);
        return true;
    }

    public bool AreAllObjectivesComplete()
    {
        foreach (var obj in Data.objectives)
        {
            if (!completedObjectives.Contains(obj.objectiveId))
                return false;
        }

        return true;
    }

    public void Complete()
    {
        State = QuestState.Completed;
    }

    public void Fail()
    {
        State = QuestState.Failed;
    }

    public int GetProgress(string objectiveId)
    {
        return objectiveProgress.TryGetValue(objectiveId, out int value) ? value : 0;
    }

    public bool IsObjectiveComplete(string objectiveId)
    {
        return completedObjectives.Contains(objectiveId);
    }

    // -- Save/Load Support --

    public QuestProgressEntry ToSaveEntry()
    {
        var entry = new QuestProgressEntry
        {
            questId = Data.questId,
            state = (int)State,
            objectiveProgress = new System.Collections.Generic.List<ObjectiveProgressEntry>()
        };

        foreach (var obj in Data.objectives)
        {
            entry.objectiveProgress.Add(new ObjectiveProgressEntry
            {
                objectiveId = obj.objectiveId,
                currentAmount = GetProgress(obj.objectiveId),
                isComplete = IsObjectiveComplete(obj.objectiveId)
            });
        }

        return entry;
    }

    public void RestoreFromSaveEntry(QuestProgressEntry entry)
    {
        State = (QuestState)entry.state;

        if (entry.objectiveProgress == null)
            return;

        foreach (var objEntry in entry.objectiveProgress)
        {
            objectiveProgress[objEntry.objectiveId] = objEntry.currentAmount;

            if (objEntry.isComplete)
            {
                completedObjectives.Add(objEntry.objectiveId);
            }
        }
    }

    // -- Event Matching --

    /// <summary>
    /// 외부 이벤트(type + targetId)와 일치하는 목표를 찾아 진행
    /// 매칭된 목표 ID 목록을 반환 (빈 리스트 = 매칭 없음)
    /// </summary>
    public List<string> TryMatchEvent(ObjectiveType type, string targetId, int amount)
    {
        var matched = new List<string>();

        if (State != QuestState.Active)
            return matched;

        foreach (var obj in Data.objectives)
        {
            if (completedObjectives.Contains(obj.objectiveId))
                continue;

            if (obj.type != type)
                continue;

            if (!string.Equals(obj.targetId, targetId, System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (obj.requirePreviousComplete && !IsPreviousObjectiveComplete(obj.objectiveId))
                continue;

            bool justCompleted = ProgressObjective(obj.objectiveId, amount);
            matched.Add(obj.objectiveId);
        }

        return matched;
    }

    // -- Private --

    private QuestObjective FindObjective(string objectiveId)
    {
        foreach (var obj in Data.objectives)
        {
            if (obj.objectiveId == objectiveId)
                return obj;
        }

        return null;
    }

    private bool IsPreviousObjectiveComplete(string objectiveId)
    {
        for (int i = 0; i < Data.objectives.Count; i++)
        {
            if (Data.objectives[i].objectiveId == objectiveId)
            {
                return i == 0 || completedObjectives.Contains(Data.objectives[i - 1].objectiveId);
            }
        }

        return true;
    }
}
