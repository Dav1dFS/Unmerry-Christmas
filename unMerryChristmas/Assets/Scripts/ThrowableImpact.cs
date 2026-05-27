using UnityEngine;

public class ThrowableImpact : MonoBehaviour
{
    private bool _thrown = false;
    public void SetThrown() => _thrown = true;

    private void OnCollisionEnter(Collision col)
    {
        Debug.Log($"Impact with: {col.gameObject.name} | Was thrown={_thrown}");
        if (!_thrown) return;

        // GetComponentInParent makes sure it hits the NPC and not its children
        NpcController npc = col.gameObject.GetComponentInParent<NpcController>();
        if (npc != null)
        {
            npc.OnHit();
            _thrown = false;
            return;
        }

        IBreakable breakable = col.gameObject.GetComponentInParent<IBreakable>();
        if (breakable != null)
        {
            breakable.Break();
            _thrown = false;
            return;
        }

        // if it hits something else reset the flag
        if (!col.gameObject.CompareTag("Player"))
            _thrown = false;
    }
}