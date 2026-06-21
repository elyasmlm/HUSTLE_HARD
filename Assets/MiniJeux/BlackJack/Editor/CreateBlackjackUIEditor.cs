using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Outil Editor : génère la hiérarchie UI du mini-jeu Blackjack
/// dans la scène active. Menu : Tools > Blackjack > Create UI in Scene
/// </summary>
public static class CreateBlackjackUIEditor
{
    // ── Palette (même charte visuelle que Combat de Coq) ─────────────────────
    private static readonly Color BG_MODAL    = new Color(0.05f, 0.04f, 0.06f, 0.97f);
    private static readonly Color BORDER_RED  = new Color(0.75f, 0.08f, 0.08f, 1f);
    private static readonly Color BORDER_GOLD = new Color(0.75f, 0.58f, 0.05f, 1f);
    private static readonly Color PANEL_DARK  = new Color(0.08f, 0.06f, 0.08f, 1f);
    private static readonly Color PANEL_CROUP = new Color(0.07f, 0.04f, 0.07f, 1f);
    private static readonly Color PANEL_JOUEUR= new Color(0.04f, 0.07f, 0.04f, 1f);
    private static readonly Color BTN_RED     = new Color(0.65f, 0.08f, 0.08f, 1f);
    private static readonly Color BTN_DARK    = new Color(0.14f, 0.14f, 0.14f, 1f);
    private static readonly Color BTN_GOLD    = new Color(0.75f, 0.58f, 0.05f, 1f);
    private static readonly Color BTN_GREEN   = new Color(0.08f, 0.45f, 0.08f, 1f);
    private static readonly Color BTN_GREY    = new Color(0.25f, 0.25f, 0.25f, 1f);
    private static readonly Color TXT_WHITE   = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TXT_YELLOW  = new Color(1.00f, 0.90f, 0.30f, 1f);
    private static readonly Color TXT_RED     = new Color(0.90f, 0.20f, 0.20f, 1f);
    private static readonly Color TXT_GREEN   = new Color(0.30f, 0.90f, 0.30f, 1f);
    private static readonly Color SEPARATOR   = new Color(0.75f, 0.08f, 0.08f, 0.55f);
    private static readonly Color INPUT_BG    = new Color(0.12f, 0.10f, 0.12f, 1f);

    // ── Point d'entrée ────────────────────────────────────────────────────────
    [MenuItem("Tools/Blackjack/Create UI in Scene")]
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
        var ctrlGO = new GameObject("Blackjack_Controller");
        var bj = ctrlGO.AddComponent<Blackjack>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create Blackjack Controller");

        // ── Modale principale ─────────────────────────────────────────────────
        var modalGO = new GameObject("PanneauBlackjack", typeof(RectTransform), typeof(Image));
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
        bj.panneauBlackjack = modalGO;

