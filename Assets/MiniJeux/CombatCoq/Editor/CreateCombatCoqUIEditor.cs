using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Outil Editor : génère la hiérarchie UI du mini-jeu Combat de Coq
/// dans la scène active. Menu : Tools > CombatCoq > Create UI in Scene
/// </summary>
public static class CreateCombatCoqUIEditor
{
    // ── Palette ──────────────────────────────────────────────────────────
    private static readonly Color BG_MODAL    = new Color(0.07f, 0.05f, 0.05f, 0.97f);
    private static readonly Color BORDER_RED  = new Color(0.75f, 0.08f, 0.08f, 1f);
    private static readonly Color PANEL_COQ_A = new Color(0.25f, 0.06f, 0.06f, 1f);
    private static readonly Color PANEL_COQ_B = new Color(0.08f, 0.08f, 0.08f, 1f);
    private static readonly Color BTN_RED     = new Color(0.65f, 0.08f, 0.08f, 1f);
    private static readonly Color BTN_DARK    = new Color(0.14f, 0.14f, 0.14f, 1f);
    private static readonly Color BTN_GOLD    = new Color(0.75f, 0.58f, 0.05f, 1f);
    private static readonly Color TXT_WHITE   = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TXT_YELLOW  = new Color(1.00f, 0.90f, 0.30f, 1f);
    private static readonly Color TXT_RED     = new Color(0.90f, 0.20f, 0.20f, 1f);
    private static readonly Color SEPARATOR   = new Color(0.75f, 0.08f, 0.08f, 0.55f);
    private static readonly Color INPUT_BG    = new Color(0.12f, 0.10f, 0.10f, 1f);

    // ── Point d'entrée ────────────────────────────────────────────────────
    [MenuItem("Tools/CombatCoq/Create UI in Scene")]
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

        // ── Controller ────────────────────────────────────────────────────
        var ctrlGO = new GameObject("CombatCoq_Controller");
        var combatCoq = ctrlGO.AddComponent<CombatCoq>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create CombatCoq Controller");

        // ── Modale principale ─────────────────────────────────────────────
        var modalGO = new GameObject("PanneauCombatCoq", typeof(RectTransform), typeof(Image));
        modalGO.transform.SetParent(canvasGO.transform, false);
        var modalRT = modalGO.GetComponent<RectTransform>();
        modalRT.anchorMin = new Vector2(0.08f, 0.04f);
        modalRT.anchorMax = new Vector2(0.92f, 0.96f);
        modalRT.offsetMin = Vector2.zero;
        modalRT.offsetMax = Vector2.zero;
        var modalImg = modalGO.GetComponent<Image>();
        modalImg.color = BG_MODAL;
        modalImg.raycastTarget = false;   // fond du panneau, ne doit pas bloquer les boutons
        CreateBorder(modalGO.transform, BORDER_RED, 3f);
        combatCoq.panneauCombat = modalGO;

