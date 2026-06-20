using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Outil Editor : génère la hiérarchie UI du DAB dans la scène active.
/// Menu : Tools / DAB / Create UI in Scene
/// </summary>
public static class CreateDABUIEditor
{
    // ── Palette (charte visuelle commune) ────────────────────────────────────
    private static readonly Color BG_MODAL   = new Color(0.05f, 0.04f, 0.06f, 0.97f);
    private static readonly Color BORDER_COL = new Color(0.08f, 0.45f, 0.55f, 1f);
    private static readonly Color PANEL_DARK = new Color(0.08f, 0.07f, 0.10f, 1f);
    private static readonly Color BTN_BLUE   = new Color(0.08f, 0.35f, 0.65f, 1f);
    private static readonly Color BTN_GREEN  = new Color(0.08f, 0.45f, 0.08f, 1f);
    private static readonly Color BTN_DARK   = new Color(0.14f, 0.14f, 0.14f, 1f);
    private static readonly Color TXT_WHITE  = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TXT_YELLOW = new Color(1.00f, 0.90f, 0.30f, 1f);
    private static readonly Color TXT_CYAN   = new Color(0.30f, 0.85f, 0.90f, 1f);
    private static readonly Color TXT_RED    = new Color(0.90f, 0.20f, 0.20f, 1f);
    private static readonly Color TXT_GREEN  = new Color(0.30f, 0.90f, 0.30f, 1f);
    private static readonly Color SEPARATOR  = new Color(0.08f, 0.45f, 0.55f, 0.55f);
    private static readonly Color INPUT_BG   = new Color(0.12f, 0.10f, 0.14f, 1f);

    [MenuItem("Tools/DAB/Create UI in Scene")]
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

        // ── Controller ────────────────────────────────────────────────────────
        var ctrlGO = new GameObject("DAB_Controller");
        var dab = ctrlGO.AddComponent<DAB>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create DAB Controller");

        // ── Modale principale ─────────────────────────────────────────────────
        var modal = CreatePanel("PanneauDAB", canvasGO.transform, BG_MODAL,
            new Vector2(0.20f, 0.06f), new Vector2(0.80f, 0.94f));
        CreateBorder(modal.transform, BORDER_COL, 3f);
        dab.panneauDAB = modal;

        // Titre
        var titre = CreateTMP(modal.transform, "Titre", "DISTRIBUTEUR AUTOMATIQUE",
            36f, TXT_CYAN, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(titre.rectTransform, 0f, 0.89f, 1f, 0.98f);
        CreateHLine(modal.transform, new Vector2(0.03f, 0.875f), new Vector2(0.97f, 0.880f));

        // ── Bloc informations ─────────────────────────────────────────────────
        var blocInfo = CreatePanel("BlocInfos", modal.transform, PANEL_DARK,
            new Vector2(0.04f, 0.60f), new Vector2(0.96f, 0.87f));
        CreateBorder(blocInfo.transform, new Color(0.15f, 0.15f, 0.20f, 1f), 1f);

        var argentPoche = CreateTMP(blocInfo.transform, "ArgentPoche", "En poche : $0",
            22f, TXT_YELLOW, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(argentPoche.rectTransform, 0.03f, 0.65f, 0.97f, 0.95f);
        dab.texteArgentPoche = argentPoche;

        var compte = CreateTMP(blocInfo.transform, "Compte", "Compte : $0",
            22f, TXT_WHITE, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(compte.rectTransform, 0.03f, 0.35f, 0.97f, 0.65f);
        dab.texteCompte = compte;

        CreateHLine(blocInfo.transform, new Vector2(0.03f, 0.33f), new Vector2(0.97f, 0.335f));

        var dette = CreateTMP(blocInfo.transform, "Dette", "Pret a rembourser : $35 000",
            24f, TXT_RED, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        SetAnchors(dette.rectTransform, 0.03f, 0.05f, 0.97f, 0.33f);
        dab.texteDette = dette;

        // ── Barre de progression ──────────────────────────────────────────────
        var progres = CreateTMP(modal.transform, "Progres", "Rembourse : 0% ($0 / $35 000)",
            18f, TXT_WHITE, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(progres.rectTransform, 0.04f, 0.55f, 0.96f, 0.60f);
        dab.texteProgresRemboursement = progres;

        // ── Saisie montant ────────────────────────────────────────────────────
        var zoneSaisie = CreatePanel("ZoneSaisie", modal.transform, PANEL_DARK,
            new Vector2(0.04f, 0.38f), new Vector2(0.96f, 0.54f));
        CreateBorder(zoneSaisie.transform, new Color(0.15f, 0.15f, 0.20f, 1f), 1f);

        var labelSaisie = CreateTMP(zoneSaisie.transform, "LabelSaisie", "Montant ($)",
            20f, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(labelSaisie.rectTransform, 0.03f, 0.50f, 0.30f, 0.95f);

        var inputGO = new GameObject("InputMontant", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        inputGO.transform.SetParent(zoneSaisie.transform, false);
        SetAnchors(inputGO.GetComponent<RectTransform>(), 0.32f, 0.08f, 0.97f, 0.92f);
        inputGO.GetComponent<Image>().color = INPUT_BG;
        var inputField = inputGO.GetComponent<TMP_InputField>();
        inputField.contentType = TMP_InputField.ContentType.DecimalNumber;

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
        phTMP.text = "Ex : 1000";
        phTMP.fontSize = 22f;
        phTMP.color = new Color(0.5f, 0.5f, 0.5f);
        phTMP.fontStyle = FontStyles.Italic;
        phTMP.alignment = TextAlignmentOptions.MidlineLeft;
        phTMP.raycastTarget = false;
        inputField.placeholder = phTMP;
        dab.inputMontant = inputField;

        // ── Erreur ────────────────────────────────────────────────────────────
        var erreur = CreateTMP(modal.transform, "Erreur", "",
            18f, TXT_RED, TextAlignmentOptions.MidlineLeft, FontStyles.Italic);
        SetAnchors(erreur.rectTransform, 0.04f, 0.33f, 0.96f, 0.38f);
        dab.texteErreur = erreur;

        // ── Boutons Deposer / Retirer ─────────────────────────────────────────
        var btnDeposer = CreateButton(modal.transform, "BtnDeposer",
            "DEPOSER  (rembourser la dette)",
            BTN_GREEN, 20f, TXT_WHITE,
            new Vector2(0.04f, 0.20f), new Vector2(0.96f, 0.32f));
        dab.boutonDeposer = btnDeposer.GetComponent<Button>();

        var btnRetirer = CreateButton(modal.transform, "BtnRetirer",
            "RETIRER  (depuis le compte)",
            BTN_BLUE, 20f, TXT_WHITE,
            new Vector2(0.04f, 0.07f), new Vector2(0.96f, 0.19f));
        dab.boutonRetirer = btnRetirer.GetComponent<Button>();

        // ── Bouton Fermer ─────────────────────────────────────────────────────
        var btnFermer = CreateButton(modal.transform, "BtnFermer", "Fermer",
            BTN_DARK, 18f, TXT_WHITE,
            new Vector2(0.35f, 0.01f), new Vector2(0.65f, 0.065f));
        dab.boutonFermer = btnFermer.GetComponent<Button>();

        // ── Masquer par défaut ────────────────────────────────────────────────
        modal.SetActive(false);

        Undo.RegisterCreatedObjectUndo(modal, "Create DAB UI");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        Selection.activeGameObject = ctrlGO;
        Debug.Log("[DAB] UI generee avec succes dans la scene.");
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
