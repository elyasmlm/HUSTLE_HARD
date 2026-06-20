using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mini-jeu : Ticket Grattage
/// Coût : 5$ — 4 ballons + 1 rectangle à gratter
/// Victoire si au moins 2 ballons = symbole gagnant (25% de chance)
/// Folie : +5 victoire / +2 défaite
/// </summary>
public class TicketGrattage : MonoBehaviour
{
    [Header("UI Panneau")]
    public GameObject panneauTicket;
    public TextMeshProUGUI texteResultat;
    public Button boutonAcheter;
    public Button boutonNouveauTicket;
    public Button boutonFermer;

    [Header("Infos joueur")]
    public TextMeshProUGUI texteArgent;
    public TextMeshProUGUI texteErreur;

    [Header("Rectangle symbole + gain")]
    public Button boutonRectangle;
    public TextMeshProUGUI texteSymboleRect;
    public TextMeshProUGUI texteGain;
    public Image overlayRectangle;         // overlay gris par-dessus le rectangle

    [Header("Ballons (4 boutons)")]
    public Button[] boutonsBallons;           // 4 éléments
    public TextMeshProUGUI[] textesBallons;   // 4 textes enfants
    public Image[] overlaysBallons;           // 4 overlays gris à révéler

    // ── Données internes ───────────────────────────────────────────────────
    private readonly string[] symboles     = { "E", "O", "D", "T", "C" };
    private readonly float[]  tauxSymboles = { 0.35f, 0.25f, 0.20f, 0.12f, 0.08f };

    private string   symboleGagnant;
    private string[] symbolesBallons = new string[4];
    private bool[]   ballonsGrattés  = new bool[4];
    private int      gainPotentiel;
    private bool     rectGratté     = false;
    private bool     partieTerminee = false;
    private bool     ticketActif    = false;

    private const int PRIX_TICKET = 5;

    private PlayerController playerController;

    // ── Lifecycle ─────────────────────────────────────────────────────────
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();

        if (boutonAcheter != null)
            boutonAcheter.onClick.AddListener(AcheterTicket);
        if (boutonNouveauTicket != null)
            boutonNouveauTicket.onClick.AddListener(NouveauTicket);
        if (boutonFermer != null)
            boutonFermer.onClick.AddListener(FermerTicket);
        if (boutonRectangle != null)
            boutonRectangle.onClick.AddListener(GratterRectangle);

        if (boutonsBallons != null)
        {
            for (int i = 0; i < boutonsBallons.Length; i++)
            {
                if (boutonsBallons[i] == null) continue;
                int index = i;
                boutonsBallons[i].onClick.AddListener(() => GratterBallon(index));
            }
        }

