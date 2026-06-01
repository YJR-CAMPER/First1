using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// <summary>
/// GraphView 내 개별 대화 노드의 시각적 표현을 담당
/// </summary>
public class DialogueNodeView : Node
{
    private DialogueNodeExtended dialogue;
    private DialogueGraphEditorWindow editorWindow;

    public string DialogueId => dialogue.id;
    public DialogueNodeExtended Dialogue => dialogue;

    public DialogueNodeView(DialogueNodeExtended dialogueData, DialogueGraphEditorWindow window)
    {
        dialogue = dialogueData;
        editorWindow = window;

        title = dialogue.speakerName;

        var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        inputPort.portName = "Input";
        inputContainer.Add(inputPort);

        BuildContent();
        BuildOutputPorts();

        RefreshExpandedState();
        RefreshPorts();

        ApplyStyleColor();
    }

    public void RefreshNode()
    {
        title = dialogue.speakerName;

        mainContainer.Clear();
        BuildContent();

        outputContainer.Clear();
        BuildOutputPorts();

        ApplyStyleColor();

        RefreshExpandedState();
        RefreshPorts();
    }

    private void BuildContent()
    {
        var container = new VisualElement();

        var idLabel = new Label($"ID: {dialogue.id}");
        idLabel.style.fontSize = 10;
        idLabel.style.color = Color.gray;
        container.Add(idLabel);

        var styleLabel = new Label($"Style: {dialogue.dialogueStyle}");
        styleLabel.style.fontSize = 12;
        styleLabel.style.marginTop = 5;
        container.Add(styleLabel);

        string textPreview = dialogue.dialogueText != null && dialogue.dialogueText.Length > 50
            ? dialogue.dialogueText.Substring(0, 50) + "..."
            : dialogue.dialogueText ?? "";

        var textLabel = new Label(textPreview);
        textLabel.style.fontSize = 11;
        textLabel.style.whiteSpace = WhiteSpace.Normal;
        textLabel.style.maxWidth = 250;
        textLabel.style.marginTop = 5;
        container.Add(textLabel);

        var editButton = new Button(() => OpenDetailEditor())
        {
            text = "Edit"
        };
        editButton.style.marginTop = 10;
        container.Add(editButton);

        mainContainer.Add(container);
    }

    private void BuildOutputPorts()
    {
        if (dialogue.choices == null)
            dialogue.choices = new List<DialogueChoiceExtended>();

        // Next 포트는 항상 생성하여 새 노드에서도 연결 가능하게 함
        var nextPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        nextPort.portName = "Next";
        nextPort.name = "next_port";
        outputContainer.Add(nextPort);

        for (int i = 0; i < dialogue.choices.Count; i++)
        {
            var choice = dialogue.choices[i];
            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort.portName = $"Choice {i + 1}: {choice.choiceText}";
            outputPort.name = $"choice_{i}";
            outputContainer.Add(outputPort);
        }

        var addChoiceButton = new Button(() => AddChoice())
        {
            text = "Add Choice"
        };
        outputContainer.Add(addChoiceButton);
    }

    private void AddChoice()
    {
        dialogue.choices.Add(new DialogueChoiceExtended
        {
            choiceText = "New Choice",
            nextDialogueId = ""
        });

        outputContainer.Clear();
        BuildOutputPorts();
        RefreshExpandedState();
        RefreshPorts();

        editorWindow.SaveDatabase();
    }

    private void ApplyStyleColor()
    {
        Color color;

        switch (dialogue.dialogueStyle)
        {
            case DialogueStyleType.소리침:
                color = new Color(1f, 0.7f, 0.7f);
                break;
            case DialogueStyleType.생각:
                color = new Color(0.85f, 0.85f, 1f);
                break;
            default:
                color = new Color(1f, 0.94f, 0.86f);
                break;
        }

        style.backgroundColor = color;
    }

    private void OpenDetailEditor()
    {
        DialogueDetailEditorWindow.ShowWindow(dialogue, editorWindow);
    }
}
