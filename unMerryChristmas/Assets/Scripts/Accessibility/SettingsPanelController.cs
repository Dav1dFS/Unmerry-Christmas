using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the Settings panel — both the top-level sub-page navigation and the
/// individual settings values.
///
/// Sub-page layout (set up by BuildSettingsPrefab tool):
///   _navigationPage  → the page with the three category buttons
///   _audioPage       → sliders for Music / SFX / Dialogue
///   _displayPage     → toggles for Highlight Objects + Puzzle Hints
///   _controlsPage    → rebind rows + Reset All
///
/// Slider / toggle fields are wired by the same tool.
/// Each slider's OnValueChanged and each toggle's OnValueChanged must call the
/// corresponding public method on this component.
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Slider _dialogueSlider;

    [Header("Visual")]
    [SerializeField] private Toggle _highlightToggle;

    [Header("Hints")]
    [SerializeField] private Toggle _hintsToggle;

    [Header("Video")]
    [SerializeField] private Slider   _gammaSlider;
    [SerializeField] private TMP_Text _colorblindValueLabel;

    // Display names for each ColorblindMode (index matches enum order)
    private static readonly string[] ColorblindLabels =
        { "Off", "Protanopia", "Deuteranopia", "Tritanopia" };

    // Fallback index used to cycle the visible value when no AccessibilityManager
    // is present in the scene (e.g. previewing the menu standalone).
    private int _colorblindMockIndex;

    [Header("Sub-pages")]
    [SerializeField] private GameObject _navigationPage;
    [SerializeField] private GameObject _audioPage;
    [SerializeField] private GameObject _displayPage;
    [SerializeField] private GameObject _videoPage;
    [SerializeField] private GameObject _controlsPage;

    // Guards against slider/toggle OnValueChanged firing during SyncFromManager
    private bool _syncing;

    private void OnEnable()
    {
        ShowNavigation();
        SyncFromManager();
    }

    // ── Sub-page navigation ───────────────────────────────────────────────────
    public void ShowNavigation()   => ShowSubPage(_navigationPage);
    public void ShowAudioPage()    => ShowSubPage(_audioPage);
    public void ShowDisplayPage()  => ShowSubPage(_displayPage);
    public void ShowVideoPage()    => ShowSubPage(_videoPage);
    public void ShowControlsPage() => ShowSubPage(_controlsPage);

    private void ShowSubPage(GameObject page)
    {
        if (_navigationPage) _navigationPage.SetActive(false);
        if (_audioPage)      _audioPage.SetActive(false);
        if (_displayPage)    _displayPage.SetActive(false);
        if (_videoPage)      _videoPage.SetActive(false);
        if (_controlsPage)   _controlsPage.SetActive(false);
        if (page)            page.SetActive(true);
    }

    private void SyncFromManager()
    {
        if (AccessibilityManager.Instance == null) return;
        if (_musicSlider == null || _sfxSlider == null || _dialogueSlider == null
            || _highlightToggle == null || _hintsToggle == null)
        {
            Debug.LogWarning("[SettingsPanelController] One or more UI fields are not wired in the Inspector.", this);
            return;
        }

        _syncing = true;

        _musicSlider.value    = AccessibilityManager.Instance.MusicVolume;
        _sfxSlider.value      = AccessibilityManager.Instance.SfxVolume;
        _dialogueSlider.value = AccessibilityManager.Instance.DialogueVolume;
        _highlightToggle.isOn = AccessibilityManager.Instance.HighlightInteractables;
        _hintsToggle.isOn     = AccessibilityManager.Instance.HintsEnabled;

        if (_gammaSlider != null)
            _gammaSlider.value = AccessibilityManager.Instance.Gamma;
        RefreshColorblindLabel();

        _syncing = false;
    }

    // ── Called by Slider OnValueChanged (wire in Inspector) ───────────────
    public void OnMusicSliderChanged(float value)
    {
        if (_syncing || AccessibilityManager.Instance == null) return;
        AccessibilityManager.Instance.MusicVolume = value;
    }

    public void OnSfxSliderChanged(float value)
    {
        if (_syncing || AccessibilityManager.Instance == null) return;
        AccessibilityManager.Instance.SfxVolume = value;
    }

    public void OnDialogueSliderChanged(float value)
    {
        if (_syncing || AccessibilityManager.Instance == null) return;
        AccessibilityManager.Instance.DialogueVolume = value;
    }

    // ── Called by Toggle OnValueChanged (wire in Inspector) ───────────────
    public void OnHighlightToggleChanged(bool value)
    {
        if (_syncing || AccessibilityManager.Instance == null) return;
        AccessibilityManager.Instance.HighlightInteractables = value;
    }

    public void OnHintsToggleChanged(bool value)
    {
        if (_syncing || AccessibilityManager.Instance == null) return;
        AccessibilityManager.Instance.HintsEnabled = value;
    }

    // ── Called by Video UI (wire in Inspector) ───────────────────────────
    public void OnGammaSliderChanged(float value)
    {
        if (_syncing || AccessibilityManager.Instance == null) return;
        AccessibilityManager.Instance.Gamma = value;
    }

    // Cycle button advances to the next colourblind mode, wrapping around.
    // Updates the visible label even with no AccessibilityManager present (mock),
    // and drives the real setting when one exists.
    public void OnColorblindCyclePressed()
    {
        if (_syncing) return;
        int count = ColorblindLabels.Length;

        if (AccessibilityManager.Instance != null)
        {
            int next = ((int)AccessibilityManager.Instance.ColorblindMode + 1) % count;
            AccessibilityManager.Instance.ColorblindMode = (ColorblindMode)next;
        }
        else
        {
            _colorblindMockIndex = (_colorblindMockIndex + 1) % count;
        }

        RefreshColorblindLabel();
    }

    private void RefreshColorblindLabel()
    {
        if (_colorblindValueLabel == null) return;
        int index = AccessibilityManager.Instance != null
            ? (int)AccessibilityManager.Instance.ColorblindMode
            : _colorblindMockIndex;
        _colorblindValueLabel.text =
            (index >= 0 && index < ColorblindLabels.Length) ? ColorblindLabels[index] : index.ToString();
    }

    // ── Called by Reset All Button OnClick (wire in Inspector) ────────────
    public void OnResetAllPressed()
    {
        AccessibilityManager.Instance?.ResetToDefaults();
        SyncFromManager();

        // Rebind rows only refresh their labels on enable, so update them explicitly
        // here — otherwise the on-screen keys stay stale until the Controls page is
        // reopened, even though the bindings have already been reset.
        foreach (var row in GetComponentsInChildren<RebindActionRow>(true))
            row.RefreshBindingDisplay();
    }
}
