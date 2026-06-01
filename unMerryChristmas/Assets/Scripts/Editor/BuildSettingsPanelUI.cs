using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menu: Tools > UnMerry > Build Settings Panel UI
///
/// Finds the existing 'Page_Content_Settings' GameObject in the active scene,
/// removes the three placeholder buttons (Audio Settings / Video Settings / Controls),
/// and builds the full accessibility Settings panel:
///   - Music / SFX / Dialogue sliders
///   - Highlight Objects toggle
///   - Puzzle Hints toggle
///   - Controls_Container (empty — populate with 'Build Rebind UI' tool next)
///   - Reset All button
/// Also adds and fully wires SettingsPanelController on the same GameObject.
///
/// Safe to re-run: existing children are cleared before rebuilding.
/// </summary>
public static class BuildSettingsPanelUI
{
    [MenuItem("Tools/UnMerry/Build Settings Panel UI")]
    public static void Build()
    {
        // ── Find the target ────────────────────────────────────────────────
        var page = GameObject.Find("Page_Content_Settings");
        if (page == null)
        {
            EditorUtility.DisplayDialog("Build Settings Panel UI",
                "'Page_Content_Settings' was not found in the active scene.\n" +
                "Make sure BackYardScenario is open.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Build Settings Panel UI");
        int undoGroup = Undo.GetCurrentGroup();

        // ── Clear existing children (placeholder buttons) ──────────────────
        for (int i = page.transform.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(page.transform.GetChild(i).gameObject);

        // ── Layout on the container ────────────────────────────────────────
        var vlg = page.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = Undo.AddComponent<VerticalLayoutGroup>(page);
        vlg.spacing            = 8;
        vlg.padding            = new RectOffset(20, 20, 16, 16);
        vlg.childAlignment     = TextAnchor.UpperLeft;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth  = true;
        vlg.childControlHeight = true;

        TMP_FontAsset font = FindFont();

        // ── Audio ──────────────────────────────────────────────────────────
        MakeHeader(page.transform, "AUDIO", font);
        Slider musicSlider    = MakeSliderRow(page.transform, "Music",    font);
        Slider sfxSlider      = MakeSliderRow(page.transform, "SFX",      font);
        Slider dialogueSlider = MakeSliderRow(page.transform, "Dialogue", font);

        // ── Visual ─────────────────────────────────────────────────────────
        MakeHeader(page.transform, "VISUAL", font);
        Toggle highlightToggle = MakeToggleRow(page.transform, "Highlight Objects", font);

        // ── Hints ──────────────────────────────────────────────────────────
        MakeHeader(page.transform, "HINTS", font);
        Toggle hintsToggle = MakeToggleRow(page.transform, "Puzzle Hints", font);

        // ── Controls (populated later by Build Rebind UI) ──────────────────
        MakeHeader(page.transform, "CONTROLS", font);
        MakeControlsContainer(page.transform);

        // ── Reset All ──────────────────────────────────────────────────────
        MakeSpacer(page.transform, 8);
        Button resetBtn = MakeButton(page.transform, "Reset All", font);

        // ── Add SettingsPanelController & wire everything ──────────────────
        var ctrl = page.GetComponent<SettingsPanelController>();
        if (ctrl == null) ctrl = Undo.AddComponent<SettingsPanelController>(page);

        // Wire serialized field references
        var so = new SerializedObject(ctrl);
        so.FindProperty("_musicSlider")    .objectReferenceValue = musicSlider;
        so.FindProperty("_sfxSlider")      .objectReferenceValue = sfxSlider;
        so.FindProperty("_dialogueSlider") .objectReferenceValue = dialogueSlider;
        so.FindProperty("_highlightToggle").objectReferenceValue = highlightToggle;
        so.FindProperty("_hintsToggle")    .objectReferenceValue = hintsToggle;
        so.ApplyModifiedProperties();

        // Wire persistent event listeners (dynamic — passes the event's own parameter)
        WireDynamicFloat(musicSlider,     ctrl, "OnMusicSliderChanged");
        WireDynamicFloat(sfxSlider,       ctrl, "OnSfxSliderChanged");
        WireDynamicFloat(dialogueSlider,  ctrl, "OnDialogueSliderChanged");
        WireDynamicBool (highlightToggle, ctrl, "OnHighlightToggleChanged");
        WireDynamicBool (hintsToggle,     ctrl, "OnHintsToggleChanged");
        WireVoidButton  (resetBtn,        ctrl, "OnResetAllPressed");

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("[BuildSettingsPanelUI] Settings panel built and wired inside 'Page_Content_Settings'.");
        Debug.Log("[BuildSettingsPanelUI] Next: run 'Tools > UnMerry > Build Rebind UI' to populate the Controls section.");
    }

    // ── UI factory helpers ─────────────────────────────────────────────────

    static TMP_FontAsset FindFont()
    {
        // Reuse whatever font is already in the scene
        foreach (var t in Object.FindObjectsByType<TMP_Text>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.font != null) return t.font;

        // Fallback: first TMP font asset in project
        foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
        {
            var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                        AssetDatabase.GUIDToAssetPath(guid));
            if (f != null) return f;
        }
        return null;
    }

    static void MakeHeader(Transform parent, string text, TMP_FontAsset font)
    {
        var go = new GameObject($"Header_{text}", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "header");
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 30;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = new Color(0.75f, 0.75f, 0.75f, 1f);
        if (font != null) tmp.font = font;
    }

    static Slider MakeSliderRow(Transform parent, string label, TMP_FontAsset font)
    {
        // Row container
        var row = new GameObject($"Row_{label}", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(row, "slider row");
        row.transform.SetParent(parent, false);
        var rowLe = row.AddComponent<LayoutElement>();
        rowLe.preferredHeight = 38;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing                = 12;
        hlg.childAlignment         = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;

        // Label
        var lblGo = new GameObject("Label", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(lblGo, "label");
        lblGo.transform.SetParent(row.transform, false);
        var lblLe  = lblGo.AddComponent<LayoutElement>();
        lblLe.minWidth      = 90;
        lblLe.preferredWidth = 90;
        lblLe.flexibleWidth = 0;
        var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
        lblTmp.text      = label;
        lblTmp.fontSize  = 20;
        lblTmp.color     = Color.white;
        lblTmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (font != null) lblTmp.font = font;

        // Slider (DefaultControls gives us the correct Background/Fill/Handle hierarchy)
        var sliderGo = DefaultControls.CreateSlider(new DefaultControls.Resources());
        Undo.RegisterCreatedObjectUndo(sliderGo, "slider");
        sliderGo.name = $"Slider_{label}";
        sliderGo.transform.SetParent(row.transform, false);
        var sliderLe        = sliderGo.AddComponent<LayoutElement>();
        sliderLe.flexibleWidth  = 1;
        sliderLe.preferredHeight = 24;
        var slider       = sliderGo.GetComponent<Slider>();
        slider.minValue  = 0f;
        slider.maxValue  = 1f;
        slider.value     = 1f;
        slider.wholeNumbers = false;

        return slider;
    }

    static Toggle MakeToggleRow(Transform parent, string label, TMP_FontAsset font)
    {
        var toggleGo = DefaultControls.CreateToggle(new DefaultControls.Resources());
        Undo.RegisterCreatedObjectUndo(toggleGo, "toggle");
        toggleGo.name = $"Toggle_{label}";
        toggleGo.transform.SetParent(parent, false);
        var le = toggleGo.AddComponent<LayoutElement>();
        le.preferredHeight = 36;

        // DefaultControls creates a legacy Text child — update or replace it
        var legacyText = toggleGo.GetComponentInChildren<Text>();
        if (legacyText != null)
        {
            legacyText.text     = label;
            legacyText.fontSize = 16;
            legacyText.color    = Color.white;
        }

        var toggle  = toggleGo.GetComponent<Toggle>();
        toggle.isOn = true;
        return toggle;
    }

    static void MakeControlsContainer(Transform parent)
    {
        var go = new GameObject("Controls_Container", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "controls container");
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 4;   // expands as rows are added by RebindRowSetupTool
        le.flexibleHeight  = 1;
        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing                = 4;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;
    }

    static void MakeSpacer(Transform parent, float height)
    {
        var go = new GameObject("Spacer", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "spacer");
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
    }

    static Button MakeButton(Transform parent, string text, TMP_FontAsset font)
    {
        var btnGo = DefaultControls.CreateButton(new DefaultControls.Resources());
        Undo.RegisterCreatedObjectUndo(btnGo, "button");
        btnGo.name = $"Btn_{text.Replace(" ", "")}";
        btnGo.transform.SetParent(parent, false);
        var le = btnGo.AddComponent<LayoutElement>();
        le.preferredHeight = 44;

        // Update label — DefaultControls creates legacy Text
        var legacyText = btnGo.GetComponentInChildren<Text>();
        if (legacyText != null)
        {
            legacyText.text     = text;
            legacyText.fontSize = 18;
        }

        return btnGo.GetComponent<Button>();
    }

    // ── Event wiring ───────────────────────────────────────────────────────
    // Strategy: use UnityEventTools to add the listener first — this forces Unity
    // to serialise the component's event data. Then patch m_Mode to 0 (EventDefined)
    // via SerializedObject so the event's own parameter is forwarded dynamically
    // instead of the fixed argument that UnityEventTools would store.
    //
    // Doing SerializedObject.FindProperty on a brand-new component before
    // UnityEventTools has touched it returns null (not yet serialised).

    static void WireDynamicFloat(Slider slider, SettingsPanelController ctrl, string method)
    {
        var mi     = ctrl.GetType().GetMethod(method,
                         System.Reflection.BindingFlags.Public |
                         System.Reflection.BindingFlags.Instance);
        var action = (UnityEngine.Events.UnityAction<float>)
                         System.Delegate.CreateDelegate(
                             typeof(UnityEngine.Events.UnityAction<float>), ctrl, mi);
        // Add with a dummy fixed value; we'll patch mode to dynamic below
        UnityEditor.Events.UnityEventTools.AddFloatPersistentListener(
            slider.onValueChanged, action, 0f);
        PatchLastListenerToEventDefined(new SerializedObject(slider), "m_OnValueChanged");
    }

    static void WireDynamicBool(Toggle toggle, SettingsPanelController ctrl, string method)
    {
        var mi     = ctrl.GetType().GetMethod(method,
                         System.Reflection.BindingFlags.Public |
                         System.Reflection.BindingFlags.Instance);
        var action = (UnityEngine.Events.UnityAction<bool>)
                         System.Delegate.CreateDelegate(
                             typeof(UnityEngine.Events.UnityAction<bool>), ctrl, mi);
        UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(
            toggle.onValueChanged, action, false);
        PatchLastListenerToEventDefined(new SerializedObject(toggle), "m_OnValueChanged");
    }

    static void WireVoidButton(Button button, SettingsPanelController ctrl, string method)
    {
        var mi     = ctrl.GetType().GetMethod(method,
                         System.Reflection.BindingFlags.Public |
                         System.Reflection.BindingFlags.Instance);
        var action = (UnityEngine.Events.UnityAction)
                         System.Delegate.CreateDelegate(
                             typeof(UnityEngine.Events.UnityAction), ctrl, mi);
        // Void listener is already mode=Void; no further patching needed
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
            button.onClick, action);
    }

    /// <summary>
    /// Changes the last persistent listener in the named event to m_Mode=0 (EventDefined),
    /// so it receives the event's own runtime parameter rather than the fixed stored value.
    /// Must be called AFTER UnityEventTools has added the listener (so FindProperty succeeds).
    /// </summary>
    static void PatchLastListenerToEventDefined(SerializedObject so, string eventPropPath)
    {
        var calls = so.FindProperty($"{eventPropPath}.m_PersistentCalls.m_Calls");
        if (calls == null || calls.arraySize == 0)
        {
            Debug.LogWarning($"[BuildSettingsPanelUI] Could not find '{eventPropPath}' on {so.targetObject}");
            return;
        }
        var last = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        last.FindPropertyRelative("m_Mode").intValue = 0; // PersistentListenerMode.EventDefined
        so.ApplyModifiedProperties();
    }
}
