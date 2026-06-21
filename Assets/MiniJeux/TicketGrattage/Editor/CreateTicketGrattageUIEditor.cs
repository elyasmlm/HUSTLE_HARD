using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Outil Editor : génère la hiérarchie UI du mini-jeu Ticket Grattage
/// dans la scène active. Menu : Tools > TicketGrattage > Create UI in Scene
/// </summary>
public static class CreateTicketGrattageUIEditor
{
    // ── Palette casino underground rouge/noir/doré ────────────────────────
    private static readonly Color BG_MODAL      = new Color(0.04f, 0.03f, 0.02f, 0.97f);
    private static readonly Color BORDER_RED    = new Color(0.75f, 0.08f, 0.08f, 1f);
    private static readonly Color BORDER_GOLD   = new Color(0.75f, 0.58f, 0.05f, 1f);
    private static readonly Color PANEL_INFOS   = new Color(0.08f, 0.06f, 0.03f, 1f);
    private static readonly Color PANEL_TICKET  = new Color(0.10f, 0.08f, 0.04f, 1f);
    private static readonly Color PANEL_RECT    = new Color(0.12f, 0.09f, 0.02f, 1f);
    private static readonly Color PANEL_BALLON  = new Color(0.13f, 0.10f, 0.03f, 1f);
    private static readonly Color OVERLAY_COLOR = new Color(0.18f, 0.14f, 0.06f, 1f); // cache doré sombre
    private static readonly Color BTN_RED       = new Color(0.65f, 0.08f, 0.08f, 1f);
    private static readonly Color BTN_GOLD      = new Color(0.72f, 0.55f, 0.04f, 1f);
    private static readonly Color BTN_DARK      = new Color(0.14f, 0.14f, 0.14f, 1f);
    private static readonly Color TXT_WHITE     = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TXT_YELLOW    = new Color(1.00f, 0.90f, 0.30f, 1f);
    private static readonly Color TXT_GRAY      = new Color(0.55f, 0.55f, 0.55f, 1f);
    private static readonly Color SEPARATOR     = new Color(0.75f, 0.08f, 0.08f, 0.55f);

