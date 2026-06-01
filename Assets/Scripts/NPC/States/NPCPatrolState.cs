using UnityEngine;

/// <summary>
/// NPC가 웨이포인트를 순회하는 상태
/// 목적지 도달 시 Idle로 전환
/// </summary>
public class NPCPatrolState : INPCState
{
    private const float ARRIVAL_THRESHOLD = 0.1f;

    public void Enter(NPCController npc)
    {
    }

    public void Update(NPCController npc)
    {
        Vector2 target = npc.GetCurrentWaypoint();
        Vector2 current = npc.Position;
        Vector2 direction = target - current;
        float distance = direction.magnitude;

        if (distance <= ARRIVAL_THRESHOLD)
        {
            npc.AdvanceWaypoint();
            npc.TransitionTo(npc.IdleState);
            return;
        }

        Vector2 velocity = direction.normalized * npc.MoveSpeed;
        npc.SetVelocity(velocity);
        npc.SetFacingDirection(direction.normalized);
    }

    public void Exit(NPCController npc)
    {
        npc.SetVelocity(Vector2.zero);
    }
}
