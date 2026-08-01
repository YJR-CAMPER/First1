using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// characterId로 CharacterData를 조회하는 레지스트리 (ScriptableObject)
/// DialogueManager가 참조하여 노드의 characterId를 실제 캐릭터로 해석
/// </summary>
[CreateAssetMenu(fileName = "CharacterRegistry", menuName = "Game/Dialogue/Character Registry")]
public class CharacterRegistry : ScriptableObject
{
    [SerializeField] private List<CharacterData> characters = new List<CharacterData>();

    private Dictionary<string, CharacterData> lookup;

    public CharacterData GetById(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
            return null;

        BuildLookupIfNeeded();

        return lookup.TryGetValue(characterId, out var data) ? data : null;
    }

    public IReadOnlyList<CharacterData> AllCharacters => characters;

    private void BuildLookupIfNeeded()
    {
        if (lookup != null)
            return;

        lookup = new Dictionary<string, CharacterData>();
        foreach (var character in characters)
        {
            if (character == null || string.IsNullOrEmpty(character.characterId))
                continue;

            if (!lookup.ContainsKey(character.characterId))
                lookup[character.characterId] = character;
            else
                Debug.LogWarning($"[CharacterRegistry] Duplicate characterId: {character.characterId}");
        }
    }

    public void InvalidateLookup()
    {
        lookup = null;
    }
}