        if (panneauTicket != null) panneauTicket.SetActive(false);
    }

    void Update()
    {
        if (panneauTicket != null && panneauTicket.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            FermerTicket();
    }

    // ── Ouverture / Fermeture ──────────────────────────────────────────────
    public void OuvrirPanneau()
    {
        if (!GameManager.Instance.PeutJouer()) return;
        if (panneauTicket != null) panneauTicket.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;

        ResetTicket();
    }

    void FermerTicket()
    {
        if (panneauTicket != null) panneauTicket.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }

    // ── Reset ──────────────────────────────────────────────────────────────
    void ResetTicket()
    {
        partieTerminee = false;
        rectGratté     = false;
        ticketActif    = false;

        if (texteResultat != null)   { texteResultat.text = ""; }
        if (texteErreur   != null)   { texteErreur.text   = ""; }
        if (texteGain     != null)   { texteGain.text     = "$??"; }
        if (texteSymboleRect != null){ texteSymboleRect.text = "?"; }

        // Overlays : remettre les caches
        SetOverlay(overlayRectangle, true);
        if (overlaysBallons != null)
            foreach (var ov in overlaysBallons)
                SetOverlay(ov, true);

        for (int i = 0; i < 4; i++)
        {
            ballonsGrattés[i] = false;
            if (textesBallons != null && i < textesBallons.Length && textesBallons[i] != null)
                textesBallons[i].text = "?";
            if (boutonsBallons != null && i < boutonsBallons.Length && boutonsBallons[i] != null)
                boutonsBallons[i].interactable = false; // bloqué jusqu'à l'achat
        }

        if (boutonRectangle != null)   boutonRectangle.interactable = false;
        if (boutonAcheter   != null)   { boutonAcheter.gameObject.SetActive(true); boutonAcheter.interactable = true; }
        if (boutonNouveauTicket != null) boutonNouveauTicket.gameObject.SetActive(false);

        MettreAJourArgent();
    }

    void NouveauTicket() => ResetTicket();

    void MettreAJourArgent()
    {
        if (texteArgent != null)
            texteArgent.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");
    }

    // ── Achat ─────────────────────────────────────────────────────────────
    void AcheterTicket()
    {
        if (texteErreur != null) texteErreur.text = "";

        if (GameManager.Instance.argent < PRIX_TICKET)
        {
            if (texteErreur != null) texteErreur.text = "Argent insuffisant.";
            return;
        }

        GameManager.Instance.RetirerArgent(PRIX_TICKET);
        MettreAJourArgent();

        if (boutonAcheter != null) boutonAcheter.gameObject.SetActive(false);

        GenererTicket();
        ticketActif = true;

        // Activer les zones grattables
        if (boutonRectangle != null) boutonRectangle.interactable = true;
        if (boutonsBallons != null)
            foreach (var b in boutonsBallons)
                if (b != null) b.interactable = true;
    }

    // ── Génération ────────────────────────────────────────────────────────
    void GenererTicket()
    {
        bool victoire = Random.value < 0.25f;
        gainPotentiel = TirerGain();
        symboleGagnant = TirerSymbole();

        if (victoire) GenererBallonsVictoire();
        else          GenererBallonsDefaite();
    }

    int TirerGain()
    {
        float r = Random.value;
        if (r < 0.40f) return Random.Range(5,   11);
        if (r < 0.70f) return Random.Range(11,  26);
        if (r < 0.88f) return Random.Range(26,  51);
        if (r < 0.96f) return Random.Range(51,  101);
        if (r < 0.99f) return Random.Range(101, 251);
        return Random.Range(251, 501);
    }

    string TirerSymbole()
    {
        float r = Random.value, cumul = 0f;
        for (int i = 0; i < symboles.Length; i++)
        {
            cumul += tauxSymboles[i];
            if (r < cumul) return symboles[i];
        }
        return symboles[0];
    }

    string TirerSymboleDifferentDe(string exclu)
    {
        string s; int t = 0;
        do { s = TirerSymbole(); t++; } while (s == exclu && t < 20);
        return s;
    }

    void GenererBallonsVictoire()
    {
        var pos = new List<int> { 0, 1, 2, 3 };
        int p1 = pos[Random.Range(0, pos.Count)]; pos.Remove(p1);
        int p2 = pos[Random.Range(0, pos.Count)]; pos.Remove(p2);
        symbolesBallons[p1] = symboleGagnant;
        symbolesBallons[p2] = symboleGagnant;
        foreach (int p in pos) symbolesBallons[p] = TirerSymboleDifferentDe(symboleGagnant);
    }

    void GenererBallonsDefaite()
    {
        var utilisés = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            string s; int t = 0;
            do { s = TirerSymboleDifferentDe(symboleGagnant); t++; }
            while (utilisés.Contains(s) && t < 20);
            symbolesBallons[i] = s;
            if (!utilisés.Contains(s)) utilisés.Add(s);
        }
    }

    // ── Grattage ──────────────────────────────────────────────────────────
    void GratterBallon(int index)
    {
        if (!ticketActif || ballonsGrattés[index] || partieTerminee) return;
        ballonsGrattés[index] = true;

        if (textesBallons != null && index < textesBallons.Length && textesBallons[index] != null)
            textesBallons[index].text = symbolesBallons[index];

        if (boutonsBallons != null && index < boutonsBallons.Length && boutonsBallons[index] != null)
            boutonsBallons[index].interactable = false;

        // Fade overlay du ballon
        if (overlaysBallons != null && index < overlaysBallons.Length && overlaysBallons[index] != null)
            StartCoroutine(FadeOutOverlay(overlaysBallons[index]));

        VerifierFinPartie();
    }

    public void GratterRectangle()
    {
        if (!ticketActif || rectGratté || partieTerminee) return;
        rectGratté = true;

        if (texteGain        != null) texteGain.text        = "$" + gainPotentiel;
        if (texteSymboleRect != null) texteSymboleRect.text = symboleGagnant;
        if (boutonRectangle  != null) boutonRectangle.interactable = false;

        if (overlayRectangle != null)
            StartCoroutine(FadeOutOverlay(overlayRectangle));

        VerifierFinPartie();
    }

    // ── Vérification résultat ─────────────────────────────────────────────
    void VerifierFinPartie()
    {
        if (!rectGratté) return;
        for (int i = 0; i < 4; i++)
            if (!ballonsGrattés[i]) return;

        partieTerminee = true;

        int matches = 0;
        foreach (string s in symbolesBallons)
            if (s == symboleGagnant) matches++;

        if (matches >= 2)
        {
            if (texteResultat != null)
            {
                texteResultat.text  = "🏆 GAGNÉ !  +" + gainPotentiel + "$";
                texteResultat.color = new Color(0.2f, 1f, 0.2f);
            }
            GameManager.Instance.AjouterArgent(gainPotentiel);
            GameManager.Instance.AjouterFolie(5f);
        }
        else
        {
            if (texteResultat != null)
            {
                texteResultat.text  = "💀 Perdu...";
                texteResultat.color = new Color(1f, 0.25f, 0.25f);
            }
            GameManager.Instance.AjouterFolie(2f);
        }

        MettreAJourArgent();
        if (boutonNouveauTicket != null) boutonNouveauTicket.gameObject.SetActive(true);
    }

    // ── Helpers visuels ───────────────────────────────────────────────────
    static void SetOverlay(Image img, bool visible)
    {
        if (img == null) return;
        img.gameObject.SetActive(visible);
        var c = img.color; c.a = 1f; img.color = c;
    }

    IEnumerator FadeOutOverlay(Image overlay)
    {
        if (overlay == null) yield break;
        float t = 0f;
        const float dur = 0.35f;
        Color start = overlay.color;

        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / dur);
            overlay.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }
        overlay.gameObject.SetActive(false);
    }
}