using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// NPC 설정 헬퍼 - InteractPrompt를 자동으로 생성
/// </summary>
[ExecuteInEditMode]
public class NPCSetupHelper : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoCreatePrompt = true;
    [SerializeField] private float promptHeight = 1.2f;
    [SerializeField] private string promptText = "Press E";
    
    [Header("References (Auto-assigned)")]
    [SerializeField] private Canvas promptCanvas;
    [SerializeField] private TextMeshProUGUI promptTextComponent;
    
    private void OnValidate()
    {
        if (!autoCreatePrompt) return;
        
        // InteractPrompt가 없으면 자동 생성
        if (promptCanvas == null)
        {
            CreateInteractPrompt();
        }
    }
    
    /// <summary>
    /// InteractPrompt 자동 생성
    /// </summary>
    [ContextMenu("Create Interact Prompt")]
    public void CreateInteractPrompt()
    {
        // 이미 있는지 확인
        Transform existing = transform.Find("InteractPrompt");
        if (existing != null)
        {
            promptCanvas = existing.GetComponent<Canvas>();
            promptTextComponent = existing.GetComponentInChildren<TextMeshProUGUI>();
            Debug.Log("InteractPrompt already exists.");
            return;
        }
        
        // Canvas 생성
        GameObject canvasObj = new GameObject("InteractPrompt");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0, promptHeight, 0);
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        // Canvas 컴포넌트
        promptCanvas = canvasObj.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        
        // RectTransform 설정
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(200, 50);
        
        // Text 생성
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(canvasObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale = Vector3.one;
        
        promptTextComponent = textObj.AddComponent<TextMeshProUGUI>();
        promptTextComponent.text = promptText;
        promptTextComponent.fontSize = 36;
        promptTextComponent.alignment = TextAlignmentOptions.Center;
        promptTextComponent.color = Color.white;
        
        // Outline 추가
        promptTextComponent.outlineWidth = 0.2f;
        promptTextComponent.outlineColor = Color.black;
        
        // RectTransform 설정
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(200, 50);
        textRect.anchoredPosition = Vector2.zero;

        // DialogueTrigger에 자동 할당
        DialogueTriggerByID trigger = GetComponent<DialogueTriggerByID>();
        if (trigger != null)
        {
            // Reflection으로 private 필드에 접근 (Editor only)
#if UNITY_EDITOR
            var field = typeof(DialogueTriggerByID).GetField("interactPrompt", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(trigger, canvasObj);
                UnityEditor.EditorUtility.SetDirty(trigger);
            }
#endif
        }

        Debug.Log("InteractPrompt created successfully!");
    }
    
    /// <summary>
    /// InteractPrompt 제거
    /// </summary>
    [ContextMenu("Remove Interact Prompt")]
    public void RemoveInteractPrompt()
    {
        Transform existing = transform.Find("InteractPrompt");
        if (existing != null)
        {
            DestroyImmediate(existing.gameObject);
            promptCanvas = null;
            promptTextComponent = null;
            Debug.Log("InteractPrompt removed.");
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(NPCSetupHelper))]
public class NPCSetupHelperEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        NPCSetupHelper helper = (NPCSetupHelper)target;
        
        UnityEditor.EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Interact Prompt", GUILayout.Height(30)))
        {
            helper.CreateInteractPrompt();
        }
        
        if (GUILayout.Button("Remove Interact Prompt", GUILayout.Height(30)))
        {
            helper.RemoveInteractPrompt();
        }
    }
}
#endif
