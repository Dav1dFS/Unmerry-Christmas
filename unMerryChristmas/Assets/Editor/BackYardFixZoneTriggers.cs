using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Fixes gnome placement zone triggers.
/// Zones must have colliders set as triggers for OnTriggerEnter/OnTriggerStay to fire.
/// </summary>
public class BackYardFixZoneTriggers
{
    [MenuItem("Tools/BackYard Fix Zone Triggers")]
    public static void FixZoneTriggers()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[BackYardFixZoneTriggers] Cannot run during play mode. Stopping play...");
            EditorApplication.isPlaying = false;
            return;
        }

        Debug.Log("[BackYardFixZoneTriggers] Fixing gnome placement zone triggers...\n");

        int fixedCount = 0;

        for (int i = 1; i <= 3; i++)
        {
            var zone = GameObject.Find($"GnomePlacementZone_{i}");
            if (zone == null)
            {
                Debug.LogWarning($"  ✗ GnomePlacementZone_{i} not found");
                continue;
            }

            var collider = zone.GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogWarning($"  ✗ GnomePlacementZone_{i} has no collider!");
                continue;
            }

            if (!collider.isTrigger)
            {
                collider.isTrigger = true;
                fixedCount++;
                Debug.Log($"  ✓ Set GnomePlacementZone_{i} collider to trigger");
            }
            else
            {
                Debug.Log($"  ✓ GnomePlacementZone_{i} already has trigger collider");
            }
        }

        Debug.Log($"\n[BackYardFixZoneTriggers] ✓ Fixed {fixedCount} zones!");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
