using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mini-jeu : Mini-Roulette
/// Le joueur paie 5$ pour faire tourner la roulette (10 cases).
/// Lots possibles : partie gratuite, argent, multiplicateur x2/x3,
/// chapeau cosmétique, technique de triche, ou rien.
/// Une partie gratuite permet de rejouer sans payer.
/// Les multiplicateurs s'appliquent au prochain gain du joueur.
/// </summary>
public class MiniRoulette : MonoBehaviour
{
    [Header("UI Panneau")]
    public GameObject panneauRoulette;

    [Header("Roulette")]
    public TextMeshProUGUI texteCaseActuelle;
    public TextMeshProUGUI texteAngleCaseIndicateur;

    [Header("Resultat")]
    public TextMeshProUGUI texteNomLot;
    public TextMeshProUGUI texteDescriptionLot;
    public TextMeshProUGUI texteResultatSpecial;

    [Header("UI Infos joueur")]
    public TextMeshProUGUI texteArgent;
    public TextMeshProUGUI textePartiesGratuites;
    public TextMeshProUGUI texteMultiplicateurActif;

    [Header("Boutons")]
    public Button boutonTourner;
    public Button boutonFermer;

    // --- Constantes ---
    private const int PRIX_TOUR = 5;

    // --- Definition des cases (10 cases, total = 100%) ---
    // Note : "Rien du tout" est mentionne dans les regles mais absent du tableau.
    // On l'inclut avec 0% (peut etre ajuste si besoin).
    private readonly List<CaseRoulette> cases = new List<CaseRoulette>
    {
        new CaseRoulette(TypeLot.NouvellePartieGratuite,    "Partie gratuite",          "Rejouez sans payer !",                     0.30f),
        new CaseRoulette(TypeLot.PetitGainArgent,           "Petit gain",               "Vous gagnez entre 1$ et 5$",               0.25f),
        new CaseRoulette(TypeLot.MultiplicateurX2,          "Multiplicateur x2",        "Votre prochain gain est double",           0.15f),
        new CaseRoulette(TypeLot.MultiplicateurX3,          "Multiplicateur x3",        "Votre prochain gain est triple",           0.08f),
        new CaseRoulette(TypeLot.ChapeauCommun,             "Chapeau commun",           "Un chapeau cosmétique simple",             0.10f),
        new CaseRoulette(TypeLot.ChapeauRare,               "Chapeau rare",             "Un chapeau cosmétique plus special !",     0.05f),
        new CaseRoulette(TypeLot.TechniqueTriche,           "Technique de triche",      "Un avantage utilisable dans le jeu",       0.07f),
        // Total = 1.00 (pas de "rien du tout" selon le tableau fourni)
    };

    // --- Etat ---
    private bool enRotation = false;
    private int partiesGratuitesDisponibles = 0;

    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();

        boutonTourner.onClick.AddListener(TenterTourner);
        boutonFermer.onClick.AddListener(FermerPanneau);

