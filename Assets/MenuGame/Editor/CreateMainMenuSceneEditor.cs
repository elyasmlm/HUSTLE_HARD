using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;
using MenuGame;


public static class CreateMainMenuSceneEditor
{
    private const string SCENE_PATH = "Assets/MenuGame/Scenes/MainMenu.unity";
    private const string GAMEPLAY_SCENE = "Assets/Scenes/SampleScene.unity";

    // ── Palette de couleurs ────────────────────────────────────
    private static readonly Color BG_DARK        = new Color(0.04f, 0.04f, 0.06f, 1f);
    private static readonly Color PANEL_DARK      = new Color(0.06f, 0.06f, 0.09f, 0.97f);
    private static readonly Color BTN_NORMAL      = new Color(0f,    0f,    0f,    0f); 
    private static readonly Color TEXT_WHITE      = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TEXT_RED     = new Color(0.82f, 0.06f, 0.06f, 1f);
    private static readonly Color SEPARATOR       = new Color(0.82f, 0.06f, 0.06f, 0.6f);
    private static readonly Color OVERLAY_COLOR   = new Color(0f,    0f,    0f,    0.55f);

    [MenuItem("Tools/MenuGame/Create Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Caméra ─────────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = BG_DARK;
        cam.orthographic = false;
        cam.tag = "MainCamera";
        cam.cullingMask = ~(1 << 31);
        camGO.AddComponent<AudioListener>();

        // ── EventSystem ────────────────────────────────────────────────────
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── Canvas principal ───────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Fond sombre ──────────────────────────────────────────
        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(UnityEngine.UI.RawImage));
        bgGO.transform.SetParent(canvasGO.transform, false);
        StretchFull(bgGO.GetComponent<RectTransform>());
        var rawImg = bgGO.GetComponent<UnityEngine.UI.RawImage>();
        var bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MenuGame/Background/bg.jpg");
        if (bgTex != null)
            rawImg.texture = bgTex;
        else
            rawImg.color = BG_DARK;

        // Vignette/overlay sombre par-dessus l'image
        var overlay = CreatePanel(canvasGO.transform, "DarkOverlay", OVERLAY_COLOR);
        StretchFull(overlay.GetComponent<RectTransform>());

        // ── Colonne gauche : boutons ───────────────────────────────────────
        var leftColumn = CreateEmptyRect(canvasGO.transform, "LeftColumn");
        var leftRT = leftColumn.GetComponent<RectTransform>();
        leftRT.anchorMin = new Vector2(0f,    0f);
        leftRT.anchorMax = new Vector2(0.42f, 1f);
        leftRT.offsetMin = Vector2.zero;
        leftRT.offsetMax = Vector2.zero;

        // Panneau boutons principal
        var mainButtonsPanel = CreateEmptyRect(leftColumn.transform, "MainButtonsPanel");
        var mbRT = mainButtonsPanel.GetComponent<RectTransform>();
        StretchFull(mbRT);

        // Sous-titre (genre du jeu)
        var subLabel = CreateTMPText(mainButtonsPanel.transform, "SubLabel", "— HUSTLE HARD —",
            28f, TEXT_RED, TextAlignmentOptions.MidlineLeft);
        var subRT = subLabel.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0f, 0.65f);
        subRT.anchorMax = new Vector2(1f, 0.72f);
        SetRectOffsets(subRT, 80, 0, 0, 0);

        // Ligne horizontale sous le sous-titre
        CreateHorizontalLine(mainButtonsPanel.transform, new Vector2(0f, 0.64f), new Vector2(0.85f, 0.645f));

        // Boutons
        string[] btnLabels = { "JOUER", "OPTIONS", "SÉLECTION SKIN", "QUITTER" };
        float[] btnAnchors = { 0.54f, 0.44f, 0.34f, 0.20f };

        var btnGOs = new GameObject[4];
        for (int i = 0; i < btnLabels.Length; i++)
        {
            btnGOs[i] = CreateMenuButton(mainButtonsPanel.transform, btnLabels[i], btnAnchors[i]);
        }

        // ── Colonne droite : titre ─────────────────────────────────────────
        var rightColumn = CreateEmptyRect(canvasGO.transform, "RightColumn");
        var rightRT = rightColumn.GetComponent<RectTransform>();
        rightRT.anchorMin = new Vector2(0.44f, 0f);
        rightRT.anchorMax = new Vector2(1f,    1f);
        rightRT.offsetMin = Vector2.zero;
        rightRT.offsetMax = Vector2.zero;

