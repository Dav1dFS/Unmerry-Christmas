using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TutorialToast _tutorialToast;
    [SerializeField] private AbilityUnlockToast _abilityUnlockToast;
    [SerializeField] private TaskCompleteFlash _taskCompleteFlash;
    [SerializeField] private ContextualHint _contextualHint;

    [Header("Menus")]
    [SerializeField] private DrawingBookMenu2 _drawingBookMenu;

    [Header("Input")]
    [SerializeField] private InputAction _pauseAction;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        AbilityTokenManager.OnAbilityUnlocked += OnAbilityUnlocked;
        BackYardTaskTracker.OnTaskCompleted += OnTaskCompleted;
        _pauseAction.Enable();
        _pauseAction.started += OnPausePressed;
    }

    private void OnDisable()
    {
        AbilityTokenManager.OnAbilityUnlocked -= OnAbilityUnlocked;
        BackYardTaskTracker.OnTaskCompleted -= OnTaskCompleted;
        _pauseAction.started -= OnPausePressed;
        _pauseAction.Disable();
    }

    // ── Input ────────────────────────────────────────────────────────────────

    private void OnPausePressed(InputAction.CallbackContext ctx)
        => _drawingBookMenu?.Toggle();

    // ── Public API ───────────────────────────────────────────────────────────

    public void ToggleBook() => _drawingBookMenu?.Toggle();
    public void OpenBook() => _drawingBookMenu?.Open();
    public void CloseBook() => _drawingBookMenu?.Close();

    public void ShowTutorialPrompt(string text) => _tutorialToast?.Show(text);
    public void ShowContextHint(string text) => _contextualHint?.Show(text);
    public void HideContextHint() => _contextualHint?.Hide();

    // ── Gameplay events ──────────────────────────────────────────────────────

    private void OnAbilityUnlocked(PlayerAbility ability)
        => _abilityUnlockToast?.Show(ability);

    private void OnTaskCompleted(BackYardTaskTracker.TaskId id)
        => _taskCompleteFlash?.Show(id.ToString());
}