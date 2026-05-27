using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private string _gameSceneName = "BackYardScenario";

    public void OnPlayPressed() => SceneManager.LoadScene(_gameSceneName);
    public void OnQuitPressed() => Application.Quit();

    [Header("Title Screen")]
    [SerializeField] private CanvasGroup _titleScreenGroup;
    [SerializeField] private TextMeshProUGUI _pressAnyKeyText;
    [SerializeField] private float _fadeOutDuration = 0.6f;

    [Header("Book")]
    [SerializeField] private DrawingBookMenu2 _bookMenu;

    private bool _inputEnabled = false;
    private bool _triggered = false;

    private void Start()
    {
        StartCoroutine(EnableInputAfterDelay(0.8f));
        StartCoroutine(BlinkText());
    }

    private void Update()
    {
        if (!_inputEnabled || _triggered) return;
        if (UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame)
        {
            _triggered = true;
            StartCoroutine(OnAnyKeyPressed());
        }
    }

    private IEnumerator OnAnyKeyPressed()
    {
        // Fade out só do texto
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            _pressAnyKeyText.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
            yield return null;
        }
        _pressAnyKeyText.alpha = 0f;

        // Abre o livro
        _bookMenu.Open();
    }

    private IEnumerator BlinkText()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.time * Mathf.PI * 1.2f) + 1f) / 2f;
            _pressAnyKeyText.alpha = Mathf.Lerp(0.15f, 1f, t);
            yield return null;
        }
    }

    private IEnumerator EnableInputAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _inputEnabled = true;
    }
}