        // Titre "HUSTLE"
        var titleTop = CreateTMPText(rightColumn.transform, "TitleTop", "HUSTLE",
            140f, TEXT_WHITE, TextAlignmentOptions.Center);
        var ttRT = titleTop.GetComponent<RectTransform>();
        ttRT.anchorMin = new Vector2(0f, 0.52f);
        ttRT.anchorMax = new Vector2(1f, 0.78f);
        SetRectOffsets(ttRT, 0, 0, 0, 0);
        titleTop.fontStyle = FontStyles.Bold;

        // Titre "HARD" (jaune)
        var titleBottom = CreateTMPText(rightColumn.transform, "TitleBottom", "HARD",
            170f, TEXT_RED, TextAlignmentOptions.Center);
        var tbRT = titleBottom.GetComponent<RectTransform>();
        tbRT.anchorMin = new Vector2(0f, 0.32f);
        tbRT.anchorMax = new Vector2(1f, 0.58f);
        SetRectOffsets(tbRT, 0, 0, 0, 0);
        titleBottom.fontStyle = FontStyles.Bold;

        // Tagline
        var tagline = CreateTMPText(rightColumn.transform, "Tagline", "SURVIVE. HUSTLE. REPEAT.",
            22f, new Color(0.6f, 0.6f, 0.6f, 0.8f), TextAlignmentOptions.Center);
        var tagRT = tagline.GetComponent<RectTransform>();
        tagRT.anchorMin = new Vector2(0f, 0.27f);
        tagRT.anchorMax = new Vector2(1f, 0.32f);

        // ── Panneau OPTIONS ────────────────────────────────────────────────
        var optionsPanel = CreateDarkPanel(canvasGO.transform, "OptionsPanel",
            new Vector2(0.02f, 0.1f), new Vector2(0.42f, 0.9f));
        optionsPanel.SetActive(false);

        var optTitle = CreateTMPText(optionsPanel.transform, "OptionsTitle", "OPTIONS",
            48f, TEXT_RED, TextAlignmentOptions.Center);
        var otRT = optTitle.GetComponent<RectTransform>();
        otRT.anchorMin = new Vector2(0f, 1f);
        otRT.anchorMax = new Vector2(1f, 1f);
        otRT.pivot     = new Vector2(0.5f, 1f);
        otRT.offsetMin = new Vector2(0f, -90f);
        otRT.offsetMax = new Vector2(0f,  -20f);

        LayoutOptionsPanel(optionsPanel.transform);

        // Bouton Retour Options
        var optBackBtn = CreateMenuButton(optionsPanel.transform, "← RETOUR", 0.08f, 36f);

        // ── Panneau SKIN ──────────────────────────────────────────────────────────────────────────────────────────────────────────
        var skinPanel = CreateDarkPanel(canvasGO.transform, "SkinPanel",
            new Vector2(0.02f, 0.1f), new Vector2(0.42f, 0.9f));
        skinPanel.SetActive(false);

        var skinRefs = BuildSkinSelectionPanel(skinPanel);

        // ── Scripts sur le canvas ──────────────────────────────────────────
        var mainController = canvasGO.AddComponent<MainMenuController>();
        mainController.gameplaySceneName = "SampleScene";
        mainController.mainButtonsPanel = mainButtonsPanel;
        mainController.optionsPanel = optionsPanel;
        mainController.skinPanel = skinPanel;

        var optController = optionsPanel.AddComponent<OptionsMenuController>();

        // Glitch sur les deux parties du titre
        titleTop.gameObject.AddComponent<GlitchTextEffect>();
        titleBottom.gameObject.AddComponent<GlitchTextEffect>();

        // Hover effect sur les boutons
        foreach (var btn in btnGOs)
            btn.AddComponent<MenuButtonHoverEffect>();

        optBackBtn.AddComponent<MenuButtonHoverEffect>();


        // ── Liaisons des boutons du menu principal ─────────────────────────
        WireMainMenuButton(btnGOs[0], mainController, "play");
        WireMainMenuButton(btnGOs[1], mainController, "options");
        WireMainMenuButton(btnGOs[2], mainController, "skin");
        WireMainMenuButton(btnGOs[3], mainController, "quit");

