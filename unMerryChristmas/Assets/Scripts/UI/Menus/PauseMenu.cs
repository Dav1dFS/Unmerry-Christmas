using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Target")]
    [SerializeField] private GameObject _pauseCanvasObject; // ◄── Drag "Pause_Canvas" here!
    
    [Header("Input Setup")]
    [SerializeField] private InputActionProperty _togglePauseAction;

    private bool _paused;

    private void OnEnable()
    {
        _togglePauseAction.action?.Enable();
        if (_togglePauseAction.action != null)
        {
            _togglePauseAction.action.started += OnPauseActionTriggered;
        }
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
        if (_pauseCanvasObject != null)
        {
            _pauseCanvasObject.SetActive(false); // Hide the whole canvas on wake
        }
    }

    private void OnPauseActionTriggered(InputAction.CallbackContext ctx)
    {
        Toggle();
    }

    public void Toggle()
    {
        _paused = !_paused;
        
        if (_pauseCanvasObject != null)
        {
            _pauseCanvasObject.SetActive(_paused);
        }

        Time.timeScale = _paused ? 0f : 1f;
        PlayerFreezeManager.Instance?.SetMenuFrozen(_paused);
    }

    public void OnResumePressed()
    {
        if (_paused) Toggle();
    }

    public void OnOpenBookPressed()
    {
        if (_paused) Toggle();
        string text = "ToggleBook";
        // UIManager.Instance?.ToggleBook();
    }

    public void OnQuitToMenuPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}