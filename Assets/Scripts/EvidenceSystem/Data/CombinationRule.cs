using UnityEngine;

/// <summary>
/// 카드 조합 규칙 (ScriptableObject)
/// 특정 hashId 카드를 requiredCount만큼 모으면 resultCard로 교환
/// </summary>
[CreateAssetMenu(fileName = "NewCombinationRule", menuName = "Game/Evidence System/Combination Rule")]
public class CombinationRule : ScriptableObject
{
    [Header("조합 조건")]
    [Tooltip("조합에 필요한 카드의 hashId")]
    public string requiredHashId;

    [Tooltip("필요한 카드 장수 (기본 3, 규칙마다 다르게 설정 가능)")]
    [Min(2)]
    public int requiredCount = 3;

    [Header("결과물")]
    [Tooltip("조합 성공 시 획득하는 카드")]
    public EvidenceCard resultCard;
}
