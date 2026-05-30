using UnityEngine;

namespace MapSystem.Core
{
    /// <summary>
    /// 동적으로 로드되는 맵의 루트 컨테이너
    /// MapManager가 맵 프리팹을 이 오브젝트 하위에 인스턴스화함
    /// </summary>
    public class MapContainer : MonoBehaviour
    {
        [SerializeField] private Transform mapRoot;
        
        /// <summary>
        /// 맵이 인스턴스화될 부모 Transform
        /// </summary>
        public Transform MapRoot => mapRoot != null ? mapRoot : transform;
        
        /// <summary>
        /// 현재 로드된 맵 인스턴스
        /// </summary>
        public GameObject CurrentMapInstance { get; private set; }
        
        /// <summary>
        /// 현재 맵 데이터
        /// </summary>
        public MapData CurrentMapData { get; private set; }
        
        /// <summary>
        /// 맵이 로드되어 있는지
        /// </summary>
        public bool HasLoadedMap => CurrentMapInstance != null;
        
        /// <summary>
        /// 새 맵 인스턴스 설정
        /// </summary>
        public void SetMap(GameObject mapInstance, MapData mapData)
        {
            CurrentMapInstance = mapInstance;
            CurrentMapData = mapData;
            
            if (mapInstance != null)
            {
                mapInstance.transform.SetParent(MapRoot);
                mapInstance.transform.localPosition = Vector3.zero;
            }
        }
        
        /// <summary>
        /// 현재 맵 정리
        /// </summary>
        public void ClearMap()
        {
            CurrentMapInstance = null;
            CurrentMapData = null;
        }
        
        private void OnValidate()
        {
            if (mapRoot == null)
            {
                mapRoot = transform;
            }
        }
    }
}
