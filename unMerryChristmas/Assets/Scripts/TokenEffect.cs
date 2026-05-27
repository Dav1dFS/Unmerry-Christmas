using UnityEngine;

// Attach to an AbilityToken or DrawingPageCollectable GameObject.
// Rotates it on the Y axis and adds a soft glow via a child Point Light
// and material emission — no shader changes required.
public class TokenEffect : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 90f;   // degrees per second

    [Header("Bob")]
    [SerializeField] private bool  _bob           = true;
    [SerializeField] private float _bobHeight      = 0.15f; // metres up/down
    [SerializeField] private float _bobSpeed       = 1.5f;  // cycles per second

    [Header("Glow (Point Light)")]
    [SerializeField] private Color _glowColor      = new Color(1f, 0.85f, 0.3f); // warm gold
    [SerializeField] private float _glowIntensity   = 1.2f;
    [SerializeField] private float _glowRange       = 1.5f;

    [Header("Emission")]
    [SerializeField] private bool  _enableEmission  = true;
    [SerializeField] private Color _emissionColor   = new Color(0.4f, 0.3f, 0.1f); // subtle warm tint

    private Light  _light;
    private float  _startY;

    private void Awake()
    {
        _startY = transform.localPosition.y;
        SetupLight();
        if (_enableEmission) SetupEmission();
    }

    private void SetupLight()
    {
        // Reuse existing child light if already present
        _light = GetComponentInChildren<Light>();
        if (_light == null)
        {
            var lightGO = new GameObject("GlowLight");
            lightGO.transform.SetParent(transform, false);
            _light = lightGO.AddComponent<Light>();
        }

        _light.type      = LightType.Point;
        _light.color     = _glowColor;
        _light.intensity = _glowIntensity;
        _light.range     = _glowRange;
        _light.shadows   = LightShadows.None;
    }

    private void SetupEmission()
    {
        var rend = GetComponentInChildren<Renderer>();
        if (rend == null) return;

        // Use MaterialPropertyBlock to avoid modifying the shared material asset
        var block = new MaterialPropertyBlock();
        rend.GetPropertyBlock(block);
        block.SetColor("_EmissionColor", _emissionColor);
        rend.SetPropertyBlock(block);

        // Also enable the emission keyword on the material instance if the shader supports it
        // (Standard, URP Lit, etc.)
        foreach (var mat in rend.materials)
        {
            if (mat.HasProperty("_EmissionColor"))
                mat.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        // Rotate around world-space Y axis
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);

        // Gentle vertical bob
        if (_bob)
        {
            var pos = transform.localPosition;
            pos.y = _startY + Mathf.Sin(Time.time * _bobSpeed * Mathf.PI * 2f) * _bobHeight;
            transform.localPosition = pos;
        }

        // Pulse the light intensity slightly for extra life
        if (_light != null)
            _light.intensity = _glowIntensity + Mathf.Sin(Time.time * 2f) * 0.15f;
    }
}
