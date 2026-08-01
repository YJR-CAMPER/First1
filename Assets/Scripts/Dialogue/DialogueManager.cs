using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// 대화 시스템의 진행 흐름을 조율하는 매니저
/// UI 표시, 타이핑, 초상화(3슬롯), 선택지는 각각의 헬퍼 클래스에 위임
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

    [Header("Portrait Stage (3 slots)")]
    [SerializeField] private PortraitSlotView leftSlot;
    [SerializeField] private PortraitSlotView centerSlot;
    [SerializeField] private PortraitSlotView rightSlot;

    [Header("Character Data")]
    [SerializeField] private CharacterRegistry characterRegistry;

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
    private DialoguePortraitStage portraitStage;
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

        leftSlot?.Initialize();
        centerSlot?.Initialize();
        rightSlot?.Initialize();

        portraitStage = new DialoguePortraitStage(
            this,
            leftSlot, centerSlot, rightSlot,
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

    private void ShowDialogueNode(DialogueNodeExtended dialogue)
    {
        currentDialogue = dialogue;

        UpdateSpeakerName();
        ApplyStyle(currentDialogue.dialogueStyle);
        UpdatePortrait();

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

    /// <summary>
    /// 화자 이름 갱신
    /// characterId로 CharacterData를 찾으면 그 displayName을 우선 사용,
    /// 없으면 노드의 speakerName을 폴백으로 사용
    /// </summary>
    private void UpdateSpeakerName()
    {
        if (speakerNameText == null)
            return;

        string name = currentDialogue.speakerName;

        if (characterRegistry != null && !string.IsNullOrEmpty(currentDialogue.characterId))
        {
            var character = characterRegistry.GetById(currentDialogue.characterId);
            if (character != null && !string.IsNullOrEmpty(character.displayName))
                name = character.displayName;
        }

        speakerNameText.text = name;
    }

    /// <summary>
    /// characterId + emotion으로 스프라이트를 조회하여 지정 슬롯에 표시
    /// </summary>
    private void UpdatePortrait()
    {
        if (portraitStage == null)
            return;

        if (currentDialogue.slot == PortraitSlot.없음)
        {
            portraitStage.HideAll();
            return;
        }

        Sprite sprite = null;
        if (characterRegistry != null && !string.IsNullOrEmpty(currentDialogue.characterId))
        {
            var character = characterRegistry.GetById(currentDialogue.characterId);
            if (character != null)
                sprite = character.GetEmotionSprite(currentDialogue.emotion);
        }

        portraitStage.ShowSpeaker(currentDialogue.slot, sprite, currentDialogue.effect);
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
                typingEffect.Skip();
            else
                AdvanceDialogue();
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
            OnDialogueEvent?.Invoke(choice.eventName);

        if (!string.IsNullOrEmpty(choice.nextDialogueId))
            ContinueToNextDialogue(choice.nextDialogueId);
        else
            EndDialogue();
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        choicePresenter.Hide();
        portraitStage.HideAll();

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
            StartShake(style.shakeIntensity);
        else
            StopShake();
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
