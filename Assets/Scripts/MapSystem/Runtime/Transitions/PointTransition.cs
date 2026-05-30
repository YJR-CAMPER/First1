using UnityEngine;

namespace MapSystem.Transitions
{
    /// <summary>
    /// 전환 발동 방식
    /// </summary>
    public enum TransitionActivationType
    {
        /// <summary>
        /// 영역 진입 시 즉시 전환 (계단, 동굴 입구 등)
        /// </summary>
        OnEnter,
        
        /// <summary>
        /// 상호작용 키를 눌러야 전환 (문, NPC 대화 후 이동 등)
        /// </summary>
        OnInteract
    }
    
    /// <summary>
    /// 문, 계단, 동굴 입구 등 특정 지점에서의 맵 전환 트리거
    /// </summary>
    public class PointTransition : TransitionTrigger
    {
        #region 설정
        
        [Header("전환 방식")]
        [Tooltip("전환이 발동되는 조건")]
        [SerializeField] private TransitionActivationType activationType = TransitionActivationType.OnInteract;
        
        [Header("상호작용 설정 (OnInteract 모드)")]
        [Tooltip("상호작용 입력 키")]
        [SerializeField] private KeyCode interactKey = KeyCode.Z;
        
        [Tooltip("상호작용 가능할 때 표시할 UI 프롬프트 (선택)")]
        [SerializeField] private GameObject interactPrompt;
        
        [Header("방향 조건 (선택)")]
        [Tooltip("특정 방향을 바라볼 때만 전환 가능")]
        [SerializeField] private bool requireFacingDirection = false;
        
        [Tooltip("필요한 방향 (위쪽 문이면 Up)")]
        [SerializeField] private Core.Direction requiredDirection = Core.Direction.Up;
        
        #endregion
        
        #region 상태
        
        private bool isPlayerInTrigger = false;
        private Collider2D playerInTrigger;
        
        #endregion
        
        #region Unity 생명주기
        
        private void Start()
        {
            // 프롬프트 초기 상태
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (!isEnabled || !isPlayerInTrigger) return;
            
            // OnInteract 모드일 때만 키 입력 체크
            if (activationType == TransitionActivationType.OnInteract)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    TryTransition();
                }
            }
        }
        
        #endregion
        
        #region 트리거 처리
        
        protected override void OnPlayerEnter(Collider2D playerCollider)
        {
            isPlayerInTrigger = true;
            playerInTrigger = playerCollider;
            
            if (activationType == TransitionActivationType.OnEnter)
            {
                // 즉시 전환
                TryTransition();
            }
            else if (activationType == TransitionActivationType.OnInteract)
            {
                // 프롬프트 표시
                ShowPrompt(true);
            }
        }
        
        protected override void OnPlayerExit(Collider2D playerCollider)
        {
            isPlayerInTrigger = false;
            playerInTrigger = null;
            
            // 프롬프트 숨김
            ShowPrompt(false);
        }
        
        #endregion
        
        #region Private Methods
        
        private void TryTransition()
        {
            // 방향 조건 체크
            if (requireFacingDirection && !CheckPlayerFacingDirection())
            {
                return;
            }
            
            // 전환 실행
            ExecuteTransition();
        }
        
        private bool CheckPlayerFacingDirection()
        {
            if (playerInTrigger == null) return false;
            
            // 플레이어의 현재 방향 가져오기
            // TODO: 플레이어 컴포넌트에서 방향 정보를 가져오는 방식으로 수정 필요
            // 지금은 임시로 항상 true 반환
            
            // 예시 구현:
            // var playerController = playerInTrigger.GetComponent<PlayerController>();
            // return playerController != null && playerController.FacingDirection == requiredDirection;
            
            return true;
        }
        
        private void ShowPrompt(bool show)
        {
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(show);
            }
        }
        
        #endregion
        
        #region 기즈모
        
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            
            // 전환 타입에 따라 아이콘 색상 구분
            Gizmos.color = activationType == TransitionActivationType.OnEnter 
                ? new Color(1f, 0.5f, 0f, 0.5f)  // 주황색 (즉시)
                : new Color(0f, 0.5f, 1f, 0.5f); // 파란색 (상호작용)
            
            // 작은 구체로 타입 표시
            Gizmos.DrawSphere(transform.position, 0.15f);
        }
        
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            #if UNITY_EDITOR
            // 추가 정보 표시
            string info = activationType == TransitionActivationType.OnEnter 
                ? "즉시 전환" 
                : $"상호작용 ({interactKey})";
                
            if (requireFacingDirection)
            {
                info += $"\n방향: {requiredDirection}";
            }
            
            UnityEditor.Handles.Label(
                transform.position + Vector3.down * 0.5f,
                info
            );
            #endif
        }
        
        #endregion
    }
}
