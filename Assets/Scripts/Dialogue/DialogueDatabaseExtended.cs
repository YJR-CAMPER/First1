using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogueDatabase", menuName = "대화 시스템/Dialogue Database")]
public class DialogueDatabaseExtended : ScriptableObject
{
    public List<DialogueNodeExtended> dialogues = new List<DialogueNodeExtended>();
}
