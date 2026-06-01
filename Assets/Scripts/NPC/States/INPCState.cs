/// <summary>
/// NPC 상태 머신의 개별 상태 인터페이스
/// </summary>
public interface INPCState
{
    void Enter(NPCController npc);
    void Update(NPCController npc);
    void Exit(NPCController npc);
}