        // Boutons Retour
        WireOptionsBackButton(optBackBtn, optController);
        WireSkinButtons(skinRefs.skinBackBtn, skinRefs.prevBtn, skinRefs.nextBtn,
                        skinRefs.selectBtn, skinRefs.controller);

        // Injecter automatiquement les vrais prefabs de skins
        SkinSelectionAutoSetupEditor.PopulateControllerDirect(skinRefs.controller);

        // ── References Options ─────────────────────────────────────────────
        AssignOptionsReferences(optController, optionsPanel.transform);

        // ── Sauvegarder la scène ───────────────────────────────────────────
        System.IO.Directory.CreateDirectory("Assets/MenuGame/Scenes");
        bool saved = EditorSceneManager.SaveScene(scene, SCENE_PATH);

        if (saved)
        {
            AssetDatabase.Refresh();
            AddScenesToBuildSettings();
            Debug.Log("[MenuGame] Scène MainMenu créée et sauvegardée : " + SCENE_PATH);
            EditorUtility.DisplayDialog("MenuGame",
                "✅ Scène MainMenu créée !\n\n" +
                "Chemin : " + SCENE_PATH + "\n\n" +
                "MainMenu est maintenant la première scène dans les Build Settings.\n\n" +
                "Appuyez sur Play pour tester le menu.",
                "OK");
        }
        else
        {
            Debug.LogError("[MenuGame] Échec de la sauvegarde de la scène.");
        }
    }

    // ── Construction du panneau Skin ─────────────────────────────────────────

    private struct SkinPanelRefs
    {
        public SkinSelectionController controller;
        public GameObject skinBackBtn;
        public GameObject prevBtn;
        public GameObject nextBtn;
        public GameObject selectBtn;
    }

    private static SkinPanelRefs BuildSkinSelectionPanel(GameObject panel)
    {
        // Titre
        var title   = CreateTMPText(panel.transform, "SkinTitle", "SELECTION DU SKIN",
            42f, TEXT_RED, TextAlignmentOptions.Center);
        var titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot     = new Vector2(0.5f, 1f);
        titleRT.offsetMin = new Vector2(0f, -85f);
        titleRT.offsetMax = new Vector2(0f, -20f);

        // Zone de preview 3D (RawImage — alimentee par RenderTexture)
        var rawPreviewGO = new GameObject("SkinPreviewRaw",
            typeof(RectTransform), typeof(UnityEngine.UI.RawImage));
        rawPreviewGO.transform.SetParent(panel.transform, false);
        var rawRT = rawPreviewGO.GetComponent<RectTransform>();
        rawRT.anchorMin = new Vector2(0.1f, 0.42f);
        rawRT.anchorMax = new Vector2(0.9f, 0.80f);
        rawRT.offsetMin = Vector2.zero;
        rawRT.offsetMax = Vector2.zero;
        var rawImg = rawPreviewGO.GetComponent<UnityEngine.UI.RawImage>();
        rawImg.color = Color.white;

        // Label placeholder
        var noPreview = CreateTMPText(panel.transform, "NoPreviewLabel", "[ Apercu ]",
            28f, new Color(0.4f, 0.4f, 0.4f, 0.8f), TextAlignmentOptions.Center);
        var npRT = noPreview.GetComponent<RectTransform>();
        npRT.anchorMin = new Vector2(0.1f, 0.52f);
        npRT.anchorMax = new Vector2(0.9f, 0.70f);
        npRT.offsetMin = Vector2.zero;
        npRT.offsetMax = Vector2.zero;
        noPreview.gameObject.SetActive(false);

        // Nom du skin
        var nameText = CreateTMPText(panel.transform, "SkinNameText", "",
            34f, TEXT_WHITE, TextAlignmentOptions.Center);
        var nameRT = nameText.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0.05f, 0.36f);
        nameRT.anchorMax = new Vector2(0.95f, 0.43f);
        nameRT.offsetMin = Vector2.zero;
        nameRT.offsetMax = Vector2.zero;
        nameText.fontStyle = FontStyles.Bold;

        // Bouton gauche
        var prevBtnGO = new GameObject("Btn_Prev", typeof(RectTransform),
            typeof(UnityEngine.UI.Button), typeof(UnityEngine.UI.Image));
        prevBtnGO.transform.SetParent(panel.transform, false);
        var prevRT = prevBtnGO.GetComponent<RectTransform>();
        prevRT.anchorMin = new Vector2(0f,    0.52f);
        prevRT.anchorMax = new Vector2(0.14f, 0.70f);
        prevRT.offsetMin = new Vector2(10f, 0f);
        prevRT.offsetMax = Vector2.zero;
        StyleArrowButton(prevBtnGO, "<");

        // Bouton droit
        var nextBtnGO = new GameObject("Btn_Next", typeof(RectTransform),
            typeof(UnityEngine.UI.Button), typeof(UnityEngine.UI.Image));
        nextBtnGO.transform.SetParent(panel.transform, false);
        var nextRT = nextBtnGO.GetComponent<RectTransform>();
        nextRT.anchorMin = new Vector2(0.86f, 0.52f);
        nextRT.anchorMax = new Vector2(1f,    0.70f);
        nextRT.offsetMin = Vector2.zero;
        nextRT.offsetMax = new Vector2(-10f, 0f);
        StyleArrowButton(nextBtnGO, ">");

        // Bouton Selectionner (centre)
        var selectBtnGO = CreateMenuButton(panel.transform, "SELECTIONNER", 0.24f, 32f);
        selectBtnGO.AddComponent<MenuButtonHoverEffect>();
        var selectRT = selectBtnGO.GetComponent<RectTransform>();
        selectRT.anchorMin = new Vector2(0.15f, 0.24f);
        selectRT.anchorMax = new Vector2(0.85f, 0.325f);
        selectRT.offsetMin = Vector2.zero;
        selectRT.offsetMax = Vector2.zero;
        var selectLabel = selectBtnGO.GetComponentInChildren<TextMeshProUGUI>();
        if (selectLabel != null) selectLabel.alignment = TextAlignmentOptions.Center;

        // Feedback
        var feedbackText = CreateTMPText(panel.transform, "FeedbackText", "Skin selectionne !",
            24f, TEXT_RED, TextAlignmentOptions.Center);
        var fbRT = feedbackText.GetComponent<RectTransform>();
        fbRT.anchorMin = new Vector2(0f, 0.17f);
        fbRT.anchorMax = new Vector2(1f, 0.24f);
        fbRT.offsetMin = Vector2.zero;
        fbRT.offsetMax = Vector2.zero;
        feedbackText.gameObject.SetActive(false);

        // Bouton Retour
        var skinBackBtnGO = CreateMenuButton(panel.transform, "< RETOUR", 0.08f, 32f);
        skinBackBtnGO.AddComponent<MenuButtonHoverEffect>();

        // ── PreviewCamera (layer 31, rend dans RenderTexture) ──────────────
        var camGO = new GameObject("SkinPreviewCamera");
        camGO.transform.SetParent(panel.transform.root, false);
        camGO.transform.position = new Vector3(0f, 1f, -4f);
        camGO.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        var previewCam = camGO.AddComponent<Camera>();
        previewCam.clearFlags       = CameraClearFlags.SolidColor;
        previewCam.backgroundColor  = new Color(0.06f, 0.06f, 0.09f, 0f);
        previewCam.cullingMask      = 1 << 31;
        previewCam.fieldOfView      = 40f;
        previewCam.nearClipPlane    = 0.1f;
        previewCam.farClipPlane     = 50f;
        previewCam.depth            = -1;
        previewCam.allowHDR         = false;

        // ── Anchor 3D (la ou le modele sera instancie) ─────────────────────
        var anchorGO = new GameObject("SkinPreviewAnchor");
        anchorGO.transform.SetParent(panel.transform.root, false);
        anchorGO.transform.position = new Vector3(0f, 0f, 0f);

        // ── Controleur ─────────────────────────────────────────────────────
        var ctrl = panel.AddComponent<SkinSelectionController>();
        ctrl.previewRawImage = rawImg;
        ctrl.skinNameText    = nameText;
        ctrl.feedbackText    = feedbackText;
        ctrl.noPreviewLabel  = noPreview.gameObject;
        ctrl.previewCamera   = previewCam;
        ctrl.previewAnchor   = anchorGO.transform;

        return new SkinPanelRefs
        {
            controller  = ctrl,
            skinBackBtn = skinBackBtnGO,
            prevBtn     = prevBtnGO,
            nextBtn     = nextBtnGO,
            selectBtn   = selectBtnGO,
        };
    }

    private static void StyleArrowButton(GameObject go, string arrow)
    {
        var btn = go.GetComponent<UnityEngine.UI.Button>();
        var img = go.GetComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.1f, 0.1f, 0.14f, 0.9f);
        var colors = btn.colors;
        colors.normalColor      = new Color(0.1f, 0.1f, 0.14f, 0.9f);
        colors.highlightedColor = new Color(0.82f, 0.06f, 0.06f, 0.8f);
        colors.pressedColor     = new Color(0.55f, 0.02f, 0.02f, 1f);
        btn.colors    = colors;
        btn.targetGraphic = img;
        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(go.transform, false);
        StretchFull(label.GetComponent<RectTransform>());
        var tmp = label.GetComponent<TextMeshProUGUI>();
        tmp.text      = arrow;
        tmp.fontSize  = 48f;
        tmp.color     = TEXT_WHITE;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
    }

    private static void WireSkinButtons(
        GameObject backBtn, GameObject prevBtn, GameObject nextBtn,
        GameObject selectBtn, SkinSelectionController ctrl)
    {
        if (ctrl == null) return;
        var back = backBtn?.GetComponent<UnityEngine.UI.Button>();
        if (back != null)
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(back.onClick, ctrl.OnBackClicked);
        var prev = prevBtn?.GetComponent<UnityEngine.UI.Button>();
        if (prev != null)
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(prev.onClick, ctrl.OnPreviousClicked);
        var next = nextBtn?.GetComponent<UnityEngine.UI.Button>();
        if (next != null)
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(next.onClick, ctrl.OnNextClicked);
        var select = selectBtn?.GetComponent<UnityEngine.UI.Button>();
        if (select != null)
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(select.onClick, ctrl.OnSelectClicked);
    }
    // ── Helpers de construction UI ─────────────────────────────────────────

    private static Image CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    private static GameObject CreateEmptyRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject CreateDarkPanel(Transform canvasTransform, string name,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvasTransform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = PANEL_DARK;
        return go;
    }

    private static TMP_Text CreateTMPText(Transform parent, string name, string text,
        float fontSize, Color color, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return tmp;
    }

    private static GameObject CreateMenuButton(Transform parent, string label,
        float anchorY, float fontSize = 52f)
    {
        var btnGO = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Button));
        btnGO.transform.SetParent(parent, false);

        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, anchorY);
        rt.anchorMax = new Vector2(0.92f, anchorY + 0.085f);
        rt.offsetMin = new Vector2(70f, 0f);
        rt.offsetMax = Vector2.zero;

        var btn = btnGO.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = BTN_NORMAL;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.04f);
        colors.pressedColor     = new Color(1f, 1f, 1f, 0.08f);
        colors.selectedColor    = BTN_NORMAL;
        btn.colors = colors;
        btn.transition = Selectable.Transition.ColorTint;

        // Texte du bouton
        var textGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(btnGO.transform, false);
        var trt = textGO.GetComponent<RectTransform>();
        StretchFull(trt);
        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = TEXT_WHITE;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;

        return btnGO;
    }

    private static void CreateHorizontalLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject("HLine", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = SEPARATOR;
    }

    private static void LayoutOptionsPanel(Transform parent)
    {
        const float L = 50f; 
        const float R = -20f;

        // ── Label VOLUME ──────────────────────────────────────────────────
        var volumeLabel = CreateTMPText(parent, "VolumeLabel", "VOLUME",
            26f, TEXT_WHITE, TextAlignmentOptions.MidlineLeft);
        var vlRT = volumeLabel.GetComponent<RectTransform>();
        vlRT.anchorMin = new Vector2(0f, 0.72f);
        vlRT.anchorMax = new Vector2(1f, 0.80f);
        vlRT.offsetMin = new Vector2(L,  0f);
        vlRT.offsetMax = new Vector2(R,  0f);

        // ── Slider Volume ─────────────────────────────────────────────────
        var sliderGO = new GameObject("VolumeSlider", typeof(RectTransform));
        sliderGO.transform.SetParent(parent, false);
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0f, 0.63f);
        sliderRT.anchorMax = new Vector2(1f, 0.71f);
        sliderRT.offsetMin = new Vector2(L,  0f);
        sliderRT.offsetMax = new Vector2(R,  0f);
        var slider = sliderGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        StyleSlider(slider);

        // ── Label PLEIN ÉCRAN ─────────────────────────────────────────────
        var fsLabel = CreateTMPText(parent, "FullscreenLabel", "PLEIN ÉCRAN",
            26f, TEXT_WHITE, TextAlignmentOptions.MidlineLeft);
        var fsLabelRT = fsLabel.GetComponent<RectTransform>();
        fsLabelRT.anchorMin = new Vector2(0f, 0.52f);
        fsLabelRT.anchorMax = new Vector2(1f, 0.60f);
        fsLabelRT.offsetMin = new Vector2(L,  0f);
        fsLabelRT.offsetMax = new Vector2(R,  0f);

        // ── Toggle Plein écran ────────────────────────────────────────────
        var toggleGO = new GameObject("FullscreenToggle", typeof(RectTransform));
        toggleGO.transform.SetParent(parent, false);
        var toggleRT = toggleGO.GetComponent<RectTransform>();
        toggleRT.anchorMin = new Vector2(0f, 0.43f);
        toggleRT.anchorMax = new Vector2(0.6f, 0.51f);
        toggleRT.offsetMin = new Vector2(L, 0f);
        toggleRT.offsetMax = new Vector2(0f, 0f);
        var toggle = toggleGO.AddComponent<Toggle>();
        toggle.isOn = Screen.fullScreen;
        StyleToggle(toggle);

        // ── Label QUALITÉ GRAPHIQUE ───────────────────────────────────────
        var qLabel = CreateTMPText(parent, "QualityLabel", "QUALITÉ GRAPHIQUE",
            26f, TEXT_WHITE, TextAlignmentOptions.MidlineLeft);
        var qRT = qLabel.GetComponent<RectTransform>();
        qRT.anchorMin = new Vector2(0f, 0.33f);
        qRT.anchorMax = new Vector2(1f, 0.41f);
        qRT.offsetMin = new Vector2(L,  0f);
        qRT.offsetMax = new Vector2(R,  0f);

        // ── Dropdown Qualité ──────────────────────────────────────────────
        var dropGO = new GameObject("QualityDropdown", typeof(RectTransform));
        dropGO.transform.SetParent(parent, false);
        var dropRT = dropGO.GetComponent<RectTransform>();
        dropRT.anchorMin = new Vector2(0f, 0.23f);
        dropRT.anchorMax = new Vector2(1f, 0.32f);
        dropRT.offsetMin = new Vector2(L,  0f);
        dropRT.offsetMax = new Vector2(R,  0f);
        var dropdown = dropGO.AddComponent<TMP_Dropdown>();
        StyleDropdown(dropdown);
    }

    private static void StyleSlider(Slider slider)
    {
        // Background
        var bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(slider.transform, false);
        StretchFull(bgGO.GetComponent<RectTransform>());
        bgGO.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Fill area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(slider.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.25f);
        faRT.anchorMax = new Vector2(1f, 0.75f);
        faRT.offsetMin = new Vector2(5f, 0f);
        faRT.offsetMax = new Vector2(-15f, 0f);

        var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(fillArea.transform, false);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(1f, 1f);
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        fillGO.GetComponent<Image>().color = TEXT_RED;
        slider.fillRect = fillRT;

        // Handle
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(slider.transform, false);
        var haRT = handleArea.GetComponent<RectTransform>();
        StretchFull(haRT);
        haRT.offsetMin = new Vector2(10f, 0f);
        haRT.offsetMax = new Vector2(-10f, 0f);

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
        bgRT.anchorMin = new Vector2(0f, 0.5f);
        bgRT.anchorMax = new Vector2(0f, 0.5f);
        bgRT.pivot = new Vector2(0f, 0.5f);
        bgGO.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);
        toggle.targetGraphic = bgGO.GetComponent<Image>();

        var checkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
        checkGO.transform.SetParent(bgGO.transform, false);
        StretchFull(checkGO.GetComponent<RectTransform>());
        var checkImg = checkGO.GetComponent<Image>();
        checkImg.color = TEXT_RED;
        toggle.graphic = checkImg;
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
        lbl.color = TEXT_WHITE;
        lbl.fontSize = 22f;
        lbl.alignment = TextAlignmentOptions.MidlineLeft;
        dropdown.captionText = lbl;

        var templateGO = new GameObject("Template", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        templateGO.transform.SetParent(dropdown.transform, false);
        templateGO.SetActive(false);
        templateGO.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.14f, 1f);
        var tRT = templateGO.GetComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0f, 0f);
        tRT.anchorMax = new Vector2(1f, 0f);
        tRT.pivot = new Vector2(0.5f, 1f);
        tRT.sizeDelta = new Vector2(0f, 150f);
        dropdown.template = tRT;

        var vpGO = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        vpGO.transform.SetParent(templateGO.transform, false);
        vpGO.GetComponent<Image>().color = Color.clear;
        StretchFull(vpGO.GetComponent<RectTransform>());
        templateGO.GetComponent<ScrollRect>().viewport = vpGO.GetComponent<RectTransform>();

        var contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(vpGO.transform, false);
        var cRT = contentGO.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0f, 1f);
        cRT.anchorMax = new Vector2(1f, 1f);
        cRT.pivot = new Vector2(0.5f, 1f);
        templateGO.GetComponent<ScrollRect>().content = cRT;

        var itemGO = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
        itemGO.transform.SetParent(contentGO.transform, false);
        var itemRT = itemGO.GetComponent<RectTransform>();
        itemRT.anchorMin = new Vector2(0f, 0.5f);
        itemRT.anchorMax = new Vector2(1f, 0.5f);
        itemRT.sizeDelta = new Vector2(0f, 30f);
        var itemToggle = itemGO.GetComponent<Toggle>();

        var itemBgGO = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
        itemBgGO.transform.SetParent(itemGO.transform, false);
        StretchFull(itemBgGO.GetComponent<RectTransform>());
        itemBgGO.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.14f, 1f);
        itemToggle.targetGraphic = itemBgGO.GetComponent<Image>();

        // Checkmark
        var itemCheckGO = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
        itemCheckGO.transform.SetParent(itemGO.transform, false);
        var itemCheckRT = itemCheckGO.GetComponent<RectTransform>();
        itemCheckRT.anchorMin = Vector2.zero;
        itemCheckRT.anchorMax = new Vector2(0.08f, 1f);
        itemCheckRT.offsetMin = Vector2.zero;
        itemCheckRT.offsetMax = Vector2.zero;
        var itemCheckImg = itemCheckGO.GetComponent<Image>();
        itemCheckImg.color = TEXT_RED;
        itemToggle.graphic = itemCheckImg;

        var itemLabelGO = new GameObject("Item Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        itemLabelGO.transform.SetParent(itemGO.transform, false);
        StretchFull(itemLabelGO.GetComponent<RectTransform>());
        var itemLabel = itemLabelGO.GetComponent<TextMeshProUGUI>();
        itemLabel.color = TEXT_WHITE;
        itemLabel.fontSize = 20f;
        itemLabel.alignment = TextAlignmentOptions.MidlineLeft;
        dropdown.itemText = itemLabel;
    }

    private static void AssignOptionsReferences(OptionsMenuController ctrl, Transform panelTransform)
    {
        ctrl.volumeSlider    = panelTransform.Find("VolumeSlider")?.GetComponent<Slider>();
        ctrl.fullscreenToggle = panelTransform.Find("FullscreenToggle")?.GetComponent<Toggle>();
        ctrl.qualityDropdown  = panelTransform.Find("QualityDropdown")?.GetComponent<TMP_Dropdown>();
    }


    private static void WireMainMenuButton(GameObject btnGO, MainMenuController ctrl, string action)
    {
        var btn = btnGO.GetComponent<Button>();
        if (btn == null || ctrl == null) return;
        switch (action)
        {
            case "play":
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, ctrl.OnPlayClicked);
                break;
            case "options":
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, ctrl.OnOptionsClicked);
                break;
            case "skin":
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, ctrl.OnSkinClicked);
                break;
            case "quit":
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, ctrl.OnQuitClicked);
                break;
        }
    }

    private static void WireOptionsBackButton(GameObject btnGO, OptionsMenuController ctrl)
    {
        var btn = btnGO.GetComponent<Button>();
        if (btn == null || ctrl == null) return;
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, ctrl.OnBackClicked);
    }



    private static void AddScenesToBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>();

        scenes.Add(new EditorBuildSettingsScene(SCENE_PATH, true));

        if (System.IO.File.Exists(GAMEPLAY_SCENE))
            scenes.Add(new EditorBuildSettingsScene(GAMEPLAY_SCENE, true));

        foreach (var existing in EditorBuildSettings.scenes)
        {
            bool alreadyAdded = existing.path == SCENE_PATH || existing.path == GAMEPLAY_SCENE;
            if (!alreadyAdded)
                scenes.Add(existing);
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[MenuGame] Build Settings mis à jour : MainMenu (index 0), SampleScene (index 1).");
    }


    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetRectOffsets(RectTransform rt, float left, float right, float bottom, float top)
    {
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }
}
