using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Carte du jeu : minimap en bas a droite, plein ecran avec M.
/// Projette la position monde (X,Z) des batiments et du joueur sur le rectangle UI.
/// Fonctionne a n'importe quelle taille de panneau (minimap ou plein ecran).
/// </summary>
public class CarteManager : MonoBehaviour
{
    [Header("UI")]
    public RectTransform panneauCarte;   // panneau redimensionne au basculement
    public RectTransform zoneCarte;       // zone (clippee) ou vivent les marqueurs
    public TextMeshProUGUI texteTitre;    // optionnel, visible en plein ecran

    [Header("Joueur")]
    public Transform joueur;

    [Header("Bornes du monde (X,Z)")]
    public bool ajusterAutomatiquement = true;
    public Vector2 mondeMin = new Vector2(-100f, -100f);
    public Vector2 mondeMax = new Vector2(100f, 100f);
    [Tooltip("Marge ajoutee autour des batiments en cadrage auto.")]
    public float marge = 15f;

    [Header("Marqueurs")]
    public float tailleMarqueurBatiment = 14f;
    public float tailleMarqueurJoueur = 16f;
    public Color couleurJoueur = new Color(0.20f, 0.80f, 1f, 1f);
    public float tailleLabel = 14f;

    [Header("Ancrages")]
    public Vector2 coinAncreMin   = new Vector2(0.79f, 0.04f);
    public Vector2 coinAncreMax   = new Vector2(0.99f, 0.32f);
    public Vector2 pleinAncreMin  = new Vector2(0.12f, 0.10f);
    public Vector2 pleinAncreMax  = new Vector2(0.88f, 0.92f);

    public KeyCode toucheBascule = KeyCode.M;

    // --- Interne ---
    private class Marqueur
    {
        public Transform cible;
        public RectTransform rect;
        public TextMeshProUGUI label;
    }

    private readonly List<Marqueur> marqueurs = new List<Marqueur>();
    private Marqueur marqueurJoueur;
    private bool pleinEcran = false;
    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();
        if (joueur == null && playerController != null)
            joueur = playerController.transform;

        BatimentCarte[] batiments = Object.FindObjectsByType<BatimentCarte>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (ajusterAutomatiquement && batiments.Length > 0)
            CalculerBornes(batiments);

        foreach (var b in batiments)
            marqueurs.Add(CreerMarqueur(b.nomBatiment, b.couleur, tailleMarqueurBatiment, true, b.TransformCarte));

        // Le joueur en dernier => rendu au-dessus des autres marqueurs.
        if (joueur != null)
            marqueurJoueur = CreerMarqueur("", couleurJoueur, tailleMarqueurJoueur, false, joueur);

        AppliquerMode();
    }

    // -----------------------------------------------------------------------
    void Update()
    {
        // Masque la carte quand un ecran de jeu/dialogue est ouvert.
        bool ecranOuvert = playerController != null && playerController.menuOuvert;
        if (panneauCarte != null && panneauCarte.gameObject.activeSelf == ecranOuvert)
            panneauCarte.gameObject.SetActive(!ecranOuvert);

        if (ecranOuvert) return;

        // Detection robuste (compatible AZERTY) : touche configurable OU caractere 'm'.
        KeyCode touche = toucheBascule == KeyCode.None ? KeyCode.M : toucheBascule;
        bool basculer = Input.GetKeyDown(touche)
                        || Input.inputString.Contains("m")
                        || Input.inputString.Contains("M");
        if (basculer)
        {
            pleinEcran = !pleinEcran;
            AppliquerMode();
        }

        PositionnerMarqueurs();
    }

    // -----------------------------------------------------------------------
    void CalculerBornes(BatimentCarte[] batiments)
    {
        Vector2 mn = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 mx = new Vector2(float.MinValue, float.MinValue);

        foreach (var b in batiments)
        {
            Vector3 p = b.TransformCarte.position;
            mn.x = Mathf.Min(mn.x, p.x); mn.y = Mathf.Min(mn.y, p.z);
            mx.x = Mathf.Max(mx.x, p.x); mx.y = Mathf.Max(mx.y, p.z);
        }

        if (joueur != null)
        {
            Vector3 p = joueur.position;
            mn.x = Mathf.Min(mn.x, p.x); mn.y = Mathf.Min(mn.y, p.z);
            mx.x = Mathf.Max(mx.x, p.x); mx.y = Mathf.Max(mx.y, p.z);
        }

        mondeMin = new Vector2(mn.x - marge, mn.y - marge);
        mondeMax = new Vector2(mx.x + marge, mx.y + marge);
    }

    // -----------------------------------------------------------------------
    Marqueur CreerMarqueur(string nom, Color couleur, float taille, bool avecLabel, Transform cible)
    {
        var go = new GameObject("Marqueur_" + (string.IsNullOrEmpty(nom) ? "Joueur" : nom),
            typeof(RectTransform), typeof(Image));
        go.transform.SetParent(zoneCarte, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(taille, taille);
        var img = go.GetComponent<Image>();
        img.color = couleur;

        TextMeshProUGUI label = null;
        if (avecLabel)
        {
            var lblGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblGO.transform.SetParent(go.transform, false);
            var lrt = lblGO.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.5f, 1f);
            lrt.anchorMax = new Vector2(0.5f, 1f);
            lrt.pivot = new Vector2(0.5f, 0f);
            lrt.anchoredPosition = new Vector2(0f, 2f);
            lrt.sizeDelta = new Vector2(160f, 22f);
            label = lblGO.GetComponent<TextMeshProUGUI>();
            label.text = nom;
            label.fontSize = tailleLabel;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.raycastTarget = false;
        }

        img.raycastTarget = false;
        return new Marqueur { cible = cible, rect = rt, label = label };
    }

    // -----------------------------------------------------------------------
    void PositionnerMarqueurs()
    {
        if (zoneCarte == null) return;
        Vector2 size = zoneCarte.rect.size;

        foreach (var m in marqueurs)
            PlacerUn(m, size);

        if (marqueurJoueur != null)
            PlacerUn(marqueurJoueur, size);
    }

    void PlacerUn(Marqueur m, Vector2 size)
    {
        if (m.cible == null) return;
        Vector3 w = m.cible.position;
        float u = Mathf.Clamp01(Mathf.InverseLerp(mondeMin.x, mondeMax.x, w.x));
        float v = Mathf.Clamp01(Mathf.InverseLerp(mondeMin.y, mondeMax.y, w.z));
        m.rect.anchoredPosition = new Vector2((u - 0.5f) * size.x, (v - 0.5f) * size.y);

        // Noms toujours visibles ; police plus petite en minimap.
        if (m.label != null)
        {
            if (!m.label.gameObject.activeSelf) m.label.gameObject.SetActive(true);
            m.label.fontSize = pleinEcran ? tailleLabel : tailleLabel * 0.7f;
        }
    }

    // -----------------------------------------------------------------------
    void AppliquerMode()
    {
        if (panneauCarte != null)
        {
            panneauCarte.anchorMin = pleinEcran ? pleinAncreMin : coinAncreMin;
            panneauCarte.anchorMax = pleinEcran ? pleinAncreMax : coinAncreMax;
            panneauCarte.offsetMin = Vector2.zero;
            panneauCarte.offsetMax = Vector2.zero;
        }

        if (texteTitre != null)
            texteTitre.gameObject.SetActive(pleinEcran);
    }
}
