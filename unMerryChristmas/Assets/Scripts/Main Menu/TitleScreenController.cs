using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private string _gameSceneName = "BackYardScenario";

    public void OnPlayPressed() => SceneManager.LoadScene(_gameSceneName);
    public void OnQuitPressed() => Application.Quit();

    private Coroutine _blinkRoutine;

    [Header("Title Screen")]
    [SerializeField] private CanvasGroup _titleScreenGroup;
    [SerializeField] private TextMeshProUGUI _pressAnyKeyText;
    [SerializeField] private float _fadeOutDuration = 0.6f;
    [SerializeField] private GameObject _firstSelectedButton;

    [Header("Book")]
    [SerializeField] private DrawingBookMenu2 _bookMenu;

    private bool _inputEnabled = false;
    private bool _triggered = false;

    private void Start()
    {
        StartCoroutine(EnableInputAfterDelay(0.8f));
        _blinkRoutine = StartCoroutine(BlinkText());
    }

   private void Update()
    {
        if (!_inputEnabled || _triggered)
            return;

        bool keyboardPressed =
            UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame;

        bool gamepadPressed =
            UnityEngine.InputSystem.Gamepad.current != null &&
            (
                UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame ||
                UnityEngine.InputSystem.Gamepad.current.startButton.wasPressedThisFrame
            );

        if (keyboardPressed || gamepadPressed)
        {
            _triggered = true;
            StartCoroutine(OnAnyKeyPressed());
        }
    }

    private IEnumerator OnAnyKeyPressed()
    {
        float elapsed = 0f;
        StopCoroutine(_blinkRoutine);
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            _pressAnyKeyText.alpha =
                Mathf.Lerp(1f, 0f, elapsed / 0.3f);

            yield return null;
        }

        _pressAnyKeyText.alpha = 0f;

        _bookMenu.Open();

        yield return null;

        EventSystem.current.SetSelectedGameObject(_firstSelectedButton);
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