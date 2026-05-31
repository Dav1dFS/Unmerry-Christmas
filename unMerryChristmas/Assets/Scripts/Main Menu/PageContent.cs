using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PageContent : MonoBehaviour
{
    [SerializeField] private float _fadeInDuration = 0.25f;
    [SerializeField] private float _fadeInDelay = 0.05f; // pequeno delay após o flip terminar

    [SerializeField] private Button _firstSelected; // primeiro botão a focar nesta página


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

        if (_firstSelected != null)
            StartCoroutine(SelectAfterFrame(_firstSelected));
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
    public void ShowImmediate()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        gameObject.SetActive(true);
        if (_group != null)
        {
            _group.alpha = 1f;
            _group.blocksRaycasts = true;
            _group.interactable = true;
        }
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
    private IEnumerator SelectAfterFrame(Button btn)
    {
        yield return null;
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(btn.gameObject);
    }

}