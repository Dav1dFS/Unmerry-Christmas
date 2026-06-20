using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject _pauseCanvasObject;
    [SerializeField] private GameObject _mainPanel;      // contains Resume / Book / Settings / Quit buttons
    [SerializeField] private GameObject _settingsPanel;  // contains SettingsPanelController

    [Header("Input Setup")]
    [SerializeField] private InputActionProperty _togglePauseAction;

    private bool _paused;

    private void OnEnable()
    {
        _togglePauseAction.action?.Enable();
        if (_togglePauseAction.action != null)
            _togglePauseAction.action.started += OnPauseActionTriggered;
    }

    private void OnDisable()
    {
        if (_togglePauseAction.action != null)
        {
            _togglePauseAction.action.started -= OnPauseActionTriggered;
            _togglePauseAction.action.Disable();
        }
    }

    private void Start()
    {
        _paused = false;
        if (_pauseCanvasObject != null) _pauseCanvasObject.SetActive(false);
    }

    private void OnPauseActionTriggered(InputAction.CallbackContext ctx) => Toggle();

    public void Toggle()
    {
        _paused = !_paused;

        if (_pauseCanvasObject != null) _pauseCanvasObject.SetActive(_paused);

        // Always show main panel when opening; hide settings panel
        if (_paused)
        {
            _mainPanel?.SetActive(true);
            _settingsPanel?.SetActive(false);
        }

        Time.timeScale = _paused ? 0f : 1f;
        PlayerFreezeManager.Instance?.SetMenuFrozen(_paused);
    }

    public void OnResumePressed()    { if (_paused) Toggle(); }
    public void OnOpenBookPressed()  { if (_paused) Toggle(); }

    // ── Settings navigation ────────────────────────────────────────────────
    public void OnSettingsPressed()
    {
        _mainPanel?.SetActive(false);
        _settingsPanel?.SetActive(true);
    }

    public void OnSettingsBackPressed()
    {
        _settingsPanel?.SetActive(false);
        _mainPanel?.SetActive(true);
    }

    public void OnQuitToMenuPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
