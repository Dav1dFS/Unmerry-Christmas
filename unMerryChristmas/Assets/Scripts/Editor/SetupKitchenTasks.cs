using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// One-click wiring for the four Kitchen tasks. Run with the Kitchen scene open:
///
///   Tools ▸ UnMerry ▸ Kitchen ▸ 2. Setup Kitchen Tasks
///
/// It ensures the task assets exist (via <see cref="CreateKitchenTaskAssets"/>) and then
/// finds the props you've placed by name and wires every component / collider / layer /
/// ScriptableObject reference:
///
///   • SceneTaskRegistrar           — registers the Kitchen TaskSet
///   • Nutcrackers (×2)             — Nutcracker marker + PickupObject + Rigidbody + collider, Pickable layer
///   • Trash can                    — standalone trigger volume + TrashReceptacle (task 1)
///   • Hot chocolates (×N)          — DrinkableHotChocolate (+ Mug prefab), Pickable layer; HotChocolateTask (task 2)
///   • Wreath + Candle              — LightableEmissive + blast trigger collider; LightingTask (task 3)
///   • Oven pot                     — standalone trigger volume + OvenPotTask (task 4)
///   • DrawingPage_BY4              — swaps the mis-pointed TaskReward for TaskRewardReveal → task 1
///   • ExplosivePresent(s)AT        — Pickable layer + TaskRewardReveal → task 2
///
/// Idempotent: re-running reuses what it already created. It logs a summary and warns
/// about anything it couldn't find so you can place/rename it and run again.
/// </summary>
public static class SetupKitchenTasks
{
    private const string MenuPath = "Tools/UnMerry/Kitchen/2. Setup Kitchen Tasks";
    private const string MugPrefabPath = "Assets/Art/Poly Christmas/Prefabs/Christmas/Mug_Christmas.prefab";
    private const string PickableLayerName = "Pickable";

