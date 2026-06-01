using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menu: Tools > UnMerry > Build Settings Prefab
///
/// Creates Assets/Prefabs/UI/SettingsPageContent.prefab — a fully styled,
/// fully wired settings panel that matches the book UI style. The prefab
/// uses sub-page navigation matching the MainMenu's Video/Audio/Controls layout:
///
///   Page_Navigation  — three category nav buttons (same style as MainMenu)
///   Page_Audio       — Music / SFX / Dialogue sliders
///   Page_Display     — Highlight Objects + Puzzle Hints toggles
///   Page_Controls    — rebind rows (Controls_Container) + Reset All
///
/// After creating the prefab, use the second menu item to place it into the
/// active scene's Page_Content_Settings (works for both MainMenu and BackYard).
///
/// Font used: the book font (GUID 05f1775dfe1cd6b44ae9053c5f6d8b29) — same as
/// the MainMenu navigation buttons. Falls back to any TMP font in the project.
/// </summary>
public static class BuildSettingsPrefab
{
    private const string PrefabPath = "Assets/Prefabs/UI/SettingsPageContent.prefab";

    // ── Book font GUID (Acme-Regular SDF or equivalent used by MainMenu) ─────
    private const string BookFontGuid = "05f1775dfe1cd6b44ae9053c5f6d8b29";

    // ── Book colour palette (extracted from MainMenu TMP components) ──────────
    private static readonly Color InkBlack   = Color.black;                              // text normal
    private static readonly Color InkNormal  = new Color(0.079f, 0.079f, 0.079f, 1f);   // ButtonTextColor _normalColor
    private static readonly Color InkOrange  = new Color(1f, 0.315f, 0f, 1f);            // hover
    private static readonly Color InkDarkRed = new Color(0.238f, 0.039f, 0.039f, 1f);   // pressed
    private static readonly Color InkGrey    = new Color(0.30f, 0.22f, 0.10f, 1f);      // section headers

