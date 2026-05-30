using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MapSystem.Core
{
    /// <summary>
    /// 개별 맵의 메타데이터를 담는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewMapData", menuName = "Game/Map System/Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("맵 고유 식별자")]
        public string mapId;
        
        [Tooltip("표시용 맵 이름 (UI에 표시)")]
        public string displayName;
        
        [Header("리소스")]
        [Tooltip("Addressable 맵 프리팹 참조")]
        public AssetReference mapPrefab;
        
        [Tooltip("맵 BGM (null이면 이전 BGM 유지)")]
        public AudioClip bgm;
        
        [Header("스폰 설정")]
        [Tooltip("이 맵의 스폰 포인트들")]
        public SpawnPoint[] spawnPoints;
        
        /// <summary>
        /// 스폰 포인트 ID로 위치 정보 가져오기
        /// </summary>
        public bool TryGetSpawnPoint(string pointId, out SpawnPoint spawnPoint)
        {
            if (spawnPoints != null)
            {
                foreach (var sp in spawnPoints)
                {
                    if (sp.pointId == pointId)
                    {
                        spawnPoint = sp;
                        return true;
                    }
                }
            }
            
            // 못 찾으면 기본 스폰 포인트 (첫 번째) 반환
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPoint = spawnPoints[0];
                return true;
            }
            
            spawnPoint = default;
            return false;
        }
        
        /// <summary>
        /// 기본 스폰 포인트 가져오기
        /// </summary>
        public SpawnPoint GetDefaultSpawnPoint()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                return spawnPoints[0];
            }
            
            return new SpawnPoint
            {
                pointId = "default",
                position = Vector2.zero,
                facingDirection = Direction.Down
            };
        }
    }
    
    /// <summary>
    /// 맵 내 스폰 위치 정보
    /// </summary>
    [System.Serializable]
    public struct SpawnPoint
    {
        [Tooltip("스폰 포인트 식별자 (예: from_village, from_basement)")]
        public string pointId;
        
        [Tooltip("스폰 위치 (월드 좌표)")]
        public Vector2 position;
        
        [Tooltip("스폰 시 플레이어가 바라보는 방향")]
        public Direction facingDirection;
    }
    
    /// <summary>
    /// 캐릭터가 바라보는 방향
    /// </summary>
    public enum Direction
    {
        Down = 0,
        Left = 1,
        Right = 2,
        Up = 3
    }
}
