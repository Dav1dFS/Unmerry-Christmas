using UnityEngine;
public class ShedShelf : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject[] _contents;        // inactive children to activate + physics
    [SerializeField] private Rigidbody[]  _contentBodies;   // same objects' Rigidbodies (pre-assigned)
    [SerializeField] private Vector3      _scatterImpulse = new(0f, 2f, 2f);

    private bool _cleared;

    public void Interact()
    {
        if (_cleared) return;
        _cleared = true;

        foreach (var obj in _contents)
        {
            if (obj != null) obj.SetActive(true);
        }

        foreach (var rb in _contentBodies)
        {
            if (rb == null) continue;
            rb.isKinematic = false;
            // Add slight random spread so items don't fly in a single direction
            Vector3 scatter = _scatterImpulse + Random.insideUnitSphere * 0.5f;
            rb.AddForce(transform.TransformDirection(scatter), ForceMode.Impulse);
        }

        ShedHeistTracker.Instance?.ReportShelfCleared();
    }

    public string GetHintText() => "E — Clear Shelf";
}