    // ──────────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/UnMerry/Build Settings Prefab")]
    public static void CreatePrefab()
    {
        EnsureFolder(Path.GetDirectoryName(PrefabPath));

        TMP_FontAsset font = LoadBookFont();

        // ── Temp Canvas (required so RectTransforms initialise correctly) ─────
        var canvasGo = new GameObject("__TempCanvas__");
        var canvas   = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // ── Root (fills Page_Content_Settings = entire BookPanel) ────────────
        var rootGo = new GameObject("SettingsPageContent", typeof(RectTransform));
        rootGo.transform.SetParent(canvasGo.transform, false);
        StretchFill(rootGo.GetComponent<RectTransform>());

        var ctrl = rootGo.AddComponent<SettingsPanelController>();

        // ── Left-page panel — constrains content to the left page only ────────
        // Page_Content_Settings fills the full BookPanel (both pages).
        // The bookmarks (Play / Settings / Credits) are siblings in the same canvas
        // and visually overlap the leftmost ~100 px of the page.  The MainMenu's
        // own buttons start at x≈390 from the left edge of Page_Content_Settings,
        // confirming the usable writing area begins well right of the left edge.
        // We mirror that: start at x=110 (clears the bookmarks) and give 440 px
        // of width (stops comfortably before the centre spine).
        var leftPanelGo = new GameObject("LeftPagePanel", typeof(RectTransform));
        leftPanelGo.transform.SetParent(rootGo.transform, false);
        var leftPanelRt = leftPanelGo.GetComponent<RectTransform>();
        leftPanelRt.anchorMin        = new Vector2(0f, 0f);
        leftPanelRt.anchorMax        = new Vector2(0f, 1f);   // left-edge anchored, full height
        leftPanelRt.pivot            = new Vector2(0f, 0.5f);
        leftPanelRt.anchoredPosition = new Vector2(110f, 0f); // clears the bookmark tabs
        leftPanelRt.sizeDelta        = new Vector2(440f, 0f); // stays left of the spine

        // ── Pages: each page GO is used for show/hide; items go into its Content child ──
        // MakePage returns the Content transform (top-anchored, ContentSizeFitter+VLG)
        // so items are always compactly packed from the top regardless of page height.
        var navContent  = MakePage(leftPanelGo.transform, "Page_Navigation", true,  out var navPage,  font);
        var audioContent= MakePage(leftPanelGo.transform, "Page_Audio",      false, out var audioPage, font);
        var dispContent = MakePage(leftPanelGo.transform, "Page_Display",    false, out var dispPage,  font);
        var videoContent= MakePage(leftPanelGo.transform, "Page_Video",      false, out var videoPage, font);
        var ctrlContent = MakePage(leftPanelGo.transform, "Page_Controls",   false, out var ctrlPage,  font);

        // ── Page_Navigation content ───────────────────────────────────────────
        Button audioNavBtn   = MakeNavButton(navContent, "Btn_AudioSettings",   "AUDIO SETTINGS",  font, fontSize: 60f);
        Button displayNavBtn = MakeNavButton(navContent, "Btn_DisplaySettings", "DISPLAY & HINTS", font, fontSize: 60f);
        Button videoNavBtn   = MakeNavButton(navContent, "Btn_VideoSettings",   "VIDEO",           font, fontSize: 60f);
        Button controlsNavBtn= MakeNavButton(navContent, "Btn_Controls",        "CONTROLS",        font, fontSize: 60f);

        // ── Page_Audio content ────────────────────────────────────────────────
        MakeBackButton(audioContent, "Btn_Back_Audio", font);
        MakeSectionHeader(audioContent, "AUDIO", font);
        Slider musicSlider    = MakeSliderRow(audioContent, "Music",    font);
        Slider sfxSlider      = MakeSliderRow(audioContent, "SFX",      font);
        Slider dialogueSlider = MakeSliderRow(audioContent, "Dialogue", font);

        // ── Page_Display content ──────────────────────────────────────────────
        MakeBackButton(dispContent, "Btn_Back_Display", font);
        MakeSectionHeader(dispContent, "DISPLAY & HINTS", font);
        Toggle highlightToggle = MakeToggleRow(dispContent, "Highlight Objects", font);
        Toggle hintsToggle     = MakeToggleRow(dispContent, "Puzzle Hints",      font);

        // ── Page_Video content ────────────────────────────────────────────────
        MakeBackButton(videoContent, "Btn_Back_Video", font);
        MakeSectionHeader(videoContent, "VIDEO", font);
        Slider gammaSlider = MakeSliderRow(videoContent, "Gamma", font);
        gammaSlider.minValue = AccessibilityManager.MinGamma;
        gammaSlider.maxValue = AccessibilityManager.MaxGamma;
        gammaSlider.value    = AccessibilityManager.DefaultGamma;
        Button colorblindBtn = MakeCycleRow(
            videoContent, "Colourblind", "Off", font, out TMP_Text colorblindValue);

        // ── Page_Controls content ─────────────────────────────────────────────
        MakeBackButton(ctrlContent, "Btn_Back_Controls", font);
        MakeSectionHeader(ctrlContent, "CONTROLS", font);
        var controlsContainer  = MakeControlsContainer(ctrlContent);
        Button resetBtn        = MakeNavButton(ctrlContent, "Btn_ResetAll", "RESET ALL", font, fontSize: 50f);

        // ── Wire SettingsPanelController fields ───────────────────────────────
        var so = new SerializedObject(ctrl);
        so.FindProperty("_musicSlider")      .objectReferenceValue = musicSlider;
        so.FindProperty("_sfxSlider")        .objectReferenceValue = sfxSlider;
        so.FindProperty("_dialogueSlider")   .objectReferenceValue = dialogueSlider;
        so.FindProperty("_highlightToggle")  .objectReferenceValue = highlightToggle;
        so.FindProperty("_hintsToggle")      .objectReferenceValue = hintsToggle;
        so.FindProperty("_gammaSlider")         .objectReferenceValue = gammaSlider;
        so.FindProperty("_colorblindValueLabel").objectReferenceValue = colorblindValue;
        so.FindProperty("_navigationPage")   .objectReferenceValue = navPage;   // PAGE GOs for show/hide
        so.FindProperty("_audioPage")        .objectReferenceValue = audioPage;
        so.FindProperty("_displayPage")      .objectReferenceValue = dispPage;
        so.FindProperty("_videoPage")        .objectReferenceValue = videoPage;
        so.FindProperty("_controlsPage")     .objectReferenceValue = ctrlPage;
        so.ApplyModifiedProperties();

        // ── Wire navigation buttons ───────────────────────────────────────────
        WireVoidButton(audioNavBtn,    ctrl, "ShowAudioPage");
        WireVoidButton(displayNavBtn,  ctrl, "ShowDisplayPage");
        WireVoidButton(videoNavBtn,    ctrl, "ShowVideoPage");
        WireVoidButton(controlsNavBtn, ctrl, "ShowControlsPage");

        // ── Wire Back buttons (first Button in each content area) ─────────────
        WireVoidButton(audioContent.GetComponentInChildren<Button>(), ctrl, "ShowNavigation");
        WireVoidButton(dispContent.GetComponentInChildren<Button>(),  ctrl, "ShowNavigation");
        WireVoidButton(videoContent.GetComponentInChildren<Button>(), ctrl, "ShowNavigation");
        WireVoidButton(ctrlContent.GetComponentInChildren<Button>(),  ctrl, "ShowNavigation");

        // ── Wire sliders (dynamic float) ──────────────────────────────────────
        WireDynamicFloat(musicSlider,    ctrl, "OnMusicSliderChanged");
        WireDynamicFloat(sfxSlider,      ctrl, "OnSfxSliderChanged");
        WireDynamicFloat(dialogueSlider, ctrl, "OnDialogueSliderChanged");

        // ── Wire toggles (dynamic bool) ───────────────────────────────────────
        WireDynamicBool(highlightToggle, ctrl, "OnHighlightToggleChanged");
        WireDynamicBool(hintsToggle,     ctrl, "OnHintsToggleChanged");

        // ── Wire video controls ───────────────────────────────────────────────
        WireDynamicFloat(gammaSlider,   ctrl, "OnGammaSliderChanged");
        WireVoidButton  (colorblindBtn, ctrl, "OnColorblindCyclePressed");

        // ── Wire Reset All ────────────────────────────────────────────────────
        WireVoidButton(resetBtn, ctrl, "OnResetAllPressed");

        // ── Save prefab ───────────────────────────────────────────────────────
        PrefabUtility.SaveAsPrefabAsset(rootGo, PrefabPath, out bool success);
        Object.DestroyImmediate(canvasGo);

        if (success)
        {
            Debug.Log($"[BuildSettingsPrefab] Saved '{PrefabPath}'.");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
        }
        else
        {
            Debug.LogError($"[BuildSettingsPrefab] Failed to save '{PrefabPath}'.");
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/UnMerry/Place Settings Prefab in Active Scene")]
    public static void PlaceInActiveScene()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Place Settings Prefab",
                $"Prefab not found at '{PrefabPath}'.\nRun 'Build Settings Prefab' first.", "OK");
            return;
        }

