using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카드 조합 실행 오케스트레이터
/// 인벤토리 보유, 규칙 판정(Validator), 조합 실행을 조율
/// </summary>
public class CardCombiner : MonoBehaviour
{
    [Header("Combination Rules")]
    [SerializeField] private List<CombinationRule> allRules = new List<CombinationRule>();

    /// <summary>
    /// 조합 성공 시 발생 (사용된 규칙, 획득한 결과 카드)
    /// </summary>
    public event Action<CombinationRule, EvidenceCard> OnCombinationSuccess;

    /// <summary>
    /// 조합 실패 시 발생
    /// </summary>
    public event Action<CombinationRule> OnCombinationFailed;

    private readonly EvidenceInventory inventory = new EvidenceInventory();

    public EvidenceInventory Inventory => inventory;
    public IReadOnlyList<CombinationRule> AllRules => allRules;

    // -- Combination --

    /// <summary>
    /// 지정한 규칙으로 조합을 시도
    /// 성공 시 소모 가능한 재료 카드를 제거하고 결과 카드를 지급
    /// </summary>
    public bool TryCombine(CombinationRule rule)
    {
        if (!CombinationValidator.CanCombine(inventory, rule))
        {
            OnCombinationFailed?.Invoke(rule);
            return false;
        }

        if (!ConsumeMaterials(rule.requiredHashId, rule.requiredCount))
        {
            OnCombinationFailed?.Invoke(rule);
            return false;
        }

        inventory.AddCard(rule.resultCard, 1);
        OnCombinationSuccess?.Invoke(rule, rule.resultCard);
        return true;
    }

    /// <summary>
    /// 현재 조합 가능한 모든 규칙 조회
    /// </summary>
    public List<CombinationRule> GetAvailableCombinations()
    {
        return CombinationValidator.GetAvailableCombinations(inventory, allRules);
    }

    // -- Material Consumption --

    /// <summary>
    /// hashId가 일치하는 카드를 count만큼 재료로 사용
    /// isRemovable이 false인 카드는 재료 조건을 충족시키되 소모되지 않는다
    /// 소모 가능한 카드를 먼저 제거하여, 보존 카드가 불필요하게 소진되는 것을 막는다
    /// </summary>
    private bool ConsumeMaterials(string hashId, int count)
    {
        var matching = inventory.GetCardsByHashId(hashId);

        int available = 0;
        foreach (var kvp in matching)
            available += kvp.Value;

        if (available < count)
            return false;

        var removable = new List<KeyValuePair<EvidenceCard, int>>();
        var preserved = new List<KeyValuePair<EvidenceCard, int>>();

        foreach (var kvp in matching)
        {
            if (kvp.Key.isRemovable)
                removable.Add(kvp);
            else
                preserved.Add(kvp);
        }

        int remaining = count;

        // 1단계: 소모 가능한 카드를 실제로 제거
        foreach (var kvp in removable)
        {
            if (remaining <= 0)
                break;

            int take = Mathf.Min(kvp.Value, remaining);
            inventory.RemoveCard(kvp.Key, take);
            remaining -= take;
        }

        // 2단계: 남은 요구량을 보존 카드로 충족 (제거하지 않음)
        foreach (var kvp in preserved)
        {
            if (remaining <= 0)
                break;

            int contribute = Mathf.Min(kvp.Value, remaining);
            remaining -= contribute;
        }

        return remaining == 0;
    }
}
