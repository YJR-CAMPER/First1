using UnityEngine;
using System.Collections.Generic;

public enum DialogueStyleType
{
    일반,
    소리침,
    생각
}

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

[System.Serializable]
public class DialogueChoiceExtended
{
    public string choiceText;
    public string nextDialogueId;
    public string eventName;
    public string conditionFlag;
    public bool requiredValue = true;
}

[System.Serializable]
public class DialogueStyle
{
    public DialogueStyleType styleType;
    public Sprite panelSprite;
    public Color textColor = Color.black;
    public Color speakerNameColor = Color.black;
    public bool enableShake = false;
    public float shakeIntensity = 5f;
    public float panelScale = 1.0f;
}
