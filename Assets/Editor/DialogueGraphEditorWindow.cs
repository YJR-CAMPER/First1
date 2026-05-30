using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// 대화 에디터의 메인 윈도우
/// 툴바, 데이터베이스 로드/저장, 그래프 뷰 관리를 담당
/// </summary>
public class DialogueGraphEditorWindow : EditorWindow
{
    private DialogueDatabaseExtended database;
    private DialogueGraphView graphView;
    private string databasePath;

    private const string PREF_KEY = "DialogueEditor_DatabasePath";

    private bool isRefreshing = false;
    public bool IsRefreshing => isRefreshing;

    [MenuItem("Tools/Dialogue Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<DialogueGraphEditorWindow>("Dialogue Editor");
        window.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        TryLoadSavedDatabase();
        ConstructGraphView();
        GenerateToolbar();

        if (database != null)
        {
            LoadNodesFromDatabase();
        }
    }

    private void OnDisable()
    {
        if (database != null)
        {
            SaveDatabase();
        }

        if (graphView != null)
        {
            rootVisualElement.Remove(graphView);
        }
    }

    public DialogueDatabaseExtended GetDatabase()
    {
        return database;
    }

    public void SaveDatabase()
    {
        if (database == null)
            return;

        SyncNodePositionsToData();

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssetIfDirty(database);
    }

    public void RefreshGraph()
    {
        if (database == null)
            return;

        isRefreshing = true;

        SaveDatabase();
        graphView.DeleteElements(graphView.graphElements.ToList());
        LoadNodesFromDatabase();

        isRefreshing = false;
    }

    public void RefreshAllNodes()
    {
        var nodes = graphView.nodes.ToList().Cast<DialogueNodeView>();
        foreach (var node in nodes)
        {
            node.RefreshNode();
        }
    }

    public void RefreshCurrentNode(string dialogueId)
    {
        var nodes = graphView.nodes.ToList().Cast<DialogueNodeView>();
        var node = nodes.FirstOrDefault(n => n.DialogueId == dialogueId);
        node?.RefreshNode();
    }

    // -- Database I/O --

    private void TryLoadSavedDatabase()
    {
        databasePath = EditorPrefs.GetString(PREF_KEY, "");

        if (string.IsNullOrEmpty(databasePath) || !File.Exists(databasePath))
            return;

        database = AssetDatabase.LoadAssetAtPath<DialogueDatabaseExtended>(databasePath);
    }

    private void SelectDatabaseFile()
    {
        string path = EditorUtility.OpenFilePanel("Select Dialogue Database", "Assets", "asset");
        if (string.IsNullOrEmpty(path))
            return;

        if (!TryConvertToAssetPath(path, out string assetPath))
            return;

        database = AssetDatabase.LoadAssetAtPath<DialogueDatabaseExtended>(assetPath);
        if (database == null)
        {
            EditorUtility.DisplayDialog("Load Failed", $"Failed to load database at:\n{assetPath}", "OK");
            return;
        }

        databasePath = assetPath;
        EditorPrefs.SetString(PREF_KEY, databasePath);

        RefreshGraph();
        GenerateToolbar();
    }

    private void CreateNewDatabase()
    {
        string path = EditorUtility.SaveFilePanel("Create New Dialogue Database", "Assets", "DialogueDatabase", "asset");
        if (string.IsNullOrEmpty(path))
            return;

        if (!TryConvertToAssetPath(path, out string assetPath))
            return;

        string directory = Path.GetDirectoryName(assetPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        database = ScriptableObject.CreateInstance<DialogueDatabaseExtended>();
        AssetDatabase.CreateAsset(database, assetPath);
        AssetDatabase.SaveAssets();

        databasePath = assetPath;
        EditorPrefs.SetString(PREF_KEY, databasePath);

        RefreshGraph();
        GenerateToolbar();
    }

    /// <summary>
    /// 절대 경로를 Assets/ 상대 경로로 변환한다.
    /// Assets 폴더 외부 경로는 거부한다.
    /// </summary>
    private bool TryConvertToAssetPath(string absolutePath, out string assetPath)
    {
        assetPath = "";

        if (!absolutePath.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog("Invalid Path", "Database must be inside the Assets folder.", "OK");
            return false;
        }

        assetPath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
        return true;
    }

    private void SyncNodePositionsToData()
    {
        var nodes = graphView.nodes.ToList().Cast<DialogueNodeView>();
        foreach (var node in nodes)
        {
            var dialogue = database.dialogues.FirstOrDefault(d => d.id == node.DialogueId);
            if (dialogue != null)
            {
                dialogue.nodePosition = node.GetPosition().position;
            }
        }
    }

    // -- Graph Construction --

    private void ConstructGraphView()
    {
        if (graphView != null)
        {
            rootVisualElement.Remove(graphView);
        }

        graphView = new DialogueGraphView(this)
        {
            name = "Dialogue Graph"
        };
        graphView.style.marginTop = 27;
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void LoadNodesFromDatabase()
    {
        if (database == null || database.dialogues == null)
            return;

        var nodeDict = new Dictionary<string, DialogueNodeView>();

        foreach (var dialogue in database.dialogues)
        {
            var node = graphView.CreateDialogueNode(dialogue);
            graphView.AddElement(node);
            nodeDict[dialogue.id] = node;
        }

        foreach (var dialogue in database.dialogues)
        {
            if (nodeDict.TryGetValue(dialogue.id, out var node))
            {
                graphView.CreateConnectionsForNode(node, nodeDict);
            }
        }
    }

    private void CreateNewDialogueNode()
    {
        if (database == null)
        {
            EditorUtility.DisplayDialog("Error", "No database loaded.\nClick 'Select Database' or 'Create New' first.", "OK");
            return;
        }

        var dialogue = new DialogueNodeExtended
        {
            id = $"dialogue_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
            speakerName = "New Speaker",
            dialogueText = "Enter dialogue text here...",
            dialogueStyle = DialogueStyleType.일반,
            position = SpeakerPosition.Left,
            choices = new List<DialogueChoiceExtended>(),
            nodePosition = new Vector2(100, 100)
        };

        database.dialogues.Add(dialogue);

        var node = graphView.CreateDialogueNode(dialogue);
        graphView.AddElement(node);

        SaveDatabase();
    }

    // -- Toolbar --

    private void GenerateToolbar()
    {
        var existingToolbar = rootVisualElement.Q<VisualElement>("toolbar");
        if (existingToolbar != null)
        {
            rootVisualElement.Remove(existingToolbar);
        }

        var toolbar = new VisualElement();
        toolbar.name = "toolbar";
        toolbar.style.flexDirection = FlexDirection.Row;
        toolbar.style.height = 25;
        toolbar.style.position = Position.Absolute;
        toolbar.style.left = 0;
        toolbar.style.right = 0;
        toolbar.style.top = 0;
        toolbar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        toolbar.style.paddingLeft = 5;
        toolbar.style.paddingRight = 5;

        toolbar.Add(new Button(SelectDatabaseFile) { text = "Select Database" });
        toolbar.Add(new Button(CreateNewDatabase) { text = "Create New" });
        toolbar.Add(new Button(SaveDatabase) { text = "Save" });
        toolbar.Add(new Button(CreateNewDialogueNode) { text = "New Dialogue" });
        toolbar.Add(new Button(RefreshGraph) { text = "Refresh" });
        toolbar.Add(new Button(ShowHelp) { text = "Help" });

        string displayPath = database != null ? Path.GetFileName(databasePath) : "No database";
        var pathLabel = new Label($"DB: {displayPath}");
        pathLabel.style.unityTextAlign = TextAnchor.MiddleRight;
        pathLabel.style.flexGrow = 1;
        pathLabel.style.color = database != null ? Color.gray : Color.red;
        pathLabel.style.fontSize = 10;
        toolbar.Add(pathLabel);

        rootVisualElement.Add(toolbar);
    }

    private void ShowHelp()
    {
        EditorUtility.DisplayDialog(
            "Dialogue Editor Help",
            "1. Select Database or Create New\n" +
            "2. New Dialogue to create a node\n" +
            "3. Click a node to edit content\n" +
            "4. Drag from output port to input port\n" +
            "5. Save to persist changes\n\n" +
            "Shortcuts:\n" +
            "- Delete: Remove selected\n" +
            "- Ctrl+S: Save\n" +
            "- F: Frame all",
            "OK"
        );
    }
}
