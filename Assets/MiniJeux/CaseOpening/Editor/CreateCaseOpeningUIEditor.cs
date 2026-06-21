using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Outil Editor : génère la hiérarchie UI du mini-jeu Case Opening
/// dans la scène active. Menu : Tools > CaseOpening > Create UI in Scene
/// </summary>
public static class CreateCaseOpeningUIEditor
{
    // ── Palette (même charte visuelle que MiniRoulette / Combat de Coq) ──────
    private static readonly Color BG_MODAL     = new Color(0.04f, 0.04f, 0.05f, 0.97f);
    private static readonly Color BORDER_BLUE  = new Color(0.10f, 0.40f, 0.80f, 1f);
    private static readonly Color BORDER_GOLD  = new Color(0.75f, 0.58f, 0.05f, 1f);
    private static readonly Color PANEL_ANIM   = new Color(0.06f, 0.06f, 0.10f, 1f);
    private static readonly Color PANEL_INFOS  = new Color(0.07f, 0.07f, 0.09f, 1f);
    private static readonly Color PANEL_CHOIX  = new Color(0.08f, 0.06f, 0.12f, 1f);
    private static readonly Color PANEL_RES    = new Color(0.04f, 0.04f, 0.08f, 0.97f);
    private static readonly Color BTN_BLUE     = new Color(0.10f, 0.35f, 0.70f, 1f);
    private static readonly Color BTN_PURPLE   = new Color(0.38f, 0.05f, 0.60f, 1f);
    private static readonly Color BTN_DARK     = new Color(0.14f, 0.14f, 0.18f, 1f);
    private static readonly Color BTN_GOLD     = new Color(0.72f, 0.55f, 0.04f, 1f);
    private static readonly Color TXT_WHITE    = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TXT_YELLOW   = new Color(1.00f, 0.90f, 0.30f, 1f);
    private static readonly Color TXT_GRAY     = new Color(0.60f, 0.60f, 0.65f, 1f);
    private static readonly Color SEPARATOR    = new Color(0.10f, 0.40f, 0.80f, 0.50f);

