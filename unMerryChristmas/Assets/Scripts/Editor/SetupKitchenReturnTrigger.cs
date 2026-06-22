using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click creation of the Kitchen → Back Yard return trigger.
///
/// The Back Yard reaches the Kitchen via WindowClimb (sets entry "from_backyard_window"),
/// but the Kitchen had no way back — no SceneTransitionTrigger existed in either scene.
/// The Back Yard already has a "from_kitchen" SceneEntryPoint waiting for the player.
///
/// This tool creates "ReturnToBackyardTrigger" in the open Kitchen scene:
///   • positions it on the "EntryPoint_FromBackyard" spawn (the window you climb in at),
///     so the same window is the way back out,
///   • adds a trigger BoxCollider sized for a doorway/window,
///   • adds SceneTransitionTrigger → BackYardScenario, entry "from_kitchen".
///
/// The SceneTransitionTrigger's re-arm guard means spawning on top of it from the
/// Back Yard won't bounce you straight back — you must walk out and return.
///
/// Run it with the Kitchen scene open:  Tools ▸ UnMerry ▸ Setup Kitchen Return Trigger.
/// Then reposition/resize the box to match your exit geometry and save the scene.
/// </summary>
public static class SetupKitchenReturnTrigger
{
    private const string MenuPath     = "Tools/UnMerry/Setup Kitchen Return Trigger";
    private const string TriggerName  = "ReturnToBackyardTrigger";
    private const string TargetScene  = "BackYardScenario";
    private const string EntryPointId = "from_kitchen";

    [MenuItem(MenuPath)]
    public static void Run()
    {
        Scene scene = SceneManager.GetActiveScene();

        // Reuse an existing trigger if the tool was already run, otherwise create one.
        GameObject existing = GameObject.Find(TriggerName);
        GameObject go = existing != null ? existing : new GameObject(TriggerName);

        if (existing == null)
        {
            Undo.RegisterCreatedObjectUndo(go, "Create Kitchen Return Trigger");

            // Sit it on the arrival spawn point so the window doubles as the exit.
            Vector3 pos = FindArrivalPoint(out bool found);
            go.transform.position = pos;
            if (!found)
                Debug.LogWarning("[SetupKitchenReturnTrigger] Could not find " +
                    "'EntryPoint_FromBackyard' — placed the trigger at the origin. " +
                    "Move it to your Kitchen exit (the window/door) manually.", go);
        }
        else
        {
            Undo.RegisterFullObjectHierarchyUndo(go, "Configure Kitchen Return Trigger");
        }

        // Trigger BoxCollider — sized for a doorway, raised to body height.
        var box = go.GetComponent<BoxCollider>();
        if (box == null) box = Undo.AddComponent<BoxCollider>(go);
        box.isTrigger = true;
        box.center = new Vector3(0f, 1f, 0f);
        box.size   = new Vector3(2.5f, 2.5f, 2.5f);

        // The transition component, with its target scene + entry point wired.
        var trigger = go.GetComponent<SceneTransitionTrigger>();
        if (trigger == null) trigger = Undo.AddComponent<SceneTransitionTrigger>(go);

        var so = new SerializedObject(trigger);
        so.FindProperty("_targetScene").stringValue  = TargetScene;
        so.FindProperty("_entryPointId").stringValue = EntryPointId;
        so.ApplyModifiedPropertiesWithoutUndo();

        // The Kitchen shipped with TWO entry points flagged _isDefault — both
        // "from_backyard_window" and "default" — so the fallback spawn was decided
        // by OnEnable order. Make sure only "default" is the default.
        FixDuplicateDefaultEntryPoints();

        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(scene);

        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);

        Debug.Log($"[SetupKitchenReturnTrigger] '{TriggerName}' ready at " +
                  $"{go.transform.position} → {TargetScene} [{EntryPointId}]. " +
                  "Reposition/resize the BoxCollider to your exit, then save the scene.", go);
    }

    // Only enabled while a scene is loaded.
    [MenuItem(MenuPath, validate = true)]
    private static bool Validate() => SceneManager.GetActiveScene().isLoaded;

    /// <summary>
    /// Ensure exactly one SceneEntryPoint is marked default: the one whose
    /// _entryId is literally "default". Any other point flagged default is cleared,
    /// so SceneEntryPoint's static _defaultPoint fallback is deterministic.
    /// </summary>
    private static void FixDuplicateDefaultEntryPoints()
    {
        foreach (var ep in Object.FindObjectsByType<SceneEntryPoint>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var so = new SerializedObject(ep);
            string id = so.FindProperty("_entryId").stringValue;
            var isDefault = so.FindProperty("_isDefault");

            bool shouldBeDefault = id == "default";
            if (isDefault.boolValue != shouldBeDefault)
            {
                isDefault.boolValue = shouldBeDefault;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(ep);
                Debug.Log($"[SetupKitchenReturnTrigger] Entry point '{id}' _isDefault " +
                          $"→ {shouldBeDefault}.", ep);
            }
        }
    }

    /// <summary>
    /// Locate the spawn the player arrives at from the Back Yard. Prefer a
    /// SceneEntryPoint whose serialized _entryId is "from_backyard_window";
    /// fall back to the GameObject named "EntryPoint_FromBackyard".
    /// </summary>
    private static Vector3 FindArrivalPoint(out bool found)
    {
        foreach (var ep in Object.FindObjectsByType<SceneEntryPoint>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var so = new SerializedObject(ep);
            if (so.FindProperty("_entryId").stringValue == "from_backyard_window")
            {
                found = true;
                return ep.transform.position;
            }
        }

        var byName = GameObject.Find("EntryPoint_FromBackyard");
        if (byName != null) { found = true; return byName.transform.position; }

        found = false;
        return Vector3.zero;
    }
}
