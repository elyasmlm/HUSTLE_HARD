using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Outil Editor : genere la carte (minimap + plein ecran) et le CarteManager.
/// Menu : Tools / Carte / Create UI in Scene
/// </summary>
public static class CreateCarteUIEditor
{
    private static readonly Color BG_CARTE  = new Color(0.06f, 0.07f, 0.09f, 0.85f);
    private static readonly Color BORDER    = new Color(0.30f, 0.35f, 0.45f, 1f);
    private static readonly Color TXT_TITRE = new Color(0.85f, 0.90f, 1f, 1f);

    [MenuItem("Tools/Carte/Create UI in Scene")]
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

        // Idempotent : supprime une carte existante.
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && (t.name == "PanneauCarte" || t.name == "CarteManager_Controller"))
                Undo.DestroyObjectImmediate(t.gameObject);
        }

        var ctrlGO = new GameObject("CarteManager_Controller");
        var carte = ctrlGO.AddComponent<CarteManager>();
        ctrlGO.transform.SetParent(canvasGO.transform, false);
        Undo.RegisterCreatedObjectUndo(ctrlGO, "Create CarteManager");

        // Panneau (ancre coin bas droite par defaut).
        var panneau = new GameObject("PanneauCarte", typeof(RectTransform), typeof(Image));
        panneau.transform.SetParent(canvasGO.transform, false);
        var prt = panneau.GetComponent<RectTransform>();
        prt.anchorMin = carte.coinAncreMin;
        prt.anchorMax = carte.coinAncreMax;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        panneau.GetComponent<Image>().color = BG_CARTE;
        panneau.GetComponent<Image>().raycastTarget = false;
        CreateBorder(panneau.transform, BORDER, 2f);
        carte.panneauCarte = prt;

        // Titre (visible en plein ecran).
        var titreGO = new GameObject("Titre", typeof(RectTransform), typeof(TextMeshProUGUI));
        titreGO.transform.SetParent(panneau.transform, false);
        var trt = titreGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 0.93f); trt.anchorMax = new Vector2(1f, 1f);
        trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
        var titre = titreGO.GetComponent<TextMeshProUGUI>();
        titre.text = "CARTE";
        titre.fontSize = 28f; titre.color = TXT_TITRE;
        titre.alignment = TextAlignmentOptions.Center;
        titre.fontStyle = FontStyles.Bold;
        titre.raycastTarget = false;
        titre.gameObject.SetActive(false);
        carte.texteTitre = titre;

        // Zone des marqueurs (clippee).
        var zoneGO = new GameObject("ZoneCarte", typeof(RectTransform), typeof(RectMask2D));
        zoneGO.transform.SetParent(panneau.transform, false);
        var zrt = zoneGO.GetComponent<RectTransform>();
        zrt.anchorMin = Vector2.zero; zrt.anchorMax = Vector2.one;
        zrt.offsetMin = new Vector2(4f, 4f); zrt.offsetMax = new Vector2(-4f, -4f);
        carte.zoneCarte = zrt;

        // Joueur.
        var pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null) carte.joueur = pc.transform;

        Undo.RegisterCreatedObjectUndo(panneau, "Create Carte UI");
        Selection.activeGameObject = ctrlGO;
        Debug.Log("[Carte] UI generee. Pense a poser un composant BatimentCarte sur chaque batiment.");
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
}
