using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class FixGnomePivotsAndPlacement
{
    [MenuItem("Tools/Unpack Gnome Prefabs")]
    public static void UnpackGnomes()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        var scene = EditorSceneManager.GetActiveScene();
        var rootObjects = scene.GetRootGameObjects();

        foreach (var root in rootObjects)
        {
            UnpackGnomesInHierarchy(root);
        }

        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static void UnpackGnomesInHierarchy(GameObject obj)
    {
        if (obj.CompareTag("Gnome"))
        {
            UnpackGnome(obj);
        }

        foreach (Transform child in obj.transform)
        {
            UnpackGnomesInHierarchy(child.gameObject);
        }
    }

    private static void UnpackGnome(GameObject gnome)
    {
        string gnomeName = gnome.name;

        // Find the root prefab instance
        var root = gnome;
        while (root.transform.parent != null && PrefabUtility.IsPartOfAnyPrefab(root.transform.parent.gameObject))
        {
            root = root.transform.parent.gameObject;
        }

        // Only unpack if this is a prefab instance root
        if (PrefabUtility.IsAnyPrefabInstanceRoot(root))
        {
            try
            {
                PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[FixGnomePivots] Failed to unpack {gnomeName}: {ex.Message}");
            }
        }
    }
}
