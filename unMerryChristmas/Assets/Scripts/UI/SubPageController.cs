using UnityEngine;
using System.Collections;

public class SubPageController : MonoBehaviour
{
    [SerializeField] private RectTransform _leftFlipPage;
    [SerializeField] private RectTransform _rightFlipPage;
    [SerializeField] private GameObject[] _subPages;

    [Header("Flip Settings")]
    [SerializeField] private float _halfFlipDuration = 0.25f;
    [SerializeField] private AnimationCurve _foldCurve;
    [SerializeField] private AnimationCurve _unfoldCurve;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeDuration = 0.15f;

    private int _currentSub = 0;
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

    private void Start()
    {
        for (int i = 0; i < _subPages.Length; i++)
        {
            _subPages[i].SetActive(true);
            var cg = GetOrAddCanvasGroup(_subPages[i]);
            cg.alpha = i == 0 ? 1f : 0f;
            cg.blocksRaycasts = i == 0;
            cg.interactable = i == 0;
        }
    }

    public void Next()
    {
        if (_isFlipping || _currentSub >= _subPages.Length - 1) return;
        StartCoroutine(FlipRoutine(_currentSub + 1, flipRight: true));
    }

    public void Prev()
    {
        if (_isFlipping || _currentSub <= 0) return;
        StartCoroutine(FlipRoutine(_currentSub - 1, flipRight: false));
    }

    private IEnumerator FlipRoutine(int targetIndex, bool flipRight)
    {
        _isFlipping = true;

        RectTransform flipPage = flipRight ? _rightFlipPage : _leftFlipPage;
        float midAngle = flipRight ? -90f : 90f;
        float endAngle = flipRight ? -180f : 180f;

        // Phase 1 — fold to 90
        yield return RotatePage(flipPage, 0f, midAngle, _halfFlipDuration, _foldCurve);

        // Hide old subpage instantly at the midpoint
        CanvasGroup oldCG = GetOrAddCanvasGroup(_subPages[_currentSub]);
        oldCG.alpha = 0f;
        oldCG.blocksRaycasts = false;
        oldCG.interactable = false;

        _currentSub = targetIndex;

        // Prepare new subpage — invisible until unfold completes
        CanvasGroup newCG = GetOrAddCanvasGroup(_subPages[_currentSub]);
        newCG.alpha = 0f;
        newCG.blocksRaycasts = false;
        newCG.interactable = false;

        // Phase 2 — unfold back to 0
        yield return RotatePage(flipPage, midAngle, endAngle, _halfFlipDuration, _unfoldCurve);

        flipPage.localEulerAngles = Vector3.zero;

        // Fade in new subpage after flip completes
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            newCG.alpha = Mathf.Clamp01(elapsed / _fadeDuration);
            yield return null;
        }
        newCG.alpha = 1f;
        newCG.blocksRaycasts = true;
        newCG.interactable = true;

        _isFlipping = false;
    }

    private IEnumerator RotatePage(RectTransform page, float from, float to,
                                    float duration, AnimationCurve curve)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            page.localEulerAngles = new Vector3(0f, Mathf.LerpUnclamped(from, to, curve.Evaluate(t)), 0f);
            yield return null;
        }
        page.localEulerAngles = new Vector3(0f, to, 0f);
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}