using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BackYardAutoWirer
{
    [MenuItem("Tools/BackYard Auto-Wire Scene")]
    public static void AutoWireScene()
    {
        Debug.Log("[BackYardAutoWirer] Starting automatic scene wiring...");

        // Task 2: Garden Chaos
        WireGardenChaos();

        // Task 3: Gnome Parade
        WireGnomeParade();

        // Task 4: Moving Boxes
        WireBoxStack();

        // Task 8: Birdbath Demolition
        WireBirdbathDemolition();

        // Task 9: Shed Heist
        WireShedHeist();

        // Drawing Page Collectables (including BY2 with task reward)
        WireDrawingPages();

        // Validate everything is wired
        ValidateWiring();

        Debug.Log("[BackYardAutoWirer] ✓ Scene wiring complete!");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void ValidateWiring()
    {
        Debug.Log("[Validation] Checking scene wiring...");

        // Check GardenChaosTracker has TippablePatioTable
        var patioTable = GameObject.Find("PatioTable");
        if (patioTable != null)
        {
            if (patioTable.GetComponent<TippablePatioTable>() == null)
                Debug.LogWarning("[Validation] PatioTable missing TippablePatioTable component");
            else
                Debug.Log("[Validation] ✓ PatioTable properly configured");
        }

        // Check GnomeParadeTracker has zones
        var gnomeTracker = Object.FindAnyObjectByType<GnomeParadeTracker>();
        if (gnomeTracker != null)
        {
            var so = new SerializedObject(gnomeTracker);
            var zones = so.FindProperty("_zones");
            if (zones == null || zones.arraySize == 0)
                Debug.LogWarning("[Validation] GnomeParadeTracker has no zones wired");
            else
                Debug.Log("[Validation] ✓ GnomeParadeTracker properly configured");
        }

        // Check BoxStackTracker has boxes
        var boxTracker = Object.FindAnyObjectByType<BoxStackTracker>();
        if (boxTracker != null)
        {
            var so = new SerializedObject(boxTracker);
            var boxes = so.FindProperty("_boxes");
            if (boxes == null || boxes.arraySize == 0)
                Debug.LogWarning("[Validation] BoxStackTracker has no boxes wired");
            else
                Debug.Log("[Validation] ✓ BoxStackTracker properly configured");
        }

        // Check DrawingPage_BY2 has TaskReward for GnomeParade
        var by2 = GameObject.Find("DrawingPage_BY2");
        if (by2 != null)
        {
            var reward = by2.GetComponent<TaskReward>();
            if (reward == null)
                Debug.LogWarning("[Validation] DrawingPage_BY2 missing TaskReward component");
            else
                Debug.Log("[Validation] ✓ DrawingPage_BY2 has TaskReward for GnomeParade");
        }

        Debug.Log("[Validation] ✓ Validation complete");
    }

    private static void WireGardenChaos()
    {
        Debug.Log("[GardenChaos] Wiring...");
        var tracker = Object.FindAnyObjectByType<GardenChaosTracker>();
        if (tracker == null)
        {
            Debug.LogWarning("[GardenChaos] Tracker not found!");
            return;
        }

        var patioTable = GameObject.Find("PatioTable");
        if (patioTable == null)
        {
            Debug.LogWarning("[GardenChaos] PatioTable not found!");
            return;
        }

        // Ensure TippablePatioTable component exists
        var tippable = patioTable.GetComponent<TippablePatioTable>();
        if (tippable == null)
        {
            tippable = patioTable.AddComponent<TippablePatioTable>();
            Debug.Log("[GardenChaos] Added TippablePatioTable component to PatioTable");
        }

        // Wire tracker reference to the TippablePatioTable component
        var so = new SerializedObject(tippable);
        so.FindProperty("_gardenChaosTracker").objectReferenceValue = tracker;
        so.ApplyModifiedProperties();

        Debug.Log("[GardenChaos] ✓ Wired PatioTable");
    }

    private static void WireGnomeParade()
    {
        Debug.Log("[GnomeParade] Wiring...");
        var tracker = Object.FindAnyObjectByType<GnomeParadeTracker>();
        if (tracker == null)
        {
            Debug.LogWarning("[GnomeParade] Tracker not found!");
            return;
        }

        // Wire zones
        var zones = new GnomePlacementZone[3];
        for (int i = 1; i <= 3; i++)
        {
            var zoneGO = GameObject.Find($"GnomePlacementZone_{i}");
            if (zoneGO == null)
            {
                Debug.LogWarning($"[GnomeParade] GnomePlacementZone_{i} GameObject not found!");
                continue;
            }

            // Add component if missing
            var zone = zoneGO.GetComponent<GnomePlacementZone>();
            if (zone == null)
            {
                zone = zoneGO.AddComponent<GnomePlacementZone>();
                Debug.Log($"[GnomeParade] Added GnomePlacementZone component to GnomePlacementZone_{i}");
            }
            zones[i - 1] = zone;
        }

        var so = new SerializedObject(tracker);
        var zonesArray = so.FindProperty("_zones");
        zonesArray.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            if (zones[i] != null)
                zonesArray.GetArrayElementAtIndex(i).objectReferenceValue = zones[i];
        }
        so.ApplyModifiedProperties();

        Debug.Log("[GnomeParade] ✓ Wired 3 placement zones");
    }

    private static void WireBoxStack()
    {
        Debug.Log("[BoxStack] Wiring...");
        var tracker = Object.FindAnyObjectByType<BoxStackTracker>();
        if (tracker == null)
        {
            Debug.LogWarning("[BoxStack] Tracker not found!");
            return;
        }

        var boxes = new MovingBox[3];
        for (int i = 1; i <= 3; i++)
        {
            var boxGO = GameObject.Find($"MovingBox_{i}");
            if (boxGO == null)
            {
                Debug.LogWarning($"[BoxStack] MovingBox_{i} GameObject not found!");
                continue;
            }

            // Add component if missing
            var box = boxGO.GetComponent<MovingBox>();
            if (box == null)
            {
                box = boxGO.AddComponent<MovingBox>();
                Debug.Log($"[BoxStack] Added MovingBox component to MovingBox_{i}");
            }
            boxes[i - 1] = box;
        }

        var so = new SerializedObject(tracker);
        var boxesArray = so.FindProperty("_boxes");
        boxesArray.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            if (boxes[i] != null)
                boxesArray.GetArrayElementAtIndex(i).objectReferenceValue = boxes[i];
        }
        so.ApplyModifiedProperties();

        Debug.Log("[BoxStack] ✓ Wired 3 moving boxes");
    }

    private static void WireBirdbathDemolition()
    {
        Debug.Log("[Birdbath] Wiring...");
        var birdbath = GameObject.Find("Birdbath");
        if (birdbath == null)
        {
            Debug.LogWarning("[Birdbath] Birdbath not found!");
            return;
        }

        // Ensure BreakableBirdbath component exists
        if (birdbath.GetComponent<BreakableBirdbath>() == null)
        {
            birdbath.AddComponent<BreakableBirdbath>();
            Debug.Log("[Birdbath] Added BreakableBirdbath component");
        }

        Debug.Log("[Birdbath] ✓ Birdbath configured");
    }

    private static void WireShedHeist()
    {
        Debug.Log("[ShedHeist] Wiring...");
        var tracker = Object.FindAnyObjectByType<ShedHeistTracker>();
        if (tracker == null)
        {
            Debug.LogWarning("[ShedHeist] Tracker not found!");
            return;
        }

        var door = GameObject.Find("ShedDoor");
        var padlock = GameObject.Find("Padlock");

        if (door == null)
        {
            Debug.LogWarning("[ShedHeist] ShedDoor not found!");
            return;
        }
        if (padlock == null)
        {
            Debug.LogWarning("[ShedHeist] Padlock not found!");
            return;
        }

        // Ensure components exist
        var doorComponent = door.GetComponent<ShedDoor>();
        if (doorComponent == null)
        {
            doorComponent = door.AddComponent<ShedDoor>();
            Debug.Log("[ShedHeist] Added ShedDoor component");
        }

        var padlockComponent = padlock.GetComponent<Padlock>();
        if (padlockComponent == null)
        {
            padlockComponent = padlock.AddComponent<Padlock>();
            Debug.Log("[ShedHeist] Added Padlock component");
        }

        // Wire padlock to door
        var padlockSO = new SerializedObject(padlockComponent);
        padlockSO.FindProperty("_shedDoor").objectReferenceValue = doorComponent;
        padlockSO.ApplyModifiedProperties();

        // Try to auto-wire door pivot (look for child with "Pivot" in name, or first child)
        var doorSO = new SerializedObject(doorComponent);
        var doorPivotProp = doorSO.FindProperty("_doorPivot");
        if (doorPivotProp.objectReferenceValue == null)
        {
            Transform pivotChild = null;
            // First look for a child named "Pivot" or similar
            for (int i = 0; i < door.transform.childCount; i++)
            {
                var child = door.transform.GetChild(i);
                if (child.name.Contains("Pivot") || child.name.Contains("pivot"))
                {
                    pivotChild = child;
                    break;
                }
            }
            // If not found, use first child
            if (pivotChild == null && door.transform.childCount > 0)
            {
                pivotChild = door.transform.GetChild(0);
                Debug.Log("[ShedHeist] Using first child as door pivot");
            }

            if (pivotChild != null)
            {
                doorPivotProp.objectReferenceValue = pivotChild;
                doorSO.ApplyModifiedProperties();
                Debug.Log("[ShedHeist] Auto-wired door pivot");
            }
            else
            {
                Debug.LogWarning("[ShedHeist] Could not find door pivot child. Please wire manually.");
            }
        }

        Debug.Log("[ShedHeist] ✓ Padlock and Door configured");
    }

    private static void WireDrawingPages()
    {
        Debug.Log("[DrawingPages] Wiring...");

        var pageNames = new[] { "DrawingPage_BY1", "DrawingPage_BY2", "DrawingPage_BY3" };
        int wiredCount = 0;

        foreach (var pageName in pageNames)
        {
            var page = GameObject.Find(pageName);
            if (page == null)
            {
                Debug.LogWarning($"[DrawingPages] {pageName} not found!");
                continue;
            }

            // Ensure DrawingPageCollectable component exists
            if (page.GetComponent<DrawingPageCollectable>() == null)
            {
                page.AddComponent<DrawingPageCollectable>();
                Debug.Log($"[DrawingPages] Added DrawingPageCollectable to {pageName}");
            }

            // Ensure it has a trigger collider
            var collider = page.GetComponent<Collider>();
            if (collider == null || !collider.isTrigger)
            {
                if (collider != null)
                    Object.DestroyImmediate(collider);

                var boxCollider = page.AddComponent<BoxCollider>();
                boxCollider.isTrigger = true;
                Debug.Log($"[DrawingPages] Added trigger collider to {pageName}");
            }

            // BY2 gets TaskReward (spawns on GnomeParade completion)
            if (pageName == "DrawingPage_BY2")
            {
                var reward = page.GetComponent<TaskReward>();
                if (reward == null)
                {
                    reward = page.AddComponent<TaskReward>();
                    Debug.Log($"[DrawingPages] Added TaskReward to DrawingPage_BY2");
                }

                var so = new SerializedObject(reward);
                so.FindProperty("_requiredTask").enumValueIndex = (int)BackYardTaskTracker.TaskId.GnomeParade;
                so.ApplyModifiedProperties();

                Debug.Log($"[DrawingPages] ✓ DrawingPage_BY2 wired to spawn on GnomeParade completion");
            }

            wiredCount++;
            Debug.Log($"[DrawingPages] ✓ {pageName} configured");
        }

        Debug.Log($"[DrawingPages] ✓ Wired {wiredCount} drawing pages");
    }
}
