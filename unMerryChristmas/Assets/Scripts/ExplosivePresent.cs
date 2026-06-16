using UnityEngine;

public class ExplosivePresent : MonoBehaviour
{
    [Header("Screen Shake — NPC Direct Hit")]
    [SerializeField] private float _npcHitShakeDuration = 0.1f;
    [SerializeField] private float _npcHitShakeMagnitude = 0.05f;

    [Header("Screen Shake — Explosion")]
    [SerializeField] private float _explosionShakeDuration = 0.3f;
    [SerializeField] private float _explosionShakeMagnitude = 0.2f;
    [SerializeField] private float _explosionShakeFrequency = 20f;

    [Header("Explosion Settings")]
    [SerializeField] private float _explosionRadius = 4f;
    [SerializeField] private float _explosionForce = 12f;
    [SerializeField] private float _fuseTime = 2f;
    [SerializeField] private LayerMask _affectedLayers;

    [Header("Impact Frame — NPC Direct Hit")]
    [SerializeField] private float _npcHitFrameScale = 2f;
    [SerializeField] private float _npcHitFrameDuration = 0.1f;
    [SerializeField] private Color _npcHitFrameColor = Color.red;
    [SerializeField] private float _npcHitStopDuration = 0.06f;

    [Header("Impact Frame — Explosion")]
    [SerializeField] private float _explosionFrameScale = 5f;
    [SerializeField] private float _explosionFrameDuration = 0.15f;
    [SerializeField] private Color _explosionFrameColor = Color.yellow;
    [SerializeField] private float _explosionHitStopDuration = 0.12f;

    private float _timer;
    private bool _armed = false;

    public void Arm()
    {
        _armed = true;
        _timer = _fuseTime;
    }

    private void Update()
    {
        if (!_armed) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
            Explode();
    }

    private void OnCollisionEnter(Collision col)
    {
        if (!_armed) return;

        NpcController npc = col.gameObject.GetComponentInParent<NpcController>();
        if (npc != null)
        {
            npc.OnHit();
            npc.ApplyHitStop();

            // Screen shake for NPC direct hit
            CameraShake.Instance?.Shake(_npcHitShakeDuration, _npcHitShakeMagnitude);

            // Impact frame at contact point
            HitStopManager.Instance?.TriggerHitStop(_npcHitStopDuration);
            SpawnImpactFrame(col.GetContact(0).point, _npcHitFrameScale,
                             _npcHitFrameDuration, _npcHitFrameColor);
        }
    }

    private void Explode()
    {
        // Screen shake for explosion

        CameraShake.Instance?.Shake(_explosionShakeDuration, _explosionShakeMagnitude, _explosionShakeFrequency);

        // Explosion impact frame at own position
        HitStopManager.Instance?.TriggerHitStop(_explosionHitStopDuration);
        SpawnImpactFrame(transform.position, _explosionFrameScale,
                         _explosionFrameDuration, _explosionFrameColor);

        Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius, _affectedLayers);
        foreach (Collider hit in hits)
        {
            NpcController npc = hit.GetComponent<NpcController>();
            if (npc != null)
            {
                npc.OnHit();
                npc.ApplyHitStop();
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                rb.AddForce(dir * _explosionForce, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }

    private void SpawnImpactFrame(Vector3 point, float scale, float duration, Color color)
    {
        GameObject go = new GameObject("ImpactFrame");
        go.transform.position = point;

        Camera cam = Camera.main;
        if (cam != null)
            go.transform.rotation = Quaternion.LookRotation(
                cam.transform.forward, cam.transform.up);

        ImpactFrameRenderer r = go.AddComponent<ImpactFrameRenderer>();
        r.Init(scale, duration, color);
    }
}