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
        // ── Step 1: build prefab ──────────────────────────────────────────────
        BuildSettingsPrefab.CreatePrefab();

        // ── Step 2: place into active scene ───────────────────────────────────
        BuildSettingsPrefab.PlaceInActiveScene();

        // ── Step 3: generate rebind rows ──────────────────────────────────────
        GenerateRebindRows();

        Debug.Log("[SetupSettingsAll] Done — settings panel fully built and wired.");
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

        // Instantiate one row per action
        int count = 0;
        foreach (var action in playerMap.actions)
        {
            var rowGo = (GameObject)PrefabUtility.InstantiatePrefab(rowPrefab, container);
            rowGo.name = $"Row_{action.name}";
            var row = rowGo.GetComponent<RebindActionRow>();
            if (row == null) continue;
            row.SetupFromEditor(InputActionReference.Create(action), action.name);
            EditorUtility.SetDirty(rowGo);
            count++;
        }

        EditorUtility.SetDirty(containerGo);
        Debug.Log($"[SetupSettingsAll] Generated {count} rebind rows in '{containerGo.name}'.");
    }
}
