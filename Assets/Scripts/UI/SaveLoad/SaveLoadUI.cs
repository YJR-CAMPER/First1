using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 세이브/로드 패널 UI
/// Save/Load 모드 전환, 3슬롯 표시, 덮어쓰기 확인
/// 
/// Setup:
///   PauseMenuUI에서 saveLoadPanel을 열어주는 버튼 추가
///   또는 독립적으로 ESC 메뉴에 배치
/// </summary>
public class SaveLoadUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject saveLoadPanel;
    [SerializeField] private GameObject confirmPanel;

    [Header("Slots")]
    [SerializeField] private SaveSlotUI[] slots;

    [Header("Mode Toggle")]
    [SerializeField] private Button saveTabButton;
    [SerializeField] private Button loadTabButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image saveTabHighlight;
    [SerializeField] private Image loadTabHighlight;

    [Header("Confirm Dialog")]
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    [Header("Tab Colors")]
    [SerializeField] private Color activeTabColor = Color.white;
    [SerializeField] private Color inactiveTabColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private bool isSaveMode = true;
    private int pendingSlotIndex = -1;

    /// <summary>
    /// 외부에서 패널을 닫을 때 호출할 콜백
    /// PauseMenuUI에서 구독하여 이전 패널로 복귀
    /// </summary>
    public event System.Action OnBackRequested;

    private void Awake()
    {
        SetupButtons();
        InitializeSlots();
        HideConfirm();
    }

    private void SetupButtons()
    {
        if (saveTabButton != null)
            saveTabButton.onClick.AddListener(() => SetMode(true));

        if (loadTabButton != null)
            loadTabButton.onClick.AddListener(() => SetMode(false));

        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(OnConfirmYes);

        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(HideConfirm);

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

    /// <summary>
    /// Save 모드로 패널 열기
    /// </summary>
    public void OpenSave()
    {
        SetMode(true);
        Show();
    }

    /// <summary>
    /// Load 모드로 패널 열기
    /// </summary>
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
        HideConfirm();
    }

    public void Hide()
    {
        if (saveLoadPanel != null)
            saveLoadPanel.SetActive(false);

        HideConfirm();
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
            {
                ShowOverwriteConfirm(slotIndex);
            }
            else
            {
                ExecuteSave(slotIndex);
            }
        }
        else
        {
            if (SaveManager.Instance.SlotExists(slotIndex))
            {
                ShowLoadConfirm(slotIndex);
            }
        }
    }

    private void ExecuteSave(int slotIndex)
    {
        if (SaveManager.Instance == null)
            return;

        SaveManager.Instance.Save(slotIndex);
        RefreshAllSlots();
        HideConfirm();
    }

    private async void ExecuteLoad(int slotIndex)
    {
        if (SaveManager.Instance == null)
            return;

        bool success = await SaveManager.Instance.LoadAsync(slotIndex);
        if (success)
        {
            HideConfirm();
            Hide();

            if (PauseManager.Instance != null)
            {
                PauseManager.Instance.Resume();
            }

            // 맵 복원은 LoadAsync 내부의 Addressables 맵 로드로 처리된다.
            // 과거에는 여기서 SceneManager.LoadScene을 호출했으나,
            // 복원 직후 씬을 새로 로드하면 복원 상태가 날아가는 버그였으므로 제거함.
        }
    }

    // -- Confirm Dialog --

    private void ShowOverwriteConfirm(int slotIndex)
    {
        pendingSlotIndex = slotIndex;

        if (confirmText != null)
        {
            var meta = SaveManager.Instance.GetSlotMetadata(slotIndex);
            string time = meta != null ? meta.timestamp : "";
            confirmText.text = $"Slot {slotIndex + 1} ({time})\n\nOverwrite?";
        }

        if (confirmPanel != null)
            confirmPanel.SetActive(true);
    }

    private void ShowLoadConfirm(int slotIndex)
    {
        pendingSlotIndex = slotIndex;

        if (confirmText != null)
        {
            var meta = SaveManager.Instance.GetSlotMetadata(slotIndex);
            string time = meta != null ? meta.timestamp : "";
            confirmText.text = $"Slot {slotIndex + 1} ({time})\n\nLoad?";
        }

        if (confirmPanel != null)
            confirmPanel.SetActive(true);
    }

    private void OnConfirmYes()
    {
        if (pendingSlotIndex < 0)
            return;

        if (isSaveMode)
        {
            ExecuteSave(pendingSlotIndex);
        }
        else
        {
            ExecuteLoad(pendingSlotIndex);
        }

        pendingSlotIndex = -1;
    }

    private void HideConfirm()
    {
        pendingSlotIndex = -1;

        if (confirmPanel != null)
            confirmPanel.SetActive(false);
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
