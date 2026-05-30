using UnityEngine;

namespace MapSystem.Transitions
{
    /// <summary>
    /// 맵 경계 방향
    /// </summary>
    public enum EdgeDirection
    {
        Top,
        Bottom,
        Left,
        Right
    }
    
    /// <summary>
    /// 맵 경계에 도달했을 때 인접 맵으로 전환하는 트리거
    /// 주로 필드 맵 간 이동에 사용
    /// </summary>
    public class EdgeTransition : TransitionTrigger
    {
        #region 설정
        
        [Header("경계 설정")]
        [Tooltip("이 트리거가 위치한 맵의 경계 방향")]
        [SerializeField] private EdgeDirection edgeDirection = EdgeDirection.Right;
        
        [Tooltip("다음 맵에서 플레이어가 등장할 오프셋 (경계 기준)")]
        [SerializeField] private float spawnOffset = 1f;
        
        #endregion
        
        #region 트리거 처리
        
        protected override void OnPlayerEnter(Collider2D playerCollider)
        {
            // 경계 전환은 즉시 발동
            ExecuteTransition();
        }
        
        #endregion
        
        #region 기즈모
        
        protected override void OnDrawGizmos()
        {
            if (!showGizmos) return;
            
            // 경계 방향에 따른 색상
            Color color = edgeDirection switch
            {
                EdgeDirection.Top => new Color(1f, 1f, 0f, 0.3f),    // 노랑
                EdgeDirection.Bottom => new Color(1f, 0.5f, 0f, 0.3f), // 주황
                EdgeDirection.Left => new Color(0f, 1f, 0f, 0.3f),   // 초록
                EdgeDirection.Right => new Color(0f, 1f, 1f, 0.3f),  // 청록
                _ => gizmoColor
            };
            
            Gizmos.color = color;
            
            var col = GetComponent<Collider2D>();
            if (col is BoxCollider2D box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.offset, box.size);
                Gizmos.matrix = oldMatrix;
            }
            
            // 방향 화살표 표시
            DrawDirectionArrow();
        }
        
        private void DrawDirectionArrow()
        {
            Vector3 arrowDir = edgeDirection switch
            {
                EdgeDirection.Top => Vector3.up,
                EdgeDirection.Bottom => Vector3.down,
                EdgeDirection.Left => Vector3.left,
                EdgeDirection.Right => Vector3.right,
                _ => Vector3.zero
            };
            
            Gizmos.color = Color.white;
            Vector3 start = transform.position;
            Vector3 end = start + arrowDir * 0.5f;
            Gizmos.DrawLine(start, end);
            
            // 화살촉
            Vector3 perpendicular = Vector3.Cross(arrowDir, Vector3.forward) * 0.15f;
            Gizmos.DrawLine(end, end - arrowDir * 0.2f + perpendicular);
            Gizmos.DrawLine(end, end - arrowDir * 0.2f - perpendicular);
        }
        
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.down * 0.3f,
                $"경계: {edgeDirection}"
            );
            #endif
        }
        
        #endregion
        
        #region 에디터 헬퍼
        
        /// <summary>
        /// 맵 경계에 맞춰 콜라이더 자동 설정
        /// </summary>
        [ContextMenu("Auto Setup Collider")]
        private void AutoSetupCollider()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null)
            {
                box = gameObject.AddComponent<BoxCollider2D>();
            }
            
            box.isTrigger = true;
            
            // 방향에 따라 콜라이더 크기/위치 조정
            switch (edgeDirection)
            {
                case EdgeDirection.Top:
                case EdgeDirection.Bottom:
                    box.size = new Vector2(10f, 0.5f); // 가로로 긴 형태
                    break;
                case EdgeDirection.Left:
                case EdgeDirection.Right:
                    box.size = new Vector2(0.5f, 10f); // 세로로 긴 형태
                    break;
            }
            
            Debug.Log($"[EdgeTransition] 콜라이더 설정 완료: {edgeDirection}");
        }
        
        #endregion
    }
}
