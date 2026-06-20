using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HintProvider))]
public class HintProviderEditor : Editor
{
    private const int MaxChars = 120;
    private SerializedProperty _hintTextProp;

    private void OnEnable()
    {
        _hintTextProp = serializedObject.FindProperty("_hintText");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Accessibility Hint", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // ── Text area ──────────────────────────────────────────────────────
        EditorGUILayout.LabelField("Hint Text");
        _hintTextProp.stringValue = EditorGUILayout.TextArea(
            _hintTextProp.stringValue ?? "",
            GUILayout.MinHeight(60));

        // ── Character count ────────────────────────────────────────────────
        int    charCount   = _hintTextProp.stringValue?.Length ?? 0;
        var    countStyle  = new GUIStyle(EditorStyles.miniLabel);
        if (charCount > MaxChars)
            countStyle.normal.textColor = new Color(1f, 0.55f, 0f); // orange warning
        EditorGUILayout.LabelField($"{charCount} / {MaxChars} chars", countStyle);

        // ── Empty warning ──────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(_hintTextProp.stringValue))
        {
            EditorGUILayout.HelpBox(
                "Hint text is empty. Players holding H near this object will see no hint.",
                MessageType.Warning);
        }

        EditorGUILayout.Space(6);

        // ── Preview button (Play Mode only) ───────────────────────────────
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        if (GUILayout.Button("▶ Preview Hint"))
            UIManager.Instance?.ShowContextHint(_hintTextProp.stringValue);
        EditorGUI.EndDisabledGroup();

        if (!Application.isPlaying)
            EditorGUILayout.LabelField("Enter Play Mode to use Preview.", EditorStyles.centeredGreyMiniLabel);

        serializedObject.ApplyModifiedProperties();
    }
}
