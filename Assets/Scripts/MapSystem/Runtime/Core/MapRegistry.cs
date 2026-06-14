using System.Collections.Generic;
using UnityEngine;

namespace MapSystem.Core
{
    /// <summary>
    /// mapId 문자열로 MapData를 조회하는 레지스트리
    /// 세이브/로드 시 저장된 mapId로 맵을 복원하기 위한 룩업 테이블
    /// </summary>
    [CreateAssetMenu(fileName = "MapRegistry", menuName = "MapSystem/Map Registry")]
    public class MapRegistry : ScriptableObject
    {
        [SerializeField] private List<MapData> maps = new List<MapData>();

        private Dictionary<string, MapData> lookup;

        public MapData GetById(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
                return null;

            EnsureLookup();
            return lookup.TryGetValue(mapId, out var data) ? data : null;
        }

        private void EnsureLookup()
        {
            if (lookup != null)
                return;

            lookup = new Dictionary<string, MapData>();
            foreach (var map in maps)
            {
                if (map == null || string.IsNullOrEmpty(map.mapId))
                    continue;

                if (!lookup.ContainsKey(map.mapId))
                    lookup[map.mapId] = map;
                else
                    Debug.LogWarning($"[MapRegistry] Duplicate mapId: {map.mapId}");
            }
        }
    }
}
