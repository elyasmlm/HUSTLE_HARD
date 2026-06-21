using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Outil Editor : genere un petit panneau d'inventaire dans le HUD
/// et le branche sur le champ texteInventaire du HUDManager.
/// Menu : Tools / HUD / Create Inventaire
/// </summary>
public static class CreateInventaireHUDEditor
{
    private static readonly Color PANEL_BG  = new Color(0.05f, 0.04f, 0.06f, 0.75f);
    private static readonly Color BORDER    = new Color(0.30f, 0.30f, 0.40f, 1f);
    private static readonly Color TXT_WHITE = new Color(0.92f, 0.92f, 0.92f, 1f);

    [MenuItem("Tools/HUD/Create Inventaire")]
    public static void CreateUI()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Inventaire] Aucun Canvas dans la scene. Cree d'abord le HUD.");
            return;
        }

        HUDManager hud = Object.FindFirstObjectByType<HUDManager>();
        if (hud == null)
            Debug.LogWarning("[Inventaire] Aucun HUDManager trouve : le panneau ne sera pas branche automatiquement.");

        // Idempotent : supprime tout panneau d'inventaire existant pour eviter les doublons.
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && t.name == "PanneauInventaire")
                Undo.DestroyObjectImmediate(t.gameObject);
        }

        // Panneau ancre en bas a gauche.
        var panel = new GameObject("PanneauInventaire", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.01f, 0.04f);
        rt.anchorMax = new Vector2(0.18f, 0.30f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = panel.GetComponent<Image>();
        img.color = PANEL_BG;
        img.raycastTarget = false;
        CreateBorder(panel.transform, BORDER, 2f);

        // Texte de l'inventaire.
        var txtGO = new GameObject("TexteInventaire", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(panel.transform, false);
        var trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(12, 10); trt.offsetMax = new Vector2(-12, -10);
        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "INVENTAIRE\n(vide)";
        tmp.fontSize = 20f;
        tmp.color = TXT_WHITE;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;

        if (hud != null)
        {
            hud.texteInventaire = tmp;
            hud.panneauInventaire = panel;
            EditorUtility.SetDirty(hud);
        }

        Undo.RegisterCreatedObjectUndo(panel, "Create Inventaire HUD");
        Selection.activeGameObject = panel;
        Debug.Log("[Inventaire] Panneau d'inventaire genere et branche sur le HUDManager.");
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
