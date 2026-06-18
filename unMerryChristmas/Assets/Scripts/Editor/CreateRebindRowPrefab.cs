using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Menu: Tools > UnMerry > Create RebindActionRow Prefab
///
/// Creates Assets/Prefabs/UI/RebindActionRow.prefab styled to match the book UI:
///   [ Action name (flex, left) ]   [ Keyboard key button ]   [ Gamepad button ]
///
/// The binding key is shown ON each button (book-style bold + underline ink text),
/// and clicking the button rebinds that slot. There is no separate "current binding"
/// label or empty change-button — the key label and the rebind button are the same
/// control. Empty slots are hidden by RebindActionRow at runtime.
/// Safe to re-run: overwrites the existing prefab.
/// </summary>
public static class CreateRebindRowPrefab
{
    private const string PrefabPath    = "Assets/Prefabs/UI/RebindActionRow.prefab";
    private const string BookFontGuid  = "05f1775dfe1cd6b44ae9053c5f6d8b29";

    // Book colour palette (matches BuildSettingsPrefab)
    private static readonly Color InkBlack   = Color.black;
    private static readonly Color InkNormal  = new Color(0.079f, 0.079f, 0.079f, 1f);
    private static readonly Color InkOrange  = new Color(1f, 0.315f, 0f, 1f);
    private static readonly Color InkDarkRed = new Color(0.238f, 0.039f, 0.039f, 1f);

    [MenuItem("Tools/UnMerry/Create RebindActionRow Prefab")]
    public static void Create()
    {
        EnsureFolder(Path.GetDirectoryName(PrefabPath));

        var canvasGo = new GameObject("__TempCanvas__");
        canvasGo.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        TMP_FontAsset font = LoadBookFont();

        // ── Root row ───────────────────────────────────────────────────────
        var rowGo = new GameObject("RebindActionRow", typeof(RectTransform));
        rowGo.transform.SetParent(canvasGo.transform, false);
        rowGo.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 44);

        var le = rowGo.AddComponent<LayoutElement>();
        le.preferredHeight = 44;
        le.flexibleWidth   = 1;

        var hlg = rowGo.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing                = 8;
        hlg.padding                = new RectOffset(0, 0, 2, 2);
        hlg.childAlignment         = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;

        var row = rowGo.AddComponent<RebindActionRow>();

        // ── Action name ────────────────────────────────────────────────────
        var nameGo = new GameObject("ActionName", typeof(RectTransform));
        nameGo.transform.SetParent(rowGo.transform, false);
        var nameLe     = nameGo.AddComponent<LayoutElement>();
        nameLe.flexibleWidth = 1;
        nameLe.minWidth      = 140;
        var nameTmp    = nameGo.AddComponent<TextMeshProUGUI>();
        nameTmp.text      = "Action";
        nameTmp.fontSize  = 26;
        nameTmp.fontStyle = FontStyles.Bold;
        nameTmp.color     = InkBlack;
        nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (font != null) nameTmp.font = font;

        // ── Keyboard + Gamepad key buttons ─────────────────────────────────
        Button kbBtn = MakeKeyButton(rowGo.transform, "KeyboardButton", "W",   140f, font, out TMP_Text kbLabel);
        Button gpBtn = MakeKeyButton(rowGo.transform, "GamepadButton",  "B-S", 160f, font, out TMP_Text gpLabel);

        // ── Wire RebindActionRow fields ────────────────────────────────────
        var so = new SerializedObject(row);
        so.FindProperty("_actionNameLabel")    .objectReferenceValue = nameTmp;
        so.FindProperty("_keyboardButton")     .objectReferenceValue = kbBtn;
        so.FindProperty("_keyboardLabel")      .objectReferenceValue = kbLabel;
        so.FindProperty("_gamepadButton")      .objectReferenceValue = gpBtn;
        so.FindProperty("_gamepadLabel")       .objectReferenceValue = gpLabel;
        so.ApplyModifiedProperties();

        // ── Save ───────────────────────────────────────────────────────────
        PrefabUtility.SaveAsPrefabAsset(rowGo, PrefabPath, out bool success);
        Object.DestroyImmediate(canvasGo);

        if (success)
        {
            Debug.Log($"[CreateRebindRowPrefab] Prefab saved to '{PrefabPath}'.");
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
        }
        else
        {
            Debug.LogError($"[CreateRebindRowPrefab] Failed to save prefab at '{PrefabPath}'.");
        }
    }

    // A book-styled key button: bold+underline ink text (the binding) on a
    // transparent button, with ButtonTextColor hover/press feedback. The text
    // auto-sizes so long names (e.g. "Left Stick Press") shrink instead of clipping.
    static Button MakeKeyButton(Transform parent, string goName, string initial,
                                float width, TMP_FontAsset font, out TMP_Text label)
    {
        var btnGo = new GameObject(goName, typeof(RectTransform));
        btnGo.transform.SetParent(parent, false);
        var btnLe = btnGo.AddComponent<LayoutElement>();
        btnLe.minWidth       = width;
        btnLe.preferredWidth = width;
        btnLe.flexibleWidth  = 0;

        var btn = btnGo.AddComponent<Button>();
        btn.transition = Selectable.Transition.None; // ButtonTextColor drives colour

        var lblGo = new GameObject("Text", typeof(RectTransform));
        lblGo.transform.SetParent(btnGo.transform, false);
        var lblRt = lblGo.GetComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.sizeDelta = Vector2.zero;

        label = lblGo.AddComponent<TextMeshProUGUI>();
        label.text               = initial;
        label.enableAutoSizing    = true;
        label.fontSizeMin         = 16f;
        label.fontSizeMax         = 26f;
        label.enableWordWrapping  = false;
        label.fontStyle           = FontStyles.Bold | FontStyles.Underline;
        label.color               = InkBlack;
        label.alignment           = TextAlignmentOptions.Center;
        if (font != null) label.font = font;

        var btc   = btnGo.AddComponent<ButtonTextColor>();
        var btcSo = new SerializedObject(btc);
        btcSo.FindProperty("_text")        .objectReferenceValue = label;
        btcSo.FindProperty("_normalColor") .colorValue = InkNormal;
        btcSo.FindProperty("_hoverColor")  .colorValue = InkOrange;
        btcSo.FindProperty("_pressedColor").colorValue = InkDarkRed;
        btcSo.ApplyModifiedProperties();

        return btn;
    }

    static TMP_FontAsset LoadBookFont()
    {
        string path = AssetDatabase.GUIDToAssetPath(BookFontGuid);
        if (!string.IsNullOrEmpty(path))
        {
            var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (f != null) return f;
        }
        foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
        {
            var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
            if (f != null) return f;
        }
        return null;
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
