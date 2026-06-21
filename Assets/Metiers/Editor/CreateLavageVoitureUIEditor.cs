using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Outil Editor : génère la hiérarchie UI du mini-jeu Lavage Voiture
/// dans la scène active. Menu : Tools > MiniGames > Create Lavage Voiture UI
/// </summary>
public static class CreateLavageVoitureUIEditor
{
    // ── Palette (charte sombre, aqua/bleu pour le lavage) ─────────────────
    private static readonly Color BG_MODAL     = new Color(0.03f, 0.04f, 0.06f, 0.97f);
    private static readonly Color BORDER_AQUA  = new Color(0.10f, 0.65f, 0.80f, 1f);
    private static readonly Color BORDER_GOLD  = new Color(0.75f, 0.58f, 0.05f, 1f);
    private static readonly Color PANEL_DARK   = new Color(0.05f, 0.07f, 0.09f, 1f);
    private static readonly Color PANEL_CAR    = new Color(0.06f, 0.09f, 0.13f, 1f);
    private static readonly Color BTN_START    = new Color(0.05f, 0.45f, 0.70f, 1f);
    private static readonly Color BTN_REPLAY   = new Color(0.15f, 0.45f, 0.10f, 1f);
    private static readonly Color BTN_DARK     = new Color(0.14f, 0.14f, 0.18f, 1f);
    private static readonly Color TXT_WHITE    = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TXT_YELLOW   = new Color(1.00f, 0.85f, 0.20f, 1f);
    private static readonly Color TXT_GRAY     = new Color(0.60f, 0.60f, 0.60f, 1f);
    private static readonly Color SEPARATOR    = new Color(0.10f, 0.65f, 0.80f, 0.55f);
    private static readonly Color CAR_DIRTY    = new Color(0.42f, 0.35f, 0.25f, 1f);
    private static readonly Color CAR_CLEAN    = new Color(0.20f, 0.55f, 0.80f, 1f);
    private static readonly Color SPONGE_COLOR = new Color(0.95f, 0.80f, 0.10f, 1f);
    private static readonly Color SLIDER_BG    = new Color(0.10f, 0.12f, 0.16f, 1f);
    private static readonly Color SLIDER_FILL  = new Color(0.10f, 0.70f, 0.85f, 1f);

