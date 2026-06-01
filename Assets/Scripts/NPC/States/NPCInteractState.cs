using UnityEngine;

/// <summary>
/// NPC가 플레이어와 상호작용 중인 상태
/// 대화 종료 시 이전 상태로 복귀
/// </summary>
public class NPCInteractState : INPCState
{
    public void Enter(NPCController npc)
    {
        npc.SetVelocity(Vector2.zero);
        FacePlayer(npc);
    }

    public void Update(NPCController npc)
    {
        if (!IsDialogueActive())
        {
            npc.TransitionTo(npc.IdleState);
        }
    }

    public void Exit(NPCController npc)
    {
    }

    private void FacePlayer(NPCController npc)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        Vector2 direction = (Vector2)player.transform.position - npc.Position;
        npc.SetFacingDirection(direction.normalized);
    }

    private bool IsDialogueActive()
    {
        return DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive();
    }
}
