using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Canvas overlay that briefly shows an icon when a flagged sound event fires.
/// Attach to a UI GameObject with an Image child. Wire via AudioManager._audioEventIcon.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class AudioEventIcon : MonoBehaviour
{
    [SerializeField] private Image    _iconImage;
    [SerializeField] private Sprite[] _icons;          // indexed by (int)AudioFeedbackType
    [SerializeField] private float    _displayDuration = 1.5f;
    [SerializeField] private float    _fadeDuration    = 0.25f;

    private CanvasGroup _group;
    private Coroutine   _current;

    private void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (_current != null)
        {
            StopCoroutine(_current);
            _current = null;
        }
        if (_group != null) _group.alpha = 0f;
    }

    /// <summary>Show the icon for the given feedback type, interrupting any current display.</summary>
    public void Show(AudioFeedbackType type)
    {
        int idx = (int)type;
        if (_iconImage != null && _icons != null && idx >= 0 && idx < _icons.Length)
            _iconImage.sprite = _icons[idx];

        if (_current != null) StopCoroutine(_current);
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
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            _group.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _group.alpha = to;
    }
}

/// <summary>Icon type shown in the audio visual-feedback overlay.</summary>
public enum AudioFeedbackType { Alert, Impact, Collectible, Failure }
