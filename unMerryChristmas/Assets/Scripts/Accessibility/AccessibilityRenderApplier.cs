using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Applies the Gamma and Colourblind accessibility settings to what the player
/// actually sees. <see cref="AccessibilityManager"/> persists and fires events for
/// these settings, but on its own it never touches the rendered image — this
/// component is the missing consumer.
///
/// It creates a runtime global URP <see cref="Volume"/> (no authored profile asset
/// required) holding two overrides:
///   • <see cref="ColorAdjustments"/> — postExposure drives perceived brightness ("Gamma").
///   • <see cref="ChannelMixer"/>     — a 3×3 colour matrix performs colourblind daltonisation.
///
/// Subscribes to <see cref="AccessibilityManager.OnGammaChanged"/> /
/// <see cref="AccessibilityManager.OnColorblindChanged"/> so it reacts the moment a
/// slider/button changes, and re-applies the current values on enable so it is
/// correct after a scene load too.
///
/// Requires post-processing to be enabled on the URP asset and on the rendering
/// Camera (it is, by default, when a Volume profile is present in Project Settings).
///
/// Auto-attached by <see cref="AccessibilityManager"/>; no Inspector wiring needed.
/// </summary>
[RequireComponent(typeof(AccessibilityManager))]
public class AccessibilityRenderApplier : MonoBehaviour
{
    // Highest priority so it always wins over scene-authored volumes.
    private const float VolumePriority = 10000f;

    private Volume           _volume;
    private ColorAdjustments _colorAdjustments;
    private ChannelMixer     _channelMixer;

    // ── Colourblind daltonisation ─────────────────────────────────────────────
    // Standard sRGB dichromat simulation matrices (rows = output channel).
    private static readonly float[][] SimProtanopia =
    {
        new[] { 0.567f, 0.433f, 0.000f },
        new[] { 0.558f, 0.442f, 0.000f },
        new[] { 0.000f, 0.242f, 0.758f },
    };
    private static readonly float[][] SimDeuteranopia =
    {
        new[] { 0.625f, 0.375f, 0.000f },
        new[] { 0.700f, 0.300f, 0.000f },
        new[] { 0.000f, 0.300f, 0.700f },
    };
    private static readonly float[][] SimTritanopia =
    {
        new[] { 0.950f, 0.050f, 0.000f },
        new[] { 0.000f, 0.433f, 0.567f },
        new[] { 0.000f, 0.475f, 0.525f },
    };

    // Error-redistribution matrix: pushes the colour the player cannot perceive
    // into channels they can. Classic daltonise weights.
    private static readonly float[][] ErrorShift =
    {
        new[] { 0.0f, 0.0f, 0.0f },
        new[] { 0.7f, 1.0f, 0.0f },
        new[] { 0.7f, 0.0f, 1.0f },
    };

    private void OnEnable()
    {
        EnsureVolume();
        AccessibilityManager.OnGammaChanged      += ApplyGamma;
        AccessibilityManager.OnColorblindChanged += ApplyColorblind;

        // Apply whatever the manager currently holds (events may have fired before
        // this component existed, e.g. during prefs load).
        if (AccessibilityManager.Instance != null)
        {
            ApplyGamma(AccessibilityManager.Instance.Gamma);
            ApplyColorblind(AccessibilityManager.Instance.ColorblindMode);
        }
        else
        {
            ApplyGamma(AccessibilityManager.DefaultGamma);
            ApplyColorblind(ColorblindMode.None);
        }
    }

    private void OnDisable()
    {
        AccessibilityManager.OnGammaChanged      -= ApplyGamma;
        AccessibilityManager.OnColorblindChanged -= ApplyColorblind;
    }

    private void OnDestroy()
    {
        if (_volume != null && _volume.profile != null)
            Destroy(_volume.profile);
    }

