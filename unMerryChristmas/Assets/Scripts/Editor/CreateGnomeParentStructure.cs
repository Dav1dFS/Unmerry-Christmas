using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateGnomeParentStructure
{
    [MenuItem("Tools/Create Gnome Parent Structure")]
    public static void CreateParents()
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
            ProcessGnomesInHierarchy(root);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[CreateGnomeParent] Completed gnome parent structure creation.");
    }

    private static void ProcessGnomesInHierarchy(GameObject obj)
    {
        if (obj.CompareTag("Gnome"))
        {
            CreateParentForGnome(obj);
        }

        foreach (Transform child in obj.transform)
        {
            ProcessGnomesInHierarchy(child.gameObject);
        }
    }

    private static void CreateParentForGnome(GameObject gnome)
    {
        string gnomeName = gnome.name;
        Debug.Log($"[CreateGnomeParent] Processing {gnomeName}");

        // Check if already has parent structure
        if (gnome.transform.parent != null && gnome.transform.parent.name == $"{gnomeName}_Origin")
        {
            Debug.Log($"[CreateGnomeParent] {gnomeName} already has parent structure");
            return;
        }

        // Create origin parent at gnome's current position
        var originName = $"{gnomeName}_Origin";
        var originObj = new GameObject(originName);
        originObj.transform.position = gnome.transform.position;
        originObj.transform.rotation = Quaternion.identity;
        originObj.tag = "Gnome";

        // Move visual mesh under origin
        gnome.transform.SetParent(originObj.transform, worldPositionStays: true);
        gnome.transform.localPosition = Vector3.zero;
        gnome.transform.localRotation = Quaternion.identity;

        // Move physics components to origin
        var rb = gnome.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.transform.SetParent(originObj.transform, worldPositionStays: false);
            rb.transform.localPosition = Vector3.zero;
        }

        var col = gnome.GetComponent<Collider>();
        if (col != null)
        {
            col.transform.SetParent(originObj.transform, worldPositionStays: false);
            col.transform.localPosition = Vector3.zero;
        }

        var pickup = gnome.GetComponent<PickupObject>();
        if (pickup != null)
        {
            pickup.transform.SetParent(originObj.transform, worldPositionStays: false);
            pickup.transform.localPosition = Vector3.zero;
        }

        Debug.Log($"[CreateGnomeParent] Created parent structure for {gnomeName}");
    }
}
