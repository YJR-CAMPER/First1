using System;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 진행 상태 저장 데이터
/// </summary>
[Serializable]
public class QuestSaveData
{
    public List<QuestProgressEntry> questEntries;
    public List<StoryFlagEntry> storyFlags;

    public QuestSaveData()
    {
        questEntries = new List<QuestProgressEntry>();
        storyFlags = new List<StoryFlagEntry>();
    }

    public void SetStoryFlag(string key, bool value)
    {
        for (int i = 0; i < storyFlags.Count; i++)
        {
            if (storyFlags[i].key == key)
            {
                var entry = storyFlags[i];
                entry.value = value;
                storyFlags[i] = entry;
                return;
            }
        }

        storyFlags.Add(new StoryFlagEntry { key = key, value = value });
    }

    public bool GetStoryFlag(string key)
    {
        foreach (var entry in storyFlags)
        {
            if (entry.key == key)
                return entry.value;
        }

        return false;
    }
}

/// <summary>
/// 개별 퀘스트 진행 상태
/// </summary>
[Serializable]
public struct QuestProgressEntry
{
    public string questId;
    public int state;
    public List<ObjectiveProgressEntry> objectiveProgress;
}

/// <summary>
/// 개별 목표 진행 상태
/// </summary>
[Serializable]
public struct ObjectiveProgressEntry
{
    public string objectiveId;
    public int currentAmount;
    public bool isComplete;
}

/// <summary>
/// 스토리 플래그 (key-value)
/// </summary>
[Serializable]
public struct StoryFlagEntry
{
    public string key;
    public bool value;
}
