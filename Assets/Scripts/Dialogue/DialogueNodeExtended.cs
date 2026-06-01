using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 개별 대화 노드 데이터
/// </summary>
[System.Serializable]
public class DialogueNodeExtended
{
    public string id;
    public string speakerName;

    public DialogueStyleType dialogueStyle = DialogueStyleType.일반;

    public Sprite portrait;
    public SpeakerPosition position;

    [TextArea(3, 10)]
    public string dialogueText;

    public string nextDialogueId;

    public string eventName;

    public List<DialogueChoiceExtended> choices = new List<DialogueChoiceExtended>();

    /// <summary>
    /// GraphView 에디터에서 노드 위치를 저장하기 위한 필드
    /// 런타임에서는 사용하지 않음
    /// </summary>
    [HideInInspector]
    public Vector2 nodePosition;
}
