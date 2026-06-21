using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mini-jeu : Mini-Roulette (100$ par tour)
/// Triche : aimant -> garantit le meilleur lot (Gain 1000$)
/// Bloque si folie >= 100%
/// </summary>
public class MiniRoulette : MonoBehaviour
{
    [Header("UI Panneau")]
    public GameObject panneauRoulette;

    [Header("Roulette")]
    public TextMeshProUGUI texteCaseActuelle;

    [Header("Resultat")]
    public GameObject zoneResultat;
    public TextMeshProUGUI texteNomLot;
    public TextMeshProUGUI texteDescriptionLot;
    public TextMeshProUGUI texteResultatSpecial;

    [Header("UI Infos joueur")]
    public TextMeshProUGUI texteArgent;
    public TextMeshProUGUI textePartiesGratuites;
    public TextMeshProUGUI texteMultiplicateurActif;
    public TextMeshProUGUI texteErreur;

    [Header("Boutons")]
    public Button boutonTourner;
    public Button boutonUtiliserAimant;
    public Button boutonRejouer;
    public Button boutonFermer;

    [Header("Triche")]
    public TextMeshProUGUI texteAimantsStock;

    // --- Constantes ---
    private const int PRIX_TOUR = 100;

    // --- Cases : total = 10+20+15+8+15+8+3+21 = 100% ---
    private readonly List<CaseRoulette> cases = new List<CaseRoulette>
    {
        new CaseRoulette(TypeLot.NouvellePartieGratuite, "Partie gratuite",   "Rejouez sans payer !",           0.10f),
        new CaseRoulette(TypeLot.Gain100,                "Gain 100$",         "Vous gagnez 100$ !",             0.20f),
        new CaseRoulette(TypeLot.MultiplicateurX2,       "Multiplicateur x2", "Votre prochain gain est double", 0.15f),
        new CaseRoulette(TypeLot.MultiplicateurX3,       "Multiplicateur x3", "Votre prochain gain est triple", 0.08f),
        new CaseRoulette(TypeLot.Gain200,                "Gain 200$",         "Vous gagnez 200$ !",             0.15f),
        new CaseRoulette(TypeLot.Gain500,                "Gain 500$",         "Vous gagnez 500$ !",             0.08f),
        new CaseRoulette(TypeLot.Gain1000,               "Gain 1000$",        "Vous gagnez 1 000$ !",           0.03f),
        new CaseRoulette(TypeLot.Perdu,                  "Perdu",             "Pas de chance...",               0.21f),
    };

    // --- Etat ---
    private bool enRotation = false;
    private int partiesGratuitesDisponibles = 0;
    private bool aimantActif = false;

    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();

        boutonTourner.onClick.AddListener(TenterTourner);
        boutonUtiliserAimant.onClick.AddListener(UtiliserAimant);
        boutonFermer.onClick.AddListener(FermerPanneau);

