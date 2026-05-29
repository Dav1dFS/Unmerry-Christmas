using UnityEngine;

// Attach to a token or drawing page that should only become accessible
// after a specific task is completed.
//
// On Start: hides itself (inactive collider, hidden renderer, disabled effects).
// When the linked task completes: re-enables the collider, renderer, and effects,
// making the reward visible and collectible.
//
// Works for both AbilityToken and DrawingPageCollectable objects.
// Place this on the root of the reward GameObject.
[RequireComponent(typeof(Collider))]
public class TaskReward : MonoBehaviour
{
    [SerializeField] private BackYardTaskTracker.TaskId _requiredTask;

    private Collider[]      _colliders;
    private Renderer[]      _renderers;
    private Light[]         _effectLights;
    private TokenEffect[]   _tokenEffects;
    private bool            _revealed;

    private void Awake()
    {
        _colliders    = GetComponentsInChildren<Collider>(true);
        _renderers    = GetComponentsInChildren<Renderer>(true);
        _effectLights = GetComponentsInChildren<Light>(true);
        _tokenEffects = GetComponentsInChildren<TokenEffect>(true);
    }

    private void Start()
    {
        // If the task was already completed before this scene loaded
        // (persisted across sessions), reveal immediately.
        if (BackYardTaskTracker.IsCompleted(_requiredTask))
        {
            Reveal();
            return;
        }

        Hide();
        BackYardTaskTracker.OnTaskCompleted += OnTaskCompleted;
    }

    private void OnDestroy()
    {
        BackYardTaskTracker.OnTaskCompleted -= OnTaskCompleted;
    }

    private void OnTaskCompleted(BackYardTaskTracker.TaskId id)
    {
        if (id != _requiredTask) return;
        Reveal();
    }

    public void SetRequiredTask(BackYardTaskTracker.TaskId taskId)
    {
        _requiredTask = taskId;
    }

    private void Hide()
    {
        foreach (var col in _colliders)    col.enabled = false;
        foreach (var rend in _renderers)   rend.enabled = false;
        foreach (var fx in _tokenEffects)  fx.enabled = false;
        foreach (var light in _effectLights) light.enabled = false;
    }

    private void Reveal()
    {
        if (_revealed) return;
        _revealed = true;

        foreach (var col in _colliders)    col.enabled = true;
        foreach (var rend in _renderers)   rend.enabled = true;
        foreach (var fx in _tokenEffects)  fx.enabled = true;
        foreach (var light in _effectLights) light.enabled = true;

        BackYardTaskTracker.OnTaskCompleted -= OnTaskCompleted;
    }
}
