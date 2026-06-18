using UnityEngine;

public class ThrowableImpact : MonoBehaviour
{
    [Tooltip("Radius around the impact point in which NPCs become distracted by the noise.")]
    [SerializeField] private float _noiseRadius = 5f;

    [Header("Destruction on Impact")]
    [SerializeField] private bool _destroyOnImpact = false;

    [Header("Particle Settings")]
    [SerializeField] private Color _particleColorA = Color.white;
    [SerializeField] private Color _particleColorB = Color.white;
    [SerializeField] private float _particleSpeedMin = 0.1f;
    [SerializeField] private float _particleSpeedMax = 2f;
    [SerializeField] private float _particleSizeMin = 0.03f;
    [SerializeField] private float _particleSizeMax = 0.1f;
    [SerializeField] private int _particleCount = 25;
    [SerializeField] private float _particleLifetime = 1f;
    [SerializeField] private float _particleGravity = 1f;

    [Header("Impact Frame")]
    [SerializeField] private bool _showImpactFrame = true;
    [SerializeField] private float _impactFrameScale = 0.7f;
    [SerializeField] private float _impactFrameDuration = 0.11f;
    [SerializeField] private Color _impactFrameColor = Color.white;

    [Header("Screen Shake — NPC Hit")]
    [SerializeField] private float _npcShakeDuration = 0.3f;
    [SerializeField] private float _npcShakeMagnitude = 0.2f;
    [SerializeField] private float _npcShakeFrequency = 20f;

    private bool _thrown = false;

    public void SetThrown() => _thrown = true;

    private void OnCollisionEnter(Collision col)
    {
        if (!_thrown) return;

        NpcController npc = col.gameObject.GetComponentInParent<NpcController>();
        if (npc != null)
        {
            npc.OnHit();
            npc.ApplyHitStop();
            _thrown = false;
            HitStopManager.Instance?.TriggerHitStop(0.06f);
            CameraShake.Instance?.Shake(_npcShakeDuration, _npcShakeMagnitude, _npcShakeFrequency);
            SpawnImpactFrame(col.GetContact(0).point, _impactFrameScale);
            DestroyIfNeeded(col.GetContact(0).point);
            return;
        }

        IBreakable breakable = col.gameObject.GetComponentInParent<IBreakable>();
        if (breakable != null)
        {
            breakable.Break();
            _thrown = false;
            DistractNearbyNpcs(col.GetContact(0).point, null);
            DestroyIfNeeded(col.GetContact(0).point);
            return;
        }

        if (!col.gameObject.CompareTag("Player"))
        {
            _thrown = false;
            DistractNearbyNpcs(col.GetContact(0).point, null);
            DestroyIfNeeded(col.GetContact(0).point);
        }
    }

    public void TriggerExplosionImpact(Vector3 point)
    {
        HitStopManager.Instance?.TriggerHitStop(0.12f);
        SpawnImpactFrame(point, _impactFrameScale * 2.5f);
        SpawnParticleEffect(point);
    }

    private void SpawnImpactFrame(Vector3 point, float scale)
    {
        if (!_showImpactFrame) return;

        GameObject go = new GameObject("ImpactFrame");
        go.transform.position = point;

        Camera cam = Camera.main;
        if (cam != null)
            go.transform.rotation = Quaternion.LookRotation(
                cam.transform.forward, cam.transform.up);

        ImpactFrameRenderer r = go.AddComponent<ImpactFrameRenderer>();
        r.Init(scale, _impactFrameDuration, _impactFrameColor);
    }

    private void DestroyIfNeeded(Vector3 point)
    {
        if (!_destroyOnImpact) return;
        SpawnParticleEffect(point);
        Destroy(gameObject);
    }

    private void DistractNearbyNpcs(Vector3 point, NpcController directHitNpc)
    {
        Collider[] hits = Physics.OverlapSphere(point, _noiseRadius);
        foreach (Collider hit in hits)
        {
            NpcController nearby = hit.GetComponentInParent<NpcController>();
            if (nearby == null || nearby == directHitNpc) continue;
            if (nearby.CurrentState != NpcStates.Watching &&
                nearby.CurrentState != NpcStates.Disabled)
                nearby.EnterDistracted();
        }
    }

    private void SpawnParticleEffect(Vector3 point)
    {
        GameObject vfxGo = new GameObject("ImpactVFX");
        vfxGo.transform.position = point;

        ParticleSystem ps = vfxGo.AddComponent<ParticleSystem>();
        ps.Stop();

        var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
        psRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        psRenderer.material.color = _particleColorA;

        var main = ps.main;
        main.duration = 0.3f;
        main.loop = false;
        main.startSpeed = new ParticleSystem.MinMaxCurve(_particleSpeedMin, _particleSpeedMax);
        main.startSize = new ParticleSystem.MinMaxCurve(_particleSizeMin, _particleSizeMax);
        main.startColor = new ParticleSystem.MinMaxGradient(_particleColorA, _particleColorB);
        main.startLifetime = new ParticleSystem.MinMaxCurve(_particleLifetime * 0.5f, _particleLifetime);
        main.gravityModifier = _particleGravity;

        var emission = ps.emission;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, _particleCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        ps.Play();
        Destroy(vfxGo, _particleLifetime + 0.5f);
    }
}