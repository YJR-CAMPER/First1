/// <summary>
/// 캐릭터 표정 유형
/// enum으로 고정하여 오타 방지 및 최적화
/// </summary>
public enum EmotionType
{
    기본,
    미소,
    놀람,
    분노,
    슬픔,
    당황,
    의심
}

/// <summary>
/// 초상화가 표시될 무대 슬롯 (최대 3명)
/// </summary>
public enum PortraitSlot
{
    없음,   // 내레이션 등 초상화 미표시
    좌,
    중,
    우
}

/// <summary>
/// 말하는 슬롯에 재생할 연출 효과
/// </summary>
public enum PortraitEffect
{
    없음,
    흔들림,
    튀어오름,
    색플래시
}
