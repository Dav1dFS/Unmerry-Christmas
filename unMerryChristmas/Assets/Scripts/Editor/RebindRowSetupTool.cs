using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

/// <summary>
/// Menu: Tools > UnMerry > Build Rebind UI
/// Reads all actions from the Player map and instantiates one RebindActionRow
/// prefab per action inside the supplied Controls_Container transform.
/// </summary>
public class RebindRowSetupTool : EditorWindow
{
    private Transform         _controlsContainer;
    private GameObject        _rowPrefab;
    private InputActionAsset  _inputAsset;

    [MenuItem("Tools/UnMerry/Build Rebind UI")]
    public static void ShowWindow() => GetWindow<RebindRowSetupTool>("Rebind UI Builder");

    private void OnGUI()
    {
        GUILayout.Label("Rebind UI Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        _controlsContainer = (Transform)EditorGUILayout.ObjectField(
            "Controls Container", _controlsContainer, typeof(Transform), allowSceneObjects: true);

        _rowPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Row Prefab", _rowPrefab, typeof(GameObject), allowSceneObjects: false);

        _inputAsset = (InputActionAsset)EditorGUILayout.ObjectField(
            "Input Action Asset", _inputAsset, typeof(InputActionAsset), allowSceneObjects: false);

        EditorGUILayout.Space(8);

        bool ready = _controlsContainer != null && _rowPrefab != null && _inputAsset != null;
        EditorGUI.BeginDisabledGroup(!ready);

        if (GUILayout.Button("Generate Rows"))        GenerateRows(clear: false);
        if (GUILayout.Button("Clear & Regenerate"))   GenerateRows(clear: true);

        EditorGUI.EndDisabledGroup();

        if (!ready)
            EditorGUILayout.HelpBox("Assign all three fields to enable generation.", MessageType.Info);
    }

    private void GenerateRows(bool clear)
    {
        if (clear)
        {
            for (int i = _controlsContainer.childCount - 1; i >= 0; i--)
                DestroyImmediate(_controlsContainer.GetChild(i).gameObject);
        }

        var playerMap = _inputAsset.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogError("[RebindRowSetupTool] 'Player' action map not found in asset.");
            return;
        }

        int created = 0;
        foreach (var action in playerMap.actions)
        {
            var rowGo = (GameObject)PrefabUtility.InstantiatePrefab(_rowPrefab, _controlsContainer);
            rowGo.name = $"Row_{action.name}";

            var row = rowGo.GetComponent<RebindActionRow>();
            if (row == null) continue;

            var actionRef = InputActionReference.Create(action);
            row.SetupFromEditor(actionRef, action.name);
            EditorUtility.SetDirty(rowGo);
            created++;
        }

        EditorUtility.SetDirty(_controlsContainer.gameObject);
        Debug.Log($"[RebindRowSetupTool] Generated {created} rebind rows.");
    }
}
