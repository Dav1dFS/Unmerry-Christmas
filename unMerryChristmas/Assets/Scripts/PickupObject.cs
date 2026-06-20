using System;
using System.Collections;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public event Action OnPickedUp;
    public event Action OnDropped;

    [Tooltip("Size while held, as a multiplier of the object's natural on-map size. " +
             "(1,1,1) = exactly the size it is on the map; (0.5,0.5,0.5) = half.")]
    [SerializeField] private Vector3 holdScale = Vector3.one;

    [Tooltip("Rotation applied while held, relative to the hand's orientation.")]
    [SerializeField] private Vector3 holdRotationOffset = Vector3.zero;

    [Tooltip("Position offset from the hand, in world units. Use to re-centre objects " +
             "whose pivot isn't at their visual centre.")]
    [SerializeField] private Vector3 holdPositionOffset = Vector3.zero;

    private const string AnchorName = "PickupAnchor";

    public bool HasBeenPickedUp { get; private set; }
    public Quaternion NaturalWorldRotation { get; private set; }
    public Vector3 NaturalWorldScale { get; private set; }

    private Rigidbody rb;
    private Collider col;
    private Collider playerCol;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        // Record world-space values while still parented correctly in the scene.
        // NaturalWorldScale is the TRUE on-map size (lossyScale), already baking in
        // any scaled parent groups — this is the size we reproduce in-hand and restore on drop.
        NaturalWorldRotation = transform.rotation;
        NaturalWorldScale    = transform.lossyScale;
    }

    public void OnPickup(Transform holdPoint)
    {
        HasBeenPickedUp = true;
        rb.isKinematic = true;
        col.enabled = false;

        // Cache the player's collider (capsule lives on the holdPoint's root) so we
        // can avoid ejecting the player when the collider is re-enabled on drop.
        if (playerCol == null)
            playerCol = holdPoint.GetComponentInParent<Collider>();

        // The hold point inherits the player's NON-UNIFORM scale. Parenting a rotated
        // object directly under a non-uniformly scaled transform shears (skews) it, and
        // no localScale can undo that. So we hold under a child "anchor" whose world
        // scale is forced back to a uniform (1,1,1); on that clean basis, the rotation
        // and scale we set are shear-free and the object keeps its natural shape.
        Transform anchor = GetOrCreateUniformAnchor(holdPoint);

        transform.SetParent(anchor);
        transform.localPosition = holdPositionOffset;
        transform.localRotation = Quaternion.Euler(holdRotationOffset);
        // Anchor world scale is 1, so localScale == world scale.
        transform.localScale = Vector3.Scale(NaturalWorldScale, holdScale);

        OnPickedUp?.Invoke();
    }

    public void OnDrop()
    {
        // Un-parent to the world root. With no parent, localScale == lossyScale, so
        // assigning NaturalWorldScale reproduces the exact on-map size instead of a
        // raw localScale that would be wrong by any parent group's scale factor.
        transform.SetParent(null);
        transform.localScale = NaturalWorldScale;

        rb.isKinematic = false;
        col.enabled = true;

        // The drop point overlaps the player's capsule; re-enabling the collider here
        // would let PhysX depenetration shove the player backwards. Ignore collisions
        // with the player until the rock has separated from it.
        if (playerCol != null)
            StartCoroutine(RestorePlayerCollisionWhenClear());

        OnDropped?.Invoke();
    }

    // Returns a child of the hold point whose WORLD scale is uniform (1,1,1), creating
    // it once and reusing it for every pickup. The counter-scale is recomputed each call
    // so it stays correct even if the player's scale changes at runtime.
    private static Transform GetOrCreateUniformAnchor(Transform holdPoint)
    {
        Transform anchor = holdPoint.Find(AnchorName);
        if (anchor == null)
        {
            anchor = new GameObject(AnchorName).transform;
            anchor.SetParent(holdPoint, worldPositionStays: false);
            anchor.localPosition = Vector3.zero;
            anchor.localRotation = Quaternion.identity;
        }

        Vector3 hp = holdPoint.lossyScale;
        anchor.localScale = new Vector3(
            hp.x != 0f ? 1f / hp.x : 1f,
            hp.y != 0f ? 1f / hp.y : 1f,
            hp.z != 0f ? 1f / hp.z : 1f);
        return anchor;
    }

    private IEnumerator RestorePlayerCollisionWhenClear()
    {
        Physics.IgnoreCollision(col, playerCol, true);

        // Wait until the rock and player no longer overlap (with a safety timeout so
        // collision is always restored even if the rock rests touching the player).
        float timeout = 2f;
        while (timeout > 0f && col != null && playerCol != null
               && col.bounds.Intersects(playerCol.bounds))
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (col != null && playerCol != null)
            Physics.IgnoreCollision(col, playerCol, false);
    }
}
