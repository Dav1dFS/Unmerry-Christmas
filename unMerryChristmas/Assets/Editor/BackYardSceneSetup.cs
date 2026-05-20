using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Run from the Unity menu: Tools → BackYard Setup
/// Must have BackYardScenario open as the active scene.
/// Safe to run multiple times — skips anything already present.
/// After running, check the Console for warnings about references
/// that still need to be wired manually.
/// </summary>
public static class BackYardSceneSetup
{
    [MenuItem("Tools/BackYard Setup")]
    public static void Run()
    {
        int fixed_count   = 0;
        int created_count = 0;
        int warn_count    = 0;

        // ── 1. Fix HUD Canvas sort order ─────────────────────────────────────
        var hudCanvas = GameObject.Find("HUD_Canvas");
        if (hudCanvas != null)
        {
            var canvas = hudCanvas.GetComponent<Canvas>();
            if (canvas != null && canvas.sortingOrder != 10)
            {
                Undo.RecordObject(canvas, "Fix HUD sort order");
                canvas.sortingOrder = 10;
                fixed_count++;
                Debug.Log("[BackYardSetup] Fixed HUD_Canvas sort order → 10");
            }
        }
        else
        {
            Debug.LogWarning("[BackYardSetup] HUD_Canvas not found — UI may not be set up yet.");
            warn_count++;
        }

        // ── 2. Fix wrong ability token (ability 8 = Throwing in BackYard) ────
        var allTokens = Object.FindObjectsByType<AbilityToken>(FindObjectsSortMode.None);
        foreach (var token in allTokens)
        {
            var so = new SerializedObject(token);
            var abilityProp = so.FindProperty("_ability");
            if (abilityProp != null && abilityProp.enumValueIndex == 8) // 8 = Throwing
            {
                Undo.RecordObject(token.gameObject, "Remove wrong Throwing token");
                Undo.DestroyObjectImmediate(token.gameObject);
                fixed_count++;
                Debug.Log("[BackYardSetup] Removed incorrect Throwing token from BackYard.");
            }
        }

        // ── 3. WindowClimb trigger ────────────────────────────────────────────
        if (GameObject.Find("WindowClimbTrigger") == null)
        {
            var go = CreateGO("WindowClimbTrigger", null);
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size      = new Vector3(1.5f, 2f, 0.3f);
            var wc = go.AddComponent<WindowClimb>();
            SetString(wc, "_targetScene",  "KitchenScenario");
            SetString(wc, "_entryPointId", "from_backyard_window");
            created_count++;
            Debug.Log("[BackYardSetup] Created WindowClimbTrigger — position it at the kitchen window in the Scene view.");
            Warn(ref warn_count, "WindowClimbTrigger needs to be repositioned at the kitchen window.");
        }

        // ── 4. GardenChaosTracker ─────────────────────────────────────────────
        if (FindComponent<GardenChaosTracker>() == null)
        {
            var go = CreateGO("GardenChaosTracker", null);
            go.AddComponent<GardenChaosTracker>();
            created_count++;
            Debug.Log("[BackYardSetup] Created GardenChaosTracker.");
            Warn(ref warn_count, "GardenChaosTracker: wire _chairA and _chairB to the patio chair TopplableChair components.");

            // Try to wire patio table if name is guessable
            TryWireTable(go, ref warn_count);
        }

        // ── 5. GnomeParadeTracker ─────────────────────────────────────────────
        if (FindComponent<GnomeParadeTracker>() == null)
        {
            var go = CreateGO("GnomeParadeTracker", null);
            go.AddComponent<GnomeParadeTracker>();
            created_count++;
            Debug.Log("[BackYardSetup] Created GnomeParadeTracker.");
            Warn(ref warn_count, "GnomeParadeTracker: wire _zones[0..2] to the 3 GnomePlacementZone components on the placement markers.");
        }

        // ── 6. BoxStackTracker ────────────────────────────────────────────────
        if (FindComponent<BoxStackTracker>() == null)
        {
            var go = CreateGO("BoxStackTracker", null);
            go.AddComponent<BoxStackTracker>();
            created_count++;
            Debug.Log("[BackYardSetup] Created BoxStackTracker.");
            Warn(ref warn_count, "BoxStackTracker: wire _boxes[0..2] to the 3 MovingBox components on the box stacks.");
        }

        // ── 7. ShedHeistTracker ───────────────────────────────────────────────
        if (FindComponent<ShedHeistTracker>() == null)
        {
            var go = CreateGO("ShedHeistTracker", null);
            var tracker = go.AddComponent<ShedHeistTracker>();
            SetInt(tracker, "_totalShelves", 3);
            created_count++;
            Debug.Log("[BackYardSetup] Created ShedHeistTracker (_totalShelves = 3).");
        }

        // ── 8. IcePatch ───────────────────────────────────────────────────────
        if (FindComponent<IcePatch>() == null)
        {
            var go = CreateGO("IcePatch", null);
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = false;
            col.size      = new Vector3(3f, 0.05f, 3f);
            go.AddComponent<IcePatch>();
            created_count++;
            Debug.Log("[BackYardSetup] Created IcePatch GO — position it on the ground near the frozen pipe.");
            Warn(ref warn_count, "IcePatch: assign _iceVisual (a child flat mesh) and _iceMaterial (IceSurface PhysicsMaterial).");

            // Create IceSurface physics material if missing
            EnsureIceMaterial();
        }

        // ── 9. IceSlideTracker ────────────────────────────────────────────────
        if (FindComponent<IceSlideTracker>() == null)
        {
            var go = CreateGO("IceSlideTracker", null);
            var tracker = go.AddComponent<IceSlideTracker>();

            var icePatch = FindComponent<IcePatch>();
            if (icePatch != null)
            {
                SetRef(tracker, "_icePatch", icePatch);
                Debug.Log("[BackYardSetup] IceSlideTracker: auto-wired _icePatch.");
            }
            else
                Warn(ref warn_count, "IceSlideTracker: wire _icePatch manually.");

            var compost = FindComponent<CompostBin>();
            if (compost != null)
            {
                SetRef(tracker, "_compostBin", compost);
                Debug.Log("[BackYardSetup] IceSlideTracker: auto-wired _compostBin.");
            }
            else
                Warn(ref warn_count, "IceSlideTracker: wire _compostBin (CompostBin component) manually.");

            var controller = FindComponent<Controller>();
            if (controller != null)
            {
                SetRef(tracker, "_playerController", controller);
                Debug.Log("[BackYardSetup] IceSlideTracker: auto-wired _playerController.");
            }
            else
                Warn(ref warn_count, "IceSlideTracker: wire _playerController (Controller on Player) manually.");

            created_count++;
            Debug.Log("[BackYardSetup] Created IceSlideTracker.");
        }

        // ── 10. Compost bin component check ───────────────────────────────────
        var compostBin = FindComponent<CompostBin>();
        if (compostBin == null)
        {
            Warn(ref warn_count, "No CompostBin component found in scene. Add CompostBin to the compost bin GameObject, wire _lidTransform and _drawingPage.");
        }
        else
        {
            // Check if lid is wired
            var so = new SerializedObject(compostBin);
            if (so.FindProperty("_lidTransform").objectReferenceValue == null)
                Warn(ref warn_count, "CompostBin._lidTransform is not set — drag the lid child GameObject into it.");
        }

        // ── 11. Remind about task objects that can't be auto-created ─────────
        if (FindComponent<LightsPullable>() == null)
            Warn(ref warn_count, "LightsPullable not found — add it to the Christmas lights strand and wire _looseEnd, _lightsRigidbody, _tangledVisual.");

        if (FindComponent<WindowClimb>() == null)
            Warn(ref warn_count, "WindowClimb not found — create a trigger zone at the kitchen window and add WindowClimb.");

        if (FindComponent<BreakablePipe>() == null)
            Warn(ref warn_count, "BreakablePipe not found — add it to the frozen pipe mesh and wire _intactVisual, _burstVisual, _icePatch.");

        if (FindComponent<BreakableBirdbath>() == null)
            Warn(ref warn_count, "BreakableBirdbath not found — add it to the birdbath mesh and wire _intactVisual, _shatteredBasinVisual.");

        if (FindComponent<ShedDoor>() == null)
            Warn(ref warn_count, "ShedDoor not found — add it to the shed door and create a DoorPivot child at the hinge.");

        if (FindComponent<Padlock>() == null)
            Warn(ref warn_count, "Padlock not found — add it to the padlock mesh and wire _shedDoor, _intactVisual, _brokenVisual.");

        if (Object.FindObjectsByType<ShedShelf>(FindObjectsSortMode.None).Length == 0)
            Warn(ref warn_count, "No ShedShelf components found — add ShedShelf to each of the 3 shed shelves and wire _contents and _contentBodies.");

        if (Object.FindObjectsByType<GnomePlacementZone>(FindObjectsSortMode.None).Length == 0)
            Warn(ref warn_count, "No GnomePlacementZone components found — add GnomePlacementZone + BoxCollider to each of the 3 gnome placement markers.");

        if (Object.FindObjectsByType<MovingBox>(FindObjectsSortMode.None).Length == 0)
            Warn(ref warn_count, "No MovingBox components found — add MovingBox to each of the 3 box stacks and wire _closedVisual, _openVisual.");

        if (Object.FindObjectsByType<TopplableChair>(FindObjectsSortMode.None).Length == 0)
            Warn(ref warn_count, "No TopplableChair components found — add Rigidbody + PickupObject + TopplableChair to each patio chair.");

        if (Object.FindObjectsByType<TippablePatioTable>(FindObjectsSortMode.None).Length == 0)
            Warn(ref warn_count, "No TippablePatioTable found — add Rigidbody + PushableObject + TippablePatioTable to the patio table.");

        // ── Done ──────────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"[BackYardSetup] Done. Fixed: {fixed_count} | Created: {created_count} | Warnings: {warn_count}  (check Console for details)");
        EditorUtility.DisplayDialog("BackYard Setup",
            $"Done!\n\nFixed:    {fixed_count}\nCreated:  {created_count}\nWarnings: {warn_count}\n\nCheck the Console for details on what still needs manual wiring.",
            "OK");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject CreateGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        return go;
    }

