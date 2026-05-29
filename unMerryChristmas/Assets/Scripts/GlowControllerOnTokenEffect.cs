using UnityEngine;

/// <summary>
/// Disables the token's glow light whenever the token's renderer is disabled,
/// preventing invisible tokens from casting visible light.
/// Added to ability token GameObjects by the BackYard pickup fix tool.
/// </summary>
public class GlowControllerOnTokenEffect : MonoBehaviour
{
    private Light    _glowLight;
    private Renderer _renderer;
    private bool     _wasEnabled = true;

    private void Start()
    {
        _glowLight = GetComponentInChildren<Light>();
        _renderer  = GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        if (_glowLight == null || _renderer == null) return;

        bool shouldGlow = _renderer.enabled;
        if (shouldGlow == _wasEnabled) return;

        _glowLight.enabled = shouldGlow;
        _wasEnabled        = shouldGlow;
    }
}
