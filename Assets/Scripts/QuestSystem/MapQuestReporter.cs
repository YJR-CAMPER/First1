using UnityEngine;
using MapSystem.Core;

/// <summary>
/// 맵 전환 완료 시 QuestManager에 GoTo 이벤트를 보고
/// MapManager와 같은 오브젝트 또는 별도 오브젝트에 부착
/// 
/// QuestData의 GoTo 목표에서 targetId를 MapData.mapId와 일치시키면 된다.
/// 예: targetId = "cave_entrance" / MapData.mapId = "cave_entrance"
/// </summary>
public class MapQuestReporter : MonoBehaviour
{
    private void OnEnable()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnMapTransitionComplete += HandleMapLoaded;
        }
    }

    private void OnDisable()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnMapTransitionComplete -= HandleMapLoaded;
        }
    }

    private void HandleMapLoaded(MapData mapData)
    {
        if (mapData == null)
            return;

        if (QuestManager.Instance == null)
            return;

        QuestManager.Instance.ReportEvent(ObjectiveType.GoTo, mapData.mapId);
    }
}
