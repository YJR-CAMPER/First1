/// <summary>
/// 대화 선택지 데이터
/// </summary>
[System.Serializable]
public class DialogueChoiceExtended
{
    public string choiceText;
    public string nextDialogueId;
    public string eventName;
    public string conditionFlag;
    public bool requiredValue = true;
}
