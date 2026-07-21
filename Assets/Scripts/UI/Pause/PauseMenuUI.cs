using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 일시정지 메뉴 UI 컨트롤러
/// Resume, Settings, Save/Load, MainMenu, Quit 버튼 관리
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Save/Load")]
    [SerializeField] private SaveLoadUI saveLoadUI;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button saveLoadButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool isSubscribed;

    private void Awake()
    {
        SetupButtons();
        HideAllPanels();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPauseStateChanged -= HandlePauseStateChanged;
            isSubscribed = false;
        }

        if (saveLoadUI != null)
        {
            saveLoadUI.OnBackRequested -= OnSaveLoadBack;
        }
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        if (PauseManager.Instance == null)
            return;

        PauseManager.Instance.OnPauseStateChanged += HandlePauseStateChanged;
        isSubscribed = true;

        if (saveLoadUI != null)
        {
            saveLoadUI.OnBackRequested += OnSaveLoadBack;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            OnPauseInput();
        }
    }

    private void SetupButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (saveLoadButton != null)
            saveLoadButton.onClick.AddListener(OnSaveLoadClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnPauseInput()
    {
        if (saveLoadUI != null && saveLoadUI.IsOpen)
        {
            saveLoadUI.Hide();
            ShowPauseMenu();
            return;
        }

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
            return;
        }

        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.TogglePause();
        }
    }

    private void HandlePauseStateChanged(object sender, PauseStateChangedEventArgs e)
    {
        if (e.IsPaused)
            ShowPauseMenu();
        else
            HideAllPanels();
    }

    private void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (saveLoadUI != null)
            saveLoadUI.Hide();
    }

    private void HideAllPanels()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (saveLoadUI != null)
            saveLoadUI.Hide();
    }

    // -- Button Handlers --

    private void OnResumeClicked()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.Resume();
        }
    }

    private void OnSettingsClicked()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void OnSaveLoadClicked()
    {
        if (saveLoadUI != null)
            saveLoadUI.OpenSave();
    }

    public void CloseSettings()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    private void OnSaveLoadBack()
    {
        ShowPauseMenu();
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}