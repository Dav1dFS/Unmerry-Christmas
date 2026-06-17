using UnityEngine;
using System.Collections;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void TriggerHitStop(float duration = 0.08f, float timeScale = 0f)
    {
        StartCoroutine(HitStopRoutine(duration, timeScale));
    }

    public void TriggerNpcHitStop(Animator npcAnimator, float duration = 0.1f, float animSpeed = 0.05f)
    {
        StartCoroutine(NpcHitStopRoutine(npcAnimator, duration, animSpeed));
    }

    private IEnumerator HitStopRoutine(float duration, float frozenScale)
    {
        Time.timeScale = frozenScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    private IEnumerator NpcHitStopRoutine(Animator anim, float duration, float slowSpeed)
    {
        if (anim == null) yield break;
        float original = anim.speed;
        anim.speed = slowSpeed;  // near Freeze, not full stop
        yield return new WaitForSecondsRealtime(duration);
        anim.speed = original;
    }
}