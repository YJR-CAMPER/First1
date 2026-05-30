using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MapSystem.Effects
{
    /// <summary>
    /// 기본 페이드 인/아웃 전환 효과
    /// UI Image를 사용해 화면을 덮는 방식
    /// </summary>
    public class FadeEffect : MonoBehaviour, ITransitionEffect
    {
        [Header("설정")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private Color fadeColor = Color.black;
        
        public bool IsPlaying { get; private set; }
        
        private void Awake()
        {
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
                fadeImage.raycastTarget = false;
                fadeImage.gameObject.SetActive(true);
            }
        }
        
        /// <summary>
        /// 페이드 아웃 (화면 가리기)
        /// </summary>
        public async Task PlayOutAsync()
        {
            if (fadeImage == null)
            {
                Debug.LogWarning("[FadeEffect] fadeImage가 할당되지 않음. 효과 스킵.");
                return;
            }
            
            IsPlaying = true;
            fadeImage.raycastTarget = true; // 입력 차단
            
            await FadeAsync(0f, 1f);
            
            IsPlaying = false;
        }
        
        /// <summary>
        /// 페이드 인 (화면 보여주기)
        /// </summary>
        public async Task PlayInAsync()
        {
            if (fadeImage == null)
            {
                Debug.LogWarning("[FadeEffect] fadeImage가 할당되지 않음. 효과 스킵.");
                return;
            }
            
            IsPlaying = true;
            
            await FadeAsync(1f, 0f);
            
            fadeImage.raycastTarget = false; // 입력 허용
            IsPlaying = false;
        }
        
        public void Reset()
        {
            IsPlaying = false;
            if (fadeImage != null)
            {
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
                fadeImage.raycastTarget = false;
            }
        }
        
        private async Task FadeAsync(float startAlpha, float endAlpha)
        {
            float elapsed = 0f;
            Color color = fadeColor;
            
            while (elapsed < fadeDuration)
            {
                // 게임이 종료되었거나 오브젝트가 파괴된 경우 체크
                if (this == null || fadeImage == null) return;
                
                elapsed += Time.unscaledDeltaTime; // 타임스케일 영향 안 받음
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                
                color.a = Mathf.Lerp(startAlpha, endAlpha, t);
                fadeImage.color = color;
                
                await Task.Yield();
            }
            
            // 최종값 보장
            if (fadeImage != null)
            {
                color.a = endAlpha;
                fadeImage.color = color;
            }
        }
        
        #region 에디터 헬퍼
        
        [ContextMenu("Setup Fade Image")]
        private void SetupFadeImage()
        {
            if (fadeImage == null)
            {
                // Canvas 찾거나 생성
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    Debug.LogError("Canvas를 찾을 수 없습니다. Canvas 하위에 배치하세요.");
                    return;
                }
                
                // Fade Image 생성
                GameObject fadeObj = new GameObject("FadeImage");
                fadeObj.transform.SetParent(transform);
                
                fadeImage = fadeObj.AddComponent<Image>();
                fadeImage.color = new Color(0, 0, 0, 0);
                
                // 전체 화면 덮기
                RectTransform rect = fadeImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                Debug.Log("FadeImage 생성 완료!");
            }
        }
        
        #endregion
    }
}
