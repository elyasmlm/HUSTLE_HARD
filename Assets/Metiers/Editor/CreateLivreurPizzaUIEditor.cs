using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Outil Editor : génère la hiérarchie UI du mini-jeu Livreur Pizza
/// dans la scène active. Menu : Tools > MiniGames > Create Livreur Pizza UI
/// </summary>
public static class CreateLivreurPizzaUIEditor
{
    // ── Palette (même charte visuelle orange/sombre que les autres mini-jeux) ──
    private static readonly Color BG_MODAL     = new Color(0.04f, 0.03f, 0.02f, 0.97f);
    private static readonly Color BORDER_PIZZA = new Color(0.85f, 0.35f, 0.02f, 1f);
    private static readonly Color BORDER_GOLD  = new Color(0.75f, 0.58f, 0.05f, 1f);
    private static readonly Color PANEL_DARK   = new Color(0.07f, 0.05f, 0.03f, 1f);
    private static readonly Color PANEL_LABY   = new Color(0.06f, 0.04f, 0.02f, 1f);
    private static readonly Color WALL_COLOR   = new Color(0.55f, 0.38f, 0.10f, 1f);
    private static readonly Color BTN_START    = new Color(0.80f, 0.30f, 0.02f, 1f);
    private static readonly Color BTN_REPLAY   = new Color(0.15f, 0.45f, 0.10f, 1f);
    private static readonly Color BTN_DARK     = new Color(0.14f, 0.14f, 0.18f, 1f);
    private static readonly Color TXT_WHITE    = new Color(0.92f, 0.92f, 0.92f, 1f);
    private static readonly Color TXT_YELLOW   = new Color(1.00f, 0.85f, 0.20f, 1f);
    private static readonly Color TXT_GRAY     = new Color(0.60f, 0.60f, 0.60f, 1f);
    private static readonly Color SEPARATOR    = new Color(0.85f, 0.35f, 0.02f, 0.55f);

