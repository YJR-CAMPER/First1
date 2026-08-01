using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 초상화 슬롯 하나의 뷰
/// Image(스프라이트), CanvasGroup(알파), RectTransform(위치/색) 조작을 캡슐화
/// DialoguePortraitStage가 3개의 인스턴스를 관리
/// </summary>
[System.Serializable]
public class PortraitSlotView
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Image portraitImage;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private bool initialized;

    public CanvasGroup CanvasGroup => canvasGroup;
    public bool IsVisible => panel != null && panel.activeSelf;
    public Vector3 OriginalPosition => originalPosition;

    /// <summary>
    /// 최초 사용 전 1회 초기화 (CanvasGroup 확보, 원위치 저장)
    /// </summary>
    public void Initialize()
    {
        if (initialized)
            return;

        if (panel != null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();

            rectTransform = panel.GetComponent<RectTransform>();
            if (rectTransform != null)
                originalPosition = rectTransform.localPosition;

            panel.SetActive(false);
        }

        initialized = true;
    }

    public void SetSprite(Sprite sprite)
    {
        if (portraitImage != null && sprite != null)
        {
            portraitImage.sprite = sprite;
            portraitImage.enabled = true;
        }
    }

    public void SetVisible(bool visible)
    {
        if (panel != null)
            panel.SetActive(visible);
    }

    public void SetPosition(Vector3 localPosition)
    {
        if (rectTransform != null)
            rectTransform.localPosition = localPosition;
    }

    public void SetColor(Color color)
    {
        if (portraitImage != null)
            portraitImage.color = color;
    }

    public void ResetTransform()
    {
        if (rectTransform != null)
            rectTransform.localPosition = originalPosition;

        if (portraitImage != null)
            portraitImage.color = Color.white;
    }

    public void BringToFront()
    {
        if (panel != null)
            panel.transform.SetAsLastSibling();
    }
}
