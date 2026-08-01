using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 캐릭터 정의 (ScriptableObject)
/// 캐릭터 하나당 에셋 하나, 감정별 스프라이트를 매핑
/// 대화 노드는 characterId + emotion으로 스프라이트를 조회
/// </summary>
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Dialogue/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("캐릭터 고유 ID (대화 노드에서 참조)")]
    public string characterId;

    [Tooltip("화면에 표시할 이름")]
    public string displayName;

    [Header("Emotions")]
    [Tooltip("표정별 스프라이트. 없는 표정은 기본으로 대체됨")]
    public List<EmotionSprite> emotions = new List<EmotionSprite>();

    private Dictionary<EmotionType, Sprite> lookup;

    /// <summary>
    /// 표정에 해당하는 스프라이트 조회
    /// 해당 표정이 없으면 기본 표정으로, 그것도 없으면 null
    /// </summary>
    public Sprite GetEmotionSprite(EmotionType emotion)
    {
        BuildLookupIfNeeded();

        if (lookup.TryGetValue(emotion, out var sprite))
            return sprite;

        if (lookup.TryGetValue(EmotionType.기본, out var fallback))
            return fallback;

        return null;
    }

    private void BuildLookupIfNeeded()
    {
        if (lookup != null)
            return;

        lookup = new Dictionary<EmotionType, Sprite>();
        foreach (var entry in emotions)
        {
            if (!lookup.ContainsKey(entry.emotion))
                lookup[entry.emotion] = entry.sprite;
        }
    }

    /// <summary>
    /// 에디터에서 emotions 리스트를 수정한 뒤 캐시를 무효화할 때 호출
    /// </summary>
    public void InvalidateLookup()
    {
        lookup = null;
    }
}

/// <summary>
/// 표정 - 스프라이트 쌍
/// </summary>
[System.Serializable]
public class EmotionSprite
{
    public EmotionType emotion;
    public Sprite sprite;
}
