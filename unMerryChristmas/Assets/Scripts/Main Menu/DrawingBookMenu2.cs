using UnityEngine;
using System.Collections;

public class DrawingBookMenu2 : MonoBehaviour
{
    [Header("Book Root")]
    [SerializeField] private RectTransform _bookRoot;

    [Header("Dark Overlay")]
    [SerializeField] private CanvasGroup _darkOverlay;
    [SerializeField] private float _overlayMaxAlpha = 0.6f;

    [Header("Animation")]
    [SerializeField] private float _slideDuration = 0.5f;
    [SerializeField] private AnimationCurve _slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _slideOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sub-systems")]
    [SerializeField] private BookmarkTabGroup _tabGroup;
    [SerializeField] private PageFlipController _pageFlip;

    [Header("Page References")]
    [SerializeField] private TutorialPage _tutorialPage;
    [SerializeField] private DrawingPagesPage _drawingPagesPage;
    [SerializeField] private TaskListPage _taskListPage;

    [Tooltip("The page the book opens to the first time the elf picks it up. " +
             "Assign your Tasks page (Page_Content_Tasks). Resolved by reference, " +
             "so its tab position can change freely. If unassigned, the book just " +
             "opens to its default tab.")]
    [SerializeField] private PageContent _bookFoundPage;

    [Header("Audio")]
    [SerializeField] private FMODUnity.EventReference _bookOpenSound;

    [Header("Progression Gate")]
    [Tooltip("When true, the book can only be opened after the elf has found it " +
             "in the Back Yard (BookState.HasFoundBook). Leave OFF until a save " +
             "system persists BookState, otherwise loading a later scene directly " +
             "would lock the menu. The pickup auto-open is unaffected either way.")]
    [SerializeField] private bool _requireBookFound = false;

    // ── Page index constants ──────────────────────────────────────────────────
    // TODO(HUD): Wire these indices to the actual tab/page order once the
    //            BookmarkTabGroup and PageFlipController APIs are finalised.
    //            Call OpenToPage(BookPage.X) from HUD buttons or in-world events.
    public enum BookPage
    {
        Play         = 0,
        Settings     = 1,
        Tutorial     = 2,   // Commands Page
        DrawingPages = 3,   // Collectables Page
        Tasks        = 4,   
    }

    public bool IsOpen => _isOpen;

    private Vector2 _hiddenPos;
    private Vector2 _visiblePos;
    private bool _isOpen;
    private Coroutine _slideRoutine;

    private void Awake()
    {
        _visiblePos = _bookRoot.anchoredPosition;
        _hiddenPos = _visiblePos + Vector2.down * (_bookRoot.rect.height + 200f);

        _bookRoot.anchoredPosition = _hiddenPos;
        _bookRoot.gameObject.SetActive(false);

        if (_darkOverlay != null)
        {
            _darkOverlay.alpha = 0f;
            _darkOverlay.blocksRaycasts = false;
        }
    }

    public void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }

    /// <summary>
    /// Opens the book and navigates directly to a specific page.
    /// </summary>
    public void OpenToPage(BookPage page)
    {
        Open(); 
        if (!_isOpen) return;
        int targetIndex = (int)page;

        if (_pageFlip != null)
        {
            _pageFlip.ShowPageImmediate(targetIndex);
        }

        if (_tabGroup != null)
        {
            _tabGroup.SelectTab(targetIndex);
        }


    }

    /// <summary>
    /// Opens the book to <see cref="_bookFoundPage"/> (the drawing-book pickup
    /// landing page), found by reference so its tab order doesn't matter. Falls
    /// back to the default tab if the page is unassigned or not in this book.
    /// </summary>
    public void OpenToBookFoundPage()
    {
        Open();
        if (!_isOpen) return;
        StartCoroutine(SelectBookFoundPageDeferred());
    }

    // Wait one frame so the tab group / page-flip Start() (which resets to tab 0
    // on the book's first activation) runs BEFORE we jump to the found page —
    // otherwise it would snap back off the Tasks page.
    private IEnumerator SelectBookFoundPageDeferred()
    {
        yield return null;
        int index = _pageFlip != null ? _pageFlip.IndexOf(_bookFoundPage) : -1;
        if (index >= 0) _tabGroup?.SelectTab(index);
    }

    public void Open()
    {
        if (_isOpen) return;
        if (_requireBookFound && !BookState.HasFoundBook) return;
        _isOpen = true;

        if (!_bookOpenSound.IsNull)
        {
            FMODUnity.RuntimeManager.PlayOneShot(_bookOpenSound);
        }

        _tutorialPage?.Refresh();
        _drawingPagesPage?.Refresh();
        _taskListPage?.Refresh();

        PlayerFreezeManager.Instance?.SetMenuFrozen(true);

        _bookRoot.anchoredPosition = _hiddenPos;
        _bookRoot.gameObject.SetActive(true);

        if (_slideRoutine != null) StopCoroutine(_slideRoutine);
        _slideRoutine = StartCoroutine(
            SlideRoutine(_hiddenPos, _visiblePos, _slideInCurve, _overlayMaxAlpha));
    }

    public void Close()
    {
        if (!_isOpen) return;
        _isOpen = false;

        PlayerFreezeManager.Instance?.SetMenuFrozen(false);

        if (_slideRoutine != null) StopCoroutine(_slideRoutine);
        _slideRoutine = StartCoroutine(
            SlideRoutine(_visiblePos, _hiddenPos, _slideOutCurve, 0f, () =>
            {
                _bookRoot.gameObject.SetActive(false);
                if (_darkOverlay != null)
                    _darkOverlay.blocksRaycasts = false;
            }));
    }

    private IEnumerator SlideRoutine(Vector2 from, Vector2 to,
                                     AnimationCurve curve,
                                     float overlayTargetAlpha,
                                     System.Action onComplete = null)
    {
        float startOverlayAlpha = _darkOverlay != null ? _darkOverlay.alpha : 0f;

        if (_darkOverlay != null)
            _darkOverlay.blocksRaycasts = overlayTargetAlpha > 0f;

        float elapsed = 0f;
        while (elapsed < _slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _slideDuration);
            float tC = curve.Evaluate(t);

            _bookRoot.anchoredPosition = Vector2.LerpUnclamped(from, to, tC);

            if (_darkOverlay != null)
                _darkOverlay.alpha = Mathf.Lerp(startOverlayAlpha, overlayTargetAlpha, tC);

            yield return null;
        }

        _bookRoot.anchoredPosition = to;
        if (_darkOverlay != null)
            _darkOverlay.alpha = overlayTargetAlpha;

        onComplete?.Invoke();
    }
}