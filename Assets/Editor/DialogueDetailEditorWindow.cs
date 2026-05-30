using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 대화 노드의 상세 내용을 편집하는 에디터 윈도우
/// </summary>
public class DialogueDetailEditorWindow : EditorWindow
{
    private DialogueNodeExtended currentDialogue;
    private DialogueGraphEditorWindow graphEditorWindow;

    private Vector2 scrollPos;
    private bool showRichTextHelp = false;

    public static void ShowWindow(DialogueNodeExtended dialogue, DialogueGraphEditorWindow editorWindow)
    {
        var window = GetWindow<DialogueDetailEditorWindow>("Edit Dialogue");
        window.currentDialogue = dialogue;
        window.graphEditorWindow = editorWindow;
        window.minSize = new Vector2(500, 700);
        window.Show();
    }

    private void OnGUI()
    {
        if (currentDialogue == null)
        {
            EditorGUILayout.HelpBox("No dialogue selected.", MessageType.Info);
            return;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawHeader();
        EditorGUILayout.Space(10);
        DrawBasicInfo();
        EditorGUILayout.Space(10);
        DrawDialogueText();
        EditorGUILayout.Space(10);
        DrawChoices();
        EditorGUILayout.Space(10);
        DrawActionButtons();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Dialogue Editor", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("ID:");
        EditorGUILayout.SelectableLabel(currentDialogue.id, GUILayout.Height(18));
        EditorGUILayout.EndHorizontal();
    }

    private void DrawBasicInfo()
    {
        EditorGUILayout.LabelField("Basic Info", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        currentDialogue.speakerName = EditorGUILayout.TextField(
            "Speaker Name",
            currentDialogue.speakerName
        );

        currentDialogue.portrait = (Sprite)EditorGUILayout.ObjectField(
            "Portrait",
            currentDialogue.portrait,
            typeof(Sprite),
            false
        );

        currentDialogue.position = (SpeakerPosition)EditorGUILayout.EnumPopup(
            "Position",
            currentDialogue.position
        );

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Dialogue Style", EditorStyles.boldLabel);

        currentDialogue.dialogueStyle = (DialogueStyleType)EditorGUILayout.EnumPopup(
            "Style Type",
            currentDialogue.dialogueStyle
        );

        DrawStyleDescription(currentDialogue.dialogueStyle);

        if (EditorGUI.EndChangeCheck())
        {
            SaveAndMarkDirty();
        }
    }

    private void DrawStyleDescription(DialogueStyleType styleType)
    {
        string description;
        Color previewColor;

        switch (styleType)
        {
            case DialogueStyleType.소리침:
                description = "Shouting - spiky balloon + shake effect";
                previewColor = new Color(1f, 0.7f, 0.7f);
                break;
            case DialogueStyleType.생각:
                description = "Thought / Inner voice - cloud balloon";
                previewColor = new Color(0.85f, 0.85f, 1f);
                break;
            default:
                description = "Normal dialogue - rounded balloon";
                previewColor = new Color(1f, 0.94f, 0.86f);
                break;
        }

        var originalColor = GUI.backgroundColor;
        GUI.backgroundColor = previewColor;
        EditorGUILayout.HelpBox(description, MessageType.None);
        GUI.backgroundColor = originalColor;
    }

    private void DrawDialogueText()
    {
        EditorGUILayout.LabelField("Dialogue Text", EditorStyles.boldLabel);

        showRichTextHelp = EditorGUILayout.Foldout(showRichTextHelp, "Rich Text Guide");
        if (showRichTextHelp)
        {
            DrawRichTextHelp();
        }

        EditorGUILayout.Space(5);

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Text:", EditorStyles.miniBoldLabel);
        currentDialogue.dialogueText = EditorGUILayout.TextArea(
            currentDialogue.dialogueText,
            GUILayout.Height(150)
        );

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Preview:", EditorStyles.miniBoldLabel);

        var previewStyle = new GUIStyle(GUI.skin.box)
        {
            richText = true,
            wordWrap = true,
            padding = new RectOffset(10, 10, 10, 10)
        };
        EditorGUILayout.LabelField(currentDialogue.dialogueText, previewStyle, GUILayout.Height(100));

        if (EditorGUI.EndChangeCheck())
        {
            SaveAndMarkDirty();
        }

        EditorGUILayout.Space(5);
        currentDialogue.eventName = EditorGUILayout.TextField(
            "Event (Optional)",
            currentDialogue.eventName
        );

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Next Dialogue", EditorStyles.boldLabel);
        currentDialogue.nextDialogueId = EditorGUILayout.TextField(
            "Next Dialogue ID",
            currentDialogue.nextDialogueId
        );

        EditorGUILayout.HelpBox(
            "Auto-continues to this dialogue ID when no choices exist.\n" +
            "Example: dialogue_abc123",
            MessageType.Info
        );
    }

    private void DrawRichTextHelp()
    {
        EditorGUILayout.HelpBox(
            "Rich Text Tags:\n\n" +
            "<b>Bold</b>\n" +
            "<i>Italic</i>\n" +
            "<color=red>Red</color>\n" +
            "<size=150%>Large</size>\n" +
            "<size=50%>Small</size>\n\n" +
            "Colors: red, blue, green, yellow, #FF0000",
            MessageType.Info
        );

        EditorGUILayout.LabelField("Quick Insert:", EditorStyles.miniBoldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Bold"))
        {
            InsertRichTextTag("<b>", "</b>");
        }
        if (GUILayout.Button("Red"))
        {
            InsertRichTextTag("<color=red>", "</color>");
        }
        if (GUILayout.Button("Large"))
        {
            InsertRichTextTag("<size=150%>", "</size>");
        }
        EditorGUILayout.EndHorizontal();
    }

    private void InsertRichTextTag(string openTag, string closeTag)
    {
        currentDialogue.dialogueText += openTag + "text" + closeTag;
        Repaint();
    }

    private void DrawChoices()
    {
        EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);

        if (currentDialogue.choices == null)
            currentDialogue.choices = new List<DialogueChoiceExtended>();

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < currentDialogue.choices.Count; i++)
        {
            if (!DrawSingleChoice(i))
                break;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SaveAndMarkDirty();
        }

        if (GUILayout.Button("+ Add Choice"))
        {
            currentDialogue.choices.Add(new DialogueChoiceExtended
            {
                choiceText = "New Choice",
                nextDialogueId = ""
            });
            SaveAndMarkDirty();
        }
    }

    /// <summary>
    /// 개별 선택지 UI를 그린다. 삭제 시 false를 반환하여 루프를 중단시킨다.
    /// </summary>
    private bool DrawSingleChoice(int index)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Choice {index + 1}", EditorStyles.boldLabel);

        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            currentDialogue.choices.RemoveAt(index);
            SaveAndMarkDirty();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return false;
        }
        EditorGUILayout.EndHorizontal();

        var choice = currentDialogue.choices[index];

        choice.choiceText = EditorGUILayout.TextField("Text", choice.choiceText);
        choice.nextDialogueId = EditorGUILayout.TextField("Next Dialogue ID", choice.nextDialogueId);
        choice.eventName = EditorGUILayout.TextField("Event (Optional)", choice.eventName);
        choice.conditionFlag = EditorGUILayout.TextField("Condition Flag (Optional)", choice.conditionFlag);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);

        return true;
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save & Close", GUILayout.Height(40)))
        {
            SaveAndMarkDirty();
            Close();
        }

        if (GUILayout.Button("Cancel", GUILayout.Height(40)))
        {
            Close();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void SaveAndMarkDirty()
    {
        if (graphEditorWindow != null)
        {
            graphEditorWindow.SaveDatabase();
        }
    }
}
