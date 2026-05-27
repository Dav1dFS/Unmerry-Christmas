using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class BackYardInteractionSetup
{
    [MenuItem("Tools/BackYard Setup Interactions")]
    public static void SetupAllInteractions()
    {
        Debug.Log("[BackYardInteractionSetup] Starting interaction setup...\n");

        // First, ensure the "Pickable" layer exists
        EnsureLayerExists("Pickable");

        // Setup tokens - should be on Pickable layer with colliders
        SetupTokens();

        // Setup pages - should be on Pickable layer with colliders
        SetupPages();

        // Setup gnomes - should be on Pickable layer, NOT as pickups, for zone placement
        SetupGnomes();

        // Setup moving boxes - should be on Pickable layer and interactive
        SetupMovingBoxes();

        Debug.Log("[BackYardInteractionSetup] ✓ All interactions configured!");
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void EnsureLayerExists(string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty layers = tagManager.FindProperty("layers");

        bool layerExists = false;
        for (int i = 0; i < layers.arraySize; i++)
        {
            SerializedProperty layerProp = layers.GetArrayElementAtIndex(i);
            if (layerProp.stringValue == layerName)
            {
                layerExists = true;
                break;
            }
        }

        if (!layerExists)
        {
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layerProp = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layerProp.stringValue))
                {
                    layerProp.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"  ✓ Created '{layerName}' layer");
                    return;
                }
            }
        }
    }

    private static void SetupTokens()
    {
        Debug.Log("[Tokens] Setting up ability tokens...");

        var tokenNames = new[] { "PushingAT", "ThrowingAT", "ZiplineAT" };
        int tokensSetup = 0;

        foreach (var tokenName in tokenNames)
        {
            var token = GameObject.Find(tokenName);
            if (token == null) continue;

            // Set layer to Pickable
            SetLayerRecursive(token, LayerMask.NameToLayer("Pickable"));

            // Ensure collider exists
            if (token.GetComponent<Collider>() == null)
            {
                token.AddComponent<SphereCollider>();
                Debug.Log($"  ✓ Added collider to {tokenName}");
            }
            else
            {
                token.GetComponent<Collider>().enabled = true;
            }

            // Ensure PickupObject exists
            if (token.GetComponent<PickupObject>() == null)
            {
                token.AddComponent<PickupObject>();
                Debug.Log($"  ✓ Added PickupObject to {tokenName}");
            }

            tokensSetup++;
        }

        Debug.Log($"  ✓ Setup {tokensSetup} tokens\n");
    }

    private static void SetupPages()
    {
        Debug.Log("[Pages] Setting up drawing pages...");

        var pageNames = new[] { "DrawingPage_BY1", "DrawingPage_BY2", "DrawingPage_BY3" };
        int pagesSetup = 0;

        foreach (var pageName in pageNames)
        {
            var page = GameObject.Find(pageName);
            if (page == null) continue;

            // Set layer to Pickable
            SetLayerRecursive(page, LayerMask.NameToLayer("Pickable"));

            // Ensure collider exists
            if (page.GetComponent<Collider>() == null)
            {
                page.AddComponent<BoxCollider>();
                Debug.Log($"  ✓ Added collider to {pageName}");
            }
            else
            {
                page.GetComponent<Collider>().enabled = true;
            }

            // Ensure PickupObject exists
            if (page.GetComponent<PickupObject>() == null)
            {
                page.AddComponent<PickupObject>();
                Debug.Log($"  ✓ Added PickupObject to {pageName}");
            }

            pagesSetup++;
        }

        Debug.Log($"  ✓ Setup {pagesSetup} pages\n");
    }

    private static void SetupGnomes()
    {
        Debug.Log("[Gnomes] Setting up gnomes for zone placement...");

        for (int i = 1; i <= 3; i++)
        {
            var gnome = GameObject.Find($"Gnome_{i}");
            if (gnome == null) continue;

            // Set layer to Pickable
            SetLayerRecursive(gnome, LayerMask.NameToLayer("Pickable"));

            // Ensure collider exists
            if (gnome.GetComponent<Collider>() == null)
            {
                gnome.AddComponent<SphereCollider>();
                Debug.Log($"  ✓ Added collider to Gnome_{i}");
            }
            else
            {
                gnome.GetComponent<Collider>().enabled = true;
            }

            // Ensure PickupObject exists (gnomes must be pickable to be carried to zones)
            if (gnome.GetComponent<PickupObject>() == null)
            {
                gnome.AddComponent<PickupObject>();
                Debug.Log($"  ✓ Added PickupObject to Gnome_{i} (allows pickup and placement in zones)");
            }

            // Ensure Gnome tag exists
            gnome.tag = "Gnome";

            Debug.Log($"  ✓ Gnome_{i} ready for placement zones");
        }

        Debug.Log("");
    }

    private static void SetupMovingBoxes()
    {
        Debug.Log("[MovingBoxes] Setting up interactive boxes...");

        for (int i = 1; i <= 3; i++)
        {
            var box = GameObject.Find($"MovingBox_{i}");
            if (box == null) continue;

            // Set layer to Pickable
            SetLayerRecursive(box, LayerMask.NameToLayer("Pickable"));

            // Ensure collider exists and is NOT a trigger
            var colliders = box.GetComponentsInChildren<Collider>();
            bool hasNonTriggerCollider = false;

            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    hasNonTriggerCollider = true;
                    col.enabled = true;
                    break;
                }
            }

            if (!hasNonTriggerCollider)
            {
                if (colliders.Length > 0)
                {
                    colliders[0].isTrigger = false;
                    colliders[0].enabled = true;
                }
                else
                {
                    box.AddComponent<BoxCollider>();
                }
                Debug.Log($"  ✓ Configured collider on MovingBox_{i}");
            }

            // Ensure MovingBox component exists
            if (box.GetComponent<MovingBox>() == null)
            {
                box.AddComponent<MovingBox>();
                Debug.Log($"  ✓ Added MovingBox component to MovingBox_{i}");
            }

            Debug.Log($"  ✓ MovingBox_{i} is now interactive");
        }

        Debug.Log("");
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
