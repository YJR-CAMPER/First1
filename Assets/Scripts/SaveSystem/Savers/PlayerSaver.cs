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

        SaveManager.Register(this);
    }

    private void OnDestroy()
    {
        SaveManager.Unregister(this);
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
        // 맵은 SaveManager가 선행 로드를 완료한 상태. 여기선 위치/방향만 적용.
        Vector3 pos = new Vector3(
            saveData.playerData.positionX,
            saveData.playerData.positionY,
            transform.position.z
        );
        transform.position = pos;

        if (movementController != null)
        {
            Vector2 facing = IntToFacing(saveData.playerData.facingDirection);
            movementController.SetFacingDirection(facing);
        }
    }

    private int FacingToInt(Vector2 facing)
    {
        if (facing.y > 0.5f) return 3;
        if (facing.y < -0.5f) return 0;
        if (facing.x < -0.5f) return 1;
        if (facing.x > 0.5f) return 2;
        return 0;
    }

    private Vector2 IntToFacing(int dir)
    {
        switch (dir)
        {
            case 1: return Vector2.left;
            case 2: return Vector2.right;
            case 3: return Vector2.up;
            default: return Vector2.down;
        }
    }
}
