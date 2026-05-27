using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Diagnostic tool for checking gnome placement zone setup.
/// Identifies what's preventing gnomes from being placed in zones.
/// </summary>
public class BackYardGnomePlacementDiagnostics
{
    [MenuItem("Tools/BackYard Diagnose Gnome Placement")]
    public static void DiagnoseGnomePlacement()
    {
        Debug.Log("[GnomePlacementDiagnostics] Starting diagnosis...\n");

        var issues = 0;

        // Check 1: Gnome setup
        issues += CheckGnomeSetup();

        // Check 2: Zone setup
        issues += CheckZoneSetup();

        // Check 3: Collider and rigidbody setup
        issues += CheckPhysicsSetup();

        // Check 4: GnomeParadeTracker wiring
        issues += CheckTrackerWiring();

        if (issues == 0)
        {
            Debug.Log("[GnomePlacementDiagnostics] ✓ All checks passed! Gnome placement should work.");
        }
        else
        {
            Debug.LogWarning($"[GnomePlacementDiagnostics] ✗ Found {issues} issue(s). See above for details.");
        }

        Debug.Log("");
    }

    private static int CheckGnomeSetup()
    {
        Debug.Log("[Check 1] Gnome Setup");
        int issues = 0;

        for (int i = 1; i <= 3; i++)
        {
            var gnome = GameObject.Find($"Gnome_{i}");
            if (gnome == null)
            {
                Debug.LogError($"  ✗ Gnome_{i} not found in scene!");
                issues++;
                continue;
            }

            // Check tag
            if (gnome.tag != "Gnome")
            {
                Debug.LogError($"  ✗ Gnome_{i} has tag '{gnome.tag}' instead of 'Gnome'");
                issues++;
            }
            else
            {
                Debug.Log($"  ✓ Gnome_{i} has correct 'Gnome' tag");
            }

            // Check PickupObject
            if (gnome.GetComponent<PickupObject>() == null)
            {
                Debug.LogError($"  ✗ Gnome_{i} is missing PickupObject component");
                issues++;
            }
            else
            {
                Debug.Log($"  ✓ Gnome_{i} has PickupObject");
            }

            // Check Rigidbody
            var rb = gnome.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError($"  ✗ Gnome_{i} is missing Rigidbody");
                issues++;
            }
            else
            {
                if (rb.isKinematic)
                {
                    Debug.LogWarning($"  ⚠ Gnome_{i} rigidbody is kinematic (should be dynamic at start)");
                }
                else
                {
                    Debug.Log($"  ✓ Gnome_{i} has dynamic rigidbody");
                }
            }

            // Check collider
            var col = gnome.GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogError($"  ✗ Gnome_{i} is missing collider");
                issues++;
            }
            else
            {
                if (col.isTrigger)
                {
                    Debug.LogWarning($"  ⚠ Gnome_{i} collider is a trigger (should be solid for physics)");
                }
                else
                {
                    Debug.Log($"  ✓ Gnome_{i} has solid collider");
                }
            }
        }

        Debug.Log("");
        return issues;
    }

    private static int CheckZoneSetup()
    {
        Debug.Log("[Check 2] Zone Setup");
        int issues = 0;

        for (int i = 1; i <= 3; i++)
        {
            var zone = GameObject.Find($"GnomePlacementZone_{i}");
            if (zone == null)
            {
                Debug.LogError($"  ✗ GnomePlacementZone_{i} not found in scene!");
                issues++;
                continue;
            }

            // Check component
            var zoneComponent = zone.GetComponent<GnomePlacementZone>();
            if (zoneComponent == null)
            {
                Debug.LogError($"  ✗ GnomePlacementZone_{i} is missing GnomePlacementZone component");
                issues++;
            }
            else
            {
                Debug.Log($"  ✓ GnomePlacementZone_{i} has GnomePlacementZone component");
            }

            // Check collider
            var col = zone.GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogError($"  ✗ GnomePlacementZone_{i} is missing collider");
                issues++;
            }
            else if (!col.isTrigger)
            {
                Debug.LogError($"  ✗ GnomePlacementZone_{i} collider is NOT set as trigger!");
                issues++;
            }
            else
            {
                Debug.Log($"  ✓ GnomePlacementZone_{i} has trigger collider");
            }
        }

        Debug.Log("");
        return issues;
    }

    private static int CheckPhysicsSetup()
    {
        Debug.Log("[Check 3] Physics Setup (Collision Detection)");
        int issues = 0;

        // Check player physics layer
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log($"  ✓ Player has rigidbody with collision detection: {rb.collisionDetectionMode}");
            }
        }

        // Check gnome-zone collision detection
        for (int i = 1; i <= 3; i++)
        {
            var gnome = GameObject.Find($"Gnome_{i}");
            var zone = GameObject.Find($"GnomePlacementZone_{i}");

            if (gnome != null && zone != null)
            {
                var gnomeRb = gnome.GetComponent<Rigidbody>();
                var zoneCol = zone.GetComponent<Collider>();

                if (gnomeRb != null && zoneCol != null)
                {
                    Debug.Log($"  ✓ Gnome_{i} and Zone_{i} can collide");

                    // Check if gnome rigidbody could cause issues
                    if (gnomeRb.mass < 0.1f)
                    {
                        Debug.LogWarning($"  ⚠ Gnome_{i} has very low mass ({gnomeRb.mass}), might fall through colliders");
                    }
                }
            }
        }

        Debug.Log("");
        return issues;
    }

    private static int CheckTrackerWiring()
    {
        Debug.Log("[Check 4] GnomeParadeTracker Wiring");
        int issues = 0;

        var tracker = Object.FindAnyObjectByType<GnomeParadeTracker>();
        if (tracker == null)
        {
            Debug.LogWarning("  ⚠ GnomeParadeTracker not found (may not be needed if task not in scope)");
            return 0;
        }

        Debug.Log($"  ✓ GnomeParadeTracker found");

        var so = new SerializedObject(tracker);
        var zonesProp = so.FindProperty("_zones");

        if (zonesProp == null)
        {
            Debug.LogError("  ✗ GnomeParadeTracker has no _zones property");
            issues++;
        }
        else if (zonesProp.arraySize == 0)
        {
            Debug.LogError("  ✗ GnomeParadeTracker._zones is empty (zones not wired up)");
            issues++;
        }
        else
        {
            Debug.Log($"  ✓ GnomeParadeTracker has {zonesProp.arraySize} zones wired");

            for (int i = 0; i < zonesProp.arraySize; i++)
            {
                var zoneProp = zonesProp.GetArrayElementAtIndex(i);
                if (zoneProp.objectReferenceValue == null)
                {
                    Debug.LogError($"  ✗ Zone {i} in tracker is null");
                    issues++;
                }
                else
                {
                    Debug.Log($"  ✓ Zone {i}: {zoneProp.objectReferenceValue.name}");
                }
            }
        }

        Debug.Log("");
        return issues;
    }
}
