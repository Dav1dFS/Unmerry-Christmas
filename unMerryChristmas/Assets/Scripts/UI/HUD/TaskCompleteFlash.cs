using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class TaskCompleteFlash : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private float    _displayDuration = 2f;
    [SerializeField] private float    _fadeDuration    = 0.25f;

    private CanvasGroup _group;
    private Coroutine   _current;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
    }

    public void Show(string taskName)
    {
        if (_current != null) StopCoroutine(_current);
        _label.text = $"Task complete\n<size=70%>{taskName}</size>";
        // Must activate BEFORE StartCoroutine — coroutines cannot start on inactive GameObjects.
        gameObject.SetActive(true);
        _current = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return Fade(0f, 1f);
        yield return new WaitForSeconds(_displayDuration);
        yield return Fade(1f, 0f);
        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < _fadeDuration)
        {
            _group.alpha = Mathf.Lerp(from, to, t / _fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        _group.alpha = to;
    }
}
