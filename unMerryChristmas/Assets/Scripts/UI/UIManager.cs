using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    // FreeOutline Outline 2 watches rendering layer bit 2.
    // Unity's default renderingLayerMask for all renderers is 0xFFFFFFFF (all bits set),
    // so every object in the scene would be permanently outlined without this strip.
    // InteractableHighlight.SetHighlighted(true) re-adds bit 2 on demand.
    private const uint OutlineInteractBit = 1u << 2;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StripOutlineLayer(); // initial scene
    }

    private void OnEnable()
    {
        AbilityTokenManager.OnAbilityUnlocked += OnAbilityUnlocked;
        BackYardTaskTracker.OnTaskCompleted += OnTaskCompleted;
        SceneManager.sceneLoaded += OnSceneLoaded;
        _pauseAction.Enable();
        _pauseAction.started += OnPausePressed;
    }

    private void OnDisable()
    {
        AbilityTokenManager.OnAbilityUnlocked -= OnAbilityUnlocked;
        BackYardTaskTracker.OnTaskCompleted -= OnTaskCompleted;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        _pauseAction.started -= OnPausePressed;
        _pauseAction.Disable();
    }

    // Runs after every scene load (including additive). Strips bit 2 from every
    // renderer so nothing is outlined by default. Covers objects that don't have
    // an InteractableHighlight component (collectables, tokens, terrain, etc.).
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        => StripOutlineLayer();

    private void StripOutlineLayer()
    {
        var renderers = FindObjectsByType<Renderer>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Renderer r in renderers)
            r.renderingLayerMask &= ~OutlineInteractBit;
    }

    // ── Input ────────────────────────────────────────────────────────────────

    private void OnPausePressed(InputAction.CallbackContext ctx)
        => _drawingBookMenu?.Toggle();

    // ── Public API ───────────────────────────────────────────────────────────

    public void ToggleBook() => _drawingBookMenu?.Toggle();
    public void OpenBook()   => _drawingBookMenu?.Open();
    public void CloseBook()  => _drawingBookMenu?.Close();

    public void LoadMainMenu()
    {
        _drawingBookMenu?.Close();
        PlayerFreezeManager.Instance?.SetMenuFrozen(false);
        SceneManager.LoadScene("MainMenu");
    }

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