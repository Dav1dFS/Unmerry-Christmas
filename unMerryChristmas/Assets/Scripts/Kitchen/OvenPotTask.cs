using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Task 4 — the final, two-stage task. Lives on the Oven Pot, which needs a trigger
/// collider over its opening (added by the Setup Kitchen Tasks tool).
///
/// Stage 1 — Cook: a non-nutcracker pickable dropped into the pot is "cooked" — it
/// vanishes and smoke rises from the pot.
///
/// Stage 2 — Bomb: once something has been cooked, an explosive present detonating on
/// the pot fires the designer-supplied <see cref="_onPotBombed"/> effect and completes
/// the task. Bombing an empty pot does nothing.
/// </summary>
[RequireComponent(typeof(Collider))]
public class OvenPotTask : MonoBehaviour, IExplosionReactive
{
    [SerializeField] private TaskDefinition _task;

    [Tooltip("Where smoke appears. Defaults to this object's position if unset.")]
    [SerializeField] private Transform _smokeAnchor;

    [Tooltip("Invoked when the loaded pot is bombed. Wire the final effect here — " +
             "left intentionally blank for now.")]
    [SerializeField] private UnityEvent _onPotBombed;

    private bool _loaded;
    private bool _completed;
    private ParticleSystem _smoke;

    private void OnTriggerEnter(Collider other)
    {
        if (_completed) return;

        // Only pickable items "cook", and never a nutcracker.
        var pickup = other.GetComponentInParent<PickupObject>();
        if (pickup == null) return;
        if (other.GetComponentInParent<Nutcracker>() != null) return;

        Destroy(pickup.gameObject);
        _loaded = true;
        StartSmoke();
    }

    public void OnExplosion(Vector3 origin, float radius)
    {
        if (_completed || !_loaded) return;

        _completed = true;
        StopSmoke();
        _onPotBombed?.Invoke(); // ← final effect, intentionally left for the designer to fill in
        TaskService.ReportTask(_task);
    }

    private void StartSmoke()
    {
        if (_smoke != null) return;
        Vector3 pos = (_smokeAnchor != null ? _smokeAnchor : transform).position;
        _smoke = BuildSmoke(pos, _smokeAnchor != null ? _smokeAnchor : transform);
    }

    private void StopSmoke()
    {
        if (_smoke == null) return;
        _smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        Destroy(_smoke.gameObject, 3f);
        _smoke = null;
    }

    // Procedural smoke so no prefab/art asset is required (mirrors ThrowableImpact's VFX).
    private static ParticleSystem BuildSmoke(Vector3 position, Transform parent)
    {
        var go = new GameObject("OvenSmoke");
        go.transform.position = position;
        if (parent != null) go.transform.SetParent(parent, worldPositionStays: true);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop();

        var psr = go.GetComponent<ParticleSystemRenderer>();
        psr.material = new Material(Shader.Find("Particles/Standard Unlit"));

        var main = ps.main;
        main.loop = true;
        main.startLifetime = 2.5f;
        main.startSpeed = 0.6f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
        main.startColor = new Color(0.6f, 0.6f, 0.6f, 0.35f);
        main.gravityModifier = -0.05f; // gentle upward drift

        var emission = ps.emission;
        emission.rateOverTime = 12f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.15f;
        shape.rotation = new Vector3(-90f, 0f, 0f); // emit upward

        ps.Play();
        return ps;
    }
}
