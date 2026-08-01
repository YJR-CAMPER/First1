using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 개별 대화 노드 데이터
/// 초상화는 스프라이트 직접 지정 대신 characterId + emotion으로 조회
/// </summary>
[System.Serializable]
public class DialogueNodeExtended
{
    public string id;
    public string speakerName;

    public DialogueStyleType dialogueStyle = DialogueStyleType.일반;

    [Header("Portrait")]
    [Tooltip("말하는 캐릭터 ID (CharacterRegistry에서 조회)")]
    public string characterId;

    [Tooltip("표정")]
    public EmotionType emotion = EmotionType.기본;

    [Tooltip("초상화 표시 슬롯")]
    public PortraitSlot slot = PortraitSlot.좌;

    [Tooltip("이 슬롯 초상화에 재생할 연출 효과")]
    public PortraitEffect effect = PortraitEffect.없음;

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
