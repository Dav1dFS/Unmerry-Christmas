using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A gentle, STATIONARY emission glow for a placed mesh — e.g. the drawing book
/// key item wedged by the garden wall.
///
/// Unlike <see cref="TokenEffect"/> (which is built for floating pickups: it
/// rotates, bobs, and adds a point light), this only raises the material's
/// emission, optionally pulsing it slightly. The object never moves, so it suits
/// items that sit in the world and should merely "glow a little".
///
/// Materials are instanced per-renderer (via <c>renderer.materials</c>) so the
/// shared material asset is never modified and the glow is confined to this
/// object. Emission shows at runtime (Play mode).
/// </summary>
[DisallowMultipleComponent]
public class SubtleGlow : MonoBehaviour
{
    [Header("Glow")]
    [SerializeField] private Color _emissionColor = new Color(1f, 0.85f, 0.4f); // warm gold
    [Tooltip("Base emission strength.")]
    [SerializeField, Range(0f, 4f)] private float _intensity = 0.6f;

    [Header("Pulse (optional)")]
    [SerializeField] private bool _pulse = true;
    [Tooltip("How much the intensity breathes, as a fraction of the base intensity.")]
    [SerializeField, Range(0f, 1f)] private float _pulseAmount = 0.25f;
    [Tooltip("Pulse cycles per second.")]
    [SerializeField] private float _pulseSpeed = 0.8f;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private readonly List<Material> _emissiveMats = new();

    private void Awake()
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            // r.materials returns per-renderer INSTANCES — editing them never
            // touches the shared material asset.
            foreach (var m in r.materials)
            {
                if (m == null || !m.HasProperty(EmissionColorId)) continue;
                m.EnableKeyword("_EMISSION");
                _emissiveMats.Add(m);
            }
        }
        Apply(_intensity);
    }

    private void OnEnable() => Apply(CurrentIntensity());

    private void Update()
    {
        if (_pulse) Apply(CurrentIntensity());
    }

    private float CurrentIntensity()
    {
        if (!_pulse) return _intensity;
        float wave = Mathf.Sin(Time.time * _pulseSpeed * Mathf.PI * 2f);
        return _intensity * (1f + wave * _pulseAmount);
    }

    private void Apply(float intensity)
    {
        Color c = _emissionColor * Mathf.Max(0f, intensity);
        foreach (var m in _emissiveMats)
            if (m != null) m.SetColor(EmissionColorId, c);
    }
}