    [MenuItem(MenuPath)]
    public static void Run()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.isLoaded)
        {
            Debug.LogWarning("[SetupKitchenTasks] No scene loaded. Open the Kitchen scene first.");
            return;
        }

        var set   = CreateKitchenTaskAssets.EnsureAssets();
        var trash = CreateKitchenTaskAssets.LoadTask(CreateKitchenTaskAssets.TrashId);
        var drink = CreateKitchenTaskAssets.LoadTask(CreateKitchenTaskAssets.DrinkId);
        var light = CreateKitchenTaskAssets.LoadTask(CreateKitchenTaskAssets.LightId);
        var oven  = CreateKitchenTaskAssets.LoadTask(CreateKitchenTaskAssets.OvenId);

        int pickableLayer = LayerMask.NameToLayer(PickableLayerName);
        if (pickableLayer < 0)
            Debug.LogWarning($"[SetupKitchenTasks] Layer '{PickableLayerName}' not found — " +
                             "interactables/pickups may not be detected by the player.");

        var log = new List<string>();

        // ── Registrar ─────────────────────────────────────────────────────────
        var system = GetOrCreateRoot("KitchenTaskSystem", scene);
        var registrar = GetOrAdd<SceneTaskRegistrar>(system);
        SetRef(registrar, "_taskSet", set);

        // ── Task 1: Nutcrackers → trash ─────────────────────────────────────────
        var nutcrackers = FindRoots(scene, "Nutcracker");
        foreach (var nut in nutcrackers) SetupNutcracker(nut, pickableLayer);
        log.Add($"Nutcrackers wired: {nutcrackers.Count}");

        var trashCan = FindFirst(scene, "garbage", "Garbage", "Trash", "TrashCan", "Bin");
        if (trashCan != null && TryGetWorldBounds(trashCan, out var trashBounds))
        {
            var trigger = GetOrCreateRoot("TrashTrigger", scene);
            trigger.transform.position = trashBounds.center;
            var box = GetOrAdd<BoxCollider>(trigger);
            box.isTrigger = true;
            box.center = Vector3.zero;
            box.size = trashBounds.size;
            var receptacle = GetOrAdd<TrashReceptacle>(trigger);
            SetRef(receptacle, "_task", trash);
            log.Add($"Trash trigger placed on '{trashCan.name}'");
        }
        else log.Add("⚠ Trash can not found (looked for 'garbage'/'Trash'). Create the trigger manually.");

        // ── Task 2: Hot chocolates → drink → token ──────────────────────────────
        var mug = AssetDatabase.LoadAssetAtPath<GameObject>(MugPrefabPath);
        if (mug == null) log.Add($"⚠ Mug prefab not found at {MugPrefabPath}");

        var hotChocs = FindRoots(scene, "Hot_Chocolate");
        var drinks = new List<Object>();
        foreach (var hc in hotChocs)
        {
            var d = SetupHotChocolate(hc, mug, pickableLayer);
            drinks.Add(d);
        }
        var hotTask = GetOrAdd<HotChocolateTask>(system);
        SetRef(hotTask, "_task", drink);
        SetList(hotTask, "_drinks", drinks);
        log.Add($"Hot chocolates wired: {hotChocs.Count}");

        // ── Task 3: Light the wreath + candle ───────────────────────────────────
        var lightables = new List<Object>();
        foreach (var wreath in FindRoots(scene, "Wreath_Advent_B"))
            lightables.Add(SetupLightable(wreath));
        foreach (var candle in FindRoots(scene, "Candle_Decorated_Lit_Christmas"))
            lightables.Add(SetupLightable(candle));

        var lightTask = GetOrAdd<LightingTask>(system);
        SetRef(lightTask, "_task", light);
        SetList(lightTask, "_lightables", lightables);
        log.Add($"Lightables wired: {lightables.Count}");

        // ── Task 4: Oven pot ────────────────────────────────────────────────────
        var ovenPot = FindFirst(scene, "Oven_Pot", "Oven Pot", "Oven");
        if (ovenPot != null && TryGetWorldBounds(ovenPot, out var ovenBounds))
        {
            var trigger = GetOrCreateRoot("OvenPotTrigger", scene);
            // Sit at the rim so smoke rises from the top; box extends down into the pot.
            trigger.transform.position = new Vector3(
                ovenBounds.center.x, ovenBounds.max.y, ovenBounds.center.z);
            var box = GetOrAdd<BoxCollider>(trigger);
            box.isTrigger = true;
            box.center = new Vector3(0f, -ovenBounds.size.y * 0.4f, 0f);
            box.size = new Vector3(ovenBounds.size.x * 0.8f, ovenBounds.size.y * 0.9f, ovenBounds.size.z * 0.8f);
            var pot = GetOrAdd<OvenPotTask>(trigger);
            SetRef(pot, "_task", oven);
            log.Add($"Oven pot trigger placed on '{ovenPot.name}'");
        }
        else log.Add("⚠ Oven pot not found (looked for 'Oven_Pot'/'Oven'). Create the trigger manually.");

        // ── Rewards: drawing page + ability token ───────────────────────────────
        var page = FindFirst(scene, "DrawingPage_BY4", "DrawingPage");
        if (page != null)
        {
            var old = page.GetComponent<TaskReward>();
            if (old != null) // was pointing at a Backyard task
            {
                try { Undo.DestroyObjectImmediate(old); }
                catch { old.enabled = false; EditorUtility.SetDirty(old); } // prefab-locked: just neutralise it
            }
            var reveal = GetOrAdd<TaskRewardReveal>(page);
            SetRef(reveal, "_requiredTask", trash);
            log.Add($"Drawing page reward fixed on '{page.name}' → Trash Nutcrackers");
        }
        else log.Add("⚠ DrawingPage not found.");

        var token = FindFirst(scene, "ExplosivePresentsAT", "ExplosivePresentAT", "ExplosivePresent_AT");
        if (token != null)
        {
            if (pickableLayer >= 0) SetLayerRecursive(token, pickableLayer);

            // The collect path does GetComponent<AbilityToken>() on the hit collider's
            // GameObject. This prefab keeps AbilityToken on a child, so a root-collider
            // hit would miss it — ensure the root carries one too (as BackYardInitializer
            // does for its tokens).
            var at = GetOrAdd<AbilityToken>(token);
            SetEnum(at, "_ability", (int)PlayerAbility.ExplosingPresents);

            var reveal = GetOrAdd<TaskRewardReveal>(token);
            SetRef(reveal, "_requiredTask", drink);
            log.Add($"Ability token reward wired on '{token.name}' → Drink Hot Chocolates");
        }
        else log.Add("⚠ ExplosivePresents ability token not found.");

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = system;

        Debug.Log("[SetupKitchenTasks] Done.\n  • " + string.Join("\n  • ", log) +
                  "\n\nReview/resize the TrashTrigger and OvenPotTrigger boxes to match your geometry, then save the scene.");
    }

    [MenuItem(MenuPath, validate = true)]
    private static bool Validate() => SceneManager.GetActiveScene().isLoaded;

    // ── Per-object setup ────────────────────────────────────────────────────────

    private static void SetupNutcracker(GameObject go, int pickableLayer)
    {
        GetOrAdd<Nutcracker>(go);

        var rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = Undo.AddComponent<Rigidbody>(go);
            rb.mass = 1f;
            rb.linearDamping = 0.4f;
            rb.angularDamping = 0.5f;
        }

        // PickupObject reads its Collider from the SAME GameObject, so the root must
        // have one (not just a child mesh) or pickup throws a NullReference.
        if (go.GetComponent<Collider>() == null)
        {
            var box = Undo.AddComponent<BoxCollider>(go);
            FitBox(box);
        }
        foreach (var c in go.GetComponentsInChildren<Collider>()) c.isTrigger = false;

        GetOrAdd<PickupObject>(go);

        go.tag = "Untagged"; // not a Collectable/Token — it's a thrown pickup
        if (pickableLayer >= 0) SetLayerRecursive(go, pickableLayer);
    }

    private static DrinkableHotChocolate SetupHotChocolate(GameObject go, GameObject mug, int pickableLayer)
    {
        if (go.GetComponentInChildren<Collider>() == null)
        {
            var box = Undo.AddComponent<BoxCollider>(go);
            FitBox(box);
        }
        foreach (var c in go.GetComponentsInChildren<Collider>()) c.isTrigger = false;

        var drink = GetOrAdd<DrinkableHotChocolate>(go);
        if (mug != null) SetRef(drink, "_emptyMugPrefab", mug);

        go.tag = "Untagged"; // interactable, not a collectable
        if (pickableLayer >= 0) SetLayerRecursive(go, pickableLayer);
        return drink;
    }

    private static LightableEmissive SetupLightable(GameObject go)
    {
        // The explosion uses an unfiltered OverlapSphere; give the prop a trigger
        // collider (if it has none) so the blast can register a hit without changing
        // how it collides physically.
        if (go.GetComponentInChildren<Collider>() == null)
        {
            var box = Undo.AddComponent<BoxCollider>(go);
            box.isTrigger = true;
            FitBox(box);
        }
        return GetOrAdd<LightableEmissive>(go); // _targetRenderers auto-fills from children
    }

    // ── Scene helpers ─────────────────────────────────────────────────────────────

    private static List<GameObject> FindRoots(Scene scene, string key)
    {
        var result = new List<GameObject>();
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.gameObject.scene != scene) continue;
            if (!NameStarts(t.name, key)) continue;

            // Outermost match only, so we don't also grab child meshes of the same name.
            bool ancestorMatches = false;
            for (var p = t.parent; p != null; p = p.parent)
                if (NameStarts(p.name, key)) { ancestorMatches = true; break; }
            if (!ancestorMatches) result.Add(t.gameObject);
        }
        return result;
    }

    private static GameObject FindFirst(Scene scene, params string[] keys)
    {
        foreach (var key in keys)
        {
            var roots = FindRoots(scene, key);
            if (roots.Count > 0) return roots[0];
        }
        return null;
    }

    private static GameObject GetOrCreateRoot(string name, Scene scene)
    {
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.gameObject.scene == scene && t.parent == null && t.name == name)
                return t.gameObject;

        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : Undo.AddComponent<T>(go);
    }

    private static bool NameStarts(string name, string key)
        => name.StartsWith(key, StringComparison.OrdinalIgnoreCase);

    private static bool TryGetWorldBounds(GameObject go, out Bounds bounds)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) { bounds = default; return false; }
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return true;
    }

    // Fit a BoxCollider to its object's combined renderer bounds (axis-aligned).
    private static void FitBox(BoxCollider box)
    {
        if (!TryGetWorldBounds(box.gameObject, out var b)) return;
        box.center = box.transform.InverseTransformPoint(b.center);
        Vector3 ls = box.transform.lossyScale;
        box.size = new Vector3(
            ls.x != 0 ? b.size.x / Mathf.Abs(ls.x) : b.size.x,
            ls.y != 0 ? b.size.y / Mathf.Abs(ls.y) : b.size.y,
            ls.z != 0 ? b.size.z / Mathf.Abs(ls.z) : b.size.z);
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    private static void SetRef(Component c, string field, Object value)
    {
        var so = new SerializedObject(c);
        var prop = so.FindProperty(field);
        if (prop == null) { Debug.LogWarning($"[SetupKitchenTasks] '{field}' not found on {c.GetType().Name}.", c); return; }
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(c);
    }

    private static void SetEnum(Component c, string field, int enumIndex)
    {
        var so = new SerializedObject(c);
        var prop = so.FindProperty(field);
        if (prop == null) { Debug.LogWarning($"[SetupKitchenTasks] '{field}' not found on {c.GetType().Name}.", c); return; }
        prop.enumValueIndex = enumIndex;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(c);
    }

    private static void SetList(Component c, string field, IReadOnlyList<Object> values)
    {
        var so = new SerializedObject(c);
        var list = so.FindProperty(field);
        if (list == null) { Debug.LogWarning($"[SetupKitchenTasks] '{field}' not found on {c.GetType().Name}.", c); return; }
        list.ClearArray();
        for (int i = 0; i < values.Count; i++)
        {
            list.InsertArrayElementAtIndex(i);
            list.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(c);
    }
}
