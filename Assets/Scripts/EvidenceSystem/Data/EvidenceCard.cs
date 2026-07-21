using UnityEngine;

/// <summary>
/// 증거 카드 정의 (ScriptableObject)
/// 조합 판정은 hashId만 사용하며, 나머지 필드는 표시용
/// </summary>
[CreateAssetMenu(fileName = "NewEvidenceCard", menuName = "Game/Evidence System/Evidence Card")]
public class EvidenceCard : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("카드 고유 ID")]
    public string cardId;

    [Header("조합 판정 (유일 기준)")]
    [Tooltip("이 값이 같은 카드끼리만 조합 가능. 색깔/번지수와 무관하게 독립적으로 설정")]
    public string hashId;

    [Header("등급")]
    public CardTier tier = CardTier.Low;

    [Header("소모 설정")]
    [Tooltip("체크 해제 시 조합 재료로 사용되어도 소모되지 않고 인벤토리에 남는다 (핵심 증거 등)")]
    public bool isRemovable = true;

    [Header("표시용 정보 (조합 판정에 미사용)")]
    public CardColor cardColor = CardColor.Red;

    [Tooltip("획득한 지역")]
    public string region;

    [Tooltip("획득한 번지수")]
    public int caseNumber;

    public string title;

    [TextArea(3, 6)]
    public string content;

    [Header("Visual")]
    public Sprite cardSprite;
}
