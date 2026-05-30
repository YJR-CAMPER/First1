using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using MapSystem.Effects;

namespace MapSystem.Core
{
    /// <summary>
    /// 맵 로딩/언로딩을 총괄하는 매니저
    /// Addressables를 사용한 비동기 맵 전환 처리
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        #region Singleton
        
        private static MapManager instance;
        public static MapManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MapManager>();
                    if (instance == null)
                    {
                        Debug.LogError("[MapManager] 씬에 MapManager가 없습니다!");
                    }
                }
                return instance;
            }
        }
        
        #endregion
        
        #region 이벤트
        
        /// <summary>
        /// 맵 전환 시작 시 발생 (UI 등에서 구독)
        /// </summary>
        public event Action<MapData> OnMapTransitionStart;
        
        /// <summary>
        /// 맵 전환 완료 시 발생
        /// </summary>
        public event Action<MapData> OnMapTransitionComplete;
        
        /// <summary>
        /// 맵 로딩 진행률 (0~1)
        /// </summary>
        public event Action<float> OnLoadProgress;
        
        #endregion
        
        #region 설정
        
        [Header("필수 참조")]
        [SerializeField] private MapContainer mapContainer;
        [SerializeField] private Transform playerTransform;
        
        [Header("전환 효과")]
        [SerializeField] private MonoBehaviour transitionEffectComponent;
        private ITransitionEffect transitionEffect;
        
        [Header("디버그")]
        [SerializeField] private bool debugMode = false;
        
        #endregion
        
        #region 상태
        
        /// <summary>
        /// 현재 로드된 맵 데이터
        /// </summary>
        public MapData CurrentMap => mapContainer?.CurrentMapData;
        
        /// <summary>
        /// 맵 전환 중인지
        /// </summary>
        public bool IsTransitioning { get; private set; }
        
        // Addressable 핸들 (언로드용)
        private AsyncOperationHandle<GameObject> currentMapHandle;
        
        #endregion
        
        #region Unity 생명주기
        
        private void Awake()
        {
            // Singleton 설정
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            
            // 전환 효과 인터페이스 가져오기
            if (transitionEffectComponent != null)
            {
                transitionEffect = transitionEffectComponent as ITransitionEffect;
                if (transitionEffect == null)
                {
                    Debug.LogWarning("[MapManager] transitionEffectComponent가 ITransitionEffect를 구현하지 않습니다.");
                }
            }
        }
        
        private void OnDestroy()
        {
            // Addressable 핸들 정리
            if (currentMapHandle.IsValid())
            {
                Addressables.Release(currentMapHandle);
            }
            
            if (instance == this)
            {
                instance = null;
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// 맵 전환 (기본 스폰 포인트 사용)
        /// </summary>
        public async Task LoadMapAsync(MapData targetMap)
        {
            if (targetMap == null)
            {
                Debug.LogError("[MapManager] targetMap이 null입니다.");
                return;
            }
            
            await LoadMapAsync(targetMap, null);
        }
        
        /// <summary>
        /// 맵 전환 (특정 스폰 포인트 지정)
        /// </summary>
        public async Task LoadMapAsync(MapData targetMap, string spawnPointId)
        {
            if (targetMap == null)
            {
                Debug.LogError("[MapManager] targetMap이 null입니다.");
                return;
            }
            
            if (IsTransitioning)
            {
                Debug.LogWarning("[MapManager] 이미 맵 전환 중입니다.");
                return;
            }
            
            IsTransitioning = true;
            
            Log($"맵 전환 시작: {targetMap.displayName}");
            
            try
            {
                OnMapTransitionStart?.Invoke(targetMap);
                
                // 1. 페이드 아웃
                if (transitionEffect != null)
                {
                    await transitionEffect.PlayOutAsync();
                }
                
                // 2. 기존 맵 언로드
                await UnloadCurrentMapAsync();
                
                // 3. 새 맵 로드
                await LoadMapInternalAsync(targetMap);
                
                // 4. 플레이어 위치 설정
                SetPlayerPosition(targetMap, spawnPointId);
                
                // 5. BGM 변경 (있다면)
                if (targetMap.bgm != null)
                {
                    // TODO: AudioManager 연동
                    Log($"BGM 변경: {targetMap.bgm.name}");
                }
                
                // 6. 페이드 인
                if (transitionEffect != null)
                {
                    await transitionEffect.PlayInAsync();
                }
                
                OnMapTransitionComplete?.Invoke(targetMap);
                
                Log($"맵 전환 완료: {targetMap.displayName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MapManager] 맵 전환 실패: {e.Message}\n{e.StackTrace}");
                transitionEffect?.Reset();
            }
            finally
            {
                IsTransitioning = false;
            }
        }
        
        /// <summary>
        /// 현재 맵 데이터 가져오기
        /// </summary>
        public MapData GetCurrentMapData()
        {
            return mapContainer?.CurrentMapData;
        }
        
        #endregion
        
        #region Private Methods
        
        private async Task UnloadCurrentMapAsync()
        {
            if (mapContainer == null || !mapContainer.HasLoadedMap) return;
            
            Log("기존 맵 언로드 중...");
            
            // 기존 맵 오브젝트 파괴
            if (mapContainer.CurrentMapInstance != null)
            {
                Destroy(mapContainer.CurrentMapInstance);
            }
            
            // Addressable 핸들 해제
            if (currentMapHandle.IsValid())
            {
                Addressables.Release(currentMapHandle);
            }
            
            mapContainer.ClearMap();
            
            // 가비지 컬렉션 여유 주기
            await Task.Yield();
        }
        
        private async Task LoadMapInternalAsync(MapData mapData)
        {
            if (mapData.mapPrefab == null || !mapData.mapPrefab.RuntimeKeyIsValid())
            {
                Debug.LogError($"[MapManager] {mapData.mapId}의 mapPrefab이 유효하지 않습니다.");
                return;
            }
            
            Log($"맵 로드 중: {mapData.mapId}");
            
            // Addressables로 비동기 로드
            currentMapHandle = Addressables.InstantiateAsync(
                mapData.mapPrefab, 
                mapContainer.MapRoot
            );
            
            // 진행률 보고
            while (!currentMapHandle.IsDone)
            {
                OnLoadProgress?.Invoke(currentMapHandle.PercentComplete);
                await Task.Yield();
            }
            
            if (currentMapHandle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject mapInstance = currentMapHandle.Result;
                mapContainer.SetMap(mapInstance, mapData);
                OnLoadProgress?.Invoke(1f);
                
                Log($"맵 로드 성공: {mapData.mapId}");
            }
            else
            {
                Debug.LogError($"[MapManager] 맵 로드 실패: {mapData.mapId}");
            }
        }
        
        private void SetPlayerPosition(MapData mapData, string spawnPointId)
        {
            if (playerTransform == null)
            {
                Debug.LogWarning("[MapManager] playerTransform이 할당되지 않았습니다.");
                return;
            }
            
            SpawnPoint spawn;
            
            if (!string.IsNullOrEmpty(spawnPointId) && mapData.TryGetSpawnPoint(spawnPointId, out spawn))
            {
                // 지정된 스폰 포인트 사용
            }
            else
            {
                // 기본 스폰 포인트 사용
                spawn = mapData.GetDefaultSpawnPoint();
            }
            
            playerTransform.position = new Vector3(spawn.position.x, spawn.position.y, playerTransform.position.z);
            
            // TODO: 플레이어 방향 설정 (애니메이션 시스템 연동)
            Log($"플레이어 위치 설정: {spawn.position}, 방향: {spawn.facingDirection}");
        }
        
        private void Log(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[MapManager] {message}");
            }
        }
        
        #endregion
        
        #region 에디터 검증
        
        private void OnValidate()
        {
            if (transitionEffectComponent != null && !(transitionEffectComponent is ITransitionEffect))
            {
                Debug.LogWarning("[MapManager] transitionEffectComponent는 ITransitionEffect를 구현해야 합니다.");
                transitionEffectComponent = null;
            }
        }
        
        #endregion
    }
}
