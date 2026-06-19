using UnityEngine;

public class ExplosivePresent : MonoBehaviour
{
    [SerializeField] private float _explosionRadius = 4f;
    [SerializeField] private float _explosionForce = 12f;
    [SerializeField] private float _fuseTime = 2f;
    [SerializeField] private LayerMask _affectedLayers;

    [Header("Audio")]
    [SerializeField] private FMODUnity.EventReference _deploySound;

    private float _timer;
    private bool _armed = false;

    public void Arm()
    {
        _armed = true;
        _timer = _fuseTime;

        if (!_deploySound.IsNull)
        {
            FMODUnity.RuntimeManager.PlayOneShot(_deploySound, transform.position);
        }
    }

    void Update()
    {
        if (!_armed) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            Explode();
        }
    }
    private void OnCollisionEnter(Collision col)
    {
        if (!_armed) return;

        NpcController npc = col.gameObject.GetComponentInParent<NpcController>();
        if (npc != null)
            npc.OnHitByThrownObject();
    }
    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius, _affectedLayers);
        foreach (Collider hit in hits)
        {
            // Hit NPC goes into alerted state
            NpcController npc = hit.GetComponent<NpcController>();
            if (npc != null)
                npc.OnHit();

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                rb.AddForce(dir * _explosionForce, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
