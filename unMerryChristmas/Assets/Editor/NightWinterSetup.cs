using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public class NightWinterSetup
{
    // True daytime originals sourced from git history (BackYardScenario commit 27c97ab)
    static readonly Color   OrigLightColor    = new Color(1f, 0.95686275f, 0.8392157f);
    static readonly float   OrigLightIntensity = 1f;
    // Quaternion: x=0.40821788, y=-0.23456968, z=0.10938163, w=0.8754261
    static readonly Quaternion OrigLightRot   = new Quaternion(0.40821788f, -0.23456968f, 0.10938163f, 0.8754261f);
    static readonly AmbientMode OrigAmbientMode = AmbientMode.Skybox;
    static readonly Color   OrigAmbientSky    = new Color(0.212f, 0.227f, 0.259f);
    static readonly bool    OrigFog           = false;
    static readonly Color   OrigFogColor      = new Color(0.5f, 0.5f, 0.5f);
    static readonly FogMode OrigFogMode       = FogMode.ExponentialSquared;
    static readonly float   OrigFogDensity    = 0.01f;

    // EditorPrefs keys — only used to carry values across domain reloads
    const string KEY_ACTIVE       = "NightWinter_Active";
    const string KEY_LIGHT_R      = "NightWinter_LightR";
    const string KEY_LIGHT_G      = "NightWinter_LightG";
    const string KEY_LIGHT_B      = "NightWinter_LightB";
    const string KEY_LIGHT_INT    = "NightWinter_LightIntensity";
    const string KEY_LIGHT_QX     = "NightWinter_LightQX";
    const string KEY_LIGHT_QY     = "NightWinter_LightQY";
    const string KEY_LIGHT_QZ     = "NightWinter_LightQZ";
    const string KEY_LIGHT_QW     = "NightWinter_LightQW";
    const string KEY_AMBIENT_MODE = "NightWinter_AmbientMode";
    const string KEY_AMBIENT_R    = "NightWinter_AmbientR";
    const string KEY_AMBIENT_G    = "NightWinter_AmbientG";
    const string KEY_AMBIENT_B    = "NightWinter_AmbientB";
    const string KEY_FOG          = "NightWinter_Fog";
    const string KEY_FOG_R        = "NightWinter_FogR";
    const string KEY_FOG_G        = "NightWinter_FogG";
    const string KEY_FOG_B        = "NightWinter_FogB";
    const string KEY_FOG_MODE     = "NightWinter_FogMode";
    const string KEY_FOG_DENSITY  = "NightWinter_FogDensity";

    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Apply Night Winter Theme")]
    public static void ApplyNightWinterTheme()
    {
        Debug.Log("[NightWinterSetup] Applying night winter theme...");

        SaveOriginalLighting();
        SetupNightLighting();
        SetupSnowParticles();

        EditorPrefs.SetBool(KEY_ACTIVE, true);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[NightWinterSetup] ✓ Night winter theme applied!");
    }

    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Remove Night Winter Theme")]
    public static void RemoveNightWinterTheme()
    {
        Debug.Log("[NightWinterSetup] Removing night winter theme...");

        RemoveSnowSystem();
        RestoreOriginalLighting();

        EditorPrefs.DeleteKey(KEY_ACTIVE);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[NightWinterSetup] ✓ Night winter theme removed!");
    }

    // ─────────────────────────────────────────────────────────────────────
    private static void SaveOriginalLighting()
    {
        var dirLight = FindDirectionalLight();
        if (dirLight != null)
        {
            var q = dirLight.transform.rotation;
            EditorPrefs.SetFloat(KEY_LIGHT_R,   dirLight.color.r);
            EditorPrefs.SetFloat(KEY_LIGHT_G,   dirLight.color.g);
            EditorPrefs.SetFloat(KEY_LIGHT_B,   dirLight.color.b);
            EditorPrefs.SetFloat(KEY_LIGHT_INT, dirLight.intensity);
            EditorPrefs.SetFloat(KEY_LIGHT_QX,  q.x);
            EditorPrefs.SetFloat(KEY_LIGHT_QY,  q.y);
            EditorPrefs.SetFloat(KEY_LIGHT_QZ,  q.z);
            EditorPrefs.SetFloat(KEY_LIGHT_QW,  q.w);
        }

        EditorPrefs.SetInt  (KEY_AMBIENT_MODE, (int)RenderSettings.ambientMode);
        EditorPrefs.SetFloat(KEY_AMBIENT_R,    RenderSettings.ambientLight.r);
        EditorPrefs.SetFloat(KEY_AMBIENT_G,    RenderSettings.ambientLight.g);
        EditorPrefs.SetFloat(KEY_AMBIENT_B,    RenderSettings.ambientLight.b);

        EditorPrefs.SetBool (KEY_FOG,         RenderSettings.fog);
        EditorPrefs.SetFloat(KEY_FOG_R,       RenderSettings.fogColor.r);
        EditorPrefs.SetFloat(KEY_FOG_G,       RenderSettings.fogColor.g);
        EditorPrefs.SetFloat(KEY_FOG_B,       RenderSettings.fogColor.b);
        EditorPrefs.SetInt  (KEY_FOG_MODE,    (int)RenderSettings.fogMode);
        EditorPrefs.SetFloat(KEY_FOG_DENSITY, RenderSettings.fogDensity);

        Debug.Log("[NightWinterSetup] ✓ Original lighting saved");
    }

    private static void RestoreOriginalLighting()
    {
        // Always restore from hardcoded originals — EditorPrefs can be stale if Apply
        // was run while the scene already had night lighting applied.
        var dirLight = FindDirectionalLight();
        if (dirLight != null)
        {
            Undo.RecordObject(dirLight, "Night Winter - Restore Light");
            Undo.RecordObject(dirLight.transform, "Night Winter - Restore Light Transform");
            dirLight.color              = OrigLightColor;
            dirLight.intensity          = OrigLightIntensity;
            dirLight.transform.rotation = OrigLightRot;
            Debug.Log("[NightWinterSetup] ✓ Directional light restored");
        }

        RenderSettings.ambientMode  = OrigAmbientMode;
        RenderSettings.ambientLight = OrigAmbientSky;
        RenderSettings.fog          = OrigFog;
        RenderSettings.fogColor     = OrigFogColor;
        RenderSettings.fogMode      = OrigFogMode;
        RenderSettings.fogDensity   = OrigFogDensity;

        // Wipe any stale EditorPrefs so the next Apply saves fresh values
        foreach (var k in new[]{KEY_LIGHT_R,KEY_LIGHT_G,KEY_LIGHT_B,KEY_LIGHT_INT,
                                  KEY_LIGHT_QX,KEY_LIGHT_QY,KEY_LIGHT_QZ,KEY_LIGHT_QW,
                                  KEY_AMBIENT_MODE,KEY_AMBIENT_R,KEY_AMBIENT_G,KEY_AMBIENT_B,
                                  KEY_FOG,KEY_FOG_R,KEY_FOG_G,KEY_FOG_B,KEY_FOG_MODE,KEY_FOG_DENSITY})
            EditorPrefs.DeleteKey(k);

        Debug.Log("[NightWinterSetup] ✓ Lighting and fog restored");
    }

    private static void RemoveSnowSystem()
    {
        // Find by name — handles both active and inactive objects
        var snow = GameObject.Find("SnowSystem");
        if (snow != null)
        {
            Undo.DestroyObjectImmediate(snow);
            Debug.Log("[NightWinterSetup] ✓ SnowSystem removed");
        }
        else
        {
            Debug.LogWarning("[NightWinterSetup] SnowSystem not found in scene (already removed?)");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    private static void SetupNightLighting()
    {
        var dirLight = FindDirectionalLight();
        if (dirLight != null)
        {
            Undo.RecordObject(dirLight, "Night Winter - Directional Light");
            Undo.RecordObject(dirLight.transform, "Night Winter - Directional Light Transform");
            dirLight.color     = new Color(0.55f, 0.65f, 0.85f);
            dirLight.intensity = 0.15f;
            dirLight.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            Debug.Log("[NightWinterSetup] ✓ Directional light set to moonlight");
        }
        else
        {
            Debug.LogWarning("[NightWinterSetup] No Directional Light found in scene");
        }

        RenderSettings.ambientMode  = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.04f, 0.05f, 0.10f);

        RenderSettings.fog              = true;
        RenderSettings.fogColor         = new Color(0.10f, 0.12f, 0.18f);
        RenderSettings.fogMode          = FogMode.Linear;
        RenderSettings.fogStartDistance = 18f;
        RenderSettings.fogEndDistance   = 45f;

        Debug.Log("[NightWinterSetup] ✓ Ambient light and fog configured");
    }

    private static void SetupSnowParticles()
    {
        var existing = GameObject.Find("SnowSystem");
        if (existing != null)
        {
            Debug.Log("[NightWinterSetup] SnowSystem already exists, skipping creation");
            return;
        }

        var go = new GameObject("SnowSystem");
        go.transform.position = new Vector3(0f, 14f, 0f);

        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(4f, 7f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.03f, 0.09f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, 0.85f),
            new Color(0.85f, 0.90f, 1.00f, 0.70f));
        main.gravityModifier = 0.08f;
        main.maxParticles    = 800;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.enabled     = true;
        emission.rateOverTime = 120f;

        var shape = ps.shape;
        shape.enabled    = true;
        shape.shapeType  = ParticleSystemShapeType.Box;
        shape.scale      = new Vector3(30f, 0.1f, 30f);

        var vol = ps.velocityOverLifetime;
        vol.enabled = true;
        vol.space   = ParticleSystemSimulationSpace.World;
        vol.x       = new ParticleSystem.MinMaxCurve(-0.3f, 0.1f);
        vol.z       = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);

        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z       = new ParticleSystem.MinMaxCurve(
            -Mathf.Deg2Rad * 30f,
             Mathf.Deg2Rad * 30f);

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        var sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f,    1f);
        sizeCurve.AddKey(0.85f, 1f);
        sizeCurve.AddKey(1f,    0f);
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        Undo.RegisterCreatedObjectUndo(go, "Create SnowSystem");

        Debug.Log("[NightWinterSetup] ✓ SnowSystem particle system created");
    }

    private static Light FindDirectionalLight()
    {
        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light.type == LightType.Directional)
                return light;
        }
        return null;
    }
}
