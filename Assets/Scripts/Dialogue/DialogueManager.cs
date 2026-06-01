using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// 대화 시스템의 진행 흐름을 조율하는 매니저
/// UI 표시, 타이핑, 초상화, 선택지는 각각의 헬퍼 클래스에 위임
/// </summary>
public class DialogueManager : SingletonMonoBehaviour<DialogueManager>
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image dialoguePanelBackground;
    [SerializeField] private RectTransform dialoguePanelTransform;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject continueIcon;

    [Header("Portrait UI")]
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private GameObject leftPortraitPanel;
    [SerializeField] private GameObject rightPortraitPanel;

    [Header("Dialogue Styles")]
    [SerializeField] private DialogueStylePreset dialogueStylePreset;

    [Header("Portrait Effects")]
    [SerializeField] private float inactiveAlpha = 0.5f;
    [SerializeField] private float activeAlpha = 1f;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Choice UI")]
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Transform choiceButtonContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("Typing Effect")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool enableTypingEffect = true;

    [Header("Events")]
    public UnityEvent OnDialogueStart;
    public UnityEvent OnDialogueEnd;
    public UnityEvent<string> OnDialogueEvent;

    private DialogueTypingEffect typingEffect;
    private DialoguePortraitController portraitController;
    private DialogueChoicePresenter choicePresenter;

    private DialogueNodeExtended currentDialogue;
    private bool isDialogueActive;
    private Coroutine shakeCoroutine;
    private Vector3 originalPanelPosition;

    protected override void OnSingletonAwake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        typingEffect = new DialogueTypingEffect(this, dialogueText, typingSpeed);
        typingEffect.OnTypingComplete += HandleTypingComplete;

        portraitController = new DialoguePortraitController(
            this,
            leftPortrait, rightPortrait,
            leftPortraitPanel, rightPortraitPanel,
            activeAlpha, inactiveAlpha, fadeDuration
        );

        choicePresenter = new DialogueChoicePresenter(choicePanel, choiceButtonContainer, choiceButtonPrefab);
        choicePresenter.OnChoiceSelected += HandleChoiceSelected;

        if (dialoguePanel != null)
        {
            originalPanelPosition = dialoguePanel.transform.localPosition;
            dialoguePanel.SetActive(false);
        }

        choicePresenter.Hide();
    }

    // -- Public API --

    /// <summary>
    /// 새 대화를 시작한다 (OnDialogueStart 이벤트 1회 발화)
    /// 대화 중 다음 노드로의 진행은 내부에서 처리
    /// </summary>
    public void StartDialogue(DialogueNodeExtended dialogue)
    {
        if (dialogue == null)
            return;

        isDialogueActive = true;

        dialoguePanel.SetActive(true);
        choicePresenter.Hide();

        OnDialogueStart?.Invoke();
        ShowDialogueNode(dialogue);
    }

    public bool IsDialogueActive() => isDialogueActive;

    // -- Dialogue Display --

    /// <summary>
    /// 현재 노드를 화면에 표시한다
    /// StartDialogue와 분리하여 OnDialogueStart 중복 발화를 방지
    /// </summary>
    private void ShowDialogueNode(DialogueNodeExtended dialogue)
    {
        currentDialogue = dialogue;

        if (speakerNameText != null)
            speakerNameText.text = currentDialogue.speakerName;

        ApplyStyle(currentDialogue.dialogueStyle);
        portraitController.UpdateForDialogue(currentDialogue.position, currentDialogue.portrait);

        if (dialogueText != null)
        {
            if (enableTypingEffect)
            {
                typingEffect.Play(currentDialogue.dialogueText);
                SetContinueIconVisible(false);
            }
            else
            {
                typingEffect.SetTextImmediate(currentDialogue.dialogueText);
                SetContinueIconVisible(true);
            }
        }

        if (!string.IsNullOrEmpty(currentDialogue.eventName))
        {
            OnDialogueEvent?.Invoke(currentDialogue.eventName);
        }
    }

    // -- Input Handling --

    private void Update()
    {
        if (!isDialogueActive)
            return;

        if (choicePresenter.IsShowingChoices)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (typingEffect.IsTyping)
            {
                typingEffect.Skip();
            }
            else
            {
                AdvanceDialogue();
            }
        }
    }

    // -- Dialogue Flow --

    private void AdvanceDialogue()
    {
        if (currentDialogue.choices != null && currentDialogue.choices.Count > 0)
        {
            dialoguePanel.SetActive(false);
            choicePresenter.Show(currentDialogue.choices);
        }
        else if (!string.IsNullOrEmpty(currentDialogue.nextDialogueId))
        {
            ContinueToNextDialogue(currentDialogue.nextDialogueId);
        }
        else
        {
            EndDialogue();
        }
    }

    private void ContinueToNextDialogue(string nextDialogueId)
    {
        if (DialogueDatabaseManagerExtended.Instance == null)
        {
            Debug.LogError("DialogueDatabaseManager not found.");
            EndDialogue();
            return;
        }

        var nextDialogue = DialogueDatabaseManagerExtended.Instance.GetDialogue(nextDialogueId);
        if (nextDialogue != null)
        {
            dialoguePanel.SetActive(true);
            choicePresenter.Hide();
            ShowDialogueNode(nextDialogue);
        }
        else
        {
            EndDialogue();
        }
    }

    private void HandleChoiceSelected(DialogueChoiceExtended choice)
    {
        if (!string.IsNullOrEmpty(choice.eventName))
        {
            OnDialogueEvent?.Invoke(choice.eventName);
        }

        if (!string.IsNullOrEmpty(choice.nextDialogueId))
        {
            ContinueToNextDialogue(choice.nextDialogueId);
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        choicePresenter.Hide();

        StopShake();
        OnDialogueEnd?.Invoke();
    }

    // -- Typing Callback --

    private void HandleTypingComplete()
    {
        SetContinueIconVisible(true);
    }

    private void SetContinueIconVisible(bool visible)
    {
        if (continueIcon != null)
            continueIcon.SetActive(visible);
    }

    // -- Style Application --

    private void ApplyStyle(DialogueStyleType styleType)
    {
        if (dialogueStylePreset == null || dialoguePanelBackground == null)
            return;

        DialogueStyle style = dialogueStylePreset.GetStyle(styleType);

        if (style.panelSprite != null)
            dialoguePanelBackground.sprite = style.panelSprite;

        if (dialogueText != null)
            dialogueText.color = style.textColor;

        if (speakerNameText != null)
            speakerNameText.color = style.speakerNameColor;

        if (dialoguePanelTransform != null)
            dialoguePanelTransform.localScale = Vector3.one * style.panelScale;

        if (style.enableShake)
        {
            StartShake(style.shakeIntensity);
        }
        else
        {
            StopShake();
        }
    }

    private void StartShake(float intensity)
    {
        StopShake();
        shakeCoroutine = StartCoroutine(ShakeRoutine(intensity));
    }

    private void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
            dialoguePanel.transform.localPosition = originalPanelPosition;
        }
    }

    private IEnumerator ShakeRoutine(float intensity)
    {
        while (true)
        {
            float offsetX = Random.Range(-intensity, intensity);
            float offsetY = Random.Range(-intensity, intensity);
            dialoguePanel.transform.localPosition = originalPanelPosition + new Vector3(offsetX, offsetY, 0);
            yield return new WaitForSeconds(0.05f);
        }
    }
}