    // ── Setup ──────────────────────────────────────────────────────────────────
    private void EnsureVolume()
    {
        if (_volume != null) return;

        _volume          = gameObject.AddComponent<Volume>();
        _volume.isGlobal = true;
        _volume.priority = VolumePriority;

        // Runtime profile — never written to disk.
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.hideFlags = HideFlags.HideAndDontSave;
        _volume.profile   = profile;

        _colorAdjustments = profile.Add<ColorAdjustments>(true);
        _channelMixer     = profile.Add<ChannelMixer>(true);

        // Start neutral; the Apply* calls in OnEnable set real values.
        _colorAdjustments.active        = true;
        _colorAdjustments.postExposure.overrideState = true;
        _channelMixer.active            = false;
    }

    // ── Gamma → perceived brightness ───────────────────────────────────────────
    private void ApplyGamma(float gamma)
    {
        if (_colorAdjustments == null) return;
        // postExposure is in EV stops; log2(gamma) keeps 1.0 perfectly neutral
        // and gives a symmetric darker/brighter response across the 0.5–1.5 range.
        _colorAdjustments.postExposure.value = Mathf.Log(Mathf.Max(gamma, 0.01f), 2f);
    }

    // ── Colourblind → channel-mixing daltonisation ─────────────────────────────
    private void ApplyColorblind(ColorblindMode mode)
    {
        if (_channelMixer == null) return;

        if (mode == ColorblindMode.None)
        {
            _channelMixer.active = false;
            return;
        }

        float[][] sim = mode switch
        {
            ColorblindMode.Protanopia   => SimProtanopia,
            ColorblindMode.Deuteranopia => SimDeuteranopia,
            ColorblindMode.Tritanopia   => SimTritanopia,
            _                           => null,
        };
        if (sim == null) { _channelMixer.active = false; return; }

        // corrected = original + ErrorShift · (original − simulated)
        //           = [ I + ErrorShift · (I − sim) ] · original
        float[][] m = AddIdentity(Multiply(ErrorShift, SubtractFromIdentity(sim)));

        _channelMixer.active = true;
        SetMixerRow(_channelMixer.redOutRedIn,   _channelMixer.redOutGreenIn,   _channelMixer.redOutBlueIn,   m[0]);
        SetMixerRow(_channelMixer.greenOutRedIn, _channelMixer.greenOutGreenIn, _channelMixer.greenOutBlueIn, m[1]);
        SetMixerRow(_channelMixer.blueOutRedIn,  _channelMixer.blueOutGreenIn,  _channelMixer.blueOutBlueIn,  m[2]);
    }

    private static void SetMixerRow(ClampedFloatParameter r, ClampedFloatParameter g,
                                    ClampedFloatParameter b, float[] row)
    {
        // ChannelMixer weights are percentages: 100 == 1.0.
        r.overrideState = g.overrideState = b.overrideState = true;
        r.value = row[0] * 100f;
        g.value = row[1] * 100f;
        b.value = row[2] * 100f;
    }

    // ── Tiny 3×3 matrix helpers ────────────────────────────────────────────────
    private static float[][] Multiply(float[][] a, float[][] b)
    {
        var r = new float[3][];
        for (int i = 0; i < 3; i++)
        {
            r[i] = new float[3];
            for (int j = 0; j < 3; j++)
                r[i][j] = a[i][0] * b[0][j] + a[i][1] * b[1][j] + a[i][2] * b[2][j];
        }
        return r;
    }

    private static float[][] SubtractFromIdentity(float[][] a) // I − a
    {
        var r = new float[3][];
        for (int i = 0; i < 3; i++)
        {
            r[i] = new float[3];
            for (int j = 0; j < 3; j++)
                r[i][j] = (i == j ? 1f : 0f) - a[i][j];
        }
        return r;
    }

    private static float[][] AddIdentity(float[][] a) // I + a
    {
        var r = new float[3][];
        for (int i = 0; i < 3; i++)
        {
            r[i] = new float[3];
            for (int j = 0; j < 3; j++)
                r[i][j] = (i == j ? 1f : 0f) + a[i][j];
        }
        return r;
    }
}
