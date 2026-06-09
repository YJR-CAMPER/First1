using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 개별 세이브 슬롯 UI
/// 슬롯 번호, 타임스탬프, 플레이 위치를 표시
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI slotNumberText;
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private TextMeshProUGUI locationText;
    [SerializeField] private GameObject emptyLabel;
    [SerializeField] private GameObject dataContainer;
    [SerializeField] private Button slotButton;
    [SerializeField] private Image locationIcon;

    private int slotIndex;
    private bool hasData;

    public event Action<int> OnSlotClicked;

    public void Initialize(int index)
    {
        slotIndex = index;

        if (slotButton != null)
        {
            slotButton.onClick.AddListener(() => OnSlotClicked?.Invoke(slotIndex));
        }

        Refresh();
    }

    public void Refresh()
    {
        if (SaveManager.Instance == null)
            return;

        hasData = SaveManager.Instance.SlotExists(slotIndex);

        if (hasData)
        {
            ShowSlotData();
        }
        else
        {
            ShowEmpty();
        }
    }

    private void ShowSlotData()
    {
        var meta = SaveManager.Instance.GetSlotMetadata(slotIndex);
        if (meta == null)
        {
            ShowEmpty();
            return;
        }

        if (emptyLabel != null)
            emptyLabel.SetActive(false);

        if (dataContainer != null)
            dataContainer.SetActive(true);

        if (slotNumberText != null)
            slotNumberText.text = $"Slot {slotIndex + 1}";

        if (timestampText != null)
            timestampText.text = meta.timestamp;

        if (locationText != null)
        {
            string location = string.IsNullOrEmpty(meta.sceneName) ? "---" : meta.sceneName;
            locationText.text = location;
        }

        if (slotButton != null)
            slotButton.interactable = true;
    }

    private void ShowEmpty()
    {
        if (emptyLabel != null)
            emptyLabel.SetActive(true);

        if (dataContainer != null)
            dataContainer.SetActive(false);

        if (slotNumberText != null)
            slotNumberText.text = $"Slot {slotIndex + 1}";

        if (slotButton != null)
            slotButton.interactable = true;
    }

    /// <summary>
    /// Load 모드에서 빈 슬롯은 비활성화
    /// </summary>
    public void SetLoadMode(bool isLoadMode)
    {
        if (slotButton != null && isLoadMode && !hasData)
        {
            slotButton.interactable = false;
        }
    }
}
