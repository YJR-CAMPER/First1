using UnityEngine;

/// <summary>
/// 플레이어 오브젝트에 부착하여 위치/방향을 저장/복원
/// </summary>
public class PlayerSaver : MonoBehaviour, ISaveable
{
    [SerializeField] private TopDownMovementController movementController;

    public string SaveId => "player";

    private void Awake()
    {
        if (movementController == null)
            movementController = GetComponent<TopDownMovementController>();
    }

    public void CaptureState(SaveData saveData)
    {
        saveData.playerData.positionX = transform.position.x;
        saveData.playerData.positionY = transform.position.y;

        if (movementController != null)
        {
            Vector2 facing = movementController.GetFacingDirection();
            saveData.playerData.facingDirection = FacingToInt(facing);
        }

        var mapManager = MapSystem.Core.MapManager.Instance;
        if (mapManager != null && mapManager.CurrentMap != null)
        {
            saveData.playerData.currentMapId = mapManager.CurrentMap.mapId;
        }
    }

    public void RestoreState(SaveData saveData)
    {
        Vector3 pos = new Vector3(
            saveData.playerData.positionX,
            saveData.playerData.positionY,
            transform.position.z
        );
        transform.position = pos;
    }

    private int FacingToInt(Vector2 facing)
    {
        if (facing.y > 0.5f) return 3;
        if (facing.y < -0.5f) return 0;
        if (facing.x < -0.5f) return 1;
        if (facing.x > 0.5f) return 2;
        return 0;
    }
}
