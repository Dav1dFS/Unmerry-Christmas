using UnityEngine;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TutorialToast      _tutorialToast;
    [SerializeField] private AbilityUnlockToast _abilityUnlockToast;
    [SerializeField] private TaskCompleteFlash  _taskCompleteFlash;
    [SerializeField] private ContextualHint     _contextualHint;

    [Header("Menus")]
    [SerializeField] private DrawingBookMenu _drawingBookMenu;
    [SerializeField] private PauseMenu       _pauseMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        AbilityTokenManager.OnAbilityUnlocked   += OnAbilityUnlocked;
        BackYardTaskTracker.OnTaskCompleted      += OnTaskCompleted;
    }

    private void OnDisable()
    {
        AbilityTokenManager.OnAbilityUnlocked   -= OnAbilityUnlocked;
        BackYardTaskTracker.OnTaskCompleted      -= OnTaskCompleted;
    }

    // ── Gameplay event handlers ──────────────────────────────────────────────

    private void OnAbilityUnlocked(PlayerAbility ability)
        => _abilityUnlockToast?.Show(ability);

    private void OnTaskCompleted(BackYardTaskTracker.TaskId id)
        => _taskCompleteFlash?.Show(id.ToString());

    // ── Public API called by gameplay scripts ────────────────────────────────

    public void ShowTutorialPrompt(string text)  => _tutorialToast?.Show(text);
    public void ShowContextHint(string text)     => _contextualHint?.Show(text);
    public void HideContextHint()               => _contextualHint?.Hide();
    public void ToggleBook()                    => _drawingBookMenu?.Toggle();
    public void TogglePause()                   => _pauseMenu?.Toggle();
}
