using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click wiring for the Back Yard drawing-book key item.
///
/// The scene's "DrawingBook" object ships tagged "Collectable" on the Pickable
/// layer. With that tag, Controller.TryPickup routes it to the DRAWING-PAGE path
/// (CollectableManager.Collect) — so picking it up would wrongly count as 1 of
/// the 20 pages and just destroy the object with no book/animation.
///
/// This tool reconfigures the object to be a proper key item:
///   • adds the DrawingBookPickup component (IInteractable),
///   • clears the "Collectable"/"Token" tag (→ "Untagged") so it routes through
///     the IInteractable pickup branch instead of the page counter,
///   • ensures it sits on the Pickable layer with a non-trigger collider,
///   • fits the pickup BoxCollider to the mesh so the grab distance matches other
///     pickups (it shipped as an oversized 1×1×1 box),
///   • disables any PickupObject so it can't be grabbed/carried,
///   • pins the Rigidbody (kinematic) so the book stays wedged in place,
///   • replaces TokenEffect (rotate/bob/light) with a stationary SubtleGlow.
///
/// Run it with the Back Yard scene open:  Tools ▸ UnMerry ▸ Setup Drawing Book Pickup.
/// Then save the scene.
/// </summary>
public static class SetupDrawingBookPickup
{
    private const string MenuPath = "Tools/UnMerry/Setup Drawing Book Pickup";

    [MenuItem(MenuPath)]
    public static void Run()
    {
        GameObject book = FindBookObject();
        if (book == null)
        {
            EditorUtility.DisplayDialog(
                "Drawing Book Pickup",
                "Could not find the drawing-book object in the open scene.\n\n" +
                "Open the Back Yard scene and make sure the book object is named " +
                "\"DrawingBook\" (or has a Collider on the Pickable layer), then " +
                "run this tool again.",
                "OK");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(book, "Setup Drawing Book Pickup");

        // 1. Component
        if (book.GetComponent<DrawingBookPickup>() == null)
            Undo.AddComponent<DrawingBookPickup>(book);

        // 2. Tag — must not be Collectable/Token or it routes to the page counter.
        if (book.CompareTag("Collectable") || book.CompareTag("Token"))
            book.tag = "Untagged";

        // 3. Pickable layer on the root and on every collider object, so
        //    OverlapSphere(pickupLayer) finds the book however it's nested.
        int pickable = LayerMask.NameToLayer("Pickable");
        if (pickable >= 0)
        {
            book.layer = pickable;
            foreach (var c in book.GetComponentsInChildren<Collider>(true))
                c.gameObject.layer = pickable;
        }

        // 4. Ensure a non-trigger collider exists somewhere (add one to the root
        //    only if the hierarchy has none). The pickup resolves via
        //    GetComponentInParent, so a child collider is fine.
        var colliders = book.GetComponentsInChildren<Collider>(true);
        if (colliders.Length == 0)
        {
            var sphere = Undo.AddComponent<SphereCollider>(book);
            sphere.isTrigger = false;
        }
        else
        {
            foreach (var c in colliders)
            {
                c.isTrigger = false;
                c.enabled = true;
            }
        }

        // 4b. Fit the pickup BoxCollider tightly to the book's visible mesh, so
        //     its grab distance matches other pickups (it shipped as a 1×1×1 box,
        //     ~2 m wide once scaled — pickable from much farther than normal).
        var box = book.GetComponent<BoxCollider>();
        if (box != null)
            FitBoxToRenderers(book.transform, box);

        // 5. Disable any PickupObject so the book isn't grabbed as a carryable.
        var pickup = book.GetComponent<PickupObject>();
        if (pickup != null)
            pickup.enabled = false;

        // 5b. Pin the Rigidbody so the key item stays wedged in place instead of
        //     falling / drifting (it doesn't need physics once it's a key item).
        var rb = book.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity  = false;
        }

        // 6. Replace TokenEffect (rotates + bobs + point light — built for floating
        //    tokens, wrong for a placed book) with a stationary SubtleGlow.
        foreach (var fx in book.GetComponentsInChildren<TokenEffect>(true))
            Undo.DestroyObjectImmediate(fx);
        if (book.GetComponentInChildren<SubtleGlow>(true) == null)
            Undo.AddComponent<SubtleGlow>(book);

        EditorUtility.SetDirty(book);
        EditorSceneManager.MarkSceneDirty(book.scene);

        Selection.activeGameObject = book;
        EditorGUIUtility.PingObject(book);

        Debug.Log($"[SetupDrawingBookPickup] Configured '{book.name}' as a drawing-book " +
                  $"key item (tag='{book.tag}', layer='{LayerMask.LayerToName(book.layer)}'). " +
                  "Save the scene to keep the change.", book);
    }

    [MenuItem(MenuPath, validate = true)]
    private static bool Validate() => SceneManager.GetActiveScene().isLoaded;

    /// <summary>
    /// Resizes <paramref name="box"/> to tightly enclose every renderer under
    /// <paramref name="root"/>. World-space bounds are converted into the box's
    /// local space (via lossyScale) so the collider hugs the mesh regardless of
    /// the object's scale — giving the book a normal, consistent pickup distance.
    /// </summary>
    private static void FitBoxToRenderers(Transform root, BoxCollider box)
    {
        var rends = root.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return;

        Bounds world = rends[0].bounds;
        foreach (var r in rends) world.Encapsulate(r.bounds);

        Vector3 lossy = root.lossyScale;
        box.center = root.InverseTransformPoint(world.center);
        box.size = new Vector3(
            lossy.x != 0f ? world.size.x / Mathf.Abs(lossy.x) : world.size.x,
            lossy.y != 0f ? world.size.y / Mathf.Abs(lossy.y) : world.size.y,
            lossy.z != 0f ? world.size.z / Mathf.Abs(lossy.z) : world.size.z);
    }

    /// <summary>
    /// Locate the book mesh. The Back Yard prefab has TWO objects named
    /// "DrawingBook": the TutorialPrompt root (has TutorialPromptTrigger, a
    /// trigger collider, no renderer) and the book mesh (tag "Collectable",
    /// colliders on child mesh parts). Prefer the mesh: not a prompt, has a
    /// collider somewhere in its hierarchy. Fall back to name alone.
    /// </summary>
    private static GameObject FindBookObject()
    {
        var all = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        GameObject fallback = null;
        foreach (var t in all)
        {
            if (t.name != "DrawingBook") continue;
            fallback ??= t.gameObject;

            bool isPrompt    = t.GetComponent<TutorialPromptTrigger>() != null;
            bool hasCollider = t.GetComponentInChildren<Collider>(true) != null;
            if (!isPrompt && hasCollider)
                return t.gameObject;
        }
        return fallback; // name-only fallback (collider added by the tool)
    }
}