        var target = GameObject.Find("Page_Content_Settings");
        if (target == null)
        {
            EditorUtility.DisplayDialog("Place Settings Prefab",
                "'Page_Content_Settings' not found in active scene.\n" +
                "Open MainMenu or BackYardScenario first.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Place Settings Prefab");
        int group = Undo.GetCurrentGroup();

        // Clear existing children
        for (int i = target.transform.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(target.transform.GetChild(i).gameObject);

        // Remove any existing SettingsPanelController / VerticalLayoutGroup on the target
        var existingCtrl = target.GetComponent<SettingsPanelController>();
        if (existingCtrl != null) Undo.DestroyObjectImmediate(existingCtrl);
        var existingVlg = target.GetComponent<VerticalLayoutGroup>();
        if (existingVlg != null) Undo.DestroyObjectImmediate(existingVlg);

        // Instantiate prefab as child
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, target.transform);
        Undo.RegisterCreatedObjectUndo(instance, "place prefab");
        StretchFill(instance.GetComponent<RectTransform>());

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[BuildSettingsPrefab] Placed prefab instance in '{target.name}' in active scene.");
        Debug.Log("[BuildSettingsPrefab] Next: open 'Tools > UnMerry > Build Rebind UI' and point it at the Controls_Container inside the prefab instance.");
    }

