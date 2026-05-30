using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 선택지 버튼의 생성, 표시, 콜백 연결을 담당
/// </summary>
public class DialogueChoicePresenter
{
    private readonly GameObject choicePanel;
    private readonly Transform buttonContainer;
    private readonly GameObject buttonPrefab;

    public bool IsShowingChoices => choicePanel != null && choicePanel.activeSelf;

    public event System.Action<DialogueChoiceExtended> OnChoiceSelected;

    public DialogueChoicePresenter(GameObject choicePanel, Transform buttonContainer, GameObject buttonPrefab)
    {
        this.choicePanel = choicePanel;
        this.buttonContainer = buttonContainer;
        this.buttonPrefab = buttonPrefab;
    }

    public void Show(List<DialogueChoiceExtended> choices)
    {
        ClearButtons();

        choicePanel.SetActive(true);

        foreach (var choice in choices)
        {
            CreateChoiceButton(choice);
        }
    }

    public void Hide()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);
    }

    private void CreateChoiceButton(DialogueChoiceExtended choice)
    {
        GameObject buttonObj = Object.Instantiate(buttonPrefab, buttonContainer);

        var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = choice.choiceText;
        }

        var button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            var capturedChoice = choice;
            button.onClick.AddListener(() => OnChoiceSelected?.Invoke(capturedChoice));
        }
    }

    private void ClearButtons()
    {
        if (buttonContainer == null)
            return;

        foreach (Transform child in buttonContainer)
        {
            Object.Destroy(child.gameObject);
        }
    }
}
