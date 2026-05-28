using UnityEngine;

public class ThrowableImpact : MonoBehaviour
{
    [Tooltip("Radius around the impact point in which NPCs become distracted by the noise.")]
    [SerializeField] private float _noiseRadius = 5f;

    private bool _thrown = false;
    public void SetThrown() => _thrown = true;

    private void OnCollisionEnter(Collision col)
    {
        Debug.Log($"Impact with: {col.gameObject.name} | Was thrown={_thrown}");
        if (!_thrown) return;

        // Direct hit on an NPC → alert it (threat, not distraction)
        NpcController npc = col.gameObject.GetComponentInParent<NpcController>();
        if (npc != null)
        {
            npc.OnHit();
            _thrown = false;
            return;
        }

        // Hit a breakable object → break it
        IBreakable breakable = col.gameObject.GetComponentInParent<IBreakable>();
        if (breakable != null)
        {
            breakable.Break();
            _thrown = false;
            DistractNearbyNpcs(col.GetContact(0).point, directHitNpc: null);
            return;
        }

        // Hit the ground or any neutral surface → distract nearby NPCs with noise
        if (!col.gameObject.CompareTag("Player"))
        {
            _thrown = false;
            DistractNearbyNpcs(col.GetContact(0).point, directHitNpc: null);
        }
    }

    /// <summary>
    /// Scans a sphere around <paramref name="point"/> and calls EnterDistracted on any
    /// NpcController found within <see cref="_noiseRadius"/>, excluding the one that was
    /// directly hit (if any).
    /// </summary>
    private void DistractNearbyNpcs(Vector3 point, NpcController directHitNpc)
    {
        // Use OverlapSphere on all layers — NPCs may live on any layer
        Collider[] hits = Physics.OverlapSphere(point, _noiseRadius);
        foreach (Collider hit in hits)
        {
            NpcController nearby = hit.GetComponentInParent<NpcController>();
            if (nearby == null || nearby == directHitNpc) continue;

            // Only distract if the NPC isn't already watching or disabled
            if (nearby.CurrentState != NpcStates.Watching &&
                nearby.CurrentState != NpcStates.Disabled)
            {
                nearby.EnterDistracted();
            }
        }
    }
}