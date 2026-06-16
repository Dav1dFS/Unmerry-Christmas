using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcHitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private bool _enabled = true;
    [SerializeField] private Color _flashColor = Color.white;
    [SerializeField] private float _flashDuration = 0.12f;
    [SerializeField] private int _flashCount = 2;       // number of blinks

    private Renderer[] _renderers;
    private Dictionary<Renderer, Material[]> _originalMaterials = new();
    private Material _flashMat;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _flashMat = new Material(Shader.Find("Sprites/Default"));
        _flashMat.color = _flashColor;

        // Cache original materials
        foreach (var r in _renderers)
            _originalMaterials[r] = r.materials;
    }

    public void TriggerFlash()
    {
        if (!_enabled) return;
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float blinkDuration = _flashDuration / (_flashCount * 2);

        for (int i = 0; i < _flashCount; i++)
        {
            // Apply flash material to all renderers
            foreach (var r in _renderers)
            {
                var flashMats = new Material[r.materials.Length];
                for (int m = 0; m < flashMats.Length; m++)
                    flashMats[m] = _flashMat;
                r.materials = flashMats;
            }

            yield return new WaitForSecondsRealtime(blinkDuration);

            // Restore original materials
            foreach (var r in _renderers)
                r.materials = _originalMaterials[r];

            yield return new WaitForSecondsRealtime(blinkDuration);
        }
    }
}