using UnityEngine;

[CreateAssetMenu(fileName = "DialogueStylePreset", menuName = "대화 시스템/Dialogue Style Preset")]
public class DialogueStylePreset : ScriptableObject
{
    [Header("스타일 목록 / Style List")]
    [Tooltip("각 스타일마다 다른 대화창 스프라이트를 설정하세요 / Set different panel sprites for each style")]
    public DialogueStyle[] dialogueStyles = new DialogueStyle[]
    {
        new DialogueStyle
        {
            styleType = DialogueStyleType.일반,
            textColor = Color.black,
            panelScale = 1.0f
        },
        new DialogueStyle
        {
            styleType = DialogueStyleType.소리침,
            textColor = new Color(0.5f, 0f, 0f, 1f),
            enableShake = true,
            shakeIntensity = 8f,
            panelScale = 1.1f
        },
        new DialogueStyle
        {
            styleType = DialogueStyleType.생각,
            textColor = new Color(0.2f, 0.2f, 0.4f, 1f),
            panelScale = 1.0f
        }
    };
    
    public DialogueStyle GetStyle(DialogueStyleType styleType)
    {
        foreach (var style in dialogueStyles)
        {
            if (style.styleType == styleType)
                return style;
        }
        
        return dialogueStyles[0];
    }
}