        // ── Titre ─────────────────────────────────────────────────────────────
        var titre = CreateTMP(modalGO.transform, "Titre", "♠  BLACKJACK  ♠",
            52f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(titre.GetComponent<RectTransform>(), 0f, 0.88f, 1f, 0.97f);
        titre.GetComponent<RectTransform>().GetComponent<TextMeshProUGUI>().raycastTarget = false;
        CreateHLine(modalGO.transform, new Vector2(0.03f, 0.872f), new Vector2(0.97f, 0.878f));

        // ── Zone Argent + Erreur (bandeau du haut) ────────────────────────────
        var bandeauHaut = CreateImagePanel(modalGO.transform, "BandeauHaut",
            new Color(0.08f, 0.06f, 0.08f, 1f),
            new Vector2(0.03f, 0.80f), new Vector2(0.97f, 0.87f));

        var argentTxt = CreateTMP(bandeauHaut.transform, "ArgentDisponible", "Argent : $0",
            22f, TXT_YELLOW, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(argentTxt.GetComponent<RectTransform>(), 0.02f, 0.10f, 0.45f, 0.90f);
        bj.texteArgent = argentTxt.GetComponent<TextMeshProUGUI>();

        var erreurTxt = CreateTMP(bandeauHaut.transform, "ErreurMise", "",
            20f, Color.red, TextAlignmentOptions.MidlineRight, FontStyles.Italic);
        SetAnchors(erreurTxt.GetComponent<RectTransform>(), 0.46f, 0.10f, 0.98f, 0.90f);
        bj.texteErreurMise = erreurTxt.GetComponent<TextMeshProUGUI>();

        // ── Zone Croupier ─────────────────────────────────────────────────────
        var pCroupier = CreateImagePanel(modalGO.transform, "PanneauCroupier", PANEL_CROUP,
            new Vector2(0.03f, 0.52f), new Vector2(0.97f, 0.79f));
        CreateBorder(pCroupier.transform, new Color(0.6f, 0.1f, 0.6f, 0.7f), 2f);

        var labelCroupier = CreateTMP(pCroupier.transform, "LabelCroupier", "CROUPIER",
            22f, new Color(0.7f, 0.3f, 0.9f), TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        SetAnchors(labelCroupier.GetComponent<RectTransform>(), 0.01f, 0.70f, 0.25f, 0.97f);

        var mainCroupierTxt = CreateTMP(pCroupier.transform, "MainCroupier", "— — —",
            32f, TXT_WHITE, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        SetAnchors(mainCroupierTxt.GetComponent<RectTransform>(), 0.01f, 0.30f, 0.80f, 0.72f);
        bj.texteMainCroupier = mainCroupierTxt.GetComponent<TextMeshProUGUI>();

        var scoreCroupierTxt = CreateTMP(pCroupier.transform, "ScoreCroupier", "Score : ?",
            26f, new Color(0.7f, 0.3f, 0.9f), TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        SetAnchors(scoreCroupierTxt.GetComponent<RectTransform>(), 0.75f, 0.30f, 0.99f, 0.72f);
        bj.texteScoreCroupier = scoreCroupierTxt.GetComponent<TextMeshProUGUI>();

        var iconCroupier = CreateTMP(pCroupier.transform, "IconCroupier", "🎩",
            38f, TXT_WHITE, TextAlignmentOptions.MidlineRight, FontStyles.Normal);
        SetAnchors(iconCroupier.GetComponent<RectTransform>(), 0.82f, 0.62f, 0.99f, 0.97f);

        // ── Zone Joueur ───────────────────────────────────────────────────────
        var pJoueur = CreateImagePanel(modalGO.transform, "PanneauJoueur", PANEL_JOUEUR,
            new Vector2(0.03f, 0.24f), new Vector2(0.97f, 0.51f));
        CreateBorder(pJoueur.transform, new Color(0.1f, 0.7f, 0.1f, 0.7f), 2f);

        var labelJoueur = CreateTMP(pJoueur.transform, "LabelJoueur", "JOUEUR",
            22f, TXT_GREEN, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        SetAnchors(labelJoueur.GetComponent<RectTransform>(), 0.01f, 0.70f, 0.20f, 0.97f);

        var mainJoueurTxt = CreateTMP(pJoueur.transform, "MainJoueur", "— — —",
            32f, TXT_WHITE, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        SetAnchors(mainJoueurTxt.GetComponent<RectTransform>(), 0.01f, 0.30f, 0.80f, 0.72f);
        bj.texteMainJoueur = mainJoueurTxt.GetComponent<TextMeshProUGUI>();

        var scoreJoueurTxt = CreateTMP(pJoueur.transform, "ScoreJoueur", "Score : 0",
            26f, TXT_GREEN, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        SetAnchors(scoreJoueurTxt.GetComponent<RectTransform>(), 0.75f, 0.30f, 0.99f, 0.72f);
        bj.texteScoreJoueur = scoreJoueurTxt.GetComponent<TextMeshProUGUI>();

        var iconJoueur = CreateTMP(pJoueur.transform, "IconJoueur", "🃏",
            38f, TXT_WHITE, TextAlignmentOptions.MidlineRight, FontStyles.Normal);
        SetAnchors(iconJoueur.GetComponent<RectTransform>(), 0.82f, 0.62f, 0.99f, 0.97f);

        // ── PanneauJeu (Tirer/Rester/MiseEnCours) — caché avant CommencerPartie ──
        var panneauJeu = CreateImagePanel(modalGO.transform, "PanneauJeu",
            new Color(0f, 0f, 0f, 0f),  // transparent, sert juste de conteneur
            new Vector2(0.03f, 0.52f), new Vector2(0.97f, 0.79f));
        panneauJeu.GetComponent<Image>().raycastTarget = false;
        bj.panneauJeu = panneauJeu;

        var miseCoursTxt = CreateTMP(panneauJeu.transform, "MiseEnCours", "",
            20f, TXT_YELLOW, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        SetAnchors(miseCoursTxt.GetComponent<RectTransform>(), 0.75f, 0.02f, 0.99f, 0.25f);
        bj.texteMiseEnCours = miseCoursTxt.GetComponent<TextMeshProUGUI>();

        panneauJeu.SetActive(false);

        // ── Zone Mise ─────────────────────────────────────────────────────────
        var zoneMise = CreateImagePanel(modalGO.transform, "ZoneMise",
            new Color(0.10f, 0.07f, 0.10f, 1f),
            new Vector2(0.03f, 0.10f), new Vector2(0.97f, 0.23f));

        var labelMise = CreateTMP(zoneMise.transform, "LabelMise", "Mise ($)  —  minimum 20$",
            20f, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(labelMise.GetComponent<RectTransform>(), 0.02f, 0.50f, 0.32f, 0.95f);

        // InputField
        var inputGO = new GameObject("InputMise", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        inputGO.transform.SetParent(zoneMise.transform, false);
        SetAnchors(inputGO.GetComponent<RectTransform>(), 0.34f, 0.08f, 0.62f, 0.92f);
        inputGO.GetComponent<Image>().color = INPUT_BG;
        var inputField = inputGO.GetComponent<TMP_InputField>();
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;

        var inputTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        inputTextGO.transform.SetParent(inputGO.transform, false);
        StretchFull(inputTextGO.GetComponent<RectTransform>());
        SetOffsets(inputTextGO.GetComponent<RectTransform>(), 8, 8, 4, 4);
        var inputTMP = inputTextGO.GetComponent<TextMeshProUGUI>();
        inputTMP.fontSize = 26f;
        inputTMP.color = TXT_WHITE;
        inputTMP.alignment = TextAlignmentOptions.MidlineLeft;
        inputTMP.raycastTarget = false;
        inputField.textComponent = inputTMP;

        var phGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        phGO.transform.SetParent(inputGO.transform, false);
        StretchFull(phGO.GetComponent<RectTransform>());
        SetOffsets(phGO.GetComponent<RectTransform>(), 8, 8, 4, 4);
        var phTMP = phGO.GetComponent<TextMeshProUGUI>();
        phTMP.text = "Ex : 50";
        phTMP.fontSize = 22f;
        phTMP.color = new Color(0.5f, 0.5f, 0.5f);
        phTMP.fontStyle = FontStyles.Italic;
        phTMP.alignment = TextAlignmentOptions.MidlineLeft;
        phTMP.raycastTarget = false;
        inputField.placeholder = phTMP;
        bj.inputMise = inputField;

        // Bouton Confirmer Mise / Distribuer
        var btnConfirmer = CreateButton(zoneMise.transform, "BtnConfirmerMise", "🃏  Distribuer",
            BTN_RED, 22f, TXT_WHITE, new Vector2(0.64f, 0.05f), new Vector2(0.98f, 0.95f));
        bj.boutonConfirmerMise = btnConfirmer.GetComponent<Button>();

        // ── Boutons de jeu (Tirer / Rester) ───────────────────────────────────
        var btnTirer = CreateButton(modalGO.transform, "BtnTirer", "➕  Tirer",
            BTN_GREEN, 24f, TXT_WHITE, new Vector2(0.03f, 0.02f), new Vector2(0.27f, 0.09f));
        btnTirer.GetComponent<Button>().interactable = false;
        bj.boutonTirer = btnTirer.GetComponent<Button>();

        var btnRester = CreateButton(modalGO.transform, "BtnRester", "✋  Rester",
            BTN_GREY, 24f, TXT_WHITE, new Vector2(0.29f, 0.02f), new Vector2(0.53f, 0.09f));
        btnRester.GetComponent<Button>().interactable = false;
        bj.boutonRester = btnRester.GetComponent<Button>();

        // ── Zone Triche (affichage du soudoiement, declenche par le cube "Soudoyer") ──
        var zoneTriche = CreateImagePanel(modalGO.transform, "ZoneTriche",
            new Color(0.12f, 0.10f, 0.05f, 1f),
            new Vector2(0.55f, 0.02f), new Vector2(0.97f, 0.09f));
        CreateBorder(zoneTriche.transform, BORDER_GOLD, 2f);

        var protectionsTxt = CreateTMP(zoneTriche.transform, "ProtectionsRestantes", "",
            16f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(protectionsTxt.GetComponent<RectTransform>(), 0.02f, 0.05f, 0.98f, 0.95f);
        bj.texteProtectionsRestantes = protectionsTxt.GetComponent<TextMeshProUGUI>();

        // ── Bouton Rejouer ────────────────────────────────────────────────────
        var btnRejouer = CreateButton(modalGO.transform, "BtnRejouer", "🔄  Nouveau tour",
            BTN_GOLD, 22f, new Color(0.1f, 0.05f, 0f),
            new Vector2(0.03f, 0.10f), new Vector2(0.38f, 0.17f));
        bj.boutonRejouer = btnRejouer.GetComponent<Button>();
        btnRejouer.SetActive(false);

        // ── Bouton Fermer ─────────────────────────────────────────────────────
        var btnFermer = CreateButton(modalGO.transform, "BtnFermer", "✕  Fermer",
            BTN_DARK, 18f, TXT_WHITE,
            new Vector2(0.83f, 0.002f), new Vector2(0.98f, 0.065f));
        bj.boutonFermer = btnFermer.GetComponent<Button>();

        // ── Zone Résultat (centrée, opaque, cachée au départ) ─────────────────
        var zoneRes = CreateImagePanel(modalGO.transform, "ZoneResultat",
            new Color(0.03f, 0.02f, 0.04f, 0.97f),
            new Vector2(0.12f, 0.28f), new Vector2(0.88f, 0.72f));
        zoneRes.GetComponent<Image>().raycastTarget = false;
        CreateBorder(zoneRes.transform, BORDER_GOLD, 4f);
        zoneRes.SetActive(false);

        var resultatTxt = CreateTMP(zoneRes.transform, "Resultat", "",
            40f, TXT_WHITE, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(resultatTxt.GetComponent<RectTransform>(), 0.05f, 0.50f, 0.95f, 0.92f);
        bj.texteResultat = resultatTxt.GetComponent<TextMeshProUGUI>();

        // Note : zoneRes est passé à ResetPanneau / FinPartie via texteResultat
        // On stocke la référence en patching le Start() pour cacher le panneau au début.

        // ── Finalisation ──────────────────────────────────────────────────────
        Undo.RegisterCreatedObjectUndo(modalGO, "Create Blackjack UI");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        Selection.activeGameObject = ctrlGO;
        Debug.Log("[Blackjack] UI générée avec succès dans la scène.");
    }

    // ── Helpers (même API que CreateCombatCoqUIEditor) ────────────────────────

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
        var img = go.GetComponent<Image>();
        img.color = SEPARATOR;
        img.raycastTarget = false;
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

    private static void SetOffsets(RectTransform rt, float l, float r, float b, float t)
    {
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -t);
    }
}
