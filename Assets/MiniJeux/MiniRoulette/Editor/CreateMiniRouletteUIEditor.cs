using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Outil Editor : génère la hiérarchie UI du mini-jeu Mini-Roulette
/// dans la scène active. Menu : Tools > MiniRoulette > Create UI in Scene
/// </summary>
public static class CreateMiniRouletteUIEditor
{
    // ── Palette (même charte visuelle que Combat de Coq / Blackjack) ─────────
    private static readonly Color BG_MODAL    = new Color(0.05f, 0.05f, 0.04f, 0.97f);
    private static readonly Color BORDER_RED  = new Color(0.75f, 0.08f, 0.08f, 1f);
    private static readonly Color BORDER_GOLD = new Color(0.75f, 0.58f, 0.05f, 1f);
    private static readonly Color PANEL_ROUE  = new Color(0.09f, 0.06f, 0.02f, 1f);
    private static readonly Color PANEL_INFOS = new Color(0.08f, 0.07f, 0.04f, 1f);
    private static readonly Color PANEL_TRICHE= new Color(0.10f, 0.07f, 0.02f, 1f);
    private static readonly Color BTN_RED     = new Color(0.65f, 0.08f, 0.08f, 1f);
    private static readonly Color BTN_DARK    = new Color(0.14f, 0.14f, 0.14f, 1f);
    private static readonly Color BTN_GOLD    = new Color(0.75f, 0.58f, 0.05f, 1f);
    private static readonly Color BTN_PURPLE  = new Color(0.40f, 0.05f, 0.55f, 1f);
    private static readonly Color TXT_WHITE   = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TXT_YELLOW  = new Color(1.00f, 0.90f, 0.30f, 1f);
    private static readonly Color TXT_RED     = new Color(0.90f, 0.20f, 0.20f, 1f);
    private static readonly Color SEPARATOR   = new Color(0.75f, 0.08f, 0.08f, 0.55f);

    // ── Point d'entrée ────────────────────────────────────────────────────────
    [MenuItem("Tools/MiniRoulette/Create UI in Scene")]
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
        var ctrlGO = new GameObject("MiniRoulette_Controller");
        var roulette = ctrlGO.AddComponent<MiniRoulette>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create MiniRoulette Controller");

        // ── Modale principale ─────────────────────────────────────────────────
        var modalGO = new GameObject("PanneauRoulette", typeof(RectTransform), typeof(Image));
        modalGO.transform.SetParent(canvasGO.transform, false);
        var modalRT = modalGO.GetComponent<RectTransform>();
        modalRT.anchorMin = new Vector2(0.08f, 0.04f);
        modalRT.anchorMax = new Vector2(0.92f, 0.96f);
        modalRT.offsetMin = Vector2.zero;
        modalRT.offsetMax = Vector2.zero;
        var modalImg = modalGO.GetComponent<Image>();
        modalImg.color = BG_MODAL;
        modalImg.raycastTarget = false;
        CreateBorder(modalGO.transform, BORDER_RED, 3f);
        roulette.panneauRoulette = modalGO;

