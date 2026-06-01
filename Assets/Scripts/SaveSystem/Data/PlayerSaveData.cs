using System;

/// <summary>
/// 플레이어 상태 저장 데이터
/// </summary>
[Serializable]
public class PlayerSaveData
{
    public float positionX;
    public float positionY;
    public string currentMapId;
    public string lastSpawnPointId;
    public int facingDirection;

    public PlayerSaveData()
    {
        positionX = 0f;
        positionY = 0f;
        currentMapId = "";
        lastSpawnPointId = "";
        facingDirection = 0;
    }
}
