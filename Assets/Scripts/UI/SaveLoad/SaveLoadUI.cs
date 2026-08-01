using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 세이브/로드 패널 UI
/// Save/Load 모드 전환, 3슬롯 표시
/// 덮어쓰기/로드 확인은 공용 ConfirmDialog에 콜백으로 위임
/// </summary>
public class SaveLoadUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject saveLoadPanel;

    [Header("Slots")]
    [SerializeField] private SaveSlotUI[] slots;

    [Header("Mode Toggle")]
    [SerializeField] private Button saveTabButton;
    [SerializeField] private Button loadTabButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image saveTabHighlight;
    [SerializeField] private Image loadTabHighlight;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    [Header("Tab Colors")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private bool isSaveMode = true;

    /// <summary>
    /// 외부에서 패널을 닫을 때 호출할 콜백
    /// </summary>
    public event System.Action OnBackRequested;

    /// <summary>
    /// 패널이 열려있는지 (PauseMenuUI의 ESC 분기에서 사용)
    /// </summary>
    public bool IsOpen => saveLoadPanel != null && saveLoadPanel.activeSelf;

    private void Awake()
    {
        SetupButtons();
        InitializeSlots();
        Hide();
    }

    private void SetupButtons()
    {
        if (saveTabButton != null)
            saveTabButton.onClick.AddListener(() => SetMode(true));

        if (loadTabButton != null)
            loadTabButton.onClick.AddListener(() => SetMode(false));

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
    }

    private void InitializeSlots()
    {
        if (slots == null)
            return;

        int maxSlots = SaveManager.Instance != null ? SaveManager.Instance.MaxSlots : 3;

        for (int i = 0; i < slots.Length && i < maxSlots; i++)
        {
            if (slots[i] != null)
            {
                slots[i].Initialize(i);
                slots[i].OnSlotClicked += HandleSlotClicked;
            }
        }
    }

    // -- Public API --

    public void OpenSave()
    {
        SetMode(true);
        Show();
    }

    public void OpenLoad()
    {
        SetMode(false);
        Show();
    }

    public void Show()
    {
        if (saveLoadPanel != null)
            saveLoadPanel.SetActive(true);

        RefreshAllSlots();
    }

    public void Hide()
    {
        if (saveLoadPanel != null)
            saveLoadPanel.SetActive(false);
    }

    // -- Mode --

    private void SetMode(bool saveMode)
    {
        isSaveMode = saveMode;

        if (titleText != null)
            titleText.text = isSaveMode ? "SAVE" : "LOAD";

        if (saveTabHighlight != null)
            saveTabHighlight.color = isSaveMode ? activeTabColor : inactiveTabColor;

        if (loadTabHighlight != null)
            loadTabHighlight.color = isSaveMode ? inactiveTabColor : activeTabColor;

        RefreshAllSlots();
    }

    // -- Slot Interaction --

    private void HandleSlotClicked(int slotIndex)
    {
        if (SaveManager.Instance == null)
            return;

        if (isSaveMode)
        {
            if (SaveManager.Instance.SlotExists(slotIndex))
                RequestOverwriteConfirm(slotIndex);
            else
                ExecuteSave(slotIndex);
        }
        else
        {
            if (SaveManager.Instance.SlotExists(slotIndex))
                RequestLoadConfirm(slotIndex);
        }
    }

    private void RequestOverwriteConfirm(int slotIndex)
    {
        string message = BuildSlotMessage(slotIndex, "덮어쓸까요?");

        if (ConfirmDialog.Instance != null)
        {
            ConfirmDialog.Instance.Show(message, () => ExecuteSave(slotIndex));
        }
        else
        {
            // ConfirmDialog가 없으면 확인 없이 바로 실행 (안전 폴백)
            ExecuteSave(slotIndex);
        }
    }

    private void RequestLoadConfirm(int slotIndex)
    {
        string message = BuildSlotMessage(slotIndex, "불러올까요?");

        if (ConfirmDialog.Instance != null)
        {
            ConfirmDialog.Instance.Show(message, () => ExecuteLoad(slotIndex));
        }
        else
        {
            ExecuteLoad(slotIndex);
        }
    }

    private string BuildSlotMessage(int slotIndex, string action)
    {
        var meta = SaveManager.Instance.GetSlotMetadata(slotIndex);
        string time = meta != null ? meta.timestamp : "";
        return $"Slot {slotIndex + 1} ({time})\n\n{action}";
    }

    // -- Execution --

    private void ExecuteSave(int slotIndex)
    {
        if (SaveManager.Instance == null)
            return;

        SaveManager.Instance.Save(slotIndex);
        RefreshAllSlots();
    }

    private async void ExecuteLoad(int slotIndex)
    {
        if (SaveManager.Instance == null)
            return;

        try
        {
            bool success = await SaveManager.Instance.LoadAsync(slotIndex);
            if (success)
            {
                Hide();

                if (PauseManager.Instance != null)
                    PauseManager.Instance.Resume();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadUI] Load failed: {e.Message}");
        }
    }

    // -- Refresh --

    private void RefreshAllSlots()
    {
        if (slots == null)
            return;

        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.Refresh();
                slot.SetLoadMode(!isSaveMode);
            }
        }
    }

    // -- Navigation --

    private void OnBackClicked()
    {
        Hide();
        OnBackRequested?.Invoke();
    }
}
