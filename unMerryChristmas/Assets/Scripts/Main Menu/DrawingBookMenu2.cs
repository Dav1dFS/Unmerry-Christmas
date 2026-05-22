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

    public bool IsOpen => _isOpen;  //  o UIManager usa isto

    private Vector2 _hiddenPos;
    private Vector2 _visiblePos;
    private bool _isOpen;
    private Coroutine _slideRoutine;

    private void Awake()
    {
        _visiblePos = _bookRoot.anchoredPosition;
        _hiddenPos = _visiblePos + Vector2.down * (_bookRoot.rect.height + 200f);

        // Posiciona escondido ANTES de activar
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

    public void Open()
    {
        if (_isOpen) return;
        _isOpen = true;

        // Refresh das páginas ao abrir
        _tutorialPage?.Refresh();
        _drawingPagesPage?.Refresh();
        _taskListPage?.Refresh();

        // Congela o jogador
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

        // Descongela o jogador
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