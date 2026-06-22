using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Persistent singleton that is the single source of truth for all accessibility settings.
/// Loads from PlayerPrefs on Awake, saves on each property change.
/// Fires static Action delegates so any system can react without polling.
/// </summary>
public class AccessibilityManager : MonoBehaviour
{
    public static AccessibilityManager Instance { get; private set; }

    // ── Static delegates (not events so tests can null them for cleanup) ───
    public static Action<AudioBus, float> OnVolumeChanged;
    public static Action<bool>            OnHighlightChanged;
    public static Action<bool>            OnHintsChanged;
    public static Action<float>           OnGammaChanged;
    public static Action<ColorblindMode>  OnColorblindChanged;

    // ── FMOD Bus Paths ─────────────────────────────────────────────────────
    [Header("FMOD Bus Paths")]
    [SerializeField] private string _musicBusPath    = "bus:/Music";
    [SerializeField] private string _sfxBusPath      = "bus:/SFX";
    [SerializeField] private string _dialogueBusPath = "bus:/Dialogue";

    [Header("Input")]
    [SerializeField] private InputActionAsset _inputActions;

    // ── PlayerPrefs Keys ───────────────────────────────────────────────────
    private const string KeyMusic     = "acc_music_vol";
    private const string KeySfx       = "acc_sfx_vol";
    private const string KeyDialogue  = "acc_dialogue_vol";
    private const string KeyHighlight = "acc_highlight";
    private const string KeyHints     = "acc_hints";
    private const string KeyBindings  = "acc_bindings";
    private const string KeyGamma     = "acc_gamma";
    private const string KeyColorblind= "acc_colorblind";

    // ── Gamma Range ────────────────────────────────────────────────────────
    // Slider operates in this range; 1.0 = no correction (engine default).
    public const float MinGamma     = 0.5f;
    public const float MaxGamma     = 1.5f;
    public const float DefaultGamma = 1.0f;

    // ── Backing Fields ─────────────────────────────────────────────────────
    private float _musicVolume;
    private float _sfxVolume;
    private float _dialogueVolume;
    private bool  _highlightInteractables;
    private bool  _hintsEnabled;
    private float          _gamma;
    private ColorblindMode _colorblindMode;