    static T FindComponent<T>() where T : Component
        => Object.FindFirstObjectByType<T>();

    static void Warn(ref int count, string msg)
    {
        Debug.LogWarning($"[BackYardSetup] ACTION NEEDED: {msg}");
        count++;
    }

    static void SetString(Component c, string field, string value)
    {
        var so = new SerializedObject(c);
        var p  = so.FindProperty(field);
        if (p != null) { p.stringValue = value; so.ApplyModifiedProperties(); }
    }

    static void SetInt(Component c, string field, int value)
    {
        var so = new SerializedObject(c);
        var p  = so.FindProperty(field);
        if (p != null) { p.intValue = value; so.ApplyModifiedProperties(); }
    }

    static void SetRef(Component c, string field, Object value)
    {
        var so = new SerializedObject(c);
        var p  = so.FindProperty(field);
        if (p != null) { p.objectReferenceValue = value; so.ApplyModifiedProperties(); }
    }

    static void TryWireTable(GameObject trackerGO, ref int warnCount)
    {
        var table = FindComponent<TippablePatioTable>();
        if (table != null)
        {
            var tracker = trackerGO.GetComponent<GardenChaosTracker>();
            SetRef(table, "_gardenChaosTracker", tracker);
            Debug.Log("[BackYardSetup] TippablePatioTable: auto-wired _gardenChaosTracker.");
        }
        else
        {
            Warn(ref warnCount, "TippablePatioTable not found — add Rigidbody + PushableObject + TippablePatioTable to the patio table, then wire _gardenChaosTracker.");
        }
    }

    static void EnsureIceMaterial()
    {
        const string path = "Assets/Materials/IceSurface.physicsMaterial";
        var existing = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
        if (existing != null) return;

        // Make sure the folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        var mat = new PhysicsMaterial("IceSurface")
        {
            dynamicFriction  = 0.02f,
            staticFriction   = 0.02f,
            frictionCombine  = PhysicsMaterialCombine.Minimum,
            bounciness       = 0f,
            bounceCombine    = PhysicsMaterialCombine.Minimum
        };
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        Debug.Log("[BackYardSetup] Created IceSurface PhysicsMaterial at Assets/Materials/IceSurface.physicsMaterial");
    }
}
