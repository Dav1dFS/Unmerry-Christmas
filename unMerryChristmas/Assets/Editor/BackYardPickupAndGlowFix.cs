using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Fixes the 4 remaining issues with BackYard interactions:
/// 1. Gnomes can't be picked up - adds PickupObject so they can be carried to zones
/// 2. Tokens can't be picked up - ensures correct layer and collider setup
/// 3. Token glow visible when disabled - disables glow light on token disable
/// 4. Glow too powerful - reduces intensity from 1.2f to 0.6f
/// Also fixes: PatioTable mesh collider issue and other physics problems
/// </summary>
public class BackYardPickupAndGlowFix
{
    [MenuItem("Tools/BackYard Fix Pickups and Glow")]
    public static void FixPickupsAndGlow()
    {
        // Must stop play mode to apply editor changes
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("[BackYardPickupAndGlowFix] Cannot run during play mode. Stopping play...");
            EditorApplication.isPlaying = false;
            return;
        }

        Debug.Log("[BackYardPickupAndGlowFix] Starting pickup and glow fixes...\n");

        // Fix critical physics issues first
        FixPatioTableMeshCollider();
        FixGnomeMeshColliders();  // NEW: Fix gnome mesh colliders
        FixIceSlideTrackerSetup();

        // Issue 1: Fix gnome pickability
        FixGnomePickup();

        // Issue 2: Fix token pickup
        FixTokenPickup();

        // Issue 3 & 4: Fix token glow visibility and intensity
        FixTokenGlow();

        Debug.Log("[BackYardPickupAndGlowFix] ✓ All fixes applied!");

        // Only mark dirty if not in play mode
        if (!EditorApplication.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    private static void FixPatioTableMeshCollider()
    {
        Debug.Log("[Physics] Fixing PatioTable mesh collider...");

        var patioTable = GameObject.Find("PatioTable");
        if (patioTable == null)
        {
            Debug.LogWarning("  ✗ PatioTable not found!");
            return;
        }

        // Find all mesh colliders in the table
        var meshColliders = patioTable.GetComponentsInChildren<MeshCollider>();
        foreach (var meshCol in meshColliders)
        {
            // Make mesh colliders convex so they work with dynamic rigidbodies
            if (!meshCol.convex)
            {
                meshCol.convex = true;
                Debug.Log($"  ✓ Made mesh collider on {meshCol.gameObject.name} convex");
            }
        }

        // Ensure rigidbody is not kinematic for physics interactions
        var rb = patioTable.GetComponent<Rigidbody>();
        if (rb != null && rb.isKinematic)
        {
            rb.isKinematic = false;
            Debug.Log("  ✓ Set PatioTable rigidbody to dynamic");
        }

        Debug.Log("");
    }

    private static void FixGnomeMeshColliders()
    {
        Debug.Log("[Physics] Fixing gnome mesh colliders...");

        for (int i = 1; i <= 3; i++)
        {
            var gnome = GameObject.Find($"Gnome_{i}");
            if (gnome == null) continue;

            // Find all mesh colliders on gnomes
            var meshColliders = gnome.GetComponentsInChildren<MeshCollider>();
            foreach (var meshCol in meshColliders)
            {
                // Make mesh colliders convex so they work with dynamic rigidbodies
                if (!meshCol.convex)
                {
                    meshCol.convex = true;
                    Debug.Log($"  ✓ Made mesh collider on Gnome_{i} convex");
                }
            }
        }

        Debug.Log("");
    }

    private static void FixIceSlideTrackerSetup()
    {
        Debug.Log("[Setup] Checking IceSlideTracker initialization...");

        var tracker = Object.FindAnyObjectByType<IceSlideTracker>();
        if (tracker == null)
        {
            Debug.Log("  ℹ IceSlideTracker not found in scene (may not be needed for current focus)");
            return;
        }

        // IceSlideTracker requires CompostBin field to be assigned
        // If errors occur, ensure CompostBin is wired up in the Inspector
        var so = new SerializedObject(tracker);
        var compostBinProp = so.FindProperty("_compostBin");

        if (compostBinProp != null && compostBinProp.objectReferenceValue == null)
        {
            // Try to find CompostBin in scene
            var compostBin = Object.FindAnyObjectByType<CompostBin>();
            if (compostBin != null)
            {
                compostBinProp.objectReferenceValue = compostBin;
                so.ApplyModifiedProperties();
                Debug.Log("  ✓ Auto-wired CompostBin to IceSlideTracker");
            }
            else
            {
                Debug.LogWarning("  ✗ CompostBin not found in scene - please wire manually in Inspector");
            }
        }

        Debug.Log("");
    }

    private static void FixGnomePickup()
    {
        Debug.Log("[Issue 1] Fixing gnome pickup capability...");

        for (int i = 1; i <= 3; i++)
        {
            var gnome = GameObject.Find($"Gnome_{i}");
            if (gnome == null) continue;

            // Ensure Rigidbody exists - CRITICAL for PickupObject
            var rb = gnome.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gnome.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;
                Debug.Log($"  ✓ Added Rigidbody to Gnome_{i}");
            }
            else
            {
                rb.isKinematic = false; // Should be dynamic, made kinematic during pickup
                rb.useGravity = true;
            }

            // Ensure PickupObject exists (gnomes need this to be carried to zones)
            if (gnome.GetComponent<PickupObject>() == null)
            {
                gnome.AddComponent<PickupObject>();
                Debug.Log($"  ✓ Added PickupObject to Gnome_{i} (can now be picked up and placed in zones)");
            }
            else
            {
                Debug.Log($"  ✓ Gnome_{i} already has PickupObject");
            }

            // Ensure collider exists and is enabled
            var collider = gnome.GetComponent<Collider>();
            if (collider == null)
            {
                gnome.AddComponent<SphereCollider>();
                Debug.Log($"  ✓ Added SphereCollider to Gnome_{i}");
            }
            else
            {
                collider.enabled = true;
                // If it's a mesh collider, it should be convex (handled by FixGnomeMeshColliders)
                if (collider is MeshCollider meshCol && !meshCol.convex)
                {
                    meshCol.convex = true;
                    Debug.Log($"  ✓ Made mesh collider on Gnome_{i} convex");
                }
            }

            // Ensure Gnome tag for zone detection
            gnome.tag = "Gnome";

            Debug.Log($"  ✓ Gnome_{i} ready for pickup and placement");
        }

        Debug.Log("");
    }

