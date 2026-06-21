using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Outil Editor : genere la fenetre de dialogue et le controller SystemeDialogue.
/// Menu : Tools / Dialogue / Create UI in Scene
/// </summary>
public static class CreateDialogueUIEditor
{
    private static readonly Color BG_MODAL   = new Color(0.05f, 0.04f, 0.06f, 0.97f);
    private static readonly Color BORDER     = new Color(0.55f, 0.42f, 0.10f, 1f);
    private static readonly Color BTN_DARK   = new Color(0.14f, 0.14f, 0.14f, 1f);
    private static readonly Color TXT_WHITE  = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TXT_NOM    = new Color(0.95f, 0.78f, 0.30f, 1f);

    [MenuItem("Tools/Dialogue/Create UI in Scene")]
    public static void CreateUI()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasGO;
        if (canvas != null)
        {
            canvasGO = canvas.gameObject;
        }
        else
        {
            canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Idempotent : supprime tout dialogue existant pour eviter les doublons.
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && (t.name == "PanneauDialogue" || t.name == "SystemeDialogue_Controller"))
                Undo.DestroyObjectImmediate(t.gameObject);
        }

        var ctrlGO = new GameObject("SystemeDialogue_Controller");
        var dlg = ctrlGO.AddComponent<SystemeDialogue>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create SystemeDialogue Controller");

        // Boite de dialogue ancree en bas de l'ecran.
        var modal = CreatePanel("PanneauDialogue", canvasGO.transform, BG_MODAL,
            new Vector2(0.12f, 0.06f), new Vector2(0.88f, 0.32f));
        CreateBorder(modal.transform, BORDER, 3f);
        dlg.panneau = modal;

        var nom = CreateTMP(modal.transform, "Nom", "",
            30f, TXT_NOM, TextAlignmentOptions.TopLeft, FontStyles.Bold);
        SetAnchors(nom.rectTransform, 0.04f, 0.72f, 0.96f, 0.95f);
        dlg.texteNom = nom;

        var texte = CreateTMP(modal.transform, "Texte", "",
            24f, TXT_WHITE, TextAlignmentOptions.TopLeft, FontStyles.Normal);
        SetAnchors(texte.rectTransform, 0.04f, 0.26f, 0.96f, 0.70f);
        dlg.texteDialogue = texte;

        var btn = CreateButton(modal.transform, "BtnContinuer", "Continuer  (Entree)",
            BTN_DARK, 20f, TXT_WHITE, new Vector2(0.62f, 0.05f), new Vector2(0.96f, 0.22f));
        dlg.boutonContinuer = btn.GetComponent<Button>();

        modal.SetActive(false);

        Undo.RegisterCreatedObjectUndo(modal, "Create Dialogue UI");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        Selection.activeGameObject = ctrlGO;
        Debug.Log("[Dialogue] UI generee et branchee sur SystemeDialogue.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GameObject CreatePanel(string name, Transform parent, Color color,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return go;
    }

    private static void CreateBorder(Transform parent, Color color, float w)
    {
        float p = w / 1000f;
        (string n, Vector2 mn, Vector2 mx)[] sides =
        {
            ("Border_T", new Vector2(0, 1 - p), Vector2.one),
            ("Border_B", Vector2.zero,           new Vector2(1, p)),
            ("Border_L", Vector2.zero,           new Vector2(p, 1)),
            ("Border_R", new Vector2(1 - p, 0),  Vector2.one),
        };
        foreach (var s in sides)
        {
            var go = new GameObject(s.n, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = s.mn; rt.anchorMax = s.mx;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
        }
    }

    private static TextMeshProUGUI CreateTMP(Transform parent, string name, string text,
        float fontSize, Color color, TextAlignmentOptions align, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.color = color;
        tmp.alignment = align; tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static GameObject CreateButton(Transform parent, string name, string label,
        Color normalColor, float fontSize, Color textColor,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.color = normalColor;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;

        var txtGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(go.transform, false);
        var trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return go;
    }

    private static void SetAnchors(RectTransform rt, float x0, float y0, float x1, float y1)
    {
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
