using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 대화 텍스트의 타이핑 연출을 담당
/// MonoBehaviour가 아닌 일반 클래스로, 코루틴 실행은 외부 runner에 위임
/// </summary>
public class DialogueTypingEffect
{
    private readonly MonoBehaviour coroutineRunner;
    private readonly TextMeshProUGUI textComponent;
    private readonly float typingSpeed;

    private Coroutine currentCoroutine;

    public bool IsTyping { get; private set; }

    public event System.Action OnTypingComplete;

    public DialogueTypingEffect(MonoBehaviour coroutineRunner, TextMeshProUGUI textComponent, float typingSpeed)
    {
        this.coroutineRunner = coroutineRunner;
        this.textComponent = textComponent;
        this.typingSpeed = typingSpeed;
    }

    public void Play(string text)
    {
        Stop();
        currentCoroutine = coroutineRunner.StartCoroutine(TypeRoutine(text));
    }

    public void Skip()
    {
        if (!IsTyping)
            return;

        Stop();
        textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
        IsTyping = false;
        OnTypingComplete?.Invoke();
    }

    public void SetTextImmediate(string text)
    {
        Stop();
        textComponent.text = text;
        textComponent.maxVisibleCharacters = int.MaxValue;
        IsTyping = false;
    }

    private void Stop()
    {
        if (currentCoroutine != null)
        {
            coroutineRunner.StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        IsTyping = false;
    }

    private IEnumerator TypeRoutine(string text)
    {
        IsTyping = true;

        textComponent.text = text;
        textComponent.ForceMeshUpdate();
        textComponent.maxVisibleCharacters = 0;

        int totalChars = textComponent.textInfo.characterCount;

        for (int i = 0; i <= totalChars; i++)
        {
            textComponent.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        currentCoroutine = null;
        IsTyping = false;
        OnTypingComplete?.Invoke();
    }
}
