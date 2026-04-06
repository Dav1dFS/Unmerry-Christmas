using UnityEngine;

public class ExplosivePresent : MonoBehaviour
{
    [SerializeField] private float _explosionRadius = 4f;
    [SerializeField] private float _explosionForce = 12f;
    [SerializeField] private float _fuseTime = 2f;
    [SerializeField] private LayerMask _affectedLayers;

    private float _timer;
    private bool _armed = false;

    public void Arm()
    {
        _armed = true;
        _timer = _fuseTime;
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

    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _explosionRadius, _affectedLayers);
        foreach (Collider hit in colliders)
        {
            //Pushable Objects with RigidBody
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                rb.AddForce(dir * _explosionForce, ForceMode.Impulse);
            }
        }
        // Optionally, add explosion effects here (e.g., particle system, sound)
        Destroy(gameObject);
    }

}
