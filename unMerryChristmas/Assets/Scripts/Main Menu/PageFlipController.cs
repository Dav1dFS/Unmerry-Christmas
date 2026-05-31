using UnityEngine;
using System.Collections;

public class PageFlipController : MonoBehaviour
{
    [Header("Page Visuals")]
    [SerializeField] private RectTransform _leftPage;
    [SerializeField] private RectTransform _rightPage;

    [Header("Page Contents")]
    [SerializeField] private PageContent[] _pages;

    [Header("Flip Settings")]
    [SerializeField] private float _halfFlipDuration = 0.25f;
    [SerializeField] private AnimationCurve _foldCurve;
    [SerializeField] private AnimationCurve _unfoldCurve;

    public bool IsFlipping => _isFlipping;

    private int _currentIndex = 0;
    private bool _isFlipping = false;

    private void Awake()
    {
        if (_foldCurve == null || _foldCurve.length == 0)
            _foldCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2f),
                new Keyframe(1f, 1f, 2f, 0f));

        if (_unfoldCurve == null || _unfoldCurve.length == 0)
            _unfoldCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2f),
                new Keyframe(1f, 1f, 2f, 0f));
    }

    public void ShowPageImmediate(int index)
    {
        _currentIndex = index;
        for (int i = 0; i < _pages.Length; i++)
        {
            if (i == index) _pages[i].ShowImmediate();
            else _pages[i].Hide();
        }
        _leftPage.localEulerAngles = Vector3.zero;
        _rightPage.localEulerAngles = Vector3.zero;
    }

    // onComplete fires when the flip animation finishes — used by BookmarkTabGroup for queuing
    public void FlipToPage(int targetIndex, System.Action onComplete = null)
    {
        if (_isFlipping || targetIndex == _currentIndex)
        {
            onComplete?.Invoke();
            return;
        }
        bool flipRight = targetIndex > _currentIndex;
        StartCoroutine(FlipRoutine(targetIndex, flipRight, onComplete));
    }

    public void FocusCurrentPage()
    {
        if (_currentIndex >= 0 && _currentIndex < _pages.Length)
            _pages[_currentIndex].Show();
    }

    private IEnumerator FlipRoutine(int targetIndex, bool flipRight, System.Action onComplete)
    {
        _isFlipping = true;

        RectTransform flipPage = flipRight ? _rightPage : _leftPage;
        float midAngle = flipRight ? -90f : 90f;
        float endAngle = flipRight ? -180f : 180f;

        yield return RotatePage(flipPage, 0f, midAngle, _halfFlipDuration, _foldCurve);

        _pages[_currentIndex].Hide();
        _currentIndex = targetIndex;
        _pages[_currentIndex].Show();

        yield return RotatePage(flipPage, midAngle, endAngle, _halfFlipDuration, _unfoldCurve);

        flipPage.localEulerAngles = Vector3.zero;
        _isFlipping = false;

        onComplete?.Invoke();
    }

    private IEnumerator RotatePage(RectTransform page, float from, float to,
                                    float duration, AnimationCurve curve)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float angle = Mathf.LerpUnclamped(from, to, curve.Evaluate(t));
            page.localEulerAngles = new Vector3(0f, angle, 0f);
            yield return null;
        }
        page.localEulerAngles = new Vector3(0f, to, 0f);
    }
}