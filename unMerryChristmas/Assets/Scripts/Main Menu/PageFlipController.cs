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

    // Curva que começa rápido e abranda no meio (chegar ao -90/90 com ease out)
    // e começa devagar e acelera na segunda fase (ease in)
    // Defines no Inspector ou usa as defaults abaixo
    [SerializeField] private AnimationCurve _foldCurve;
    [SerializeField] private AnimationCurve _unfoldCurve;

    private int _currentIndex = 0;
    private bool _isFlipping = false;

    private void Awake()
    {
        // Se não foram definidas no Inspector, cria curvas suaves
        if (_foldCurve == null || _foldCurve.length == 0)
            _foldCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2f),      // começa devagar
                new Keyframe(1f, 1f, 2f, 0f));      // abranda no meio

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
            if (i == index) _pages[i].Show();
            else _pages[i].Hide();
        }
        _leftPage.localEulerAngles = Vector3.zero;
        _rightPage.localEulerAngles = Vector3.zero;
    }

    public void FlipToPage(int targetIndex)
    {
        if (_isFlipping || targetIndex == _currentIndex) return;
        bool flipRight = targetIndex > _currentIndex;
        StartCoroutine(FlipRoutine(targetIndex, flipRight));
    }

    private IEnumerator FlipRoutine(int targetIndex, bool flipRight)
    {
        _isFlipping = true;

        // flipRight = clicou num tab à direita  folha direita vira para a esquerda
        // flipLeft  = clicou num tab à esquerda folha esquerda vira para a direita
        RectTransform flipPage = flipRight ? _rightPage : _leftPage;

        float startAngle = 0f;
        float midAngle = flipRight ? -90f : 90f;
        float endAngle = flipRight ? -180f : 180f;

        // FASE 1 dobrar até ao meio
        yield return RotatePage(flipPage, startAngle, midAngle, _halfFlipDuration, _foldCurve);

        // Troca de conteúdo quando a folha está "de lado" (invisível)
        _pages[_currentIndex].Hide();
        _currentIndex = targetIndex;
        _pages[_currentIndex].Show();

        // FASE 2 — desdobrar da outra metade
        yield return RotatePage(flipPage, midAngle, endAngle, _halfFlipDuration, _unfoldCurve);

        // Reset para a próxima animação
        flipPage.localEulerAngles = Vector3.zero;
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
            float tCurved = curve.Evaluate(t);
            float angle = Mathf.LerpUnclamped(from, to, tCurved);
            page.localEulerAngles = new Vector3(0f, angle, 0f);
            yield return null;
        }
        page.localEulerAngles = new Vector3(0f, to, 0f);
    }
}