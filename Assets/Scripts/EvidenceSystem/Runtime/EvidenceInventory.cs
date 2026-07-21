using System;
using System.Collections.Generic;

/// <summary>
/// 보유 증거 카드 관리 (런타임)
/// 카드별 보유 수량을 추적
/// </summary>
public class EvidenceInventory
{
    private readonly Dictionary<EvidenceCard, int> cards = new Dictionary<EvidenceCard, int>();

    /// <summary>
    /// 인벤토리 변경 시 발생
    /// </summary>
    public event Action OnInventoryChanged;

    public void AddCard(EvidenceCard card, int amount = 1)
    {
        if (card == null || amount <= 0)
            return;

        if (cards.ContainsKey(card))
            cards[card] += amount;
        else
            cards[card] = amount;

        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 카드 제거. 보유량이 부족하면 false 반환하고 아무것도 제거하지 않음
    /// </summary>
    public bool RemoveCard(EvidenceCard card, int amount = 1)
    {
        if (card == null || amount <= 0)
            return false;

        if (!cards.TryGetValue(card, out int owned) || owned < amount)
            return false;

        cards[card] = owned - amount;
        if (cards[card] <= 0)
            cards.Remove(card);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetCount(EvidenceCard card)
    {
        return card != null && cards.TryGetValue(card, out int count) ? count : 0;
    }

    public bool HasCard(EvidenceCard card)
    {
        return GetCount(card) > 0;
    }

    /// <summary>
    /// 특정 hashId를 가진 카드들의 총 보유량
    /// </summary>
    public int GetCountByHashId(string hashId)
    {
        if (string.IsNullOrEmpty(hashId))
            return 0;

        int total = 0;
        foreach (var kvp in cards)
        {
            if (kvp.Key.hashId == hashId)
                total += kvp.Value;
        }

        return total;
    }

    /// <summary>
    /// 특정 hashId를 가진 카드 목록 (수량 포함)
    /// 반환값은 복사본이므로 순회 중 인벤토리 수정이 안전하다
    /// </summary>
    public List<KeyValuePair<EvidenceCard, int>> GetCardsByHashId(string hashId)
    {
        var result = new List<KeyValuePair<EvidenceCard, int>>();
        if (string.IsNullOrEmpty(hashId))
            return result;

        foreach (var kvp in cards)
        {
            if (kvp.Key.hashId == hashId)
                result.Add(kvp);
        }

        return result;
    }

    public IReadOnlyDictionary<EvidenceCard, int> GetAllCards()
    {
        return cards;
    }

    public void Clear()
    {
        cards.Clear();
        OnInventoryChanged?.Invoke();
    }
}