    private static void FixTokenPickup()
    {
        Debug.Log("[Issue 2] Fixing token pickup capability...");

        var tokenNames = new[] { "PushingAT", "ThrowingAT", "ZiplineAT" };

        foreach (var tokenName in tokenNames)
        {
            var token = GameObject.Find(tokenName);
            if (token == null) continue;

            // Ensure Rigidbody exists - CRITICAL for PickupObject
            var rb = token.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = token.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;
                Debug.Log($"  ✓ Added Rigidbody to {tokenName}");
            }
            else
            {
                rb.isKinematic = false; // Should be dynamic, made kinematic during pickup
                rb.useGravity = true;
            }

            // Ensure collider exists and is NOT a trigger
            var collider = token.GetComponent<Collider>();
            if (collider == null)
            {
                token.AddComponent<SphereCollider>();
                Debug.Log($"  ✓ Added SphereCollider to {tokenName}");
            }
            else if (collider.isTrigger)
            {
                collider.isTrigger = false;
                Debug.Log($"  ✓ Set {tokenName} collider to non-trigger (allows physics pickup)");
            }

            // Ensure PickupObject exists
            if (token.GetComponent<PickupObject>() == null)
            {
                token.AddComponent<PickupObject>();
                Debug.Log($"  ✓ Added PickupObject to {tokenName}");
            }

            // Ensure AbilityToken exists (for unlock functionality)
            if (token.GetComponent<AbilityToken>() == null)
            {
                token.AddComponent<AbilityToken>();
                Debug.Log($"  ✓ Added AbilityToken to {tokenName}");
            }

            // Ensure layer is Pickable
            SetLayerRecursive(token, LayerMask.NameToLayer("Pickable"));

            Debug.Log($"  ✓ {tokenName} fully configured for pickup");
        }

        Debug.Log("");
    }

    private static void FixTokenGlow()
    {
        Debug.Log("[Issue 3 & 4] Fixing token glow visibility and intensity...");

        var tokenNames = new[] { "PushingAT", "ThrowingAT", "ZiplineAT" };
        int fixedCount = 0;

        foreach (var tokenName in tokenNames)
        {
            var token = GameObject.Find(tokenName);
            if (token == null) continue;

            var tokenEffect = token.GetComponent<TokenEffect>();
            if (tokenEffect == null)
            {
                // Add TokenEffect if missing
                tokenEffect = token.AddComponent<TokenEffect>();
                Debug.Log($"  ✓ Added TokenEffect to {tokenName}");
            }

            // Access private fields via SerializedObject to reduce glow intensity
            var so = new SerializedObject(tokenEffect);

            // Reduce glow intensity from 1.2 to 0.6 (Issue 4)
            var intensityProp = so.FindProperty("_glowIntensity");
            if (intensityProp != null && intensityProp.floatValue > 0.6f)
            {
                intensityProp.floatValue = 0.6f;
                so.ApplyModifiedProperties();
                Debug.Log($"  ✓ Reduced glow intensity on {tokenName} (1.2f -> 0.6f)");
            }

            // Add GlowControllerOnTokenEffect to handle glow disable when token is disabled (Issue 3)
            if (token.GetComponent<GlowControllerOnTokenEffect>() == null)
            {
                token.AddComponent<GlowControllerOnTokenEffect>();
                Debug.Log($"  ✓ Added glow control to {tokenName} (disables light when disabled)");
            }

            fixedCount++;
        }

        Debug.Log($"  ✓ Fixed glow on {fixedCount} tokens\n");
    }

    private static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}

/// <summary>
/// Handles disabling the glow light when the token is disabled.
/// This prevents invisible tokens from showing a visible glow.
/// </summary>
public class GlowControllerOnTokenEffect : MonoBehaviour
{
    private Light _glowLight;
    private bool _wasEnabled = true;

    private void Start()
    {
        // Find the glow light (created by TokenEffect as a child)
        _glowLight = GetComponentInChildren<Light>();
    }

    private void Update()
    {
        if (_glowLight == null) return;

        // Check if the main renderer is enabled
        var renderer = GetComponentInChildren<Renderer>();
        bool shouldGlow = renderer != null && renderer.enabled;

        // Sync glow light with renderer state
        if (shouldGlow != _wasEnabled)
        {
            _glowLight.enabled = shouldGlow;
            _wasEnabled = shouldGlow;
        }
    }
}
