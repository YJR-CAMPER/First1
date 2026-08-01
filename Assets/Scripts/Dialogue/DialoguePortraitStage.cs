using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 3슬롯 초상화 무대 관리
/// 말하는 슬롯을 강조하고, 나머지는 어둡게 처리
/// 표정 스프라이트 적용 및 연출 효과(흔들림/튀어오름/색플래시) 재생
/// </summary>
public class DialoguePortraitStage
{
    private readonly MonoBehaviour runner;

    private readonly PortraitSlotView left;
    private readonly PortraitSlotView center;
    private readonly PortraitSlotView right;

    private readonly float activeAlpha;
    private readonly float inactiveAlpha;
    private readonly float fadeDuration;

    private Coroutine effectRoutine;

    public DialoguePortraitStage(
        MonoBehaviour runner,
        PortraitSlotView left,
        PortraitSlotView center,
        PortraitSlotView right,
        float activeAlpha, float inactiveAlpha, float fadeDuration)
    {
        this.runner = runner;
        this.left = left;
        this.center = center;
        this.right = right;
        this.activeAlpha = activeAlpha;
        this.inactiveAlpha = inactiveAlpha;
        this.fadeDuration = fadeDuration;
    }

    /// <summary>
    /// 특정 슬롯에 캐릭터 초상화를 표시하고 말하는 슬롯으로 강조
    /// 이미 다른 슬롯에 있던 초상화는 유지하되 어둡게 처리
    /// </summary>
    public void ShowSpeaker(PortraitSlot slot, Sprite sprite, PortraitEffect effect)
    {
        if (slot == PortraitSlot.없음)
        {
            HideAll();
            return;
        }

        var target = GetSlotView(slot);
        if (target == null)
            return;

        target.SetSprite(sprite);
        target.SetVisible(true);

        ApplyEmphasis(slot);
        PlayEffect(target, effect);
    }

    /// <summary>
    /// 모든 슬롯 숨김 (내레이션, 대화 종료 등)
    /// </summary>
    public void HideAll()
    {
        StopEffect();
        left?.SetVisible(false);
        center?.SetVisible(false);
        right?.SetVisible(false);
    }

    // -- Emphasis --

    private void ApplyEmphasis(PortraitSlot speakingSlot)
    {
        FadeSlot(left, speakingSlot == PortraitSlot.좌);
        FadeSlot(center, speakingSlot == PortraitSlot.중);
        FadeSlot(right, speakingSlot == PortraitSlot.우);
    }

    private void FadeSlot(PortraitSlotView view, bool isSpeaking)
    {
        if (view == null || !view.IsVisible)
            return;

        float target = isSpeaking ? activeAlpha : inactiveAlpha;
        runner.StartCoroutine(FadeRoutine(view.CanvasGroup, target));

        // 말하는 슬롯을 앞으로 가져와 겹침 시 우선 표시
        if (isSpeaking)
            view.BringToFront();
    }

    private IEnumerator FadeRoutine(CanvasGroup group, float targetAlpha)
    {
        if (group == null)
            yield break;

        float start = group.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            group.alpha = Mathf.Lerp(start, targetAlpha, t);
            yield return null;
        }

        group.alpha = targetAlpha;
    }

    // -- Effects --

    private void PlayEffect(PortraitSlotView view, PortraitEffect effect)
    {
        StopEffect();

        if (effect == PortraitEffect.없음 || view == null)
            return;

        switch (effect)
        {
            case PortraitEffect.흔들림:
                effectRoutine = runner.StartCoroutine(ShakeRoutine(view));
                break;
            case PortraitEffect.튀어오름:
                effectRoutine = runner.StartCoroutine(BounceRoutine(view));
                break;
            case PortraitEffect.색플래시:
                effectRoutine = runner.StartCoroutine(FlashRoutine(view));
                break;
        }
    }

    private void StopEffect()
    {
        if (effectRoutine != null)
        {
            runner.StopCoroutine(effectRoutine);
            effectRoutine = null;
        }

        left?.ResetTransform();
        center?.ResetTransform();
        right?.ResetTransform();
    }

    private IEnumerator ShakeRoutine(PortraitSlotView view)
    {
        Vector3 origin = view.OriginalPosition;
        float duration = 0.4f;
        float intensity = 8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-intensity, intensity);
            float y = Random.Range(-intensity, intensity);
            view.SetPosition(origin + new Vector3(x, y, 0));
            yield return null;
        }

        view.SetPosition(origin);
        effectRoutine = null;
    }

    private IEnumerator BounceRoutine(PortraitSlotView view)
    {
        Vector3 origin = view.OriginalPosition;
        float duration = 0.35f;
        float height = 20f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 위로 튀었다 내려오는 포물선 (sin)
            float offset = Mathf.Sin(t * Mathf.PI) * height;
            view.SetPosition(origin + new Vector3(0, offset, 0));
            yield return null;
        }

        view.SetPosition(origin);
        effectRoutine = null;
    }

    private IEnumerator FlashRoutine(PortraitSlotView view)
    {
        Color origin = Color.white;
        Color flash = new Color(1f, 0.5f, 0.5f);
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 붉게 물들었다 원래대로
            float pulse = Mathf.Sin(t * Mathf.PI);
            view.SetColor(Color.Lerp(origin, flash, pulse));
            yield return null;
        }

        view.SetColor(origin);
        effectRoutine = null;
    }

    // -- Helpers --

    private PortraitSlotView GetSlotView(PortraitSlot slot)
    {
        switch (slot)
        {
            case PortraitSlot.좌: return left;
            case PortraitSlot.중: return center;
            case PortraitSlot.우: return right;
            default: return null;
        }
    }
}
