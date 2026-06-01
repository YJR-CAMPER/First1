using UnityEngine;
using MapSystem.Core;

namespace MapSystem.Transitions
{
    /// <summary>
    /// 맵 전환 트리거의 추상 베이스 클래스
    /// 모든 전환 트리거(문, 경계, 영역 등)의 공통 기능 정의
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class TransitionTrigger : MonoBehaviour
    {
        #region 설정

        [Header("전환 대상")]
        [Tooltip("이동할 맵 데이터")]
        [SerializeField] protected MapData targetMap;

        [Tooltip("도착 시 스폰 포인트 ID (비워두면 기본 스폰)")]
        [SerializeField] protected string targetSpawnPointId;

        [Header("조건")]
        [Tooltip("전환 가능 여부")]
        [SerializeField] protected bool isEnabled = true;

        [Header("디버그")]
        [SerializeField] protected bool showGizmos = true;
        [SerializeField] protected Color gizmoColor = new Color(0f, 1f, 0.5f, 0.3f);

        #endregion

        #region 프로퍼티

        /// <summary>
        /// 전환이 활성화되어 있는지
        /// </summary>
        public bool IsEnabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }

        /// <summary>
        /// 대상 맵 데이터
        /// </summary>
        public MapData TargetMap => targetMap;

        #endregion

        #region Protected

        protected Collider2D triggerCollider;

        #endregion

        #region Unity 생명주기

        protected virtual void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();

            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        protected virtual void OnValidate()
        {
            var col = GetComponent<Collider2D>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
            }
        }

        #endregion

        #region 트리거 처리

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!isEnabled) return;
            if (!IsPlayer(other)) return;

            OnPlayerEnter(other);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!IsPlayer(other)) return;

            OnPlayerExit(other);
        }

        #endregion

        #region 추상/가상 메서드

        /// <summary>
        /// 플레이어가 트리거에 진입했을 때
        /// </summary>
        protected abstract void OnPlayerEnter(Collider2D playerCollider);

        /// <summary>
        /// 플레이어가 트리거에서 나갔을 때
        /// </summary>
        protected virtual void OnPlayerExit(Collider2D playerCollider) { }

        /// <summary>
        /// 맵 전환 실행
        /// async void이므로 내부에서 예외를 반드시 처리해야 한다
        /// </summary>
        protected async void ExecuteTransition()
        {
            if (targetMap == null)
            {
                Debug.LogError($"[TransitionTrigger] {gameObject.name}: targetMap이 설정되지 않았습니다.");
                return;
            }

            if (MapManager.Instance == null)
            {
                Debug.LogError("[TransitionTrigger] MapManager를 찾을 수 없습니다.");
                return;
            }

            if (MapManager.Instance.IsTransitioning)
            {
                return;
            }

            try
            {
                await MapManager.Instance.LoadMapAsync(targetMap, targetSpawnPointId);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TransitionTrigger] {gameObject.name}: 맵 전환 실패 - {e.Message}\n{e.StackTrace}");
            }
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// 플레이어인지 확인 (태그로)
        /// </summary>
        protected virtual bool IsPlayer(Collider2D other)
        {
            return other.CompareTag("Player");
        }

        #endregion

        #region 기즈모

        protected virtual void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Gizmos.color = gizmoColor;

            var col = GetComponent<Collider2D>();
            if (col is BoxCollider2D box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.offset, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (targetMap != null)
            {
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 0.5f,
                    $"-> {targetMap.displayName}\n({targetSpawnPointId})"
                );
                #endif
            }
        }

        #endregion
    }
}
