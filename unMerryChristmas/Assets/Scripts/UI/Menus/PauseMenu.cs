using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject  _pausePanel;
    [SerializeField] private InputAction _togglePauseAction;

    private bool _paused;

    private void OnEnable()
    {
        _togglePauseAction.Enable();
        _togglePauseAction.started += _ => Toggle();
    }

    private void OnDisable()
    {
        _togglePauseAction.started -= _ => Toggle();
        _togglePauseAction.Disable();
    }

    private void Start() => _pausePanel.SetActive(false);

    public void Toggle()
    {
        _paused = !_paused;
        _pausePanel.SetActive(_paused);
        Time.timeScale = _paused ? 0f : 1f;
        PlayerFreezeManager.Instance?.SetMenuFrozen(_paused);
    }

    // ── Button callbacks (wire in Inspector) ──────────────────────────────────

    public void OnResumePressed()
    {
        if (_paused) Toggle();
    }

    public void OnOpenBookPressed()
    {
        if (_paused) Toggle();              // resume first so time runs again
        UIManager.Instance?.ToggleBook();
    }

    public void OnQuitToMenuPressed()
    {
        Time.timeScale = 1f;               // restore time before loading
        SceneManager.LoadScene("MainMenu");
    }
}