        panneauRoulette.SetActive(false);
    }

    // -----------------------------------------------------------------------
    void Update()
    {
        if (panneauRoulette == null || !panneauRoulette.activeSelf) return;
        if (GameManager.Instance.folie >= GameManager.Instance.folieMax) FermerPanneau();
    }

    // -----------------------------------------------------------------------
    public void OuvrirPanneau()
    {
        // Bloquer si folie a 100%
        if (GameManager.Instance.folie >= GameManager.Instance.folieMax)
        {
            // On n'ouvre pas le panneau, le message sera gere par l'InteractionSystem
            return;
        }

        panneauRoulette.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;

        aimantActif = false;
        MettreAJourUI();
        ResetResultat();
    }

    // -----------------------------------------------------------------------
    void MettreAJourUI()
    {
        if (texteArgent != null)
            texteArgent.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");

        if (textePartiesGratuites != null)
            textePartiesGratuites.text = partiesGratuitesDisponibles > 0
                ? "🎟 x" + partiesGratuitesDisponibles + " gratuit(s)"
                : "";

        if (texteMultiplicateurActif != null)
            texteMultiplicateurActif.text = GameManager.Instance.multiplicateurGain > 1f
                ? "⚡ x" + GameManager.Instance.multiplicateurGain
                : "";

        if (texteAimantsStock != null)
            texteAimantsStock.text = GameManager.Instance.aimants > 0
                ? "🧲 x" + GameManager.Instance.aimants
                : "Aucun aimant";

        if (boutonUtiliserAimant != null)
            boutonUtiliserAimant.interactable = GameManager.Instance.aimants > 0 && !aimantActif;

        bool peutJouer = !enRotation &&
            (partiesGratuitesDisponibles > 0 || GameManager.Instance.argent >= PRIX_TOUR);
        if (boutonTourner != null) boutonTourner.interactable = peutJouer;

        if (texteErreur != null)
        {
            if (GameManager.Instance.folie >= GameManager.Instance.folieMax)
            {
                texteErreur.text = "Folie maximale : impossible de jouer.";
                texteErreur.color = Color.red;
                if (boutonTourner != null) boutonTourner.interactable = false;
            }
            else if (!peutJouer)
            {
                texteErreur.text = "Argent insuffisant. (100$ requis)";
                texteErreur.color = Color.red;
            }
            else
            {
                texteErreur.text = "";
            }
        }
    }

    void ResetResultat()
    {
        if (texteNomLot        != null) texteNomLot.text = "";
        if (texteDescriptionLot != null) texteDescriptionLot.text = "";
        if (texteResultatSpecial != null)
        {
            texteResultatSpecial.text = aimantActif ? "🧲 Aimant actif !" : "";
            texteResultatSpecial.color = Color.white;
        }
        if (texteCaseActuelle != null) texteCaseActuelle.text = "?";
        if (zoneResultat  != null) zoneResultat.SetActive(false);
        if (boutonRejouer != null) boutonRejouer.gameObject.SetActive(false);
    }

    void NouveauTour()
    {
        ResetResultat();
        MettreAJourUI();
    }

    // -----------------------------------------------------------------------
    void UtiliserAimant()
    {
        if (!GameManager.Instance.UtiliserAimant()) return;

        aimantActif = true;
        if (boutonUtiliserAimant != null) boutonUtiliserAimant.interactable = false;
        if (texteResultatSpecial != null)
        {
            texteResultatSpecial.text = "🧲 Aimant actif ! Prochain tour garanti Gain 1000$";
            texteResultatSpecial.color = UnityEngine.Color.magenta;
        }
        MettreAJourUI();
    }

    // -----------------------------------------------------------------------
    void TenterTourner()
    {
        if (enRotation) return;

        if (partiesGratuitesDisponibles > 0)
        {
            partiesGratuitesDisponibles--;
            MettreAJourUI();
            StartCoroutine(AnimerRoulette());
            return;
        }

        if (GameManager.Instance.argent < PRIX_TOUR)
        {
            if (texteErreur != null)
            {
                texteErreur.text = "Pas assez d'argent ! (100$ requis)";
                texteErreur.color = UnityEngine.Color.red;
            }
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
        if (boutonTourner  != null) boutonTourner.interactable = false;
        if (boutonRejouer  != null) boutonRejouer.gameObject.SetActive(false);
        if (zoneResultat   != null) zoneResultat.SetActive(false);
        if (texteNomLot        != null) texteNomLot.text = "";
        if (texteDescriptionLot != null) texteDescriptionLot.text = "";
        if (texteResultatSpecial != null)
            texteResultatSpecial.text = aimantActif ? "🧲 Aimant actif..." : "";
        if (texteErreur != null) texteErreur.text = "";

        float dureeTotal = 2.5f;
        float elapsed = 0f;
        float interval = 0.08f;

        string[] nomsAffichage = ObtenirNomsAffichage();
        int indexAnim = 0;

        while (elapsed < dureeTotal)
        {
            if (texteCaseActuelle != null)
                texteCaseActuelle.text = nomsAffichage[indexAnim % nomsAffichage.Length];
            indexAnim++;

            float ratioTemps = elapsed / dureeTotal;
            if (ratioTemps > 0.6f)
                interval = Mathf.Lerp(0.08f, 0.35f, (ratioTemps - 0.6f) / 0.4f);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // Tirer le lot
        CaseRoulette lotTire = aimantActif
            ? cases.Find(c => c.type == TypeLot.Gain1000)
            : TirerLot();

        aimantActif = false;
        if (texteCaseActuelle != null) texteCaseActuelle.text = lotTire.nom;

        yield return new WaitForSeconds(0.3f);

        AppliquerLot(lotTire);
        MettreAJourUI();

        enRotation = false;

        // Trop de defaites : Francis Mecano glisse un indice.
        if (lotTire.type == TypeLot.Perdu && GameManager.Instance.DefaiteRoulette())
        {
            yield return new WaitForSeconds(1.5f);
            FermerPanneau();
            if (SystemeDialogue.Instance != null)
                SystemeDialogue.Instance.Afficher("Francis Mécano",
                    "Putain... j'ai fait tomber ma boîte d'aimants derrière une bagnole. Si tu les retrouves, ils sont à toi.");
            yield break;
        }

        // Afficher zone résultat + bouton nouveau tour
        if (zoneResultat  != null) zoneResultat.SetActive(true);
        if (boutonRejouer != null) boutonRejouer.gameObject.SetActive(true);

        if (boutonTourner != null) boutonTourner.interactable = true;
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

        return cases[cases.Count - 1]; // fallback : Perdu
    }

    // -----------------------------------------------------------------------
    void AppliquerLot(CaseRoulette lot)
    {
        if (texteNomLot        != null) texteNomLot.text = lot.nom;
        if (texteDescriptionLot != null) texteDescriptionLot.text = lot.description;

        switch (lot.type)
        {
            case TypeLot.NouvellePartieGratuite:
                partiesGratuitesDisponibles++;
                if (texteResultatSpecial != null) { texteResultatSpecial.text = "🎟 Partie gratuite ajoutée !"; texteResultatSpecial.color = UnityEngine.Color.cyan; }
                break;

            case TypeLot.Gain100:  AppliquerGain(100f);  break;
            case TypeLot.Gain200:  AppliquerGain(200f);  break;
            case TypeLot.Gain500:  AppliquerGain(500f);  break;
            case TypeLot.Gain1000: AppliquerGain(1000f); break;

            case TypeLot.MultiplicateurX2:
                GameManager.Instance.multiplicateurGain = 2f;
                if (texteResultatSpecial != null) { texteResultatSpecial.text = "⚡ Multiplicateur x2 actif !"; texteResultatSpecial.color = UnityEngine.Color.yellow; }
                break;

            case TypeLot.MultiplicateurX3:
                GameManager.Instance.multiplicateurGain = 3f;
                if (texteResultatSpecial != null) { texteResultatSpecial.text = "⚡ Multiplicateur x3 actif !"; texteResultatSpecial.color = UnityEngine.Color.yellow; }
                break;

            case TypeLot.Perdu:
                if (texteResultatSpecial != null) { texteResultatSpecial.text = "Perdu... -100$"; texteResultatSpecial.color = UnityEngine.Color.red; }
                GameManager.Instance.AjouterFolie(5f);
                return;
        }

        GameManager.Instance.AjouterFolie(3f);
    }

    void AppliquerGain(float montant)
    {
        float gainFinal = montant * GameManager.Instance.multiplicateurGain;
        if (GameManager.Instance.multiplicateurGain > 1f)
            GameManager.Instance.multiplicateurGain = 1f;

        GameManager.Instance.AjouterArgent(gainFinal);
        if (texteResultatSpecial != null)
        {
            texteResultatSpecial.text = "+ $" + gainFinal.ToString("N0") + " !";
            texteResultatSpecial.color = UnityEngine.Color.green;
        }
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