    // ── Point d'entrée ─────────────────────────────────────────────────────
    [MenuItem("Tools/MiniGames/Create Lavage Voiture UI")]
    public static void CreateUI()
    {
        // ── Canvas ──────────────────────────────────────────────────────────
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

        // ── Controller ──────────────────────────────────────────────────────
        var ctrlGO = new GameObject("LavageVoiture_Controller");
        var lv = ctrlGO.AddComponent<LavageVoiture>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create LavageVoiture Controller");

        // Auto-assigner dans InteractionSystem dès la génération
        var interSys = Object.FindFirstObjectByType<InteractionSystem>();
        if (interSys != null)
        {
            interSys.lavageVoiture = lv;
            EditorUtility.SetDirty(interSys);
            Debug.Log("[LavageVoiture] Assigné automatiquement dans InteractionSystem.");
        }
        else
            Debug.LogWarning("[LavageVoiture] InteractionSystem introuvable – assignez manuellement le champ lavageVoiture.");

        // ── Panneau principal ───────────────────────────────────────────────
        var modalGO = new GameObject("PanneauLavage", typeof(RectTransform), typeof(Image));
        modalGO.transform.SetParent(canvasGO.transform, false);
        var modalRT = modalGO.GetComponent<RectTransform>();
        modalRT.anchorMin = new Vector2(0.08f, 0.04f);
        modalRT.anchorMax = new Vector2(0.92f, 0.96f);
        modalRT.offsetMin = modalRT.offsetMax = Vector2.zero;
        modalGO.GetComponent<Image>().color = BG_MODAL;
        CreateBorder(modalGO.transform, BORDER_AQUA, 3f);
        lv.panneauLavage = modalGO;

        // Titre
        var titre = CreateTMP(modalGO.transform, "Titre", "LAVAGE VOITURE",
            48f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(titre.GetComponent<RectTransform>(), 0f, 0.89f, 1f, 0.98f);
        CreateHLine(modalGO.transform, new Vector2(0.03f, 0.882f), new Vector2(0.97f, 0.888f));

        // ── Bandeau infos (argent + timer) ──────────────────────────────────
        var bandeauInfos = CreateImagePanel(modalGO.transform, "BandeauInfos", PANEL_DARK,
            new Vector2(0.03f, 0.83f), new Vector2(0.97f, 0.88f));

        var argentTxt = CreateTMP(bandeauInfos.transform, "TexteArgent", "Argent : $0",
            20f, TXT_YELLOW, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(argentTxt.GetComponent<RectTransform>(), 0.01f, 0.08f, 0.45f, 0.92f);
        lv.texteArgent = argentTxt.GetComponent<TextMeshProUGUI>();

        var timerTxt = CreateTMP(bandeauInfos.transform, "TexteTimer", "1:30",
            28f, TXT_WHITE, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        SetAnchors(timerTxt.GetComponent<RectTransform>(), 0.55f, 0.08f, 0.99f, 0.92f);
        lv.texteTimer = timerTxt.GetComponent<TextMeshProUGUI>();

        // ── Instruction ─────────────────────────────────────────────────────
        var instrTxt = CreateTMP(modalGO.transform, "TexteInstruction",
            "Maintenez le clic et frottez la voiture !",
            16f, TXT_GRAY, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(instrTxt.GetComponent<RectTransform>(), 0.03f, 0.795f, 0.97f, 0.830f);
        lv.texteInstruction = instrTxt.GetComponent<TextMeshProUGUI>();

        // ── Zone centrale voiture ───────────────────────────────────────────
        var zoneGO = CreateImagePanel(modalGO.transform, "ZoneVoiture", PANEL_CAR,
            new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.790f));
        CreateBorder(zoneGO.transform, BORDER_AQUA, 2f);

        // VoitureSale (rectangle gris/brun opaque – disparaît progressivement)
        var salePanelGO = new GameObject("VoitureSale", typeof(RectTransform), typeof(Image));
        salePanelGO.transform.SetParent(zoneGO.transform, false);
        var saleRT = salePanelGO.GetComponent<RectTransform>();
        saleRT.anchorMin = new Vector2(0.05f, 0.10f);
        saleRT.anchorMax = new Vector2(0.95f, 0.90f);
        saleRT.offsetMin = saleRT.offsetMax = Vector2.zero;
        var saleImg = salePanelGO.GetComponent<Image>();
        saleImg.color = CAR_DIRTY;
        lv.voitureSale = saleImg;

        // Label voiture sale
        var saleLbl = CreateTMP(salePanelGO.transform, "Label", "VOITURE SALE",
            22f, new Color(0.8f, 0.6f, 0.4f, 1f), TextAlignmentOptions.Center, FontStyles.Bold);
        StretchFull(saleLbl.GetComponent<RectTransform>());
        saleLbl.GetComponent<TextMeshProUGUI>().raycastTarget = false;

        // VoiturePropre (rectangle bleu – apparaît progressivement, alpha 0 au départ)
        var propreGO = new GameObject("VoiturePropre", typeof(RectTransform), typeof(Image));
        propreGO.transform.SetParent(zoneGO.transform, false);
        var propreRT = propreGO.GetComponent<RectTransform>();
        propreRT.anchorMin = new Vector2(0.05f, 0.10f);
        propreRT.anchorMax = new Vector2(0.95f, 0.90f);
        propreRT.offsetMin = propreRT.offsetMax = Vector2.zero;
        var propreImg = propreGO.GetComponent<Image>();
        Color clr = CAR_CLEAN; clr.a = 0f; propreImg.color = clr;
        lv.voiturePropre = propreImg;

        // Label voiture propre
        var propreLbl = CreateTMP(propreGO.transform, "Label", "VOITURE PROPRE",
            22f, new Color(0.7f, 0.95f, 1f, 1f), TextAlignmentOptions.Center, FontStyles.Bold);
        StretchFull(propreLbl.GetComponent<RectTransform>());
        propreLbl.GetComponent<TextMeshProUGUI>().raycastTarget = false;

        // voitureRect = la zone de détection (même que zoneGO)
        lv.voitureRect = zoneGO.GetComponent<RectTransform>();

        // Éponge (petit carré jaune qui suit la souris, caché par défaut)
        var epongeGO = new GameObject("EpongeIcon", typeof(RectTransform), typeof(Image));
        epongeGO.transform.SetParent(zoneGO.transform, false);
        var epongeRT = epongeGO.GetComponent<RectTransform>();
        epongeRT.anchorMin = epongeRT.anchorMax = new Vector2(0.5f, 0.5f);
        epongeRT.pivot = new Vector2(0.5f, 0.5f);
        epongeRT.sizeDelta = new Vector2(40f, 40f);
        epongeRT.anchoredPosition = Vector2.zero;
        epongeGO.GetComponent<Image>().color = SPONGE_COLOR;
        var epongeLbl = CreateTMP(epongeGO.transform, "Label", "~",
            18f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
        StretchFull(epongeLbl.GetComponent<RectTransform>());
        epongeLbl.GetComponent<TextMeshProUGUI>().raycastTarget = false;
        epongeGO.SetActive(false);
        lv.epongeIcon = epongeRT;

        // ── Barre de propreté ───────────────────────────────────────────────
        var barrePanelGO = CreateImagePanel(modalGO.transform, "PanneauBarre", PANEL_DARK,
            new Vector2(0.08f, 0.135f), new Vector2(0.78f, 0.196f));

        // Label "%"
        var pctTxt = CreateTMP(barrePanelGO.transform, "TextePourcentage", "0%",
            22f, TXT_WHITE, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        SetAnchors(pctTxt.GetComponent<RectTransform>(), 0.82f, 0.08f, 0.99f, 0.92f);
        lv.textePourcentage = pctTxt.GetComponent<TextMeshProUGUI>();

        // Slider
        var sliderGO = new GameObject("BarrePropirete", typeof(RectTransform), typeof(Slider));
        sliderGO.transform.SetParent(barrePanelGO.transform, false);
        SetAnchors(sliderGO.GetComponent<RectTransform>(), 0.01f, 0.15f, 0.80f, 0.85f);

        // Background
        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(sliderGO.transform, false);
        StretchFull(bgGO.GetComponent<RectTransform>());
        bgGO.GetComponent<Image>().color = SLIDER_BG;

        // Fill Area
        var fillAreaGO = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaGO.transform.SetParent(sliderGO.transform, false);
        var fillAreaRT = fillAreaGO.GetComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero; fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = Vector2.zero; fillAreaRT.offsetMax = Vector2.zero;

        // Fill
        var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(0f, 1f);
        fillRT.sizeDelta = new Vector2(0f, 0f);
        fillGO.GetComponent<Image>().color = SLIDER_FILL;

        // Wirer le Slider
        var slider = sliderGO.GetComponent<Slider>();
        slider.fillRect = fillRT;
        slider.targetGraphic = fillGO.GetComponent<Image>();
        slider.minValue = 0f; slider.maxValue = 100f; slider.value = 0f;
        slider.direction = Slider.Direction.LeftToRight;
        slider.interactable = false;
        lv.barrePropirete = slider;

        // ── Zone résultat ───────────────────────────────────────────────────
        var panRes = CreateImagePanel(modalGO.transform, "PanneauResultat", PANEL_DARK,
            new Vector2(0.10f, 0.11f), new Vector2(0.90f, 0.133f));
        CreateBorder(panRes.transform, BORDER_GOLD, 1f);

        var resultatTxt = CreateTMP(panRes.transform, "TexteResultat", "",
            22f, TXT_WHITE, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(resultatTxt.GetComponent<RectTransform>(), 0.02f, 0.52f, 0.62f, 1f);
        lv.texteResultat = resultatTxt.GetComponent<TextMeshProUGUI>();

        var gainTxt = CreateTMP(panRes.transform, "TexteGain", "",
            17f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Normal);
        SetAnchors(gainTxt.GetComponent<RectTransform>(), 0.02f, 0f, 0.62f, 0.50f);
        lv.texteGain = gainTxt.GetComponent<TextMeshProUGUI>();

        // ── Bouton Commencer ────────────────────────────────────────────────
        var btnCom = CreateButton(modalGO.transform, "BoutonCommencer",
            "COMMENCER", BTN_START, 20f, TXT_WHITE,
            new Vector2(0.30f, 0.028f), new Vector2(0.70f, 0.108f));
        lv.boutonCommencer = btnCom.GetComponent<Button>();

        // ── Bouton Rejouer (caché au départ) ────────────────────────────────
        var btnRej = CreateButton(modalGO.transform, "BoutonRejouer",
            "REJOUER", BTN_REPLAY, 18f, TXT_WHITE,
            new Vector2(0.04f, 0.028f), new Vector2(0.44f, 0.108f));
        lv.boutonRejouer = btnRej.GetComponent<Button>();
        btnRej.SetActive(false);

        // ── Bouton Fermer (petit, coin bas-droit) ───────────────────────────
        var btnFer = CreateButton(modalGO.transform, "BoutonFermer",
            "x  Fermer", BTN_DARK, 18f, TXT_WHITE,
            new Vector2(0.83f, 0.002f), new Vector2(0.98f, 0.065f));
        lv.boutonFermer = btnFer.GetComponent<Button>();

        // ── Finalisation ────────────────────────────────────────────────────
        // NE PAS désactiver modalGO ici : c'est Start() du runtime qui le fait.
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        Undo.RegisterCreatedObjectUndo(modalGO, "Create LavageVoiture UI");
        Undo.RegisterCreatedObjectUndo(ctrlGO,  "Create LavageVoiture Controller");
        EditorUtility.SetDirty(lv);
        EditorUtility.SetDirty(canvasGO);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = modalGO;
        Debug.Log("[LavageVoiture] UI générée avec succès. Sauvegardez avec Ctrl+S.");
    }

    // ── Helpers (mêmes signatures que LivreurPizza / CaseOpening) ─────────

    private static GameObject CreateImagePanel(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static void CreateBorder(Transform parent, Color color, float w)
    {
        string[] n = { "Border_T", "Border_B", "Border_L", "Border_R" };
        float p = w / 1000f;
        Vector2[] mn = { new(0, 1 - p), new(0, 0), new(0, 0), new(1 - p, 0) };
        Vector2[] mx = { new(1, 1),     new(1, p), new(p, 1), new(1, 1)     };
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject(n[i], typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = mn[i]; rt.anchorMax = mx[i];
            rt.offsetMin = rt.offsetMax = Vector2.zero;
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
        rt.offsetMin = rt.offsetMax = Vector2.zero;

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
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = SEPARATOR;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchors(RectTransform rt, float x0, float y0, float x1, float y1)
    {
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
