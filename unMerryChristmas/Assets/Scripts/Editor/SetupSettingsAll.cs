using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Menu: Tools > UnMerry > Setup Settings (Full — Active Scene)
///
/// One-click shortcut that runs all three settings setup steps in sequence:
///   1. Build Settings Prefab  (creates/overwrites Assets/Prefabs/UI/SettingsPageContent.prefab)
///   2. Place it into Page_Content_Settings in the active scene
///   3. Generate rebind rows inside Controls_Container
///
/// All asset paths are hardcoded — nothing to drag.
/// Run this whenever you rebuild the settings UI from scratch.
/// </summary>
public static class SetupSettingsAll
{
    private const string RowPrefabPath  = "Assets/Prefabs/UI/RebindActionRow.prefab";
    private const string ContainerName  = "Controls_Container";
    private const string ActionMapName  = "Player";

    [MenuItem("Tools/UnMerry/Setup Settings (Full — Active Scene)")]
    public static void RunAll()
    {
        // ── Step 0: (re)build the rebind row prefab so the rows match the latest
        //            structure/code (keyboard + gamepad key buttons). ───────────
        CreateRebindRowPrefab.Create();

        // ── Step 1: build prefab ──────────────────────────────────────────────
        BuildSettingsPrefab.CreatePrefab();

        // ── Step 2: place into active scene ───────────────────────────────────
        BuildSettingsPrefab.PlaceInActiveScene();

        // ── Step 3: generate rebind rows ──────────────────────────────────────
        GenerateRebindRows();

        Debug.Log("[SetupSettingsAll] Done — settings panel fully built and wired.");
    }

    // ── A single rebind row to emit: a display name + the keyboard / gamepad
    //    binding indices it targets (-1 when that slot is absent). ─────────────
    private readonly struct RowSpec
    {
        public readonly string Name;
        public readonly int    KbIndex;
        public readonly int    GpIndex;
        public RowSpec(string name, int kb, int gp) { Name = name; KbIndex = kb; GpIndex = gp; }
    }

    static void GenerateRebindRows()
    {
        // Find Controls_Container — must search inactive objects too because
        // Page_Content_Settings is inactive when the Settings tab is not open.
        GameObject containerGo = null;
        foreach (var t in Object.FindObjectsByType<Transform>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name == ContainerName)
            {
                containerGo = t.gameObject;
                break;
            }
        }
        if (containerGo == null)
        {
            Debug.LogError($"[SetupSettingsAll] '{ContainerName}' not found even among inactive objects. " +
                           "Make sure the prefab was placed correctly in step 2.");
            return;
        }

        // Load RebindActionRow prefab
        var rowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RowPrefabPath);
        if (rowPrefab == null)
        {
            Debug.LogError($"[SetupSettingsAll] RebindActionRow prefab not found at '{RowPrefabPath}'. " +
                           "Run 'Tools > UnMerry > Create RebindActionRow Prefab' first.");
            return;
        }

        // Find InputActionAsset — look for InputSystem_Actions specifically
        InputActionAsset inputAsset = null;
        foreach (var guid in AssetDatabase.FindAssets("t:InputActionAsset"))
        {
            var path  = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            if (asset != null && asset.name == "InputSystem_Actions")
            {
                inputAsset = asset;
                break;
            }
        }
        // Fallback: first InputActionAsset found
        if (inputAsset == null)
        {
            foreach (var guid in AssetDatabase.FindAssets("t:InputActionAsset"))
            {
                inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                                 AssetDatabase.GUIDToAssetPath(guid));
                if (inputAsset != null) break;
            }
        }
        if (inputAsset == null)
        {
            Debug.LogError("[SetupSettingsAll] No InputActionAsset found in the project.");
            return;
        }

        var playerMap = inputAsset.FindActionMap(ActionMapName);
        if (playerMap == null)
        {
            Debug.LogError($"[SetupSettingsAll] Action map '{ActionMapName}' not found in '{inputAsset.name}'.");
            return;
        }

        // Clear existing rows
        var container = containerGo.transform;
        for (int i = container.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(container.GetChild(i).gameObject);

        // Build + instantiate rows
        int count = 0;
        foreach (var action in playerMap.actions)
        {
            var actionRef = InputActionReference.Create(action);
            foreach (var spec in BuildRowSpecs(action))
            {
                var rowGo = (GameObject)PrefabUtility.InstantiatePrefab(rowPrefab, container);
                rowGo.name = $"Row_{spec.Name.Replace(' ', '_')}";
                var row = rowGo.GetComponent<RebindActionRow>();
                if (row == null) continue;
                row.SetupFromEditor(actionRef, spec.Name, spec.KbIndex, spec.GpIndex);
                PrefabUtility.RecordPrefabInstancePropertyModifications(row);
                EditorUtility.SetDirty(rowGo);
                count++;
            }
        }

        EditorUtility.SetDirty(containerGo);
        Debug.Log($"[SetupSettingsAll] Generated {count} rebind rows in '{containerGo.name}'.");
    }

    /// <summary>
    /// Turns one action into the rows it should display. Single-binding actions get
    /// one row pairing their keyboard + gamepad bindings. Composite actions (e.g.
    /// Move's WASD) expand into one row per keyboard direction part, plus a row for
    /// the single gamepad binding (the stick). Devices are detected by binding PATH,
    /// not by control-scheme group — some bindings (e.g. Pause) have no group.
    /// </summary>
    static IEnumerable<RowSpec> BuildRowSpecs(InputAction action)
    {
        int kbSingle = -1, gpSingle = -1;
        var kbParts  = new List<(int idx, string part)>();

        var bindings = action.bindings;
        for (int i = 0; i < bindings.Count; i++)
        {
            var b = bindings[i];
            if (b.isComposite) continue;               // composite header — not rebindable
            string path = b.effectivePath;

            if (b.isPartOfComposite)
            {
                if (IsKeyboard(path)) kbParts.Add((i, b.name));
                // (No gamepad composite parts exist in the Player map.)
            }
            else if (IsKeyboard(path)) { if (kbSingle < 0) kbSingle = i; }
            else if (IsGamepad(path))  { if (gpSingle < 0) gpSingle = i; }
        }

        if (kbParts.Count > 0)
        {
            // Composite action (Move): the gamepad stick first, then each key direction.
            if (gpSingle >= 0)
                yield return new RowSpec(action.name, -1, gpSingle);
            foreach (var (idx, part) in kbParts)
                yield return new RowSpec($"{action.name} {Capitalize(part)}", idx, -1);
        }
        else
        {
            yield return new RowSpec(action.name, kbSingle, gpSingle);
        }
    }

    static bool IsKeyboard(string p) =>
        !string.IsNullOrEmpty(p) && (p.StartsWith("<Keyboard>") || p.StartsWith("<Mouse>"));

    static bool IsGamepad(string p) =>
        !string.IsNullOrEmpty(p) && (p.StartsWith("<Gamepad>") || p.StartsWith("<Joystick>"));

    static string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
}
