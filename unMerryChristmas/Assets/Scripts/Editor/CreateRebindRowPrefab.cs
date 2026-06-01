using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Menu: Tools > UnMerry > Create RebindActionRow Prefab
///
/// Creates Assets/Prefabs/UI/RebindActionRow.prefab styled to match the book UI:
///   - Action name   : book font, black ink, bold, flex width
///   - Current binding: book font, black ink, fixed 140 px, centred
///   - Rebind button : text-only "↺" with ButtonTextColor (same hover colours as
///                     MainMenu nav buttons) — no white rectangle background
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
        var nameTmp    = nameGo.AddComponent<TextMeshProUGUI>();
        nameTmp.text      = "Action";
        nameTmp.fontSize  = 26;
        nameTmp.fontStyle = FontStyles.Bold;
        nameTmp.color     = InkBlack;
        nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (font != null) nameTmp.font = font;

        // ── Current binding ────────────────────────────────────────────────
        var bindingGo = new GameObject("CurrentBinding", typeof(RectTransform));
        bindingGo.transform.SetParent(rowGo.transform, false);
        var bindingLe     = bindingGo.AddComponent<LayoutElement>();
        bindingLe.minWidth       = 140;
        bindingLe.preferredWidth = 140;
        bindingLe.flexibleWidth  = 0;
        var bindingTmp    = bindingGo.AddComponent<TextMeshProUGUI>();
        bindingTmp.text      = "—";
        bindingTmp.fontSize  = 24;
        bindingTmp.color     = InkBlack;
        bindingTmp.alignment = TextAlignmentOptions.Center;
        if (font != null) bindingTmp.font = font;

        // ── Rebind button — text-only, no white box ────────────────────────
        var btnGo = new GameObject("RebindButton", typeof(RectTransform));
        btnGo.transform.SetParent(rowGo.transform, false);
        var btnLe = btnGo.AddComponent<LayoutElement>();
        btnLe.minWidth       = 44;
        btnLe.preferredWidth = 44;
        btnLe.flexibleWidth  = 0;

        var btn = btnGo.AddComponent<Button>();
        btn.transition = Selectable.Transition.None; // ButtonTextColor drives colour

        // TMP label for ↺ symbol
        var lblGo = new GameObject("Text", typeof(RectTransform));
        lblGo.transform.SetParent(btnGo.transform, false);
        var lblRt = lblGo.GetComponent<RectTransform>();
        lblRt.anchorMin = Vector2.zero;
        lblRt.anchorMax = Vector2.one;
        lblRt.sizeDelta = Vector2.zero;

        var lblTmp = lblGo.AddComponent<TextMeshProUGUI>();
        lblTmp.text      = "↺";
        lblTmp.fontSize  = 28;
        lblTmp.fontStyle = FontStyles.Bold;
        lblTmp.color     = InkBlack;
        lblTmp.alignment = TextAlignmentOptions.Center;
        if (font != null) lblTmp.font = font;

        // ButtonTextColor for hover/press feedback
        var btc   = btnGo.AddComponent<ButtonTextColor>();
        var btcSo = new SerializedObject(btc);
        btcSo.FindProperty("_text")         .objectReferenceValue = lblTmp;
        btcSo.FindProperty("_normalColor")  .colorValue = InkNormal;
        btcSo.FindProperty("_hoverColor")   .colorValue = InkOrange;
        btcSo.FindProperty("_pressedColor") .colorValue = InkDarkRed;
        btcSo.ApplyModifiedProperties();

        // ── Wire RebindActionRow fields ────────────────────────────────────
        var so = new SerializedObject(row);
        so.FindProperty("_actionNameLabel")   .objectReferenceValue = nameTmp;
        so.FindProperty("_currentBindingLabel").objectReferenceValue = bindingTmp;
        so.FindProperty("_rebindButton")       .objectReferenceValue = btn;
        so.FindProperty("_rebindButtonLabel")  .objectReferenceValue = lblTmp;
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
