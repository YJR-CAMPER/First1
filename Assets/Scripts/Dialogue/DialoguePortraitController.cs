using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 좌/우 초상화의 표시, 숨김, 알파 전환을 담당
/// </summary>
public class DialoguePortraitController
{
    private readonly MonoBehaviour coroutineRunner;

    private readonly Image leftPortrait;
    private readonly Image rightPortrait;
    private readonly GameObject leftPanel;
    private readonly GameObject rightPanel;
    private readonly CanvasGroup leftCanvasGroup;
    private readonly CanvasGroup rightCanvasGroup;

    private readonly float activeAlpha;
    private readonly float inactiveAlpha;
    private readonly float fadeDuration;

    public DialoguePortraitController(
        MonoBehaviour coroutineRunner,
        Image leftPortrait, Image rightPortrait,
        GameObject leftPanel, GameObject rightPanel,
        float activeAlpha, float inactiveAlpha, float fadeDuration)
    {
        this.coroutineRunner = coroutineRunner;
        this.leftPortrait = leftPortrait;
        this.rightPortrait = rightPortrait;
        this.leftPanel = leftPanel;
        this.rightPanel = rightPanel;
        this.activeAlpha = activeAlpha;
        this.inactiveAlpha = inactiveAlpha;
        this.fadeDuration = fadeDuration;

        leftCanvasGroup = EnsureCanvasGroup(leftPanel);
        rightCanvasGroup = EnsureCanvasGroup(rightPanel);
    }

    public void UpdateForDialogue(SpeakerPosition position, Sprite portrait)
    {
        switch (position)
        {
            case SpeakerPosition.Left:
                SetPortraitSprite(leftPortrait, portrait);
                Highlight(isLeftActive: true);
                break;

            case SpeakerPosition.Right:
                SetPortraitSprite(rightPortrait, portrait);
                Highlight(isLeftActive: false);
                break;

            case SpeakerPosition.Center:
                SetPortraitSprite(leftPortrait, portrait);
                SetPanelVisible(rightPanel, false);
                Highlight(isLeftActive: true);
                break;

            case SpeakerPosition.None:
                SetPanelVisible(leftPanel, false);
                SetPanelVisible(rightPanel, false);
                break;
        }
    }

    public void HideAll()
    {
        SetPanelVisible(leftPanel, false);
        SetPanelVisible(rightPanel, false);
    }

    private void Highlight(bool isLeftActive)
    {
        if (isLeftActive)
        {
            FadeTo(leftCanvasGroup, activeAlpha);
            FadeTo(rightCanvasGroup, inactiveAlpha);
            SetPanelVisible(leftPanel, true);
        }
        else
        {
            FadeTo(leftCanvasGroup, inactiveAlpha);
            FadeTo(rightCanvasGroup, activeAlpha);
            SetPanelVisible(rightPanel, true);
        }
    }

    private void FadeTo(CanvasGroup canvasGroup, float targetAlpha)
    {
        if (canvasGroup == null)
            return;

        coroutineRunner.StartCoroutine(FadeRoutine(canvasGroup, targetAlpha));
    }

    private IEnumerator FadeRoutine(CanvasGroup canvasGroup, float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    private void SetPortraitSprite(Image portraitImage, Sprite sprite)
    {
        if (portraitImage == null || sprite == null)
            return;

        portraitImage.sprite = sprite;
        portraitImage.enabled = true;
    }

    private void SetPanelVisible(GameObject panel, bool visible)
    {
        if (panel != null)
            panel.SetActive(visible);
    }

    private CanvasGroup EnsureCanvasGroup(GameObject panel)
    {
        if (panel == null)
            return null;

        var group = panel.GetComponent<CanvasGroup>();
        if (group == null)
            group = panel.AddComponent<CanvasGroup>();

        return group;
    }
}