        // ── Titre ─────────────────────────────────────────────────────────
        var titre = CreateTMP(modalGO.transform, "Titre", "⚔  COMBAT DE COQ  ⚔",
            52f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(titre.GetComponent<RectTransform>(), 0f, 0.87f, 1f, 0.97f);
        CreateHLine(modalGO.transform, new Vector2(0.03f, 0.862f), new Vector2(0.97f, 0.868f));

        // ── Coq A (gauche) ────────────────────────────────────────────────
        var pA = CreateImagePanel(modalGO.transform, "PanneauCoqA", PANEL_COQ_A,
            new Vector2(0.03f, 0.38f), new Vector2(0.46f, 0.85f));
        CreateBorder(pA.transform, new Color(0.8f, 0.1f, 0.1f, 0.7f), 2f);
        combatCoq.panneauCoqA = pA.GetComponent<Image>();

        var nomA = CreateTMP(pA.transform, "NomCoqA", "Coq Rouge",
            32f, TXT_RED, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(nomA.GetComponent<RectTransform>(), 0f, 0.76f, 1f, 0.97f);
        combatCoq.texteNomCoqA = nomA.GetComponent<TextMeshProUGUI>();

        CreateTMP(pA.transform, "IconCoqA", "🐓",
            72f, Color.white, TextAlignmentOptions.Center, FontStyles.Normal)
            .GetComponent<RectTransform>().SetAnchors(0.1f, 0.38f, 0.9f, 0.78f);

        var coteA = CreateTMP(pA.transform, "CoteCoqA", "x2.50",
            28f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Normal);
        SetAnchors(coteA.GetComponent<RectTransform>(), 0f, 0.24f, 1f, 0.40f);
        combatCoq.texteCoteCoqA = coteA.GetComponent<TextMeshProUGUI>();

        var btnMiserA = CreateButton(pA.transform, "BtnMiserA", "Choisir",
            BTN_RED, 24f, TXT_WHITE, new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.22f));
        combatCoq.boutonMiserA = btnMiserA.GetComponent<Button>();

        // Texte d'effet du coq A (energise)
        var effetA = CreateTMP(pA.transform, "EffetCoqA", "",
            24f, new Color(1f, 0.85f, 0f), TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(effetA.GetComponent<RectTransform>(), 0.1f, 0.40f, 0.9f, 0.50f);
        combatCoq.texteEffetCoqA = effetA.GetComponent<TextMeshProUGUI>();

        // Bouton boisson pour le coq A
        var btnBoissA = CreateButton(pA.transform, "BtnBoissonsA", "Energiser",
            BTN_GOLD, 18f, new Color(0.1f, 0.1f, 0.1f), new Vector2(0.1f, 0.67f), new Vector2(0.9f, 0.75f));
        combatCoq.boutonBoisssonA = btnBoissA.GetComponent<Button>();

        // ── Coq B (droite) ────────────────────────────────────────────────
        var pB = CreateImagePanel(modalGO.transform, "PanneauCoqB", PANEL_COQ_B,
            new Vector2(0.54f, 0.38f), new Vector2(0.97f, 0.85f));
        CreateBorder(pB.transform, new Color(0.4f, 0.4f, 0.4f, 0.7f), 2f);
        combatCoq.panneauCoqB = pB.GetComponent<Image>();

        var nomB = CreateTMP(pB.transform, "NomCoqB", "Coq Noir",
            32f, new Color(0.8f, 0.8f, 0.8f), TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(nomB.GetComponent<RectTransform>(), 0f, 0.76f, 1f, 0.97f);
        combatCoq.texteNomCoqB = nomB.GetComponent<TextMeshProUGUI>();

        CreateTMP(pB.transform, "IconCoqB", "🐓",
            72f, new Color(0.3f, 0.3f, 0.3f), TextAlignmentOptions.Center, FontStyles.Normal)
            .GetComponent<RectTransform>().SetAnchors(0.1f, 0.38f, 0.9f, 0.78f);

        var coteB = CreateTMP(pB.transform, "CoteCoqB", "x1.80",
            28f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Normal);
        SetAnchors(coteB.GetComponent<RectTransform>(), 0f, 0.24f, 1f, 0.40f);
        combatCoq.texteCoteCoqB = coteB.GetComponent<TextMeshProUGUI>();

        var btnMiserB = CreateButton(pB.transform, "BtnMiserB", "Choisir",
            BTN_DARK, 24f, TXT_WHITE, new Vector2(0.1f, 0.04f), new Vector2(0.9f, 0.22f));
        combatCoq.boutonMiserB = btnMiserB.GetComponent<Button>();

        // Texte d'effet du coq B (energise)
        var effetB = CreateTMP(pB.transform, "EffetCoqB", "",
            24f, new Color(1f, 0.85f, 0f), TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(effetB.GetComponent<RectTransform>(), 0.1f, 0.40f, 0.9f, 0.50f);
        combatCoq.texteEffetCoqB = effetB.GetComponent<TextMeshProUGUI>();

        // Bouton boisson pour le coq B
        var btnBoissB = CreateButton(pB.transform, "BtnBoissonsB", "Energiser",
            BTN_GOLD, 18f, new Color(0.1f, 0.1f, 0.1f), new Vector2(0.1f, 0.67f), new Vector2(0.9f, 0.75f));
        combatCoq.boutonBoissonB = btnBoissB.GetComponent<Button>();

        // ── VS ────────────────────────────────────────────────────────────
        var vs = CreateTMP(modalGO.transform, "VS", "VS",
            48f, TXT_RED, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(vs.GetComponent<RectTransform>(), 0.44f, 0.56f, 0.56f, 0.70f);

        // ── Zone mise ─────────────────────────────────────────────────────
        var zoneMise = CreateImagePanel(modalGO.transform, "ZoneMise",
            new Color(0.10f, 0.07f, 0.07f, 1f),
            new Vector2(0.03f, 0.22f), new Vector2(0.97f, 0.37f));

        var labelMise = CreateTMP(zoneMise.transform, "LabelMise", "Mise ($)  —  minimum 5$",
            20f, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(labelMise.GetComponent<RectTransform>(), 0.02f, 0.50f, 0.42f, 0.95f);

        // InputField
        var inputGO = new GameObject("InputMise", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        inputGO.transform.SetParent(zoneMise.transform, false);
        SetAnchors(inputGO.GetComponent<RectTransform>(), 0.44f, 0.08f, 0.82f, 0.92f);
        var inputImg = inputGO.GetComponent<Image>();
        inputImg.color = INPUT_BG;
        // raycastTarget doit rester true sur l'InputField
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
        phTMP.text = "Entrez votre mise...";
        phTMP.fontSize = 22f;
        phTMP.color = new Color(0.5f, 0.5f, 0.5f);
        phTMP.fontStyle = FontStyles.Italic;
        phTMP.alignment = TextAlignmentOptions.MidlineLeft;
        phTMP.raycastTarget = false;
        inputField.placeholder = phTMP;
        combatCoq.inputMise = inputField;

        var argentTxt = CreateTMP(zoneMise.transform, "ArgentDisponible", "Argent : $0",
            20f, TXT_YELLOW, TextAlignmentOptions.MidlineRight, FontStyles.Normal);
        SetAnchors(argentTxt.GetComponent<RectTransform>(), 0.65f, 0.04f, 0.98f, 0.96f);
        combatCoq.texteArgentDisponible = argentTxt.GetComponent<TextMeshProUGUI>();

        var erreurTxt = CreateTMP(zoneMise.transform, "ErreurMise", "",
            20f, Color.red, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(erreurTxt.GetComponent<RectTransform>(), 0.02f, 0.01f, 0.63f, 0.46f);
        combatCoq.texteErreurMise = erreurTxt.GetComponent<TextMeshProUGUI>();

        // ── Bouton Lancer ─────────────────────────────────────────────────
        var btnLancer = CreateButton(modalGO.transform, "BtnLancer", "⚔  LANCER LE COMBAT",
            BTN_RED, 26f, TXT_WHITE, new Vector2(0.25f, 0.13f), new Vector2(0.75f, 0.21f));
        combatCoq.boutonLancer = btnLancer.GetComponent<Button>();

        // ── Bouton Rejouer ────────────────────────────────────────────────
        var btnRejouer = CreateButton(modalGO.transform, "BtnRejouer", "🔄  Nouveau combat",
            BTN_GOLD, 22f, new Color(0.1f, 0.05f, 0f),
            new Vector2(0.25f, 0.03f), new Vector2(0.75f, 0.11f));
        combatCoq.boutonRejouer = btnRejouer.GetComponent<Button>();
        btnRejouer.SetActive(false);

        // ── Bouton Fermer ─────────────────────────────────────────────────
        var btnFermer = CreateButton(modalGO.transform, "BtnFermer", "✕  Fermer",
            BTN_DARK, 18f, TXT_WHITE, new Vector2(0.82f, 0.002f), new Vector2(0.98f, 0.065f));
        combatCoq.boutonFermer = btnFermer.GetComponent<Button>();

        // ── Zone résultat ─────────────────────────────────────────────────
        var zoneRes = CreateImagePanel(modalGO.transform, "ZoneResultat",
            new Color(0.04f, 0.02f, 0.02f, 0.97f),
            new Vector2(0.15f, 0.30f), new Vector2(0.85f, 0.70f));
        zoneRes.GetComponent<Image>().raycastTarget = false;
        CreateBorder(zoneRes.transform, BORDER_RED, 4f);
        zoneRes.SetActive(false);   // masquée au départ, visible quand on a le résultat
        combatCoq.zoneResultat = zoneRes;

        var resultatTxt = CreateTMP(zoneRes.transform, "Resultat", "",
            36f, TXT_WHITE, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(resultatTxt.GetComponent<RectTransform>(), 0.05f, 0.52f, 0.95f, 0.92f);
        combatCoq.texteResultat = resultatTxt.GetComponent<TextMeshProUGUI>();

        var gainTxt = CreateTMP(zoneRes.transform, "Gain", "",
            44f, Color.green, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(gainTxt.GetComponent<RectTransform>(), 0.05f, 0.10f, 0.95f, 0.52f);
        combatCoq.texteGainResultat = gainTxt.GetComponent<TextMeshProUGUI>();

        // ── Finalisation ──────────────────────────────────────────────────
        Undo.RegisterCreatedObjectUndo(modalGO, "Create CombatCoq UI");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        Selection.activeGameObject = ctrlGO;
        Debug.Log("[CombatCoq] UI générée avec succès dans la scène.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────

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
        img.raycastTarget = false;   // ne pas bloquer les clics sur les boutons enfants
        return go;
    }

    private static void CreateBorder(Transform parent, Color color, float w)
    {
        string[] n = { "Border_T", "Border_B", "Border_L", "Border_R" };
        float p = w / 1000f;
        Vector2[] mn = { new(0,1-p), new(0,0), new(0,0), new(1-p,0) };
        Vector2[] mx = { new(1,1),   new(1,p), new(p,1), new(1,1)   };
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject(n[i], typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = mn[i]; rt.anchorMax = mx[i];
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;   // bordure purement décorative
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
        tmp.raycastTarget = false;   // texte décoratif, ne pas bloquer les clics
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
        // raycastTarget doit rester true sur le bouton (c'est lui qui reçoit les clics)

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;                    // ← indispensable pour que le bouton réponde
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
        tmp.raycastTarget = false;   // le label ne doit pas bloquer le clic sur le bouton parent
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
        img.raycastTarget = false;   // séparateur décoratif
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

// Extension pour appel fluent sur RectTransform
public static class RectTransformExt
{
    public static void SetAnchors(this RectTransform rt, float x0, float y0, float x1, float y1)
    {
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
