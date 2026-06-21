using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Argent")]
    public TextMeshProUGUI texteArgent;

    [Header("Timer")]
    public TextMeshProUGUI texteTimer;

    [Header("Folie")]
    public RectTransform barreFolieFill;
    public TextMeshProUGUI texteFolie;

    [Header("Inventaire")]
    public TextMeshProUGUI texteInventaire;
    [Tooltip("Panneau de l'inventaire, masque quand un ecran de jeu est ouvert.")]
    public GameObject panneauInventaire;

    private float largeurMaxBarre = 200f;

    private float argent = 1250f;
    private float tempsRestant = 1440f;
    private float folie = 35f;

    void Start()
    {
        // Fallback : si le panneau n'est pas assigne, on prend le parent du texte.
        if (panneauInventaire == null && texteInventaire != null)
            panneauInventaire = texteInventaire.transform.parent.gameObject;
    }

    void Update()
    {
        tempsRestant -= Time.deltaTime;
        if (tempsRestant < 0) tempsRestant = 0;

        MettreAJourHUD();
        MettreAJourVisibiliteInventaire();
    }

    void MettreAJourVisibiliteInventaire()
    {
        if (panneauInventaire == null) return;

        // Tous les panneaux de jeu (et le dialogue) liberent le curseur.
        // On masque donc l'inventaire des que le curseur est visible.
        bool ecranOuvert = Cursor.visible;
        if (panneauInventaire.activeSelf == ecranOuvert)
            panneauInventaire.SetActive(!ecranOuvert);
    }

    void MettreAJourHUD()
    {
        if (GameManager.Instance == null) return;

        texteArgent.text = "$" + GameManager.Instance.argent.ToString("N0");

        float t = GameManager.Instance.tempsRestant;
        int heures = Mathf.FloorToInt(t / 60f);
        int minutes = Mathf.FloorToInt(t % 60f);
        texteTimer.text = string.Format("{0:00}:{1:00}", heures, minutes);
        texteTimer.color = t < 120f ? Color.red : Color.white;

        float folie = GameManager.Instance.folie;
        float largeur = (folie / 100f) * largeurMaxBarre;
        barreFolieFill.sizeDelta = new Vector2(largeur, barreFolieFill.sizeDelta.y);
        texteFolie.text = "Folie : " + Mathf.RoundToInt(folie) + "%";

        Image imgFolie = barreFolieFill.GetComponent<Image>();
        if (folie < 40f) imgFolie.color = Color.green;
        else if (folie < 70f) imgFolie.color = Color.yellow;
        else imgFolie.color = Color.red;

        if (texteInventaire != null)
        {
            var gm = GameManager.Instance;
            var lignes = new System.Text.StringBuilder();
            lignes.AppendLine("INVENTAIRE");

            bool vide = true;
            if (gm.boissonEnergisante > 0) { lignes.AppendLine("- Boisson x" + gm.boissonEnergisante); vide = false; }
            if (gm.zdeh    > 0) { lignes.AppendLine("- Zdeh x" + gm.zdeh);    vide = false; }
            if (gm.aimants > 0) { lignes.AppendLine("- Aimant x" + gm.aimants); vide = false; }
            if (gm.viagra  > 0) { lignes.AppendLine("- Viagra x" + gm.viagra); vide = false; }
            if (vide) lignes.AppendLine("(vide)");

            texteInventaire.text = lignes.ToString().TrimEnd();
        }
    }
}