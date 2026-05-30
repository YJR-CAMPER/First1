using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 대화 노드 그래프의 시각적 캔버스를 담당
/// 노드 생성, 연결 관리, 그래프 변경 감지
/// </summary>
public class DialogueGraphView : GraphView
{
    private DialogueGraphEditorWindow editorWindow;

    public DialogueGraphView(DialogueGraphEditorWindow window)
    {
        editorWindow = window;

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        graphViewChanged += OnGraphViewChanged;
    }

    public DialogueNodeView CreateDialogueNode(DialogueNodeExtended dialogue)
    {
        var node = new DialogueNodeView(dialogue, editorWindow);
        node.SetPosition(new Rect(dialogue.nodePosition, Vector2.zero));
        return node;
    }

    public void CreateConnectionsForNode(DialogueNodeView node, Dictionary<string, DialogueNodeView> nodeDict)
    {
        var dialogue = node.Dialogue;

        TryCreateEdge(node, "next_port", dialogue.nextDialogueId, nodeDict);

        if (dialogue.choices == null)
            return;

        for (int i = 0; i < dialogue.choices.Count; i++)
        {
            TryCreateEdge(node, $"choice_{i}", dialogue.choices[i].nextDialogueId, nodeDict);
        }
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort != port
                && startPort.node != port.node
                && startPort.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange changes)
    {
        if (editorWindow.IsRefreshing)
            return changes;

        bool hasChanges = false;

        if (changes.edgesToCreate != null)
        {
            hasChanges |= ProcessCreatedEdges(changes.edgesToCreate);
        }

        if (changes.elementsToRemove != null)
        {
            hasChanges |= ProcessRemovedElements(changes.elementsToRemove);
        }

        if (hasChanges)
        {
            editorWindow.SaveDatabase();
        }

        return changes;
    }

    private bool ProcessCreatedEdges(List<Edge> edges)
    {
        bool changed = false;

        foreach (var edge in edges)
        {
            var outputNode = edge.output.node as DialogueNodeView;
            var inputNode = edge.input.node as DialogueNodeView;

            if (outputNode == null || inputNode == null)
                continue;

            if (TrySetNextDialogueId(outputNode, edge.output.name, inputNode.DialogueId))
            {
                changed = true;
            }
        }

        return changed;
    }

    private bool ProcessRemovedElements(List<GraphElement> elements)
    {
        bool changed = false;

        foreach (var element in elements)
        {
            if (!(element is Edge edge))
                continue;

            var outputNode = edge.output.node as DialogueNodeView;
            if (outputNode == null)
                continue;

            if (TrySetNextDialogueId(outputNode, edge.output.name, ""))
            {
                changed = true;
            }
        }

        return changed;
    }

    /// <summary>
    /// 포트 이름을 기반으로 대화 데이터의 nextDialogueId를 설정
    /// </summary>
    private bool TrySetNextDialogueId(DialogueNodeView node, string portName, string targetId)
    {
        if (portName == "next_port")
        {
            node.Dialogue.nextDialogueId = targetId;
            return true;
        }

        if (!portName.StartsWith("choice_"))
            return false;

        string indexStr = portName.Substring("choice_".Length);
        if (!int.TryParse(indexStr, out int index))
            return false;

        if (index >= node.Dialogue.choices.Count)
            return false;

        node.Dialogue.choices[index].nextDialogueId = targetId;
        return true;
    }

    private void TryCreateEdge(DialogueNodeView sourceNode, string outputPortName, string targetDialogueId, Dictionary<string, DialogueNodeView> nodeDict)
    {
        if (string.IsNullOrEmpty(targetDialogueId))
            return;

        if (!nodeDict.TryGetValue(targetDialogueId, out var targetNode))
            return;

        var outputPort = sourceNode.outputContainer.Q<Port>(outputPortName);
        var inputPort = targetNode.inputContainer.Q<Port>();

        if (outputPort == null || inputPort == null)
            return;

        var edge = outputPort.ConnectTo(inputPort);
        AddElement(edge);
    }
}