        // ── Titre ─────────────────────────────────────────────────────────────
        var titre = CreateTMP(modalGO.transform, "Titre", "🎰  MINI-ROULETTE  🎰",
            48f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(titre.GetComponent<RectTransform>(), 0f, 0.88f, 1f, 0.97f);
        CreateHLine(modalGO.transform, new Vector2(0.03f, 0.872f), new Vector2(0.97f, 0.878f));

        // ── Bandeau infos joueur ──────────────────────────────────────────────
        var bandeauInfos = CreateImagePanel(modalGO.transform, "BandeauInfos", PANEL_INFOS,
            new Vector2(0.03f, 0.80f), new Vector2(0.97f, 0.87f));

        var argentTxt = CreateTMP(bandeauInfos.transform, "Argent", "Argent : $0",
            20f, TXT_YELLOW, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(argentTxt.GetComponent<RectTransform>(), 0.01f, 0.08f, 0.28f, 0.92f);
        roulette.texteArgent = argentTxt.GetComponent<TextMeshProUGUI>();

        var coutTxt = CreateTMP(bandeauInfos.transform, "Cout", "Coût : $100 / tour",
            18f, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(coutTxt.GetComponent<RectTransform>(), 0.29f, 0.08f, 0.54f, 0.92f);

        var gratuitsTxt = CreateTMP(bandeauInfos.transform, "PartiesGratuites", "",
            20f, new Color(0.3f, 0.9f, 0.9f), TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(gratuitsTxt.GetComponent<RectTransform>(), 0.55f, 0.08f, 0.78f, 0.92f);
        roulette.textePartiesGratuites = gratuitsTxt.GetComponent<TextMeshProUGUI>();

        var multiTxt = CreateTMP(bandeauInfos.transform, "Multiplicateur", "",
            20f, TXT_YELLOW, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        SetAnchors(multiTxt.GetComponent<RectTransform>(), 0.79f, 0.08f, 0.99f, 0.92f);
        roulette.texteMultiplicateurActif = multiTxt.GetComponent<TextMeshProUGUI>();

        // ── Zone roulette (animation texte) ───────────────────────────────────
        var zoneRoue = CreateImagePanel(modalGO.transform, "ZoneRoue", PANEL_ROUE,
            new Vector2(0.15f, 0.38f), new Vector2(0.85f, 0.79f));
        CreateBorder(zoneRoue.transform, BORDER_GOLD, 4f);

        var casesListeTxt = CreateTMP(zoneRoue.transform, "CasesListe",
            "Partie gratuite  •  Gain $100  •  Multi x2  •  Multi x3\n" +
            "Gain $200  •  Gain $500  •  Gain $1000  •  Perdu",
            14f, new Color(0.55f, 0.50f, 0.35f), TextAlignmentOptions.Center, FontStyles.Normal);
        SetAnchors(casesListeTxt.GetComponent<RectTransform>(), 0.02f, 0.72f, 0.98f, 0.95f);

        CreateHLine2(zoneRoue.transform,
            new Vector2(0.05f, 0.695f), new Vector2(0.95f, 0.705f),
            new Color(0.75f, 0.58f, 0.05f, 0.5f));

        // Texte animé (case en cours)
        var caseActTxt = CreateTMP(zoneRoue.transform, "CaseActuelle", "?",
            52f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(caseActTxt.GetComponent<RectTransform>(), 0.02f, 0.20f, 0.98f, 0.68f);
        roulette.texteCaseActuelle = caseActTxt.GetComponent<TextMeshProUGUI>();

        // Texte résultat spécial (aimant / gains immédiats)
        var resultatSpecTxt = CreateTMP(zoneRoue.transform, "ResultatSpecial", "",
            22f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(resultatSpecTxt.GetComponent<RectTransform>(), 0.02f, 0.02f, 0.98f, 0.20f);
        roulette.texteResultatSpecial = resultatSpecTxt.GetComponent<TextMeshProUGUI>();

        // ── Zone résultat (lot + description) — cachée au départ ─────────────
        var zoneRes = CreateImagePanel(modalGO.transform, "ZoneResultat",
            new Color(0.04f, 0.03f, 0.01f, 0.97f),
            new Vector2(0.15f, 0.38f), new Vector2(0.85f, 0.79f));
        zoneRes.GetComponent<Image>().raycastTarget = false;
        CreateBorder(zoneRes.transform, BORDER_GOLD, 5f);
        zoneRes.SetActive(false);
        roulette.zoneResultat = zoneRes;

        var nomLotTxt = CreateTMP(zoneRes.transform, "NomLot", "",
            42f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(nomLotTxt.GetComponent<RectTransform>(), 0.05f, 0.55f, 0.95f, 0.92f);
        roulette.texteNomLot = nomLotTxt.GetComponent<TextMeshProUGUI>();

        var descLotTxt = CreateTMP(zoneRes.transform, "DescriptionLot", "",
            26f, TXT_WHITE, TextAlignmentOptions.Center, FontStyles.Normal);
        SetAnchors(descLotTxt.GetComponent<RectTransform>(), 0.05f, 0.20f, 0.95f, 0.54f);
        roulette.texteDescriptionLot = descLotTxt.GetComponent<TextMeshProUGUI>();

        // ── Zone Triche (aimant) ───────────────────────────────────────────────
        var zoneTriche = CreateImagePanel(modalGO.transform, "ZoneTriche", PANEL_TRICHE,
            new Vector2(0.03f, 0.25f), new Vector2(0.97f, 0.37f));
        CreateBorder(zoneTriche.transform, BORDER_GOLD, 2f);

        var btnAimant = CreateButton(zoneTriche.transform, "BtnAimant",
            "🧲  Utiliser aimant — Gain $1000 garanti",
            BTN_PURPLE, 20f, TXT_WHITE,
            new Vector2(0.01f, 0.05f), new Vector2(0.65f, 0.95f));
        roulette.boutonUtiliserAimant = btnAimant.GetComponent<Button>();

        var aimantsStockTxt = CreateTMP(zoneTriche.transform, "AimantsStock", "",
            18f, new Color(0.8f, 0.5f, 1.0f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        SetAnchors(aimantsStockTxt.GetComponent<RectTransform>(), 0.67f, 0.10f, 0.99f, 0.90f);
        roulette.texteAimantsStock = aimantsStockTxt.GetComponent<TextMeshProUGUI>();

        // ── Bouton Lancer ─────────────────────────────────────────────────────
        var btnLancer = CreateButton(modalGO.transform, "BtnLancer",
            "🎰  LANCER LA ROULETTE",
            BTN_RED, 26f, TXT_WHITE,
            new Vector2(0.22f, 0.14f), new Vector2(0.78f, 0.24f));
        roulette.boutonTourner = btnLancer.GetComponent<Button>();

        // ── Bouton Nouveau tour ───────────────────────────────────────────────
        var btnRejouer = CreateButton(modalGO.transform, "BtnRejouer", "🔄  Nouveau tour",
            BTN_GOLD, 22f, new Color(0.1f, 0.05f, 0f),
            new Vector2(0.03f, 0.14f), new Vector2(0.20f, 0.24f));
        btnRejouer.SetActive(false);
        roulette.boutonRejouer = btnRejouer.GetComponent<Button>();

        // ── Message erreur ────────────────────────────────────────────────────
        var erreurTxt = CreateTMP(modalGO.transform, "ErreurMessage", "",
            20f, Color.red, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(erreurTxt.GetComponent<RectTransform>(), 0.10f, 0.08f, 0.90f, 0.13f);
        roulette.texteErreur = erreurTxt.GetComponent<TextMeshProUGUI>();

        // ── Bouton Fermer ─────────────────────────────────────────────────────
        var btnFermer = CreateButton(modalGO.transform, "BtnFermer", "✕  Fermer",
            BTN_DARK, 18f, TXT_WHITE,
            new Vector2(0.83f, 0.002f), new Vector2(0.98f, 0.065f));
        roulette.boutonFermer = btnFermer.GetComponent<Button>();

        // ── Finalisation ──────────────────────────────────────────────────────
        Undo.RegisterCreatedObjectUndo(modalGO, "Create MiniRoulette UI");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        Selection.activeGameObject = ctrlGO;
        Debug.Log("[MiniRoulette] UI générée avec succès dans la scène.");
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
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
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