    // ── Point d'entrée ────────────────────────────────────────────────────────
    [MenuItem("Tools/MiniGames/Create Livreur Pizza UI")]
    public static void CreateUI()
    {
        // ── Canvas ────────────────────────────────────────────────────────────
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
        var ctrlGO = new GameObject("LivreurPizza_Controller");
        var lp = ctrlGO.AddComponent<LivreurPizza>();
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create LivreurPizza Controller");

        // Auto-assigner dans InteractionSystem dès la génération
        var interSys = Object.FindFirstObjectByType<InteractionSystem>();
        if (interSys != null)
        {
            interSys.livreurPizza = lp;
            EditorUtility.SetDirty(interSys);
            Debug.Log("[LivreurPizza] Assigné automatiquement dans InteractionSystem.");
        }
        else
            Debug.LogWarning("[LivreurPizza] InteractionSystem introuvable – assignez manuellement le champ livreurPizza.");

        // ── Panneau principal ─────────────────────────────────────────────────
        var modalGO = new GameObject("PanneauLivreur", typeof(RectTransform), typeof(Image));
        modalGO.transform.SetParent(canvasGO.transform, false);
        var modalRT = modalGO.GetComponent<RectTransform>();
        modalRT.anchorMin = new Vector2(0.08f, 0.04f);
        modalRT.anchorMax = new Vector2(0.92f, 0.96f);
        modalRT.offsetMin = modalRT.offsetMax = Vector2.zero;
        modalGO.GetComponent<Image>().color = BG_MODAL;
        CreateBorder(modalGO.transform, BORDER_PIZZA, 3f);
        lp.panneauLivreur = modalGO;

        // Titre
        var titre = CreateTMP(modalGO.transform, "Titre", "LIVREUR PIZZA",
            48f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(titre.GetComponent<RectTransform>(), 0f, 0.89f, 1f, 0.98f);
        CreateHLine(modalGO.transform, new Vector2(0.03f, 0.882f), new Vector2(0.97f, 0.888f));

        // ── Bandeau infos (argent + timer) ────────────────────────────────────
        var bandeauInfos = CreateImagePanel(modalGO.transform, "BandeauInfos", PANEL_DARK,
            new Vector2(0.03f, 0.83f), new Vector2(0.97f, 0.88f));

        var argentTxt = CreateTMP(bandeauInfos.transform, "TexteArgent", "Argent : $0",
            20f, TXT_YELLOW, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        SetAnchors(argentTxt.GetComponent<RectTransform>(), 0.01f, 0.08f, 0.45f, 0.92f);
        lp.texteArgent = argentTxt.GetComponent<TextMeshProUGUI>();

        var timerTxt = CreateTMP(bandeauInfos.transform, "TexteTimer", "1:00",
            28f, TXT_WHITE, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
        SetAnchors(timerTxt.GetComponent<RectTransform>(), 0.55f, 0.08f, 0.99f, 0.92f);
        lp.texteTimer = timerTxt.GetComponent<TextMeshProUGUI>();

        // ── Instruction ───────────────────────────────────────────────────────
        var instrTxt = CreateTMP(modalGO.transform, "TexteInstruction",
            "Livrez la pizza avant la fin du temps !  (ZQSD / flèches)",
            16f, TXT_GRAY, TextAlignmentOptions.Center, FontStyles.Italic);
        SetAnchors(instrTxt.GetComponent<RectTransform>(), 0.03f, 0.795f, 0.97f, 0.830f);
        lp.texteInstruction = instrTxt.GetComponent<TextMeshProUGUI>();

        // ── Zone labyrinthe ───────────────────────────────────────────────────
        var labyGO = CreateImagePanel(modalGO.transform, "PanneauLabyrinthe", PANEL_LABY,
            new Vector2(0.04f, 0.185f), new Vector2(0.96f, 0.790f));
        CreateBorder(labyGO.transform, BORDER_PIZZA, 2f);
        lp.panneauLabyrinthe = labyGO;

        lp.murs = BuildMaze(labyGO.transform);

        // Joueur (carré orange)
        lp.joueurIcon = MakeIconRect(labyGO.transform, "JoueurIcon",
            new Vector2(-180f, -140f), new Vector2(32f, 32f),
            new Color(0.95f, 0.55f, 0.05f, 1f), "P");

        // Sortie (carré vert)
        lp.sortieIcon = MakeIconRect(labyGO.transform, "SortieIcon",
            new Vector2(185f, 140f), new Vector2(38f, 38f),
            new Color(0.15f, 0.78f, 0.22f, 1f), "S");

        labyGO.SetActive(false);

        // ── Zone résultat ─────────────────────────────────────────────────────
        var panRes = CreateImagePanel(modalGO.transform, "PanneauResultat", PANEL_DARK,
            new Vector2(0.10f, 0.11f), new Vector2(0.90f, 0.19f));
        CreateBorder(panRes.transform, BORDER_GOLD, 1f);

        var resultatTxt = CreateTMP(panRes.transform, "TexteResultat", "",
            22f, TXT_WHITE, TextAlignmentOptions.Center, FontStyles.Bold);
        SetAnchors(resultatTxt.GetComponent<RectTransform>(), 0.02f, 0.52f, 0.98f, 1f);
        lp.texteResultat = resultatTxt.GetComponent<TextMeshProUGUI>();

        var gainTxt = CreateTMP(panRes.transform, "TexteGain", "",
            17f, TXT_YELLOW, TextAlignmentOptions.Center, FontStyles.Normal);
        SetAnchors(gainTxt.GetComponent<RectTransform>(), 0.02f, 0f, 0.98f, 0.50f);
        lp.texteGain = gainTxt.GetComponent<TextMeshProUGUI>();

        // ── Bouton Commencer ──────────────────────────────────────────────────
        var btnCom = CreateButton(modalGO.transform, "BoutonCommencer",
            "COMMENCER", BTN_START, 20f, TXT_WHITE,
            new Vector2(0.30f, 0.028f), new Vector2(0.70f, 0.108f));
        lp.boutonCommencer = btnCom.GetComponent<Button>();

        // ── Bouton Rejouer (caché au départ) ──────────────────────────────────
        var btnRej = CreateButton(modalGO.transform, "BoutonRejouer",
            "REJOUER", BTN_REPLAY, 18f, TXT_WHITE,
            new Vector2(0.04f, 0.028f), new Vector2(0.44f, 0.108f));
        lp.boutonRejouer = btnRej.GetComponent<Button>();
        btnRej.SetActive(false);

        // ── Bouton Fermer (petit, coin bas-droit, style CaseOpening) ──────────
        var btnFer = CreateButton(modalGO.transform, "BoutonFermer",
            "x  Fermer", BTN_DARK, 18f, TXT_WHITE,
            new Vector2(0.83f, 0.002f), new Vector2(0.98f, 0.065f));
        lp.boutonFermer = btnFer.GetComponent<Button>();

        // ── Finalisation ──────────────────────────────────────────────────────
        // NE PAS désactiver modalGO ici : c'est Start() du runtime qui le fait.
        // La modale reste active pour être visible dans la Scene view après génération.
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        Undo.RegisterCreatedObjectUndo(modalGO, "Create LivreurPizza UI");
        Undo.RegisterCreatedObjectUndo(ctrlGO,  "Create LivreurPizza Controller");
        EditorUtility.SetDirty(lp);
        EditorUtility.SetDirty(canvasGO);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = modalGO;
        Debug.Log("[LivreurPizza] UI générée avec succès. Sauvegardez avec Ctrl+S.");
    }

    // ── Labyrinthe ────────────────────────────────────────────────────────────
    private static RectTransform[] BuildMaze(Transform parent)
    {
        // (x, y, largeur, hauteur) – anchoredPosition depuis le centre du panneau
        (float x, float y, float w, float h)[] defs =
        {
            // Bordures extérieures
            (    0f,  205f, 780f, 14f),   // haut
            (    0f, -205f, 780f, 14f),   // bas
            ( -383f,    0f,  14f, 424f),  // gauche
            (  383f,    0f,  14f, 424f),  // droite
            // Murs intérieurs – labyrinthe
            ( -180f,   60f,  14f, 200f),
            ( -180f,  -95f,  14f,  80f),
            (   50f,  100f, 240f,  14f),
            (   50f,   -5f,  14f, 230f),
            (  200f,  -85f, 150f,  14f),
            (  -80f,  -55f, 185f,  14f),
            (  -80f, -130f,  14f, 160f),
            (  200f,   70f,  14f, 105f),
            ( -270f,  -85f, 170f,  14f),
            ( -270f,  130f,  14f, 150f),
        };

        var liste = new RectTransform[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            var (x, y, w, h) = defs[i];
            var go = new GameObject("Mur_" + i, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = WALL_COLOR;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            liste[i] = rt;
        }
        return liste;
    }

    // ── Icône joueur / sortie ─────────────────────────────────────────────────
    private static RectTransform MakeIconRect(Transform parent, string nom,
        Vector2 pos, Vector2 size, Color couleur, string lettre)
    {
        var go = new GameObject(nom, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = couleur;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var lblGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        lblGO.transform.SetParent(go.transform, false);
        StretchFull(lblGO.GetComponent<RectTransform>());
        var t = lblGO.GetComponent<TextMeshProUGUI>();
        t.text = lettre;
        t.fontSize = size.y * 0.55f;
        t.color = Color.white;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.raycastTarget = false;
        return rt;
    }

    // ── Helpers (mêmes signatures que CaseOpening) ────────────────────────────
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
