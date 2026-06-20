using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("Accessibility")]
    [SerializeField] private AudioEventIcon _audioEventIcon;

    [SerializeField] private EventReference walkSound;
    [SerializeField] private EventReference pushSound;
    [SerializeField] private EventReference grabSound;
    [SerializeField] private EventReference unlockAbilitySound;
    [SerializeField] private EventReference throwSound;

    private FMOD.Studio.EventInstance walkInstance;
    private FMOD.Studio.EventInstance pushInstance;

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

    public void InitializeWalkingSound(Transform playerTransform, Rigidbody playerRb)
    {
        if (!walkSound.IsNull)
        {
            walkInstance = RuntimeManager.CreateInstance(walkSound);
            RuntimeManager.AttachInstanceToGameObject(walkInstance, playerTransform, playerRb);
        }
    }

    public void PlayWalkingSound()
    {
        if (walkInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            walkInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                walkInstance.start();
            }
        }
    }

    public void StopWalkingSound()
    {
        if (walkInstance.isValid())
        {
            walkInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    public void InitializePushingSound(Transform playerTransform, Rigidbody playerRb)
    {
        if (!pushSound.IsNull)
        {
            pushInstance = RuntimeManager.CreateInstance(pushSound);
            RuntimeManager.AttachInstanceToGameObject(pushInstance, playerTransform, playerRb);
        }
    }

    public void PlayPushingSound()
    {
        if (pushInstance.isValid())
        {
            FMOD.Studio.PLAYBACK_STATE playbackState;
            pushInstance.getPlaybackState(out playbackState);
            if (playbackState == FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                pushInstance.start();
            }
        }
    }

    public void StopPushingSound()
    {
        if (pushInstance.isValid())
        {
            pushInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    public void PlayGrabSound(Vector3 worldPos)
    {
        if (!grabSound.IsNull)
            PlayOneShot(grabSound, worldPos);
    }

    public void PlayUnlockAbilitySound(Vector3 worldPos)
    {
        if (!unlockAbilitySound.IsNull)
            PlayOneShot(unlockAbilitySound, worldPos);
    }

    public void PlayThrowSound(Vector3 worldPos)
    {
        if (!throwSound.IsNull)
            PlayOneShot(throwSound, worldPos);
    }

    public FMOD.Studio.EventInstance CreateInstance(EventReference sound)
    {
        return RuntimeManager.CreateInstance(sound);
    }

    public void AttachInstanceToGameObject(FMOD.Studio.EventInstance instanceToAttach, Transform transformToAttach, Rigidbody rb = null)
    {
        RuntimeManager.AttachInstanceToGameObject(instanceToAttach, transformToAttach, rb);
    }

    public void StopAndReleaseInstance(FMOD.Studio.EventInstance instanceToStop)
    {
        if (instanceToStop.isValid())
        {
            instanceToStop.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instanceToStop.release();
            instanceToStop.clearHandle();
        }
    }

    private void OnDestroy()
    {
        StopAndReleaseInstance(walkInstance);
        StopAndReleaseInstance(pushInstance);
    }
}

