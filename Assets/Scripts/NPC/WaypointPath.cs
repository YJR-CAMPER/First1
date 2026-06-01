using UnityEngine;

/// <summary>
/// NPC 순찰 경로 정의
/// Inspector에서 웨이포인트 배열을 설정
/// 기즈모로 경로를 시각화
/// </summary>
public class WaypointPath : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool loop = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 0.8f, 1f, 0.5f);

    public int Count => waypoints != null ? waypoints.Length : 0;
    public bool Loop => loop;

    public Vector2 GetPosition(int index)
    {
        if (waypoints == null || waypoints.Length == 0)
            return transform.position;

        index = Mathf.Clamp(index, 0, waypoints.Length - 1);

        return waypoints[index] != null
            ? (Vector2)waypoints[index].position
            : (Vector2)transform.position;
    }

    public int GetNextIndex(int currentIndex)
    {
        if (waypoints == null || waypoints.Length == 0)
            return 0;

        int next = currentIndex + 1;

        if (next >= waypoints.Length)
        {
            return loop ? 0 : waypoints.Length - 1;
        }

        return next;
    }

    public bool IsLastWaypoint(int index)
    {
        if (waypoints == null || waypoints.Length == 0)
            return true;

        return !loop && index >= waypoints.Length - 1;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Gizmos.color = gizmoColor;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
                continue;

            Gizmos.DrawSphere(waypoints[i].position, 0.15f);

            int nextIndex = (i + 1) % waypoints.Length;
            if (nextIndex == 0 && !loop)
                continue;

            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }
        }
    }
}
