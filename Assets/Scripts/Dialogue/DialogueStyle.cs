using UnityEngine;

/// <summary>
/// 대화 스타일 설정 (패널 외형, 색상, 효과)
/// </summary>
[System.Serializable]
public class DialogueStyle
{
    public DialogueStyleType styleType;
    public Sprite panelSprite;
    public Color textColor = Color.black;
    public Color speakerNameColor = Color.black;
    public bool enableShake = false;
    public float shakeIntensity = 5f;
    public float panelScale = 1.0f;
}