    // ── Page factory ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a page GO (StretchFill, used only for show/hide) and inside it a
    /// top-anchored ContentArea with ContentSizeFitter + VLG.  Items must be parented
    /// to the returned Transform (ContentArea), NOT to the page GO itself.
    /// The page GO is returned via <paramref name="pageGo"/> for SettingsPanelController wiring.
    /// </summary>
    static Transform MakePage(Transform parent, string name, bool activeByDefault,
                               out GameObject pageGo, TMP_FontAsset font)
    {
        // Page: fills left panel, activated/deactivated by SettingsPanelController
        pageGo = new GameObject(name, typeof(RectTransform));
        pageGo.transform.SetParent(parent, false);
        StretchFill(pageGo.GetComponent<RectTransform>());
        pageGo.SetActive(activeByDefault);

        // ContentArea: top-anchored, grows downward as items are added.
        // ContentSizeFitter ensures the rect is exactly as tall as the content —
        // this is the key difference from the old VLG-on-StretchFill approach,
        // which caused Unity to distribute leftover height between items.
        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(pageGo.transform, false);
        var rt = contentGo.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);   // top-left of page
        rt.anchorMax        = new Vector2(1f, 1f);   // top-right of page
        rt.pivot            = new Vector2(0.5f, 1f); // pivot at top centre
        rt.anchoredPosition = new Vector2(0f, -120f); // 120 px from top of page — matches Play page
        rt.sizeDelta        = new Vector2(0f, 0f);   // height driven by ContentSizeFitter

        contentGo.AddComponent<ContentSizeFitter>().verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing                = 12;
        vlg.padding                = new RectOffset(0, 0, 0, 0);
        vlg.childAlignment         = TextAnchor.UpperLeft;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;

