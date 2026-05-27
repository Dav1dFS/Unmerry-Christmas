using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IcePatch : MonoBehaviour
{
    [SerializeField] private GameObject    _iceVisual;
    [SerializeField] private PhysicsMaterial _iceMaterial;  // Dynamic=0.02, Static=0.02, Combine=Min

    public bool IsActive { get; private set; }

    private Collider _col;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        if (_iceVisual != null) _iceVisual.SetActive(false);
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;

        if (_iceVisual   != null) _iceVisual.SetActive(true);
        if (_iceMaterial != null) _col.material = _iceMaterial;
    }
}
