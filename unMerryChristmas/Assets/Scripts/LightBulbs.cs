using System.Collections;
using UnityEngine;

public class ChristmasLightBulb : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private int emissiveMaterialIndex = 1;

    [Header("Christmas Colors")]
    [SerializeField]
    private Color[] christmasColors = new Color[]
    {
        new Color(1f,   0.05f, 0.05f),
        new Color(0.05f,1f,   0.05f),
        new Color(1f,   0.85f, 0.05f),
        new Color(0.05f,0.4f,  1f),
        new Color(1f,   0.4f,  0.05f),
        new Color(1f,   0.05f, 0.8f),
        new Color(1f,   1f,    1f),
    };

    [Header("Emission Intensity")]
    // HDR brightness when the bulb is on
    [SerializeField] private float emissionIntensity = 3.5f;

    [Header("Timing (seconds)")]
    // How long each flash stays on/off
    [SerializeField] private float minOnTime = 0.08f;
    [SerializeField] private float maxOnTime = 0.35f;
    [SerializeField] private float minOffTime = 0.05f;
    [SerializeField] private float maxOffTime = 0.25f;

    [Header("Burst Settings")]
    // Number of flashes before the long pause
    [SerializeField] private int flashesPerBurst = 3;
    // Long pause between bursts - increase for a calmer effect
    [SerializeField] private float minPauseTime = 0.4f;
    [SerializeField] private float maxPauseTime = 1.8f;

    private Renderer _renderer;
    private Material _emissiveMat;
    private Color _currentColor;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        _renderer = GetComponent<Renderer>();

        // Create a per-instance material so each bulb can have its own color
        _emissiveMat = new Material(_renderer.materials[emissiveMaterialIndex]);
        Material[] mats = _renderer.materials;
        mats[emissiveMaterialIndex] = _emissiveMat;
        _renderer.materials = mats;

        _currentColor = christmasColors[Random.Range(0, christmasColors.Length)];
        SetEmission(_currentColor, emissionIntensity);
    }

    void Start()
    {
        // Random delay so bulbs don't all blink in sync
        StartCoroutine(BlinkLoop(Random.Range(0f, 2f)));
    }

    IEnumerator BlinkLoop(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            for (int i = 0; i < flashesPerBurst; i++)
            {
                SetEmission(_currentColor, emissionIntensity);
                yield return new WaitForSeconds(Random.Range(minOnTime, maxOnTime));

                SetEmission(_currentColor, 0f);
                yield return new WaitForSeconds(Random.Range(minOffTime, maxOffTime));
            }

            // Change color while the bulb is off
            _currentColor = christmasColors[Random.Range(0, christmasColors.Length)];

            yield return new WaitForSeconds(Random.Range(minPauseTime, maxPauseTime));
        }
    }

    void SetEmission(Color baseColor, float intensity)
    {
        _emissiveMat.SetColor(EmissionColor, baseColor * Mathf.Pow(2f, intensity));
    }

    void OnDestroy()
    {
        if (_emissiveMat != null)
            Destroy(_emissiveMat);
    }
}