        return contentGo.transform;
    }

    // ── Navigation / action button (exact MainMenu style: 60px bold+underline black) ──

    static Button MakeNavButton(Transform parent, string goName, string label, TMP_FontAsset font,
                                 float fontSize = 60f)
    {
        var go = new GameObject(goName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 80;
        le.flexibleWidth   = 1;

        // Unity Button with no transition — ButtonTextColor drives colour changes
        var btn = go.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;

        // TMP label fills the button
        var lblGo = new GameObject("Text", typeof(RectTransform));
        lblGo.transform.SetParent(go.transform, false);
        var lblRt = lblGo.GetComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.sizeDelta = Vector2.zero;

        var tmp = lblGo.AddComponent<TextMeshProUGUI>();
        tmp.text                 = label;
        tmp.enableAutoSizing     = true;   // fills up to fontSize, shrinks to fit single line
        tmp.fontSizeMin          = 32f;
        tmp.fontSizeMax          = fontSize;
        tmp.enableWordWrapping   = false;  // never wrap — shrink font instead
        tmp.fontStyle            = FontStyles.Bold | FontStyles.Underline;
        tmp.color                = InkBlack;
        tmp.alignment            = TextAlignmentOptions.Left;
        if (font != null) tmp.font = font;

        // ButtonTextColor — identical colours to MainMenu buttons
        var btc = go.AddComponent<ButtonTextColor>();
        var btcSo = new SerializedObject(btc);
        btcSo.FindProperty("_text")         .objectReferenceValue = tmp;
        btcSo.FindProperty("_normalColor")  .colorValue = InkNormal;
        btcSo.FindProperty("_hoverColor")   .colorValue = InkOrange;
        btcSo.FindProperty("_pressedColor") .colorValue = InkDarkRed;
        btcSo.ApplyModifiedProperties();

        return btn;
    }

    // ── Back button — same style as nav buttons but slightly smaller ─────────

    static void MakeBackButton(Transform parent, string goName, TMP_FontAsset font)
    {
        // Reuses MakeNavButton logic at a reduced size (40px) with "← BACK" label
        var go = new GameObject(goName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 48;
        le.flexibleWidth   = 1;

        var btn = go.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;

        var lblGo = new GameObject("Text", typeof(RectTransform));
        lblGo.transform.SetParent(go.transform, false);
        var lblRt = lblGo.GetComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.sizeDelta = Vector2.zero;

        var tmp = lblGo.AddComponent<TextMeshProUGUI>();
        tmp.text      = "← BACK";
        tmp.fontSize  = 32;
        tmp.fontStyle = FontStyles.Bold | FontStyles.Underline;
        tmp.color     = InkBlack;
        tmp.alignment = TextAlignmentOptions.Left;
        if (font != null) tmp.font = font;

        var btc = go.AddComponent<ButtonTextColor>();
        var btcSo = new SerializedObject(btc);
        btcSo.FindProperty("_text")         .objectReferenceValue = tmp;
        btcSo.FindProperty("_normalColor")  .colorValue = InkNormal;
        btcSo.FindProperty("_hoverColor")   .colorValue = InkOrange;
        btcSo.FindProperty("_pressedColor") .colorValue = InkDarkRed;
        btcSo.ApplyModifiedProperties();
    }

    // ── Section header — book font, bold+underline, same colour as body text ──

    static void MakeSectionHeader(Transform parent, string text, TMP_FontAsset font)
    {
        var go = new GameObject($"Header_{text}", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 52;
        le.flexibleWidth   = 1; // headers span the full content width

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 36;
        tmp.fontStyle = FontStyles.Bold | FontStyles.Underline;
        tmp.color     = InkBlack;
        tmp.alignment = TextAlignmentOptions.Left;
        if (font != null) tmp.font = font;

        // Subtle breathing room between the tab name and the first setting below it.
        MakeSpacer(parent, 16f);
    }

    // ── Spacer (fixed-height layout gap) ────────────────────────────────────

    static void MakeSpacer(Transform parent, float height)
    {
        var go = new GameObject("Spacer", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.flexibleWidth   = 1;
    }

    // ── Slider row ────────────────────────────────────────────────────────────

    static Slider MakeSliderRow(Transform parent, string label, TMP_FontAsset font)
    {
        var row = new GameObject($"Row_{label.Replace(" ", "_")}", typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 60;
        le.flexibleWidth   = 1;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing                = 12;
        hlg.childAlignment         = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;

        // Label
        var lblGo  = new GameObject("Label", typeof(RectTransform));
        lblGo.transform.SetParent(row.transform, false);
        var lblLe  = lblGo.AddComponent<LayoutElement>();
        lblLe.minWidth       = 110;
        lblLe.preferredWidth = 110;
        lblLe.flexibleWidth  = 0;
        var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
        lblTmp.text      = label;
        lblTmp.fontSize  = 38;
        lblTmp.fontStyle = FontStyles.Bold;
        lblTmp.color     = InkBlack;
        lblTmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (font != null) lblTmp.font = font;

        // Slider
        var sliderGo = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sliderGo.name = $"Slider_{label}";
        sliderGo.transform.SetParent(row.transform, false);
        var sliderLe         = sliderGo.AddComponent<LayoutElement>();
        sliderLe.flexibleWidth   = 1;
        sliderLe.preferredHeight = 32;
        sliderLe.minWidth        = 80;   // never collapses
        var slider       = sliderGo.GetComponent<Slider>();
        slider.minValue  = 0f;
        slider.maxValue  = 1f;
        slider.value     = 1f;
        slider.wholeNumbers = false;

        // ── Restyle as a thin ink-line track with a small dark handle ────────
        // Background track: thin, ink-dark
        var bgTf = sliderGo.transform.Find("Background")?.GetComponent<RectTransform>();
        if (bgTf != null)
        {
            // Collapse height to a 4 px horizontal line centred in the row
            bgTf.anchorMin = new Vector2(0f, 0.5f);
            bgTf.anchorMax = new Vector2(1f, 0.5f);
            bgTf.sizeDelta = new Vector2(0f, 4f);
            var img = bgTf.GetComponent<Image>();
            if (img != null) img.color = new Color(0.20f, 0.13f, 0.05f, 0.35f); // faint ink track
        }

        // Fill area: same thin band
        var fillAreaTf = sliderGo.transform.Find("Fill Area")?.GetComponent<RectTransform>();
        if (fillAreaTf != null)
        {
            fillAreaTf.anchorMin = new Vector2(0f, 0.5f);
            fillAreaTf.anchorMax = new Vector2(1f, 0.5f);
            fillAreaTf.offsetMin = new Vector2(0f, -2f);
            fillAreaTf.offsetMax = new Vector2(-10f, 2f);
        }
        var fillTf = sliderGo.transform.Find("Fill Area/Fill")?.GetComponent<RectTransform>();
        if (fillTf != null)
        {
            var img = fillTf.GetComponent<Image>();
            if (img != null) img.color = new Color(0.20f, 0.13f, 0.05f, 0.85f); // solid ink fill
        }

        // Handle: fixed 20x20 circle, centre-anchored (avoids inheriting row height)
        var handleTf = sliderGo.transform.Find("Handle Slide Area/Handle")?.GetComponent<RectTransform>();
        if (handleTf != null)
        {
            handleTf.anchorMin = new Vector2(0.5f, 0.5f);
            handleTf.anchorMax = new Vector2(0.5f, 0.5f);
            handleTf.pivot     = new Vector2(0.5f, 0.5f);
            handleTf.sizeDelta = new Vector2(20f, 20f);
            var img = handleTf.GetComponent<Image>();
            if (img != null) img.color = new Color(0.20f, 0.13f, 0.05f, 1f);
        }

        // Handle Slide Area: constrain to same thin band as the track
        var handleAreaTf = sliderGo.transform.Find("Handle Slide Area")?.GetComponent<RectTransform>();
        if (handleAreaTf != null)
        {
            handleAreaTf.anchorMin = new Vector2(0f, 0.5f);
            handleAreaTf.anchorMax = new Vector2(1f, 0.5f);
            handleAreaTf.sizeDelta = new Vector2(-20f, 20f); // leave 10 px each side for handle overhang
        }

        // ── Percentage label ──────────────────────────────────────────────────
        var pctGo  = new GameObject("Percentage", typeof(RectTransform));
        pctGo.transform.SetParent(row.transform, false);
        var pctLe  = pctGo.AddComponent<LayoutElement>();
        pctLe.minWidth       = 58;
        pctLe.preferredWidth = 58;
        pctLe.flexibleWidth  = 0;
        var pctTmp = pctGo.AddComponent<TextMeshProUGUI>();
        pctTmp.text      = "100%";
        pctTmp.fontSize  = 26;
        pctTmp.fontStyle = FontStyles.Bold;
        pctTmp.color     = InkBlack;
        pctTmp.alignment = TextAlignmentOptions.MidlineRight;
        if (font != null) pctTmp.font = font;

        // Wire SliderPercentageLabel
        var pctComp = sliderGo.AddComponent<SliderPercentageLabel>();
        var pctSo   = new SerializedObject(pctComp);
        pctSo.FindProperty("_label").objectReferenceValue = pctTmp;
        pctSo.ApplyModifiedProperties();

        return slider;
    }

    // ── Toggle row ────────────────────────────────────────────────────────────

    static Toggle MakeToggleRow(Transform parent, string label, TMP_FontAsset font)
    {
        var row = new GameObject($"Row_{label.Replace(" ", "_")}", typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var rowLe = row.AddComponent<LayoutElement>();
        rowLe.preferredHeight = 52;
        rowLe.flexibleWidth   = 1; // slider rows span full width
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing                = 12;
        hlg.childAlignment         = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;   // must be true so HLG actually sets widths
        hlg.childControlHeight     = true;

        // Toggle box (fixed 36 px wide, never grows)
        var toggleGo = DefaultControls.CreateToggle(new DefaultControls.Resources());
        toggleGo.name = $"Toggle_{label.Replace(" ", "_")}";
        toggleGo.transform.SetParent(row.transform, false);
        var toggleLe         = toggleGo.AddComponent<LayoutElement>();
        toggleLe.minWidth        = 36;
        toggleLe.preferredWidth  = 36;
        toggleLe.flexibleWidth   = 0;   // never expand
        toggleLe.minHeight       = 36;
        toggleLe.preferredHeight = 36;

        // Hide the default Text child — we'll use our own label
        var defaultText = toggleGo.GetComponentInChildren<Text>();
        if (defaultText != null) defaultText.gameObject.SetActive(false);

        var toggle  = toggleGo.GetComponent<Toggle>();
        toggle.isOn = true;

        // Tint checkmark to ink colour
        var checkmark = toggleGo.transform.Find("Background/Checkmark");
        if (checkmark != null)
        {
            var img = checkmark.GetComponent<Image>();
            if (img != null) img.color = InkBlack;
        }

        // TMP label
        var lblGo  = new GameObject("Label", typeof(RectTransform));
        lblGo.transform.SetParent(row.transform, false);
        var lblLe  = lblGo.AddComponent<LayoutElement>();
        lblLe.flexibleWidth = 1;
        var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
        lblTmp.text      = label;
        lblTmp.fontSize  = 38;
        lblTmp.fontStyle = FontStyles.Bold;
        lblTmp.color     = InkBlack;
        lblTmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (font != null) lblTmp.font = font;

        return toggle;
    }

    // ── Cycle-selector row ──────────────────────────────────────────────────────
    // A label on the left + a book-styled value button that advances through the
    // options on click. Matches the underlined ink nav-button aesthetic rather than
    // introducing a foreign default-skinned dropdown. Returns the Button (wire its
    // onClick) and outputs the value TMP_Text (assign to the controller).

    static Button MakeCycleRow(Transform parent, string label, string initialValue,
                                TMP_FontAsset font, out TMP_Text valueText)
    {
        var row = new GameObject($"Row_{label.Replace(" ", "_")}", typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 60;
        le.flexibleWidth   = 1;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing                = 12;
        hlg.childAlignment         = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;

        // Label (matches slider-row label styling)
        var lblGo  = new GameObject("Label", typeof(RectTransform));
        lblGo.transform.SetParent(row.transform, false);
        var lblLe  = lblGo.AddComponent<LayoutElement>();
        lblLe.minWidth       = 200;
        lblLe.preferredWidth = 200;
        lblLe.flexibleWidth  = 0;
        var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
        lblTmp.text               = label;
        lblTmp.enableAutoSizing    = true;   // shrink to fit the label column, never overflow
        lblTmp.fontSizeMin         = 24f;
        lblTmp.fontSizeMax         = 38f;
        lblTmp.enableWordWrapping  = false;
        lblTmp.fontStyle           = FontStyles.Bold;
        lblTmp.color               = InkBlack;
        lblTmp.alignment           = TextAlignmentOptions.MidlineLeft;
        if (font != null) lblTmp.font = font;

        // Value button (book style: bold + underline ink, hover/press via ButtonTextColor)
        var btnGo = new GameObject($"Btn_{label.Replace(" ", "_")}", typeof(RectTransform));
        btnGo.transform.SetParent(row.transform, false);
        var btnLe = btnGo.AddComponent<LayoutElement>();
        btnLe.flexibleWidth = 1;
        var btn = btnGo.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;

        var valGo = new GameObject("Text", typeof(RectTransform));
        valGo.transform.SetParent(btnGo.transform, false);
        var valRt = valGo.GetComponent<RectTransform>();
        valRt.anchorMin = Vector2.zero;
        valRt.anchorMax = Vector2.one;
        valRt.sizeDelta = Vector2.zero;

        var valTmp = valGo.AddComponent<TextMeshProUGUI>();
        valTmp.text               = initialValue;
        valTmp.enableAutoSizing    = true;   // long names (e.g. Deuteranopia) shrink instead of overlapping
        valTmp.fontSizeMin         = 24f;
        valTmp.fontSizeMax         = 38f;
        valTmp.enableWordWrapping  = false;
        valTmp.fontStyle           = FontStyles.Bold | FontStyles.Underline;
        valTmp.color               = InkBlack;
        valTmp.alignment           = TextAlignmentOptions.MidlineRight;  // value sits at the right, like slider %
        if (font != null) valTmp.font = font;

        var btc   = btnGo.AddComponent<ButtonTextColor>();
        var btcSo = new SerializedObject(btc);
        btcSo.FindProperty("_text")        .objectReferenceValue = valTmp;
        btcSo.FindProperty("_normalColor") .colorValue = InkNormal;
        btcSo.FindProperty("_hoverColor")  .colorValue = InkOrange;
        btcSo.FindProperty("_pressedColor").colorValue = InkDarkRed;
        btcSo.ApplyModifiedProperties();

        valueText = valTmp;
        return btn;
    }

    // ── Controls container ────────────────────────────────────────────────────

    static GameObject MakeControlsContainer(Transform parent)
    {
        // ── Scroll wrapper — fixed height, rows scroll vertically inside ──────
        var scrollGo = new GameObject("ControlsScroll", typeof(RectTransform));
        scrollGo.transform.SetParent(parent, false);
        var scrollLe = scrollGo.AddComponent<LayoutElement>();
        scrollLe.preferredHeight = 380;   // fixed: fits ~8 rows, Reset All stays visible
        scrollLe.flexibleWidth   = 1;

        var scrollRect = scrollGo.AddComponent<ScrollRect>();
        scrollRect.horizontal        = false;
        scrollRect.vertical          = true;
        scrollRect.scrollSensitivity = 30f;
        scrollRect.movementType      = ScrollRect.MovementType.Clamped;

        // ── Viewport ──────────────────────────────────────────────────────────
        var vpGo = new GameObject("Viewport", typeof(RectTransform));
        vpGo.transform.SetParent(scrollGo.transform, false);
        StretchFill(vpGo.GetComponent<RectTransform>());
        vpGo.AddComponent<RectMask2D>();
        scrollRect.viewport = vpGo.GetComponent<RectTransform>();

        // ── Controls_Container (content that grows as rebind rows are added) ──
        var contentGo = new GameObject("Controls_Container", typeof(RectTransform));
        contentGo.transform.SetParent(vpGo.transform, false);
        var contentRt   = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);  // top-anchored, grows downward
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot     = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = Vector2.zero;

        var csf = contentGo.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing                = 4;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;

        scrollRect.content = contentRt;

        // Return Controls_Container so RebindRowSetupTool can populate it
        return contentGo;
    }

    // ── Event wiring helpers ──────────────────────────────────────────────────

    static void WireVoidButton(Button btn, SettingsPanelController ctrl, string method)
    {
        if (btn == null) return;
        var mi     = typeof(SettingsPanelController).GetMethod(method,
                         BindingFlags.Public | BindingFlags.Instance);
        var action = (UnityAction)System.Delegate.CreateDelegate(
                         typeof(UnityAction), ctrl, mi);
        UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
    }

    static void WireDynamicFloat(Slider slider, SettingsPanelController ctrl, string method)
    {
        var mi     = typeof(SettingsPanelController).GetMethod(method,
                         BindingFlags.Public | BindingFlags.Instance);
        var action = (UnityAction<float>)System.Delegate.CreateDelegate(
                         typeof(UnityAction<float>), ctrl, mi);
        UnityEventTools.AddFloatPersistentListener(slider.onValueChanged, action, 0f);
        PatchLastListenerToEventDefined(new SerializedObject(slider), "m_OnValueChanged");
    }

    static void WireDynamicBool(Toggle toggle, SettingsPanelController ctrl, string method)
    {
        var mi     = typeof(SettingsPanelController).GetMethod(method,
                         BindingFlags.Public | BindingFlags.Instance);
        var action = (UnityAction<bool>)System.Delegate.CreateDelegate(
                         typeof(UnityAction<bool>), ctrl, mi);
        UnityEventTools.AddBoolPersistentListener(toggle.onValueChanged, action, false);
        PatchLastListenerToEventDefined(new SerializedObject(toggle), "m_OnValueChanged");
    }

    static void PatchLastListenerToEventDefined(SerializedObject so, string eventPropPath)
    {
        var calls = so.FindProperty($"{eventPropPath}.m_PersistentCalls.m_Calls");
        if (calls == null || calls.arraySize == 0)
        {
            Debug.LogWarning($"[BuildSettingsPrefab] Could not patch '{eventPropPath}' on {so.targetObject}");
            return;
        }
        var last = calls.GetArrayElementAtIndex(calls.arraySize - 1);
        last.FindPropertyRelative("m_Mode").intValue = 0; // EventDefined
        so.ApplyModifiedProperties();
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    static TMP_FontAsset LoadBookFont()
    {
        // Try exact book font by GUID first
        string path = AssetDatabase.GUIDToAssetPath(BookFontGuid);
        if (!string.IsNullOrEmpty(path))
        {
            var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (f != null) return f;
        }
        // Fallback: first font in project
        foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
        {
            var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                        AssetDatabase.GUIDToAssetPath(guid));
            if (f != null) return f;
        }
        return null;
    }

    static void StretchFill(RectTransform rt)
    {
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = Vector2.zero;
    }

    static void EnsureFolder(string folderPath)
    {
        string[] parts   = folderPath.Split('/');
        string   current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
