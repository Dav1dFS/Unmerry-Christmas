using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private SmoothCameraFollow _cameraFollow;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _cameraFollow = Camera.main?.GetComponent<SmoothCameraFollow>();
    }

    public void Shake(float duration, float magnitude, float frequency = 25f)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude, frequency));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude, float frequency)
    {
        if (_cameraFollow == null)
        {
            _cameraFollow = Camera.main?.GetComponent<SmoothCameraFollow>();
            if (_cameraFollow == null) yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - (elapsed / duration);

            float x = (Mathf.PerlinNoise(elapsed * frequency, 0f) * 2f - 1f);
            float y = (Mathf.PerlinNoise(0f, elapsed * frequency) * 2f - 1f);

            _cameraFollow.SetShakeOffset(new Vector3(x, y, 0f) * magnitude * t);
            yield return null;
        }

        _cameraFollow.SetShakeOffset(Vector3.zero);
    }
}