    // ── Properties ────────────────────────────────────────────────────────
    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = Mathf.Clamp01(value);
            ApplyBusVolume(_musicBusPath, _musicVolume);
            OnVolumeChanged?.Invoke(AudioBus.Music, _musicVolume);
            PlayerPrefs.SetFloat(KeyMusic, _musicVolume);
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            ApplyBusVolume(_sfxBusPath, _sfxVolume);
            OnVolumeChanged?.Invoke(AudioBus.Sfx, _sfxVolume);
            PlayerPrefs.SetFloat(KeySfx, _sfxVolume);
        }
    }

    public float DialogueVolume
    {
        get => _dialogueVolume;
        set
        {
            _dialogueVolume = Mathf.Clamp01(value);
            ApplyBusVolume(_dialogueBusPath, _dialogueVolume);
            OnVolumeChanged?.Invoke(AudioBus.Dialogue, _dialogueVolume);
            PlayerPrefs.SetFloat(KeyDialogue, _dialogueVolume);
        }
    }

    public bool HighlightInteractables
    {
        get => _highlightInteractables;
        set
        {
            _highlightInteractables = value;
            Debug.Log($"[AccessibilityManager] Highlight changed to: {_highlightInteractables}");
            OnHighlightChanged?.Invoke(_highlightInteractables);
            PlayerPrefs.SetInt(KeyHighlight, _highlightInteractables ? 1 : 0);
        }
    }

    public bool HintsEnabled
    {
        get => _hintsEnabled;
        set
        {
            _hintsEnabled = value;
            OnHintsChanged?.Invoke(_hintsEnabled);
            PlayerPrefs.SetInt(KeyHints, _hintsEnabled ? 1 : 0);
        }
    }

    public float Gamma
    {
        get => _gamma;
        set
        {
            _gamma = Mathf.Clamp(value, MinGamma, MaxGamma);
            OnGammaChanged?.Invoke(_gamma);
            PlayerPrefs.SetFloat(KeyGamma, _gamma);
        }
    }

    public ColorblindMode ColorblindMode
    {
        get => _colorblindMode;
        set
        {
            _colorblindMode = value;
            OnColorblindChanged?.Invoke(_colorblindMode);
            PlayerPrefs.SetInt(KeyColorblind, (int)_colorblindMode);
        }
    }

    // ── Lifecycle ──────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromPrefs();
        LoadBindings();

        // The Gamma and Colourblind settings only affect the rendered image through
        // AccessibilityRenderApplier. Attach it here so those settings work wherever
        // a manager exists, with no Inspector wiring required.
        if (GetComponent<AccessibilityRenderApplier>() == null)
            gameObject.AddComponent<AccessibilityRenderApplier>();
    }

    private void OnApplicationQuit() => PlayerPrefs.Save();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── Binding Save / Load ────────────────────────────────────────────────
    public void SaveBindings()
    {
        if (_inputActions == null) return;
        PlayerPrefs.SetString(KeyBindings, _inputActions.SaveBindingOverridesAsJson());
    }

    private void LoadBindings()
    {
        if (_inputActions == null) return;
        string json = PlayerPrefs.GetString(KeyBindings, "");
        if (!string.IsNullOrEmpty(json))
            _inputActions.LoadBindingOverridesFromJson(json);
    }

    // ── Reset ──────────────────────────────────────────────────────────────
    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(KeyMusic);
        PlayerPrefs.DeleteKey(KeySfx);
        PlayerPrefs.DeleteKey(KeyDialogue);
        PlayerPrefs.DeleteKey(KeyHighlight);
        PlayerPrefs.DeleteKey(KeyHints);
        PlayerPrefs.DeleteKey(KeyBindings);
        PlayerPrefs.DeleteKey(KeyGamma);
        PlayerPrefs.DeleteKey(KeyColorblind);

        _inputActions?.RemoveAllBindingOverrides();

        // Write directly to backing fields to avoid re-saving deleted keys,
        // then fire events so subscribers update.
        _musicVolume            = 1f;
        _sfxVolume              = 1f;
        _dialogueVolume         = 1f;
        _highlightInteractables = true;
        _hintsEnabled           = true;
        _gamma                  = DefaultGamma;
        _colorblindMode         = ColorblindMode.None;

        ApplyBusVolume(_musicBusPath,    1f);
        ApplyBusVolume(_sfxBusPath,      1f);
        ApplyBusVolume(_dialogueBusPath, 1f);

        OnVolumeChanged?.Invoke(AudioBus.Music,    1f);
        OnVolumeChanged?.Invoke(AudioBus.Sfx,      1f);
        OnVolumeChanged?.Invoke(AudioBus.Dialogue, 1f);
        OnHighlightChanged?.Invoke(true);
        OnHintsChanged?.Invoke(true);
        OnGammaChanged?.Invoke(DefaultGamma);
        OnColorblindChanged?.Invoke(ColorblindMode.None);
        PlayerPrefs.Save();
    }

    // ── Private Helpers ────────────────────────────────────────────────────
    private void LoadFromPrefs()
    {
        _musicVolume            = PlayerPrefs.GetFloat(KeyMusic,    1f);
        _sfxVolume              = PlayerPrefs.GetFloat(KeySfx,      1f);
        _dialogueVolume         = PlayerPrefs.GetFloat(KeyDialogue, 1f);
        _highlightInteractables = PlayerPrefs.GetInt(KeyHighlight, 1) == 1;
        _hintsEnabled           = PlayerPrefs.GetInt(KeyHints,     1) == 1;
        _gamma                  = PlayerPrefs.GetFloat(KeyGamma, DefaultGamma);
        _colorblindMode         = (ColorblindMode)PlayerPrefs.GetInt(KeyColorblind, (int)ColorblindMode.None);

        // Apply loaded volumes immediately (FMOD may not be ready — swallow exceptions)
        ApplyBusVolume(_musicBusPath,    _musicVolume);
        ApplyBusVolume(_sfxBusPath,      _sfxVolume);
        ApplyBusVolume(_dialogueBusPath, _dialogueVolume);
    }

    private void ApplyBusVolume(string busPath, float volume)
    {
        if (string.IsNullOrEmpty(busPath)) return;
        try { FMODUnity.RuntimeManager.GetBus(busPath).setVolume(volume); }
        catch (Exception e)
        {
            Debug.LogWarning($"[AccessibilityManager] Could not set '{busPath}' volume: {e.Message}");
        }
    }
}

/// <summary>Identifies an FMOD audio bus for volume control.</summary>
public enum AudioBus { Music, Sfx, Dialogue }

/// <summary>
/// Colourblind correction (daltonization) target. None = no correction.
/// Order is the index order presented in the settings dropdown.
/// </summary>
public enum ColorblindMode { None, Protanopia, Deuteranopia, Tritanopia }
