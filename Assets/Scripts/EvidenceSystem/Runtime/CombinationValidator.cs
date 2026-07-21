using System.Collections.Generic;

/// <summary>
/// 조합 규칙 판정 (stateless)
/// 인벤토리 상태와 규칙을 받아 조합 가능 여부만 판정
/// isRemovable 여부와 무관하게, 보유 수량이 요구치를 넘으면 조합 가능으로 본다
/// </summary>
public static class CombinationValidator
{
    /// <summary>
    /// 주어진 규칙으로 조합이 가능한지 판정
    /// </summary>
    public static bool CanCombine(EvidenceInventory inventory, CombinationRule rule)
    {
        if (inventory == null || rule == null)
            return false;

        if (string.IsNullOrEmpty(rule.requiredHashId))
            return false;

        if (rule.resultCard == null)
            return false;

        int owned = inventory.GetCountByHashId(rule.requiredHashId);
        return owned >= rule.requiredCount;
    }

    /// <summary>
    /// 현재 인벤토리로 조합 가능한 모든 규칙을 반환
    /// </summary>
    public static List<CombinationRule> GetAvailableCombinations(
        EvidenceInventory inventory,
        IEnumerable<CombinationRule> allRules)
    {
        var result = new List<CombinationRule>();

        if (inventory == null || allRules == null)
            return result;

        foreach (var rule in allRules)
        {
            if (CanCombine(inventory, rule))
                result.Add(rule);
        }

        return result;
    }
}