        panneauRoulette.SetActive(false);
    }

    // -----------------------------------------------------------------------
    public void OuvrirPanneau()
    {
        panneauRoulette.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;

        MettreAJourUI();
        ResetResultat();
    }

    // -----------------------------------------------------------------------
    void MettreAJourUI()
    {
        texteArgent.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");
        textePartiesGratuites.text = partiesGratuitesDisponibles > 0
            ? "Parties gratuites : " + partiesGratuitesDisponibles
            : "";
        texteMultiplicateurActif.text = GameManager.Instance.multiplicateurGain > 1f
            ? "Multiplicateur actif : x" + GameManager.Instance.multiplicateurGain
            : "";
    }

    void ResetResultat()
    {
        texteNomLot.text = "";
        texteDescriptionLot.text = "";
        texteResultatSpecial.text = "";
        texteCaseActuelle.text = "?";
        boutonTourner.interactable = true;
    }

    // -----------------------------------------------------------------------
    void TenterTourner()
    {
        if (enRotation) return;

        // Partie gratuite disponible -> gratuit
        if (partiesGratuitesDisponibles > 0)
        {
            partiesGratuitesDisponibles--;
            MettreAJourUI();
            StartCoroutine(AnimerRoulette());
            return;
        }

        // Sinon payer
        if (GameManager.Instance.argent < PRIX_TOUR)
        {
            texteResultatSpecial.text = "Pas assez d'argent ! (5$ requis)";
            texteResultatSpecial.color = UnityEngine.Color.red;
            return;
        }

        GameManager.Instance.RetirerArgent(PRIX_TOUR);
        MettreAJourUI();
        StartCoroutine(AnimerRoulette());
    }

    // -----------------------------------------------------------------------
    IEnumerator AnimerRoulette()
    {
        enRotation = true;
        boutonTourner.interactable = false;
        texteNomLot.text = "";
        texteDescriptionLot.text = "";
        texteResultatSpecial.text = "";

        // Animation : faire defiler les cases rapidement puis ralentir
        float dureeTotal = 2.5f;
        float elapsed = 0f;
        float interval = 0.08f; // vitesse initiale

        string[] nomsAffichage = ObtenirNomsAffichage();
        int indexAnim = 0;

        while (elapsed < dureeTotal)
        {
            texteCaseActuelle.text = nomsAffichage[indexAnim % nomsAffichage.Length];
            indexAnim++;

            // Ralentissement progressif dans la derniere seconde
            float ratioTemps = elapsed / dureeTotal;
            if (ratioTemps > 0.6f)
                interval = Mathf.Lerp(0.08f, 0.35f, (ratioTemps - 0.6f) / 0.4f);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // Tirer le lot final
        CaseRoulette lotTire = TirerLot();
        texteCaseActuelle.text = lotTire.nom;

        yield return new WaitForSeconds(0.3f);

        AppliquerLot(lotTire);
        MettreAJourUI();

        enRotation = false;
        boutonTourner.interactable = true;
    }

    // -----------------------------------------------------------------------
    CaseRoulette TirerLot()
    {
        float r = Random.value;
        float cumul = 0f;

        foreach (CaseRoulette c in cases)
        {
            cumul += c.probabilite;
            if (r < cumul) return c;
        }

        // Fallback (ne devrait pas arriver si les probas somment a 1)
        return cases[0];
    }

    // -----------------------------------------------------------------------
    void AppliquerLot(CaseRoulette lot)
    {
        texteNomLot.text = lot.nom;
        texteDescriptionLot.text = lot.description;

        switch (lot.type)
        {
            case TypeLot.NouvellePartieGratuite:
                partiesGratuitesDisponibles++;
                texteResultatSpecial.text = "🎟 Partie gratuite ajoutee !";
                texteResultatSpecial.color = UnityEngine.Color.cyan;
                break;

            case TypeLot.PetitGainArgent:
                float gain = Random.Range(1f, 6f);
                gain = Mathf.Floor(gain);
                // Appliquer le multiplicateur si actif
                gain = AppliquerMultiplicateur(gain);
                GameManager.Instance.AjouterArgent(gain);
                texteResultatSpecial.text = "+ $" + gain + " !";
                texteResultatSpecial.color = UnityEngine.Color.green;
                break;

            case TypeLot.MultiplicateurX2:
                GameManager.Instance.multiplicateurGain = 2f;
                texteResultatSpecial.text = "⚡ Multiplicateur x2 actif !";
                texteResultatSpecial.color = UnityEngine.Color.yellow;
                break;

            case TypeLot.MultiplicateurX3:
                GameManager.Instance.multiplicateurGain = 3f;
                texteResultatSpecial.text = "⚡ Multiplicateur x3 actif !";
                texteResultatSpecial.color = UnityEngine.Color.yellow;
                break;

            case TypeLot.ChapeauCommun:
                GameManager.Instance.AjouterChapeau(RareteChapeau.Commun);
                texteResultatSpecial.text = "🎩 Chapeau commun obtenu !";
                texteResultatSpecial.color = UnityEngine.Color.white;
                break;

            case TypeLot.ChapeauRare:
                GameManager.Instance.AjouterChapeau(RareteChapeau.Rare);
                texteResultatSpecial.text = "🎩 Chapeau RARE obtenu !";
                texteResultatSpecial.color = UnityEngine.Color.magenta;
                break;

            case TypeLot.TechniqueTriche:
                GameManager.Instance.AjouterTechniqueTriche();
                texteResultatSpecial.text = "🃏 Technique de triche obtenue !";
                texteResultatSpecial.color = UnityEngine.Color.yellow;
                break;

            case TypeLot.RienDuTout:
                texteResultatSpecial.text = "Rien... Dommage.";
                texteResultatSpecial.color = UnityEngine.Color.gray;
                break;
        }

        GameManager.Instance.AjouterFolie(3f);
    }

    // -----------------------------------------------------------------------
    /// <summary>
    /// Applique le multiplicateur de gain actif puis le remet a 1 (consomme).
    /// </summary>
    float AppliquerMultiplicateur(float gain)
    {
        float multi = GameManager.Instance.multiplicateurGain;
        if (multi > 1f)
        {
            gain *= multi;
            GameManager.Instance.multiplicateurGain = 1f; // consomme le multiplicateur
        }
        return gain;
    }

    // -----------------------------------------------------------------------
    string[] ObtenirNomsAffichage()
    {
        string[] noms = new string[cases.Count];
        for (int i = 0; i < cases.Count; i++)
            noms[i] = cases[i].nom;
        return noms;
    }

    // -----------------------------------------------------------------------
    void FermerPanneau()
    {
        panneauRoulette.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }
}