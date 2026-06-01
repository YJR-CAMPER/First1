using System;

/// <summary>
/// 전체 세이브 데이터 컨테이너
/// JsonUtility로 직렬화/역직렬화
/// </summary>
[Serializable]
public class SaveData
{
    public SaveMetadata metadata;
    public PlayerSaveData playerData;
    public QuestSaveData questData;

    public SaveData()
    {
        metadata = new SaveMetadata();
        playerData = new PlayerSaveData();
        questData = new QuestSaveData();
    }
}

/// <summary>
/// 세이브 슬롯 메타데이터 (UI 표시용)
/// </summary>
[Serializable]
public class SaveMetadata
{
    public string saveName;
    public string timestamp;
    public string sceneName;
    public float playTime;

    public void UpdateTimestamp()
    {
        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
