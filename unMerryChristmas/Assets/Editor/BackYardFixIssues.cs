using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BackYardFixIssues
{
    [MenuItem("Tools/BackYard Fix Issues")]
    public static void FixAllIssues()
    {
        Debug.Log("[BackYardFixIssues] Starting fixes...\n");

        // Issue 1 & 2: Make tokens and pages pickable + TaskReward setup
        FixAbilityTokensAndPages();

        // Issue 3: Make gnomes pickable
        FixGnomePickability();

        // Issue 4: Verify moving boxes can disappear
        FixMovingBoxes();

        // Issue 5: Fix table invisibility
        FixTableVisibility();

        // Issue 6: Fix player spinning
        FixPlayerSpinning();

        // Issue 7: Adjust jump height guidance
        GuidanceForJumpHeight();

        Debug.Log("\n[BackYardFixIssues] ✓ All fixes applied!");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void FixAbilityTokensAndPages()
    {
        Debug.Log("[Issue 1&2] Fixing ability tokens and drawing pages...");

        // Token names and their reward tasks
        var tokenConfig = new System.Collections.Generic.Dictionary<string, BackYardTaskTracker.TaskId>()
        {
            { "PushingAT", BackYardTaskTracker.TaskId.GardenChaos },
            { "ThrowingAT", BackYardTaskTracker.TaskId.BurstPipe },
            { "ZiplineAT", BackYardTaskTracker.TaskId.TheHighWall },
        };

        int fixedTokens = 0;
        foreach (var kvp in tokenConfig)
        {
            var token = GameObject.Find(kvp.Key);
            if (token == null) continue;

            // Step 1: Ensure collider exists (TaskReward requires it)
            if (token.GetComponent<Collider>() == null)
            {
                token.AddComponent<SphereCollider>();
                Debug.Log($"  ✓ Added SphereCollider to {kvp.Key}");
            }

            // Step 2: Add PickupObject so tokens are collectable
            if (token.GetComponent<PickupObject>() == null)
            {
                token.AddComponent<PickupObject>();
                Debug.Log($"  ✓ Added PickupObject to {kvp.Key}");
            }

            // Step 3: Add/verify TaskReward with correct task
            var reward = token.GetComponent<TaskReward>();
            if (reward == null)
            {
                reward = token.AddComponent<TaskReward>();
                Debug.Log($"  ✓ Added TaskReward to {kvp.Key}");
            }

            if (reward != null)
            {
                var so = new SerializedObject(reward);
                so.FindProperty("_requiredTask").enumValueIndex = (int)kvp.Value;
                so.ApplyModifiedProperties();
            }

            // Step 4: Disable initially (TaskReward will enable on task completion)
            foreach (var col in token.GetComponentsInChildren<Collider>())
                col.enabled = false;
            foreach (var rend in token.GetComponentsInChildren<Renderer>())
                rend.enabled = false;

            fixedTokens++;
            Debug.Log($"  ✓ {kvp.Key} now TaskReward-controlled (appears on {kvp.Value})");
        }

        // Drawing pages - BY1 and BY3 always visible, BY2 is TaskReward
        var pageConfig = new System.Collections.Generic.Dictionary<string, bool>()
        {
            { "DrawingPage_BY1", false }, // Always visible
            { "DrawingPage_BY2", true },  // TaskReward (appears on GnomeParade)
            { "DrawingPage_BY3", false }, // Always visible
        };

        int fixedPages = 0;
        foreach (var kvp in pageConfig)
        {
            var page = GameObject.Find(kvp.Key);
            if (page == null) continue;

            // Step 1: Ensure collider exists
            if (page.GetComponent<Collider>() == null)
            {
                page.AddComponent<BoxCollider>();
            }

            // Step 2: Add PickupObject so pages are collectable
            if (page.GetComponent<PickupObject>() == null)
            {
                page.AddComponent<PickupObject>();
            }

            // Step 3: Add DrawingPageCollectable if missing
            if (page.GetComponent<DrawingPageCollectable>() == null)
            {
                page.AddComponent<DrawingPageCollectable>();
            }

            // Step 4: Handle TaskReward or visibility
            if (kvp.Value) // TaskReward for BY2
            {
                var reward = page.GetComponent<TaskReward>();
                if (reward == null)
                {
                    reward = page.AddComponent<TaskReward>();
                }

                if (reward != null)
                {
                    var so = new SerializedObject(reward);
                    so.FindProperty("_requiredTask").enumValueIndex = (int)BackYardTaskTracker.TaskId.GnomeParade;
                    so.ApplyModifiedProperties();
                }

                // Disable initially
                foreach (var col in page.GetComponentsInChildren<Collider>())
                    col.enabled = false;
                foreach (var rend in page.GetComponentsInChildren<Renderer>())
                    rend.enabled = false;

                Debug.Log($"  ✓ {kvp.Key} is now TaskReward (appears on GnomeParade)");
            }
            else
            {
                // Always visible - enable renderers and colliders
                foreach (var col in page.GetComponentsInChildren<Collider>())
                    col.enabled = true;
                foreach (var rend in page.GetComponentsInChildren<Renderer>())
                    rend.enabled = true;

                Debug.Log($"  ✓ {kvp.Key} is always visible");
            }

            fixedPages++;
        }

        // Fix StuffedBear - should be TaskReward for Task 4 completion
        var bear = GameObject.Find("StuffedBear");
        if (bear != null)
        {
            // Ensure collider exists first
            if (bear.GetComponent<Collider>() == null)
            {
                bear.AddComponent<BoxCollider>();
            }

            var reward = bear.GetComponent<TaskReward>();
            if (reward == null)
            {
                reward = bear.AddComponent<TaskReward>();
            }

            if (reward != null)
            {
                var so = new SerializedObject(reward);
                so.FindProperty("_requiredTask").enumValueIndex = (int)BackYardTaskTracker.TaskId.UnearthThePast;
                so.ApplyModifiedProperties();
            }

            // Disable initially
            foreach (var col in bear.GetComponentsInChildren<Collider>())
                col.enabled = false;
            foreach (var rend in bear.GetComponentsInChildren<Renderer>())
                rend.enabled = false;

            Debug.Log($"  ✓ StuffedBear is now TaskReward (appears on BoxStack completion)");
        }

        Debug.Log($"  Fixed {fixedTokens} tokens + {fixedPages} pages\n");
    }

    private static void FixGnomePickability()
    {
        Debug.Log("[Issue 3] Setting up gnome placement zones...");

        for (int i = 1; i <= 3; i++)
        {
            var gnome = GameObject.Find($"Gnome_{i}");
            if (gnome == null) continue;

            // Ensure PickupObject exists (gnomes MUST be pickable to carry to zones)
            if (gnome.GetComponent<PickupObject>() == null)
            {
                gnome.AddComponent<PickupObject>();
                Debug.Log($"  ✓ Added PickupObject to Gnome_{i} (allows carrying to zones)");
            }

            // Ensure it has the "Gnome" tag for GnomePlacementZone detection
            gnome.tag = "Gnome";
            Debug.Log($"  ✓ Gnome_{i} ready for placement zones");
        }

        Debug.Log("");
    }

    private static void FixMovingBoxes()
    {
        Debug.Log("[Issue 4] Fixing moving boxes disappearance...");

        for (int i = 1; i <= 3; i++)
        {
            var box = GameObject.Find($"MovingBox_{i}");
            if (box == null) continue;

            // Ensure MovingBox component exists
            var movingBox = box.GetComponent<MovingBox>();
            if (movingBox == null)
            {
                movingBox = box.AddComponent<MovingBox>();
                Debug.Log($"  ✓ Added MovingBox component to MovingBox_{i}");
            }

            // Ensure there's a non-trigger collider for interaction detection
            var colliders = box.GetComponentsInChildren<Collider>();
            bool hasNonTrigger = false;
            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    hasNonTrigger = true;
                    break;
                }
            }

            if (!hasNonTrigger && colliders.Length > 0)
            {
                // Convert first collider to non-trigger
                colliders[0].isTrigger = false;
                Debug.Log($"  ✓ Set collider on MovingBox_{i} to non-trigger for interaction");
            }

            // On MovingBox_3, ensure StuffedBear reward is set
            if (i == 3)
            {
                var bear = GameObject.Find("StuffedBear");
                if (bear != null)
                {
                    var so = new SerializedObject(movingBox);
                    so.FindProperty("_rewardPrefab").objectReferenceValue = bear;
                    so.ApplyModifiedProperties();
                    Debug.Log($"  ✓ Wired StuffedBear reward to MovingBox_3");
                }
            }
        }

        Debug.Log("");
    }

    private static void FixTableVisibility()
    {
        Debug.Log("[Issue 5] Fixing table visibility...");

        var table = GameObject.Find("PatioTable");
        if (table == null)
        {
            Debug.LogWarning("  ✗ PatioTable not found!");
            return;
        }

        // Enable all renderers
        var renderers = table.GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            rend.enabled = true;
            Debug.Log($"  ✓ Enabled renderer on {rend.gameObject.name}");
        }

        // Check if material is the issue
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                // If alpha is 0, set to 1
                if (mat.HasProperty("_Color"))
                {
                    var color = mat.color;
                    if (color.a < 0.1f)
                    {
                        color.a = 1f;
                        mat.color = color;
                        Debug.Log($"  ✓ Fixed material alpha on {mat.name}");
                    }
                }
            }
        }

        Debug.Log("");
    }

    private static void FixPlayerSpinning()
    {
        Debug.Log("[Issue 6] Fixing player spinning...");

        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("  ✗ Player not found!");
            return;
        }

        var rb = player.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("  ✗ Player has no Rigidbody!");
            return;
        }

        // Prevent uncontrolled rotation
        // Keep Y rotation free (for player turning), freeze X and Z
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationZ;

        Debug.Log($"  ✓ Set player rigidbody constraints to prevent spinning");
        Debug.Log($"  ✓ Current mass: {rb.mass} (adjust if jump is still wrong)");
        Debug.Log("");
    }

    private static void GuidanceForJumpHeight()
    {
        Debug.Log("[Issue 7] Jump height guidance...");
        Debug.Log("  To adjust jump height:");
        Debug.Log("  1. Check Controller._jumpForce (currently set in code)");
        Debug.Log("  2. Check Player Rigidbody mass (1=too light, 2=better, 3+=heavy)");
        Debug.Log("  3. Jump should reach: bench height ~1m, table height ~0.7m");
        Debug.Log("  4. Recommended: mass=2, jumpForce=8-10");
        Debug.Log("  5. Tweak in play mode with Console to find sweet spot");
        Debug.Log("");
    }
}
