using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using MenuGame;

/// <summary>
/// Genere le menu pause (style page d'accueil) dans la scene active.
/// Menu : Tools / MenuGame / Create Pause Menu
/// </summary>
public static class CreatePauseMenuUIEditor
{
    private static readonly Color PANEL_DARK   = new Color(0.06f, 0.06f, 0.09f, 0.98f);
    private static readonly Color BTN_NORMAL   = new Color(0f, 0f, 0f, 0f);
    private static readonly Color TEXT_WHITE   = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TEXT_RED     = new Color(0.82f, 0.06f, 0.06f, 1f);
    private static readonly Color SEPARATOR    = new Color(0.82f, 0.06f, 0.06f, 0.6f);
    private static readonly Color OVERLAY      = new Color(0f, 0f, 0f, 0.88f);

    [MenuItem("Tools/MenuGame/Create Pause Menu")]
    public static void CreatePauseMenu()
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
            canvas.sortingOrder = 20;
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
            if (t != null && (t.name == "PauseOverlay" || t.name == "PauseMenu_Controller"))
                Undo.DestroyObjectImmediate(t.gameObject);
        }

        var ctrlGO = new GameObject("PauseMenu_Controller");
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        var pause = ctrlGO.AddComponent<PauseMenu>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create PauseMenu");

        // ── Overlay plein ecran ────────────────────────────────────────────
        var overlay = new GameObject("PauseOverlay", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvasGO.transform, false);
        StretchFull(overlay.GetComponent<RectTransform>());
        overlay.GetComponent<Image>().color = OVERLAY;
        pause.overlay = overlay;

        // ── Titre PAUSE (avec glitch) ──────────────────────────────────────
        var titre = CreateTMP(overlay.transform, "TitrePause", "PAUSE",
            120f, TEXT_WHITE, TextAlignmentOptions.Center, FontStyles.Bold);
        var trt = titre.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.2f, 0.70f);
        trt.anchorMax = new Vector2(0.8f, 0.88f);
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        titre.gameObject.AddComponent<GlitchTextEffect>();

        CreateHLine(overlay.transform, new Vector2(0.34f, 0.685f), new Vector2(0.66f, 0.69f));

        // ── Groupe de boutons ──────────────────────────────────────────────
        var groupe = new GameObject("GroupeBoutons", typeof(RectTransform));
        groupe.transform.SetParent(overlay.transform, false);
        var grt = groupe.GetComponent<RectTransform>();
        grt.anchorMin = new Vector2(0.36f, 0.12f);
        grt.anchorMax = new Vector2(0.64f, 0.66f);
        grt.offsetMin = Vector2.zero; grt.offsetMax = Vector2.zero;
        pause.groupeBoutons = groupe;

        string[] labels = { "REPRENDRE", "NOUVELLE PARTIE", "OPTIONS", "QUITTER" };
        float[] anchorsY = { 0.74f, 0.52f, 0.30f, 0.08f };
        var boutons = new GameObject[4];
        for (int i = 0; i < labels.Length; i++)
            boutons[i] = CreateMenuButton(groupe.transform, labels[i], anchorsY[i]);

        foreach (var b in boutons)
            b.AddComponent<MenuButtonHoverEffect>();

        // ── Sous-panneau Options ───────────────────────────────────────────
        var optionsPanel = new GameObject("PauseOptionsPanel", typeof(RectTransform), typeof(Image));
        optionsPanel.transform.SetParent(overlay.transform, false);
        var ort = optionsPanel.GetComponent<RectTransform>();
        ort.anchorMin = new Vector2(0.30f, 0.12f);
        ort.anchorMax = new Vector2(0.70f, 0.80f);
        ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;
        optionsPanel.GetComponent<Image>().color = PANEL_DARK;
        pause.panneauOptions = optionsPanel;

        var optTitre = CreateTMP(optionsPanel.transform, "OptionsTitle", "OPTIONS",
            48f, TEXT_RED, TextAlignmentOptions.Center, FontStyles.Bold);
        var otRT = optTitre.GetComponent<RectTransform>();
        otRT.anchorMin = new Vector2(0f, 0.88f); otRT.anchorMax = new Vector2(1f, 0.99f);
        otRT.offsetMin = Vector2.zero; otRT.offsetMax = Vector2.zero;

        var optController = optionsPanel.AddComponent<OptionsMenuController>();
        LayoutOptions(optionsPanel.transform);
        AssignOptionsReferences(optController, optionsPanel.transform);

        var btnRetour = CreateMenuButton(optionsPanel.transform, "← RETOUR", 0.04f, 34f);
        btnRetour.AddComponent<MenuButtonHoverEffect>();

        optionsPanel.SetActive(false);

        // ── Cablage des boutons ────────────────────────────────────────────
        Wire(boutons[0], pause.Reprendre);
        Wire(boutons[1], pause.NouvellePartie);
        Wire(boutons[2], pause.OuvrirOptions);
        Wire(boutons[3], pause.Quitter);
        Wire(btnRetour, pause.FermerOptions);

        overlay.SetActive(false);

        Undo.RegisterCreatedObjectUndo(overlay, "Create Pause Menu UI");
        Selection.activeGameObject = ctrlGO;
        Debug.Log("[PauseMenu] Menu pause genere. Echap pour l'ouvrir en jeu.");
    }

    // ── Helpers (style page d'accueil) ─────────────────────────────────────

    private static GameObject CreateMenuButton(Transform parent, string label, float anchorY, float fontSize = 46f)
    {
        var btnGO = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, anchorY);
        rt.anchorMax = new Vector2(1f, anchorY + 0.16f);
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
        btn.transition = Selectable.Transition.ColorTint;

        var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(btnGO.transform, false);
        StretchFull(textGO.GetComponent<RectTransform>());
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = TEXT_WHITE;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return btnGO;
    }

    private static void LayoutOptions(Transform parent)
    {
        const float L = 50f, R = -50f;

        var volLabel = CreateTMP(parent, "VolumeLabel", "VOLUME", 26f, TEXT_WHITE, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        Anchor(volLabel.GetComponent<RectTransform>(), 0f, 0.74f, 1f, 0.82f, L, R);

        var sliderGO = new GameObject("VolumeSlider", typeof(RectTransform));
        sliderGO.transform.SetParent(parent, false);
        Anchor(sliderGO.GetComponent<RectTransform>(), 0f, 0.66f, 1f, 0.73f, L, R);
        var slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f;
        slider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        StyleSlider(slider);

        var fsLabel = CreateTMP(parent, "FullscreenLabel", "PLEIN ECRAN", 26f, TEXT_WHITE, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        Anchor(fsLabel.GetComponent<RectTransform>(), 0f, 0.54f, 1f, 0.62f, L, R);

        var toggleGO = new GameObject("FullscreenToggle", typeof(RectTransform));
        toggleGO.transform.SetParent(parent, false);
        Anchor(toggleGO.GetComponent<RectTransform>(), 0f, 0.46f, 0.5f, 0.53f, L, 0f);
        var toggle = toggleGO.AddComponent<Toggle>();
        toggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        StyleToggle(toggle);

        var qLabel = CreateTMP(parent, "QualityLabel", "QUALITE", 26f, TEXT_WHITE, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        Anchor(qLabel.GetComponent<RectTransform>(), 0f, 0.34f, 1f, 0.42f, L, R);

        var dropGO = new GameObject("QualityDropdown", typeof(RectTransform));
        dropGO.transform.SetParent(parent, false);
        Anchor(dropGO.GetComponent<RectTransform>(), 0f, 0.24f, 1f, 0.33f, L, R);
        var dropdown = dropGO.AddComponent<TMP_Dropdown>();
        StyleDropdown(dropdown);
    }

    private static void AssignOptionsReferences(OptionsMenuController ctrl, Transform panel)
    {
        ctrl.volumeSlider     = panel.Find("VolumeSlider")?.GetComponent<Slider>();
        ctrl.fullscreenToggle = panel.Find("FullscreenToggle")?.GetComponent<Toggle>();
        ctrl.qualityDropdown  = panel.Find("QualityDropdown")?.GetComponent<TMP_Dropdown>();
    }

    private static void Wire(GameObject btnGO, UnityEngine.Events.UnityAction action)
    {
        var btn = btnGO.GetComponent<Button>();
        if (btn != null)
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
    }

    // ── Style controls (repris du menu principal) ──────────────────────────

    private static void StyleSlider(Slider slider)
    {
        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(slider.transform, false);
        StretchFull(bgGO.GetComponent<RectTransform>());
        bgGO.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(slider.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.25f); faRT.anchorMax = new Vector2(1f, 0.75f);
        faRT.offsetMin = new Vector2(5f, 0f); faRT.offsetMax = new Vector2(-15f, 0f);

        var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(fillArea.transform, false);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = Vector2.zero; fillRT.offsetMax = Vector2.zero;
        fillGO.GetComponent<Image>().color = TEXT_RED;
        slider.fillRect = fillRT;

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(slider.transform, false);
        var haRT = handleArea.GetComponent<RectTransform>();
        StretchFull(haRT);
        haRT.offsetMin = new Vector2(10f, 0f); haRT.offsetMax = new Vector2(-10f, 0f);

        var handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGO.transform.SetParent(handleArea.transform, false);
        var hRT = handleGO.GetComponent<RectTransform>();
        hRT.sizeDelta = new Vector2(20f, 0f);
        handleGO.GetComponent<Image>().color = Color.white;
        slider.handleRect = hRT;
        slider.targetGraphic = handleGO.GetComponent<Image>();
    }

    private static void StyleToggle(Toggle toggle)
    {
        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(toggle.transform, false);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.sizeDelta = new Vector2(40f, 40f);
        bgRT.anchorMin = new Vector2(0f, 0.5f); bgRT.anchorMax = new Vector2(0f, 0.5f);
        bgRT.pivot = new Vector2(0f, 0.5f);
        bgGO.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);
        toggle.targetGraphic = bgGO.GetComponent<Image>();

        var checkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkGO.transform.SetParent(bgGO.transform, false);
        StretchFull(checkGO.GetComponent<RectTransform>());
        checkGO.GetComponent<Image>().color = TEXT_RED;
        toggle.graphic = checkGO.GetComponent<Image>();
    }

    private static void StyleDropdown(TMP_Dropdown dropdown)
    {
        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(dropdown.transform, false);
        StretchFull(bgGO.GetComponent<RectTransform>());
        bgGO.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.16f, 1f);
        dropdown.targetGraphic = bgGO.GetComponent<Image>();

        var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(dropdown.transform, false);
        StretchFull(labelGO.GetComponent<RectTransform>());
        var lbl = labelGO.GetComponent<TextMeshProUGUI>();
        lbl.color = TEXT_WHITE; lbl.fontSize = 22f; lbl.alignment = TextAlignmentOptions.MidlineLeft;
        dropdown.captionText = lbl;

        var templateGO = new GameObject("Template", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        templateGO.transform.SetParent(dropdown.transform, false);
        templateGO.SetActive(false);
        templateGO.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.14f, 1f);
        var tRT = templateGO.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0f, 0f); tRT.anchorMax = new Vector2(1f, 0f);
        tRT.pivot = new Vector2(0.5f, 1f); tRT.sizeDelta = new Vector2(0f, 150f);
        dropdown.template = tRT;

        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        vpGO.transform.SetParent(templateGO.transform, false);
        vpGO.GetComponent<Image>().color = Color.clear;
        StretchFull(vpGO.GetComponent<RectTransform>());
        templateGO.GetComponent<ScrollRect>().viewport = vpGO.GetComponent<RectTransform>();

        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpGO.transform, false);
        var cRT = contentGO.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0f, 1f); cRT.anchorMax = new Vector2(1f, 1f);
        cRT.pivot = new Vector2(0.5f, 1f);
        templateGO.GetComponent<ScrollRect>().content = cRT;

        var itemGO = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
        itemGO.transform.SetParent(contentGO.transform, false);
        var itemRT = itemGO.GetComponent<RectTransform>();
        itemRT.anchorMin = new Vector2(0f, 0.5f); itemRT.anchorMax = new Vector2(1f, 0.5f);
        itemRT.sizeDelta = new Vector2(0f, 30f);
        var itemToggle = itemGO.GetComponent<Toggle>();

        var itemBgGO = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
        itemBgGO.transform.SetParent(itemGO.transform, false);
        StretchFull(itemBgGO.GetComponent<RectTransform>());
        itemBgGO.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.14f, 1f);
        itemToggle.targetGraphic = itemBgGO.GetComponent<Image>();

        var itemCheckGO = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
        itemCheckGO.transform.SetParent(itemGO.transform, false);
        var itemCheckRT = itemCheckGO.GetComponent<RectTransform>();
        itemCheckRT.anchorMin = Vector2.zero; itemCheckRT.anchorMax = new Vector2(0.08f, 1f);
        itemCheckRT.offsetMin = Vector2.zero; itemCheckRT.offsetMax = Vector2.zero;
        itemCheckGO.GetComponent<Image>().color = TEXT_RED;
        itemToggle.graphic = itemCheckGO.GetComponent<Image>();

        var itemLabelGO = new GameObject("Item Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        itemLabelGO.transform.SetParent(itemGO.transform, false);
        StretchFull(itemLabelGO.GetComponent<RectTransform>());
        var itemLabel = itemLabelGO.GetComponent<TextMeshProUGUI>();
        itemLabel.color = TEXT_WHITE; itemLabel.fontSize = 20f; itemLabel.alignment = TextAlignmentOptions.MidlineLeft;
        dropdown.itemText = itemLabel;
    }

    // ── Petits helpers ─────────────────────────────────────────────────────

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
        go.GetComponent<Image>().color = SEPARATOR;
        go.GetComponent<Image>().raycastTarget = false;
    }

    private static void Anchor(RectTransform rt, float x0, float y0, float x1, float y1, float l, float r)
    {
        rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = new Vector2(l, 0f); rt.offsetMax = new Vector2(r, 0f);
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}