    // ── Point d'entrée ────────────────────────────────────────────────────────
    [MenuItem("Tools/CaseOpening/Create UI in Scene")]
    public static void CreateUI()
    {
        // Canvas existant ou nouveau
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

        // ── Controller ────────────────────────────────────────────────────────
        var ctrlGO = new GameObject("CaseOpening_Controller");
        var co = ctrlGO.AddComponent<CaseOpening>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create CaseOpening Controller");

        // ── Modale principale ─────────────────────────────────────────────────
        var modalGO = new GameObject("PanneauCase", typeof(RectTransform), typeof(Image));
        modalGO.transform.SetParent(canvasGO.transform, false);
        var modalRT = modalGO.GetComponent<RectTransform>();
        modalRT.anchorMin = new Vector2(0.08f, 0.04f);
        modalRT.anchorMax = new Vector2(0.92f, 0.96f);
        modalRT.offsetMin = Vector2.zero;
        modalRT.offsetMax = Vector2.zero;
        var modalImg = modalGO.GetComponent<Image>();
        modalImg.color = BG_MODAL;
        modalImg.raycastTarget = false;
        CreateBorder(modalGO.transform, BORDER_BLUE, 3f);
        co.panneauCase = modalGO;

        // ── Titre ─────────────────────────────────────────────────────────────
        var titre = CreateTMP(modalGO.transform, "Titre", "🎁  CASE OPENING  🎁",
            48f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(titre.GetComponent<RectTransform>(), 0f, 0.88f, 1f, 0.97f);
        CreateHLine(modalGO.transform, new Vector2(0.03f, 0.872f), new Vector2(0.97f, 0.878f));

        // ── Bandeau infos joueur ──────────────────────────────────────────────
        var bandeauInfos = CreateImagePanel(modalGO.transform, "BandeauInfos", PANEL_INFOS,
            new Vector2(0.03f, 0.81f), new Vector2(0.97f, 0.87f));

        var argentTxt = CreateTMP(bandeauInfos.transform, "Argent", "Argent : $0",
            20f, TXT_YELLOW, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(argentTxt.GetComponent<RectTransform>(), 0.01f, 0.08f, 0.35f, 0.92f);
        co.texteArgent = argentTxt.GetComponent<TextMeshProUGUI>();

        var multiTxt = CreateTMP(bandeauInfos.transform, "Multiplicateur", "",
            20f, new Color(0.3f, 1f, 0.5f), TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(multiTxt.GetComponent<RectTransform>(), 0.36f, 0.08f, 0.64f, 0.92f);
        co.texteMultiplicateur = multiTxt.GetComponent<TextMeshProUGUI>();

        var coutTxt = CreateTMP(bandeauInfos.transform, "Cout", "Normal : $10  |  Supersonique : $20",
            17f, TXT_GRAY, TextAlignmentOptions.MidlineRight, FontStyles.Italic);
        SetAnchors(coutTxt.GetComponent<RectTransform>(), 0.65f, 0.08f, 0.99f, 0.92f);

        // ── Zone choix de caisse ──────────────────────────────────────────────
        var zoneChoix = CreateImagePanel(modalGO.transform, "ZoneChoixCaisse", PANEL_CHOIX,
            new Vector2(0.03f, 0.68f), new Vector2(0.97f, 0.80f));
        CreateBorder(zoneChoix.transform, BORDER_BLUE, 2f);

        // Label "Choisir une caisse :"
        var labelChoix = CreateTMP(zoneChoix.transform, "LabelChoix", "Choisir une caisse :",
            18f, TXT_GRAY, TextAlignmentOptions.MidlineLeft, FontStyles.Italic);
        SetAnchors(labelChoix.GetComponent<RectTransform>(), 0.01f, 0.60f, 0.25f, 0.95f);

        // Bouton Caisse Normale
        var btnNormale = CreateButton(zoneChoix.transform, "BtnCaisseNormale",
            "📦  Caisse Normale\n$10 — jusqu'à $100",
            BTN_BLUE, 18f, TXT_WHITE,
            new Vector2(0.01f, 0.05f), new Vector2(0.49f, 0.58f));
        co.boutonCaisseNormale = btnNormale.GetComponent<Button>();
        // texteDescCaisseNormale pointe sur le Label interne du bouton (pas de TMP séparé)
        co.texteDescCaisseNormale = btnNormale.GetComponentInChildren<TextMeshProUGUI>();

        // Bouton Caisse Supersonique
        var btnSuper = CreateButton(zoneChoix.transform, "BtnCaisseSupersonique",
            "🚀  Caisse Supersonique\n$20 — jusqu'à $1 000",
            BTN_PURPLE, 18f, TXT_WHITE,
            new Vector2(0.51f, 0.05f), new Vector2(0.99f, 0.58f));
        co.boutonCaisseSupersonique = btnSuper.GetComponent<Button>();
        // texteDescCaisseSupersonique pointe sur le Label interne du bouton
        co.texteDescCaisseSupersonique = btnSuper.GetComponentInChildren<TextMeshProUGUI>();

        // ── Zone animation (défilement armes) ─────────────────────────────────
        var panAnim = CreateImagePanel(modalGO.transform, "PanneauAnimation", PANEL_ANIM,
            new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.67f));
        CreateBorder(panAnim.transform, BORDER_GOLD, 4f);
        panAnim.SetActive(false);
        co.panneauAnimation = panAnim;

        // Ligne marqueur centrale
        CreateHLine2(panAnim.transform,
            new Vector2(0.04f, 0.465f), new Vector2(0.96f, 0.475f),
            new Color(1f, 0.84f, 0f, 0.9f));
        CreateHLine2(panAnim.transform,
            new Vector2(0.04f, 0.525f), new Vector2(0.96f, 0.535f),
            new Color(1f, 0.84f, 0f, 0.9f));

        // Texte défilant
        var defilTxt = CreateTMP(panAnim.transform, "TexteDefilement", "?",
            44f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(defilTxt.GetComponent<RectTransform>(), 0.02f, 0.20f, 0.98f, 0.80f);
        co.texteDefilement = defilTxt.GetComponent<TextMeshProUGUI>();

        // Label "En cours..."
        var labelAnim = CreateTMP(panAnim.transform, "LabelAnim", "Ouverture en cours...",
            16f, TXT_GRAY, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(labelAnim.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.95f, 0.18f);

        // ── Zone résultat (cachée au départ) ──────────────────────────────────
        var panRes = CreateImagePanel(modalGO.transform, "PanneauResultat", PANEL_RES,
            new Vector2(0.05f, 0.30f), new Vector2(0.95f, 0.67f));
        CreateBorder(panRes.transform, BORDER_GOLD, 5f);
        panRes.SetActive(false);
        co.panneauResultat = panRes;

        var nomArmeTxt = CreateTMP(panRes.transform, "NomArme", "",
            34f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(nomArmeTxt.GetComponent<RectTransform>(), 0.03f, 0.72f, 0.97f, 0.95f);
        co.texteNomArme = nomArmeTxt.GetComponent<TextMeshProUGUI>();

        var rareteTxt = CreateTMP(panRes.transform, "Rarete", "",
            26f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(rareteTxt.GetComponent<RectTransform>(), 0.03f, 0.50f, 0.97f, 0.70f);
        co.texteRareteArme = rareteTxt.GetComponent<TextMeshProUGUI>();

        var valeurTxt = CreateTMP(panRes.transform, "Valeur", "",
            24f, new Color(0.3f, 1f, 0.5f), TextAlignmentOptions.Center, FontStyles.Normal);
        SetAnchors(valeurTxt.GetComponent<RectTransform>(), 0.03f, 0.30f, 0.97f, 0.48f);
        co.texteValeurArme = valeurTxt.GetComponent<TextMeshProUGUI>();

        var msgTxt = CreateTMP(panRes.transform, "MessageResultat", "",
            20f, Color.green, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(msgTxt.GetComponent<RectTransform>(), 0.03f, 0.06f, 0.97f, 0.28f);
        co.texteMessageResultat = msgTxt.GetComponent<TextMeshProUGUI>();

        // ── Message erreur ────────────────────────────────────────────────────
        var erreurTxt = CreateTMP(modalGO.transform, "ErreurMessage", "",
            20f, Color.red, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(erreurTxt.GetComponent<RectTransform>(), 0.10f, 0.25f, 0.90f, 0.29f);
        co.texteErreurCo = erreurTxt.GetComponent<TextMeshProUGUI>();

        // ── Bouton Ouvrir ─────────────────────────────────────────────────────
        // (remplace le concept "Ouvrir" : les boutons caisse servent de déclencheurs)
        // Les boutons caisses SONT les boutons "Ouvrir" dans cette architecture.
        // On ajoute un label explicatif sous la zone de choix.
        var labelOuvrir = CreateTMP(modalGO.transform, "LabelOuvrir",
            "Cliquez sur une caisse pour l'ouvrir",
            16f, TXT_GRAY, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(labelOuvrir.GetComponent<RectTransform>(), 0.10f, 0.64f, 0.90f, 0.68f);

        // ── Bouton Nouvelle ouverture (caché au départ) ───────────────────────
        var btnRejouer = CreateButton(modalGO.transform, "BtnNouvellOuverture",
            "🔄  Nouvelle ouverture",
            BTN_GOLD, 22f, new Color(0.08f, 0.05f, 0f),
            new Vector2(0.28f, 0.14f), new Vector2(0.72f, 0.24f));
        btnRejouer.SetActive(false);
        co.boutonRejouer = btnRejouer.GetComponent<Button>();

        // ── Bouton Fermer ─────────────────────────────────────────────────────
        var btnFermer = CreateButton(modalGO.transform, "BtnFermer", "✕  Fermer",
            BTN_DARK, 18f, TXT_WHITE,
            new Vector2(0.83f, 0.002f), new Vector2(0.98f, 0.065f));
        co.boutonFermer = btnFermer.GetComponent<Button>();

        // ── Finalisation ──────────────────────────────────────────────────────
        Undo.RegisterCreatedObjectUndo(modalGO, "Create CaseOpening UI");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        Selection.activeGameObject = ctrlGO;
        Debug.Log("[CaseOpening] UI générée avec succès dans la scène.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GameObject CreateImagePanel(Transform parent, string name, Color color,
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
        string[] n = { "Border_T", "Border_B", "Border_L", "Border_R" };
        float p = w / 1000f;
        Vector2[] mn = { new(0, 1 - p), new(0, 0), new(0, 0), new(1 - p, 0) };
        Vector2[] mx = { new(1, 1), new(1, p), new(p, 1), new(1, 1) };
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject(n[i], typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = mn[i]; rt.anchorMax = mx[i];
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
        }
    }

    private static TMP_Text CreateTMP(Transform parent, string name, string text,
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
        var colors = btn.colors;
        colors.highlightedColor = new Color(
            Mathf.Min(normalColor.r + 0.15f, 1f),
            Mathf.Min(normalColor.g + 0.15f, 1f),
            Mathf.Min(normalColor.b + 0.15f, 1f), 1f);
        colors.pressedColor = new Color(
            Mathf.Max(normalColor.r - 0.15f, 0f),
            Mathf.Max(normalColor.g - 0.15f, 0f),
            Mathf.Max(normalColor.b - 0.15f, 0f), 1f);
        btn.colors = colors;

        var txtGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(go.transform, false);
        StretchFull(txtGO.GetComponent<RectTransform>());
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return go;
    }

    private static void CreateHLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject("HLine", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = SEPARATOR;
        go.GetComponent<Image>().raycastTarget = false;
    }

    private static void CreateHLine2(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject("HLineMid", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        go.GetComponent<Image>().raycastTarget = false;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchors(RectTransform rt, float x0, float y0, float x1, float y1)
    {
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
