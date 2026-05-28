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
    public void OpenBook()   => _drawingBookMenu?.Open();
    public void CloseBook()  => _drawingBookMenu?.Close();

    // ── Page-targeted book opening ────────────────────────────────────────────
    // TODO(HUD): The HUD colleague should call these from the on-screen buttons
    //            (or keyboard shortcuts) mapped to each section.
    //
    //   "Abilities" HUD button  → OpenBookToAbilitiesPage()
    //   "Tasks" HUD button      → OpenBookToTasksPage()
    //   "Pages" HUD button      → OpenBookToDrawingPagesPage()
    //
    // These can also be called reactively — e.g. open to Tasks when a task
    // completes, or to Abilities when an ability is unlocked.

    /// <summary>Opens the DrawingBook directly to the Abilities (Tutorial) page.</summary>
    public void OpenBookToAbilitiesPage()
        => _drawingBookMenu?.OpenToPage(DrawingBookMenu2.BookPage.Tutorial);

    /// <summary>Opens the DrawingBook directly to the Task List page.</summary>
    public void OpenBookToTasksPage()
        => _drawingBookMenu?.OpenToPage(DrawingBookMenu2.BookPage.Tasks);

    /// <summary>Opens the DrawingBook directly to the Drawing Pages counter.</summary>
    public void OpenBookToDrawingPagesPage()
        => _drawingBookMenu?.OpenToPage(DrawingBookMenu2.BookPage.DrawingPages);

    // ─────────────────────────────────────────────────────────────────────────

    public void ShowTutorialPrompt(string text) => _tutorialToast?.Show(text);

    // ── Contextual hint (driven by Controller.ContextHint every frame) ────────
    // No changes needed here — Controller calls Show/Hide directly.
    public void ShowContextHint(string text) => _contextualHint?.Show(text);
    public void HideContextHint() => _contextualHint?.Hide();

    // ── Gameplay events ───────────────────────────────────────────────────────
    // These fire in response to static C# events from game systems.
    // The task system (BackYardTaskTracker) and ability system (AbilityTokenManager)
    // broadcast events; UIManager is the single subscriber that routes them to HUD.
    //
    // TODO(HUD): If the HUD needs to refresh a live task counter or ability list
    //            on these events, add those calls here alongside the existing toasts.

    private void OnAbilityUnlocked(PlayerAbility ability)
        => _abilityUnlockToast?.Show(ability);  // shows "New note added\n{ability}" toast

    private void OnTaskCompleted(BackYardTaskTracker.TaskId id)
        => _taskCompleteFlash?.Show(id.ToString()); // shows "Task complete\n{name}" flash
}