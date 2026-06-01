using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Accessibility")]
    [SerializeField] private AudioEventIcon _audioEventIcon;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[AudioManager] Duplicate detected — destroying this instance.", this);
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public void PlayOneShotWithParameter(EventReference sound, Vector3 worldPos,
                                         string parameterName, float parameterValue)
    {
        FMOD.Studio.EventInstance instance = RuntimeManager.CreateInstance(sound);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));
        instance.setParameterByName(parameterName, parameterValue);
        instance.start();
        instance.release();
    }

    /// <summary>
    /// Plays a one-shot sound AND shows a visual icon overlay for players
    /// who cannot hear the audio. Wire _audioEventIcon in the Inspector.
    /// </summary>
    public void PlayOneShotWithFeedback(EventReference sound, Vector3 worldPos,
                                        AudioFeedbackType feedbackType)
    {
        PlayOneShot(sound, worldPos);
        if (_audioEventIcon != null)
            _audioEventIcon.Show(feedbackType);
        else
            Debug.LogWarning("[AudioManager] _audioEventIcon not wired — accessibility feedback will not show.", this);
    }
}

