using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Menu: Tools > UnMerry > Setup Accessibility Manager
/// Creates the [Accessibility] root GameObject with AccessibilityManager configured,
/// and auto-wires the InputActionAsset. Safe to run multiple times — warns if one exists.
/// </summary>
public static class AccessibilitySetupTool
{
    [MenuItem("Tools/UnMerry/Setup Accessibility Manager")]
    public static void SetupAccessibilityManager()
    {
        // Guard: don't create a duplicate
        var existing = Object.FindFirstObjectByType<AccessibilityManager>();
        if (existing != null)
        {
            Debug.LogWarning("[AccessibilitySetupTool] AccessibilityManager already exists. " +
                             "Selecting it instead.");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Create root GameObject
        var go      = new GameObject("[Accessibility]");
        var manager = go.AddComponent<AccessibilityManager>();

        // Auto-wire InputActionAsset
        string[] guids = AssetDatabase.FindAssets("InputSystem_Actions t:InputActionAsset");
        if (guids.Length > 0)
        {
            string path  = AssetDatabase.GUIDToAssetPath(guids[0]);
            var    asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            if (asset != null)
            {
                var so = new SerializedObject(manager);
                so.FindProperty("_inputActions").objectReferenceValue = asset;
                so.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"[AccessibilitySetupTool] Wired InputActionAsset from '{path}'.");
            }
        }
        else
        {
            Debug.LogWarning("[AccessibilitySetupTool] InputSystem_Actions asset not found. " +
                             "Wire _inputActions manually in the Inspector.");
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = go;
        Undo.RegisterCreatedObjectUndo(go, "Setup Accessibility Manager");

        Debug.Log("[AccessibilitySetupTool] Created [Accessibility] GameObject.");
    }
}
