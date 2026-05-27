using UnityEngine;
using System.Collections;

public class PageContent : MonoBehaviour
{
    [SerializeField] private float _fadeInDuration = 0.25f;
    [SerializeField] private float _fadeInDelay = 0.05f; // pequeno delay após o flip terminar

    private CanvasGroup _group;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        if (_group != null)
        {
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }
        else gameObject.SetActive(false);
    }

    private IEnumerator FadeIn()
    {
        // Começa invisível e sem interacção
        _group.alpha = 0f;
        _group.blocksRaycasts = false;
        _group.interactable = false;

        // Espera o flip terminar
        yield return new WaitForSecondsRealtime(_fadeInDelay);

        // Fade in
        float elapsed = 0f;
        while (elapsed < _fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Clamp01(elapsed / _fadeInDuration);
            yield return null;
        }

        _group.alpha = 1f;
        _group.blocksRaycasts = true;
        _group.interactable = true;
    }


}