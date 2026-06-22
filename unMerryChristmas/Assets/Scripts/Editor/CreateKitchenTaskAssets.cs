using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates (or updates) the Kitchen's task assets: four <see cref="TaskDefinition"/>s
/// and the <see cref="TaskSet"/> that orders them for the drawing-book Task List.
///
/// Idempotent — re-running keeps existing assets and just refreshes their fields, so
/// it's safe to run after editing. <see cref="SetupKitchenTasks"/> calls
/// <see cref="EnsureAssets"/> automatically, so you normally only run this directly if
/// you want the assets without wiring the scene.
///
///   Tools ▸ UnMerry ▸ Kitchen ▸ 1. Create Task Assets
/// </summary>
public static class CreateKitchenTaskAssets
{
    public const string Folder = "Assets/Scenes/Tasks/Kitchen";

    public const string TrashId = "Task_TrashNutcrackers";
    public const string DrinkId = "Task_DrinkHotChocolate";
    public const string LightId = "Task_LightDecorations";
    public const string OvenId  = "Task_CookOvenPot";
    public const string SetId   = "TaskSet_Kitchen";

    [MenuItem("Tools/UnMerry/Kitchen/1. Create Task Assets")]
    public static void Run()
    {
        var set = EnsureAssets();
        Selection.activeObject = set;
        EditorGUIUtility.PingObject(set);
        Debug.Log($"[CreateKitchenTaskAssets] Kitchen task assets ready in '{Folder}'.");
    }

    /// <summary>Ensure all Kitchen task assets exist and are configured; returns the TaskSet.</summary>
    public static TaskSet EnsureAssets()
    {
        EnsureFolder(Folder);

        var trash = EnsureTask(TrashId, "Trash the Nutcrackers",     false, PlayerAbility.Moving);
        var drink = EnsureTask(DrinkId, "Drink the Hot Chocolates",  false, PlayerAbility.Moving);
        var light = EnsureTask(LightId, "Light the Wreath & Candle", true,  PlayerAbility.ExplosingPresents);
        var oven  = EnsureTask(OvenId,  "Cook in the Oven Pot",      false, PlayerAbility.Moving);

        var set = LoadOrCreate<TaskSet>(SetId);
        var so = new SerializedObject(set);
        var list = so.FindProperty("_tasks");
        list.ClearArray();
        AppendRef(list, 0, trash);
        AppendRef(list, 1, drink);
        AppendRef(list, 2, light);
        AppendRef(list, 3, oven);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(set);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return set;
    }

    public static TaskDefinition LoadTask(string assetId)
        => AssetDatabase.LoadAssetAtPath<TaskDefinition>($"{Folder}/{assetId}.asset");

    private static TaskDefinition EnsureTask(
        string assetId, string displayName, bool locked, PlayerAbility unlock)
    {
        var def = LoadOrCreate<TaskDefinition>(assetId);
        var so = new SerializedObject(def);
        so.FindProperty("_displayName").stringValue = displayName;
        so.FindProperty("_lockedByDefault").boolValue = locked;
        so.FindProperty("_unlockAbility").enumValueIndex = (int)unlock;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(def);
        return def;
    }

    private static T LoadOrCreate<T>(string assetId) where T : ScriptableObject
    {
        string path = $"{Folder}/{assetId}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var inst = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(inst, path);
        return inst;
    }

    private static void AppendRef(SerializedProperty list, int index, Object value)
    {
        list.InsertArrayElementAtIndex(index);
        list.GetArrayElementAtIndex(index).objectReferenceValue = value;
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;

        var parts = folder.Split('/');
        string cur = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{cur}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }
}
