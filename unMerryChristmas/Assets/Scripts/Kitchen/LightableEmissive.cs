using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Task 3. Makes a decorative object "light up" by ramping its material emission from
/// black (0) to a target colour over a short duration — simulating it being lit. The
/// ramp is triggered when an explosive present detonates on/near it
/// (<see cref="IExplosionReactive"/>).
///
/// Works on instanced materials (<c>Renderer.materials</c> returns per-renderer copies),
/// so only this object lights up even though the Poly Christmas props share atlas
/// materials. URP/Lit uses the <c>_EmissionColor</c> property and the <c>_EMISSION</c>
/// keyword, both enabled here.
/// </summary>
public class LightableEmissive : MonoBehaviour, IExplosionReactive
{
    [Tooltip("Renderers to light. Leave empty to use every renderer under this object.")]
    [SerializeField] private Renderer[] _targetRenderers;

    [Tooltip("Emission colour when fully lit (HDR). Ramped up from black.")]
    [ColorUsage(true, true)]
    [SerializeField] private Color _litEmission = new Color(6f, 3.04f, 1.6f, 1f);

    [Tooltip("Seconds to ramp from unlit to fully lit.")]
    [SerializeField] private float _duration = 1f;

    [SerializeField] private string _emissionProperty = "_EmissionColor";

    /// <summary>True once lit.</summary>
    public bool IsLit { get; private set; }

    /// <summary>Fires once, when this object starts lighting.</summary>
    public event Action OnLit;

    private Material[] _materials;

    private void Awake()
    {
        if (_targetRenderers == null || _targetRenderers.Length == 0)
            _targetRenderers = GetComponentsInChildren<Renderer>(true);

        var mats = new List<Material>();
        foreach (var r in _targetRenderers)
        {
            if (r == null) continue;
            foreach (var m in r.materials) // instances, not the shared atlas asset
            {
                if (m == null) continue;
                m.EnableKeyword("_EMISSION");
                m.globalIlluminationFlags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                if (m.HasProperty(_emissionProperty))
                    m.SetColor(_emissionProperty, Color.black);
                mats.Add(m);
            }
        }
        _materials = mats.ToArray();
    }

    public void OnExplosion(Vector3 origin, float radius) => Light();

    /// <summary>Begin the light-up ramp (idempotent).</summary>
    public void Light()
    {
        if (IsLit) return;
        IsLit = true;
        StartCoroutine(Ramp());
        OnLit?.Invoke();
    }

    private IEnumerator Ramp()
    {
        float t = 0f;
        while (t < _duration)
        {
            float k = _duration > 0f ? t / _duration : 1f;
            SetEmission(Color.Lerp(Color.black, _litEmission, k));
            t += Time.deltaTime;
            yield return null;
        }
        SetEmission(_litEmission);
    }

    private void SetEmission(Color c)
    {
        foreach (var m in _materials)
            if (m != null && m.HasProperty(_emissionProperty))
                m.SetColor(_emissionProperty, c);
    }
}
