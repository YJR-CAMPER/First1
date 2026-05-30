using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 일시정지 메뉴 UI 컨트롤러
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private void Awake()
    {
        SetupButtons();
        HideAllPanels();
    }

    private void OnEnable()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPauseStateChanged += HandlePauseStateChanged;
        }
    }

    private void OnDisable()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.OnPauseStateChanged -= HandlePauseStateChanged;
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
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnPauseInput()
    {
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
    }

    private void HideAllPanels()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

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

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
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
