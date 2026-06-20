using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class AccessibilityManagerTests
{
    private GameObject _go;
    private AccessibilityManager _manager;

    [SetUp]
    public void SetUp()
    {
        // Clear any leftover prefs so each test starts clean
        PlayerPrefs.DeleteKey("acc_music_vol");
        PlayerPrefs.DeleteKey("acc_sfx_vol");
        PlayerPrefs.DeleteKey("acc_dialogue_vol");
        PlayerPrefs.DeleteKey("acc_highlight");
        PlayerPrefs.DeleteKey("acc_hints");
        PlayerPrefs.DeleteKey("acc_bindings");
        PlayerPrefs.DeleteKey("acc_gamma");
        PlayerPrefs.DeleteKey("acc_colorblind");

        _go      = new GameObject("TestAccessibilityManager");
        _manager = _go.AddComponent<AccessibilityManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_go);
        // Null static delegates to prevent cross-test contamination
        AccessibilityManager.OnVolumeChanged   = null;
        AccessibilityManager.OnHighlightChanged = null;
        AccessibilityManager.OnHintsChanged    = null;
        AccessibilityManager.OnGammaChanged    = null;
        AccessibilityManager.OnColorblindChanged = null;
    }

    // ── Defaults ───────────────────────────────────────────────────────────

    [Test] public void MusicVolume_DefaultsToOne()        => Assert.AreEqual(1f, _manager.MusicVolume, 0.001f);
    [Test] public void SfxVolume_DefaultsToOne()          => Assert.AreEqual(1f, _manager.SfxVolume, 0.001f);
    [Test] public void DialogueVolume_DefaultsToOne()     => Assert.AreEqual(1f, _manager.DialogueVolume, 0.001f);
    [Test] public void HighlightInteractables_DefaultsTrue() => Assert.IsTrue(_manager.HighlightInteractables);
    [Test] public void HintsEnabled_DefaultsTrue()        => Assert.IsTrue(_manager.HintsEnabled);
    [Test] public void Gamma_DefaultsToOne()              => Assert.AreEqual(1f, _manager.Gamma, 0.001f);
    [Test] public void ColorblindMode_DefaultsToNone()    => Assert.AreEqual(ColorblindMode.None, _manager.ColorblindMode);

    // ── Gamma ──────────────────────────────────────────────────────────────

    [Test]
    public void Gamma_Setter_SavesPrefs()
    {
        _manager.Gamma = 1.2f;
        Assert.AreEqual(1.2f, PlayerPrefs.GetFloat("acc_gamma", -1f), 0.001f);
    }

    [Test]
    public void Gamma_ClampedToRange()
    {
        _manager.Gamma = 5f;
        Assert.AreEqual(AccessibilityManager.MaxGamma, _manager.Gamma, 0.001f);
        _manager.Gamma = -5f;
        Assert.AreEqual(AccessibilityManager.MinGamma, _manager.Gamma, 0.001f);
    }

    [Test]
    public void Gamma_Setter_FiresOnGammaChanged()
    {
        float received = -1f;
        AccessibilityManager.OnGammaChanged = val => received = val;
        _manager.Gamma = 1.3f;
        Assert.AreEqual(1.3f, received, 0.001f);
    }

    // ── Colourblind ────────────────────────────────────────────────────────

    [Test]
    public void ColorblindMode_Setter_SavesPrefs()
    {
        _manager.ColorblindMode = ColorblindMode.Deuteranopia;
        Assert.AreEqual((int)ColorblindMode.Deuteranopia, PlayerPrefs.GetInt("acc_colorblind", -1));
    }

    [Test]
    public void ColorblindMode_Setter_FiresOnColorblindChanged()
    {
        ColorblindMode received = ColorblindMode.None;
        AccessibilityManager.OnColorblindChanged = val => received = val;
        _manager.ColorblindMode = ColorblindMode.Tritanopia;
        Assert.AreEqual(ColorblindMode.Tritanopia, received);
    }

    // ── PlayerPrefs persistence ────────────────────────────────────────────

    [Test]
    public void MusicVolume_LoadsFromPlayerPrefs()
    {
        Object.DestroyImmediate(_go);
        _go = null; // null before creating go2 so TearDown is safe regardless
        PlayerPrefs.SetFloat("acc_music_vol", 0.4f);
        var go2 = new GameObject();
        var manager2 = go2.AddComponent<AccessibilityManager>();
        try   { Assert.AreEqual(0.4f, manager2.MusicVolume, 0.001f); }
        finally { Object.DestroyImmediate(go2); }
    }

    [Test]
    public void MusicVolume_Setter_SavesPrefs()
    {
        _manager.MusicVolume = 0.3f;
        Assert.AreEqual(0.3f, PlayerPrefs.GetFloat("acc_music_vol", -1f), 0.001f);
    }

    [Test]
    public void HighlightInteractables_Setter_SavesPrefs()
    {
        _manager.HighlightInteractables = false;
        Assert.AreEqual(0, PlayerPrefs.GetInt("acc_highlight", 1));
    }

    [Test]
    public void HintsEnabled_Setter_SavesPrefs()
    {
        _manager.HintsEnabled = false;
        Assert.AreEqual(0, PlayerPrefs.GetInt("acc_hints", 1));
    }

    // ── Event firing ───────────────────────────────────────────────────────

    [Test]
    public void MusicVolume_Setter_FiresOnVolumeChanged()
    {
        AudioBus receivedBus = AudioBus.Sfx;
        float    receivedVal = -1f;
        AccessibilityManager.OnVolumeChanged = (bus, val) => { receivedBus = bus; receivedVal = val; };

        _manager.MusicVolume = 0.7f;

        Assert.AreEqual(AudioBus.Music, receivedBus);
        Assert.AreEqual(0.7f, receivedVal, 0.001f);
    }

    [Test]
    public void SfxVolume_Setter_FiresOnVolumeChanged()
    {
        AudioBus receivedBus = AudioBus.Music;
        AccessibilityManager.OnVolumeChanged = (bus, _) => receivedBus = bus;

        _manager.SfxVolume = 0.5f;

        Assert.AreEqual(AudioBus.Sfx, receivedBus);
    }

    [Test]
    public void HighlightInteractables_Setter_FiresOnHighlightChanged()
    {
        bool received = true;
        AccessibilityManager.OnHighlightChanged = val => received = val;

        _manager.HighlightInteractables = false;

        Assert.IsFalse(received);
    }

    [Test]
    public void HintsEnabled_Setter_FiresOnHintsChanged()
    {
        bool received = true;
        AccessibilityManager.OnHintsChanged = val => received = val;

        _manager.HintsEnabled = false;

        Assert.IsFalse(received);
    }

    // ── Clamping ───────────────────────────────────────────────────────────

    [Test]
    public void MusicVolume_ClampedToOne_WhenOver()
    {
        _manager.MusicVolume = 1.5f;
        Assert.AreEqual(1f, _manager.MusicVolume, 0.001f);
    }

    [Test]
    public void MusicVolume_ClampedToZero_WhenUnder()
    {
        _manager.MusicVolume = -0.5f;
        Assert.AreEqual(0f, _manager.MusicVolume, 0.001f);
    }

    // ── ResetToDefaults ────────────────────────────────────────────────────

    [Test]
    public void ResetToDefaults_RestoresAllSettings()
    {
        _manager.MusicVolume            = 0.2f;
        _manager.HintsEnabled           = false;
        _manager.HighlightInteractables = false;
        _manager.Gamma                  = 1.4f;
        _manager.ColorblindMode         = ColorblindMode.Protanopia;

        _manager.ResetToDefaults();

        Assert.AreEqual(1f,  _manager.MusicVolume,            0.001f);
        Assert.IsTrue(_manager.HintsEnabled);
        Assert.IsTrue(_manager.HighlightInteractables);
        Assert.AreEqual(1f, _manager.Gamma, 0.001f);
        Assert.AreEqual(ColorblindMode.None, _manager.ColorblindMode);
    }

    [Test]
    public void ResetToDefaults_FiresAllEvents()
    {
        bool volumeFired    = false;
        bool highlightFired = false;
        bool hintsFired     = false;
        AccessibilityManager.OnVolumeChanged    = (_, __) => volumeFired    = true;
        AccessibilityManager.OnHighlightChanged = _       => highlightFired = true;
        AccessibilityManager.OnHintsChanged     = _       => hintsFired     = true;

        _manager.ResetToDefaults();

        Assert.IsTrue(volumeFired,    "OnVolumeChanged should fire on reset");
        Assert.IsTrue(highlightFired, "OnHighlightChanged should fire on reset");
        Assert.IsTrue(hintsFired,     "OnHintsChanged should fire on reset");
    }
}
