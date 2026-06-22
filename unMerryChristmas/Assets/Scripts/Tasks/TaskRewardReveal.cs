using UnityEngine;

/// <summary>
/// Scene-agnostic counterpart to <c>TaskReward</c> (which is bound to the Backyard's
/// tracker and left untouched). Hides the reward object — collider, renderers, lights,
/// token glow — until its required <see cref="TaskDefinition"/> completes, then reveals
/// it so it can be collected.
///
/// Use on Kitchen rewards: the drawing page (revealed when the nutcrackers are trashed)
/// and the ExplosivePresents ability token (revealed when all hot chocolates are drunk).
/// </summary>
[RequireComponent(typeof(Collider))]
public class TaskRewardReveal : MonoBehaviour
{
    [SerializeField] private TaskDefinition _requiredTask;

    private Collider[]    _colliders;
    private Renderer[]    _renderers;
    private Light[]       _effectLights;
    private TokenEffect[] _tokenEffects;
    private bool          _revealed;

    private void Awake()
    {
        _colliders    = GetComponentsInChildren<Collider>(true);
        _renderers    = GetComponentsInChildren<Renderer>(true);
        _effectLights = GetComponentsInChildren<Light>(true);
        _tokenEffects = GetComponentsInChildren<TokenEffect>(true);
    }

    private void Start()
    {
        // Already completed before this scene loaded (persisted): reveal immediately.
        if (TaskService.IsCompleted(_requiredTask))
        {
            Reveal();
            return;
        }

        Hide();
        TaskService.OnTaskCompleted += OnTaskCompleted;
    }

    private void OnDestroy()
    {
        TaskService.OnTaskCompleted -= OnTaskCompleted;
    }

    private void OnTaskCompleted(TaskDefinition def)
    {
        if (def == _requiredTask) Reveal();
    }

    public void SetRequiredTask(TaskDefinition def) => _requiredTask = def;

    private void Hide()
    {
        foreach (var col in _colliders)      col.enabled  = false;
        foreach (var rend in _renderers)     rend.enabled = false;
        foreach (var fx in _tokenEffects)    fx.enabled   = false;
        foreach (var light in _effectLights) light.enabled = false;
    }

    private void Reveal()
    {
        if (_revealed) return;
        _revealed = true;

        foreach (var col in _colliders)      col.enabled  = true;
        foreach (var rend in _renderers)     rend.enabled = true;
        foreach (var fx in _tokenEffects)    fx.enabled   = true;
        foreach (var light in _effectLights) light.enabled = true;

        TaskService.OnTaskCompleted -= OnTaskCompleted;
    }
}
