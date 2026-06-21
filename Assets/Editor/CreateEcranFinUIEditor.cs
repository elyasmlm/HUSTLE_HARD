using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using MenuGame;

/// <summary>
/// Genere l'ecran de fin (victoire / game over) dans la scene active.
/// Menu : Tools / MenuGame / Create End Screens
/// </summary>
public static class CreateEcranFinUIEditor
{
    private static readonly Color BTN_NORMAL = new Color(0f, 0f, 0f, 0f);
    private static readonly Color TEXT_WHITE = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color SEPARATOR  = new Color(0.82f, 0.06f, 0.06f, 0.6f);
    private static readonly Color OVERLAY    = new Color(0f, 0f, 0f, 0.92f);

    [MenuItem("Tools/MenuGame/Create End Screens")]
    public static void CreateEndScreens()
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
            canvas.sortingOrder = 30;
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

        // Idempotent.
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && (t.name == "EcranFinOverlay" || t.name == "EcranFin_Controller"))
                Undo.DestroyObjectImmediate(t.gameObject);
        }

        var ctrlGO = new GameObject("EcranFin_Controller");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        var ecran = ctrlGO.AddComponent<EcranFin>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create EcranFin");

        var overlay = new GameObject("EcranFinOverlay", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvasGO.transform, false);
        StretchFull(overlay.GetComponent<RectTransform>());
        overlay.GetComponent<Image>().color = OVERLAY;
        ecran.panneau = overlay;

        // Titre (couleur fixee a l'execution selon victoire/defaite).
        var titre = CreateTMP(overlay.transform, "TitreFin", "VICTOIRE",
            130f, TEXT_WHITE, TextAlignmentOptions.Center, FontStyles.Bold);
        Anchor(titre.rectTransform, 0.1f, 0.66f, 0.9f, 0.85f);
        titre.gameObject.AddComponent<GlitchTextEffect>();
        ecran.texteTitre = titre;

        CreateHLine(overlay.transform, new Vector2(0.34f, 0.645f), new Vector2(0.66f, 0.65f));

        var sousTitre = CreateTMP(overlay.transform, "SousTitre", "",
            30f, TEXT_WHITE, TextAlignmentOptions.Center, FontStyles.Normal);
        sousTitre.textWrappingMode = TextWrappingModes.Normal;
        Anchor(sousTitre.rectTransform, 0.2f, 0.50f, 0.8f, 0.62f);
        ecran.texteSousTitre = sousTitre;

        // Boutons
        var groupe = new GameObject("BoutonsFin", typeof(RectTransform));
        groupe.transform.SetParent(overlay.transform, false);
        Anchor(groupe.GetComponent<RectTransform>(), 0.36f, 0.10f, 0.64f, 0.46f);

        var bRejouer = CreateMenuButton(groupe.transform, "REJOUER", 0.70f);
        var bMenu    = CreateMenuButton(groupe.transform, "MENU PRINCIPAL", 0.40f);
        var bQuitter = CreateMenuButton(groupe.transform, "QUITTER", 0.10f);
        bRejouer.AddComponent<MenuButtonHoverEffect>();
        bMenu.AddComponent<MenuButtonHoverEffect>();
        bQuitter.AddComponent<MenuButtonHoverEffect>();
        ecran.boutonRejouer = bRejouer.GetComponent<Button>();
        ecran.boutonMenu    = bMenu.GetComponent<Button>();
        ecran.boutonQuitter = bQuitter.GetComponent<Button>();

        overlay.SetActive(false);

        Undo.RegisterCreatedObjectUndo(overlay, "Create End Screen UI");
        Selection.activeGameObject = ctrlGO;
        Debug.Log("[EcranFin] Ecrans victoire / game over generes.");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static GameObject CreateMenuButton(Transform parent, string label, float anchorY, float fontSize = 44f)
    {
        var btnGO = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, anchorY);
        rt.anchorMax = new Vector2(1f, anchorY + 0.22f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        var img = btnGO.GetComponent<Image>();
        img.color = BTN_NORMAL;
        var btn = btnGO.GetComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.normalColor      = BTN_NORMAL;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.06f);
        colors.pressedColor     = new Color(1f, 1f, 1f, 0.10f);
        colors.selectedColor    = BTN_NORMAL;
        btn.colors = colors;

        var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(btnGO.transform, false);
        StretchFull(textGO.GetComponent<RectTransform>());
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = TEXT_WHITE;
        tmp.alignment = TextAlignmentOptions.Center; tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return btnGO;
    }

    private static TextMeshProUGUI CreateTMP(Transform parent, string name, string text,
        float fontSize, Color color, TextAlignmentOptions align, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.color = color;
        tmp.alignment = align; tmp.fontStyle = style;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void CreateHLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject("HLine", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = SEPARATOR; img.raycastTarget = false;
    }

    private static void Anchor(RectTransform rt, float x0, float y0, float x1, float y1)
    {
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
