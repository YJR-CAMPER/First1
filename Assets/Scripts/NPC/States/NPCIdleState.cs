using UnityEngine;

/// <summary>
/// NPC가 제자리에서 대기하는 상태
/// 설정된 시간 후 Patrol로 전환
/// </summary>
public class NPCIdleState : INPCState
{
    private float waitTimer;

    public void Enter(NPCController npc)
    {
        waitTimer = npc.IdleWaitTime;
        npc.SetVelocity(Vector2.zero);
    }

    public void Update(NPCController npc)
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f && npc.HasWaypoints)
        {
            npc.TransitionTo(npc.PatrolState);
        }
    }

    public void Exit(NPCController npc)
    {
    }
}