    // ── Point d'entrée ────────────────────────────────────────────────────
    [MenuItem("Tools/TicketGrattage/Create UI in Scene")]
    public static void CreateUI()
    {
        // Canvas
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

        // ── Controller ────────────────────────────────────────────────────
        var ctrlGO = new GameObject("TicketGrattage_Controller");
        var tg = ctrlGO.AddComponent<TicketGrattage>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create TicketGrattage Controller");

        // ── Modale principale ─────────────────────────────────────────────
        var modal = CreatePanel(canvasGO.transform, "PanneauTicket", BG_MODAL,
            new Vector2(0.10f, 0.03f), new Vector2(0.90f, 0.97f));
        CreateBorder(modal.transform, BORDER_RED, 3f);
        tg.panneauTicket = modal;

        // ── Titre ─────────────────────────────────────────────────────────
        var titre = CreateTMP(modal.transform, "Titre", "🎟  TICKET GRATTAGE  🎟",
            44f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(titre, 0f, 0.89f, 1f, 0.98f);
        CreateHLine(modal.transform, new Vector2(0.03f, 0.882f), new Vector2(0.97f, 0.888f));

        // ── Bandeau infos ─────────────────────────────────────────────────
        var bandeauInfos = CreatePanel(modal.transform, "BandeauInfos", PANEL_INFOS,
            new Vector2(0.03f, 0.82f), new Vector2(0.97f, 0.88f));

        var argentTxt = CreateTMP(bandeauInfos.transform, "Argent", "Argent : $0",
            20f, TXT_YELLOW, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(argentTxt, 0.01f, 0.08f, 0.40f, 0.92f);
        tg.texteArgent = argentTxt.GetComponent<TextMeshProUGUI>();

        var coutTxt = CreateTMP(bandeauInfos.transform, "Cout", "Coût : $5 / ticket",
            18f, TXT_GRAY, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(coutTxt, 0.41f, 0.08f, 0.70f, 0.92f);

        // ── Zone ticket ───────────────────────────────────────────────────
        var zoneTicket = CreatePanel(modal.transform, "ZoneTicket", PANEL_TICKET,
            new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.81f));
        CreateBorder(zoneTicket.transform, BORDER_GOLD, 3f);

        // Label "Symbole gagnant"
        var labelRect = CreateTMP(zoneTicket.transform, "LabelRect",
            "▼  Symbole & Gain  ▼",
            15f, TXT_GRAY, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(labelRect, 0.15f, 0.88f, 0.85f, 0.97f);

        // ── Rectangle symbole + gain ──────────────────────────────────────
        var rectGO = CreatePanel(zoneTicket.transform, "RectangleGain", PANEL_RECT,
            new Vector2(0.15f, 0.66f), new Vector2(0.85f, 0.87f));
        CreateBorder(rectGO.transform, BORDER_GOLD, 4f);

        // Texte symbole gauche
        var symTxt = CreateTMP(rectGO.transform, "SymboleRect", "?",
            42f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(symTxt, 0.02f, 0.05f, 0.38f, 0.95f);
        tg.texteSymboleRect = symTxt.GetComponent<TextMeshProUGUI>();

        // Séparateur vertical
        CreateHLine(rectGO.transform, new Vector2(0.42f, 0.10f), new Vector2(0.44f, 0.90f));

        // Texte gain droite
        var gainTxt = CreateTMP(rectGO.transform, "Gain", "$??",
            32f, new Color(0.3f, 1f, 0.4f), TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(gainTxt, 0.46f, 0.05f, 0.98f, 0.95f);
        tg.texteGain = gainTxt.GetComponent<TextMeshProUGUI>();

        // Overlay du rectangle (cache de grattage)
        var overlayRect = CreateOverlay(rectGO.transform, "OverlayRect", OVERLAY_COLOR,
            "GRATTER");
        tg.overlayRectangle = overlayRect;

        // Bouton transparent sur le rectangle pour le grattage
        var btnRectGO = CreateTransparentButton(rectGO.transform);
        btnRectGO.name = "BtnRectangle";
        tg.boutonRectangle = btnRectGO.GetComponent<Button>();

        // ── Label ballons ─────────────────────────────────────────────────
        var labelBallons = CreateTMP(zoneTicket.transform, "LabelBallons",
            "▼  Grattez les 4 ballons  ▼",
            15f, TXT_GRAY, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(labelBallons, 0.10f, 0.56f, 0.90f, 0.65f);

        // ── 4 ballons en 2x2 ─────────────────────────────────────────────
        // Layout : 2 colonnes × 2 lignes
        float[,] balX = { { 0.05f, 0.53f }, { 0.05f, 0.53f } };
        float[,] balY = { { 0.29f, 0.29f }, { 0.03f, 0.03f } };
        float bW = 0.44f, bH = 0.26f;

        var boutonsBallonsArr = new Button[4];
        var textesBallonsArr  = new TextMeshProUGUI[4];
        var overlaysBallonsArr = new Image[4];

        for (int row = 0; row < 2; row++)
        {
            for (int col = 0; col < 2; col++)
            {
                int idx = row * 2 + col;
                float x0 = balX[row, col];
                float y0 = balY[row, col];
                float x1 = x0 + bW;
                float y1 = y0 + bH;

                // Fond ballon
                var ballon = CreatePanel(zoneTicket.transform,
                    "Ballon" + (idx + 1), PANEL_BALLON,
                    new Vector2(x0, y0), new Vector2(x1, y1));
                CreateBorder(ballon.transform, BORDER_RED, 3f);

                // Texte symbole
                var symBall = CreateTMP(ballon.transform,
                    "SymboleBallon" + (idx + 1), "?",
                    52f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
                SetAnchors(symBall, 0.05f, 0.15f, 0.95f, 0.90f);
                textesBallonsArr[idx] = symBall.GetComponent<TextMeshProUGUI>();

                // Label numéro
                var numLabel = CreateTMP(ballon.transform,
                    "NumBallon" + (idx + 1), (idx + 1).ToString(),
                    12f, TXT_GRAY, TextAlignmentOptions.Center, FontStyles.Normal);
                SetAnchors(numLabel, 0.02f, 0.02f, 0.98f, 0.18f);

                // Overlay grattage
                var ovBall = CreateOverlay(ballon.transform,
                    "OverlayBallon" + (idx + 1), OVERLAY_COLOR, "✦");
                overlaysBallonsArr[idx] = ovBall;

                // Bouton transparent
                var btnBall = CreateTransparentButton(ballon.transform);
                btnBall.name = "BtnBallon" + (idx + 1);
                boutonsBallonsArr[idx] = btnBall.GetComponent<Button>();
            }
        }

        tg.boutonsBallons    = boutonsBallonsArr;
        tg.textesBallons     = textesBallonsArr;
        tg.overlaysBallons   = overlaysBallonsArr;

        // ── Message erreur ────────────────────────────────────────────────
        var erreurTxt = CreateTMP(modal.transform, "Erreur", "",
            19f, Color.red, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(erreurTxt, 0.10f, 0.17f, 0.90f, 0.22f);
        tg.texteErreur = erreurTxt.GetComponent<TextMeshProUGUI>();

        // ── Zone résultat ─────────────────────────────────────────────────
        var resTxt = CreateTMP(modal.transform, "Resultat", "",
            36f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(resTxt, 0.05f, 0.11f, 0.95f, 0.17f);
        tg.texteResultat = resTxt.GetComponent<TextMeshProUGUI>();

        // ── Bouton Acheter ────────────────────────────────────────────────
        var btnAcheter = CreateButton(modal.transform, "BtnAcheter",
            "🎟  Acheter un ticket — $5",
            BTN_RED, 24f, TXT_WHITE,
            new Vector2(0.20f, 0.002f), new Vector2(0.80f, 0.095f));
        tg.boutonAcheter = btnAcheter.GetComponent<Button>();

        // ── Bouton Nouveau ticket (caché au départ) ───────────────────────
        var btnNouveau = CreateButton(modal.transform, "BtnNouveauTicket",
            "🔄  Nouveau ticket",
            BTN_GOLD, 20f, new Color(0.08f, 0.05f, 0f),
            new Vector2(0.28f, 0.002f), new Vector2(0.72f, 0.095f));
        btnNouveau.SetActive(false);
        tg.boutonNouveauTicket = btnNouveau.GetComponent<Button>();

        // ── Bouton Fermer ─────────────────────────────────────────────────
        var btnFermer = CreateButton(modal.transform, "BtnFermer", "✕  Fermer",
            BTN_DARK, 18f, TXT_WHITE,
            new Vector2(0.83f, 0.002f), new Vector2(0.98f, 0.065f));
        tg.boutonFermer = btnFermer.GetComponent<Button>();

        // ── Finalisation ──────────────────────────────────────────────────
        Undo.RegisterCreatedObjectUndo(modal, "Create TicketGrattage UI");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        Selection.activeGameObject = ctrlGO;
        Debug.Log("[TicketGrattage] UI générée avec succès dans la scène.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// Crée un overlay opaque avec un texte centré "GRATTER"
    private static Image CreateOverlay(Transform parent, string name,
        Color color, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        StretchFull(go.GetComponent<RectTransform>());
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        // Texte "GRATTER" au centre
        var t = CreateTMP(go.transform, "LabelGratter", label,
            20f, new Color(0.85f, 0.65f, 0.05f), TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(t, 0f, 0.25f, 1f, 0.75f);

        return img;
    }

    /// Crée un bouton totalement transparent (juste pour capter les clics)
    private static GameObject CreateTransparentButton(Transform parent)
    {
        var go = new GameObject("BtnTransparent",
            typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        StretchFull(go.GetComponent<RectTransform>());
        var img = go.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);  // totalement transparent
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        // Pas d'effet de couleur visibles
        var cols = btn.colors;
        cols.normalColor      = new Color(0, 0, 0, 0);
        cols.highlightedColor = new Color(1, 1, 1, 0.05f);
        cols.pressedColor     = new Color(1, 1, 1, 0.10f);
        btn.colors = cols;
        return go;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color,
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
            go.GetComponent<Image>().color = color;
            go.GetComponent<Image>().raycastTarget = false;
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
        var cols = btn.colors;
        cols.highlightedColor = new Color(
            Mathf.Min(normalColor.r + 0.15f, 1f),
            Mathf.Min(normalColor.g + 0.15f, 1f),
            Mathf.Min(normalColor.b + 0.15f, 1f), 1f);
        cols.pressedColor = new Color(
            Mathf.Max(normalColor.r - 0.15f, 0f),
            Mathf.Max(normalColor.g - 0.15f, 0f),
            Mathf.Max(normalColor.b - 0.15f, 0f), 1f);
        btn.colors = cols;

        var lblGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        lblGO.transform.SetParent(go.transform, false);
        StretchFull(lblGO.GetComponent<RectTransform>());
        var tmp = lblGO.GetComponent<TextMeshProUGUI>();
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

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchors(TMP_Text t, float x0, float y0, float x1, float y1)
        => SetAnchors(t.GetComponent<RectTransform>(), x0, y0, x1, y1);

    private static void SetAnchors(RectTransform rt, float x0, float y0, float x1, float y1)
    {
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
