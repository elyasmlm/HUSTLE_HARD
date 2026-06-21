using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mini-jeu : Blackjack simplifie
/// Regles :
///   - Mise minimum 20$, gain x2
///   - Joueur et croupier recoivent 2 cartes (1 carte croupier cachee)
///   - Joueur : Tirer (nouvelle carte) ou Rester
///   - Croupier tire jusqu'a >= 17
///   - Plus proche de 21 sans depasser gagne
///   - Bust (> 21) = defaite immediate
///   - Blackjack naturel (21 en 2 cartes) = victoire immediate
///   - Egalite = remboursement de la mise
///
/// Triche : Soudoyer le croupier (5000$)
///   -> Sur les 3 prochaines parties perdues, la mise est remboursee
/// </summary>
public class Blackjack : MonoBehaviour
{
    [Header("UI Panneau")]
    public GameObject panneauBlackjack;

    [Header("Mise")]
    public TMP_InputField inputMise;
    public TextMeshProUGUI texteArgent;
    public TextMeshProUGUI texteErreurMise;
    public Button boutonConfirmerMise;

    [Header("Jeu")]
    public GameObject panneauJeu;
    public TextMeshProUGUI texteMainJoueur;
    public TextMeshProUGUI texteScoreJoueur;
    public TextMeshProUGUI texteMainCroupier;
    public TextMeshProUGUI texteScoreCroupier;
    public TextMeshProUGUI texteMiseEnCours;

    [Header("Boutons de jeu")]
    public Button boutonTirer;
    public Button boutonRester;

    [Header("Triche")]
    public TextMeshProUGUI texteProtectionsRestantes;

    [Header("Resultat")]
    public GameObject zoneResultat;
    public TextMeshProUGUI texteResultat;
    public Button boutonRejouer;
    public Button boutonFermer;

    // --- Constantes ---
    private const int MISE_MIN = 20;
    private const int COUT_SOUDOYER = 5000;
    private const int PARTIES_PROTEGEES = 3;
    private const int SCORE_CROUPIER_MIN = 17;

    // --- Etat ---
    private List<Carte> mainJoueur = new List<Carte>();
    private List<Carte> mainCroupier = new List<Carte>();
    private int miseActuelle = 0;
    private bool partieEnCours = false;
    private int partiesProtegees = 0;

    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();

        if (boutonConfirmerMise != null) boutonConfirmerMise.onClick.AddListener(CommencerPartie);
        if (boutonTirer       != null) boutonTirer.onClick.AddListener(Tirer);
        if (boutonRester      != null) boutonRester.onClick.AddListener(Rester);
        if (boutonRejouer     != null) boutonRejouer.onClick.AddListener(ResetPanneau);
        if (boutonFermer      != null) boutonFermer.onClick.AddListener(FermerPanneau);

        if (panneauBlackjack != null) panneauBlackjack.SetActive(false);
    }

    // -----------------------------------------------------------------------
    void Update()
    {
        if (panneauBlackjack == null || !panneauBlackjack.activeSelf) return;
        if (GameManager.Instance.folie >= GameManager.Instance.folieMax) { FermerPanneau(); return; }
        if (Input.GetKeyDown(KeyCode.Escape)) FermerPanneau();
    }

    // -----------------------------------------------------------------------
    public void OuvrirPanneau()
    {
        if (!GameManager.Instance.PeutJouer()) return;

        panneauBlackjack.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;

        ResetPanneau();
    }

    // -----------------------------------------------------------------------
    void ResetPanneau()
    {
        partieEnCours = false;
        mainJoueur.Clear();
        mainCroupier.Clear();

        if (panneauJeu != null) panneauJeu.SetActive(false);
        if (zoneResultat != null) zoneResultat.SetActive(false);
        if (texteResultat != null) texteResultat.text = "";
        if (inputMise != null) inputMise.text = "";
        if (texteErreurMise != null) texteErreurMise.text = "";

        if (boutonConfirmerMise != null) boutonConfirmerMise.gameObject.SetActive(true);
        if (boutonRejouer != null) boutonRejouer.gameObject.SetActive(false);

        MettreAJourArgent();
        MettreAJourProtections();
    }

    void MettreAJourArgent()
    {
        if (texteArgent != null)
            texteArgent.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");
    }

    void MettreAJourProtections()
    {
        if (texteProtectionsRestantes != null)
            texteProtectionsRestantes.text = partiesProtegees > 0
                ? "Croupier soudoyé - " + partiesProtegees + " partie(s) protégée(s)"
                : "";
    }

    // -----------------------------------------------------------------------
    void CommencerPartie()
    {
        if (!int.TryParse(inputMise.text, out miseActuelle) || miseActuelle < MISE_MIN)
        {
            texteErreurMise.text = "Mise minimum : $" + MISE_MIN;
            texteErreurMise.color = Color.red;
            return;
        }

        if (miseActuelle > GameManager.Instance.argent)
        {
            texteErreurMise.text = "Pas assez d'argent !";
            texteErreurMise.color = Color.red;
            return;
        }

        GameManager.Instance.RetirerArgent(miseActuelle);
        MettreAJourArgent();

        if (boutonConfirmerMise != null) boutonConfirmerMise.gameObject.SetActive(false);
        if (panneauJeu != null) panneauJeu.SetActive(true);
        if (texteResultat != null) texteResultat.text = "";
        if (zoneResultat != null) zoneResultat.SetActive(false);
        partieEnCours = true;

        // Distribuer 2 cartes a chacun
        mainJoueur.Clear();
        mainCroupier.Clear();

        mainJoueur.Add(TirerCarte());
        mainCroupier.Add(TirerCarte());
        mainJoueur.Add(TirerCarte());
        mainCroupier.Add(TirerCarte());

        if (texteMiseEnCours != null) texteMiseEnCours.text = "Mise : $" + miseActuelle;
        if (boutonTirer != null) boutonTirer.interactable = true;
        if (boutonRester != null) boutonRester.interactable = true;

        AfficherMains(croupierCache: true);

        // Verifier blackjack naturel joueur
        if (CalculerScore(mainJoueur) == 21)
            StartCoroutine(FinPartie(ResultatPartie.Blackjack));
    }

    // -----------------------------------------------------------------------
    void Tirer()
    {
        if (!partieEnCours) return;

        mainJoueur.Add(TirerCarte());
        AfficherMains(croupierCache: true);

        int score = CalculerScore(mainJoueur);
        if (score > 21)
            StartCoroutine(FinPartie(ResultatPartie.Bust));
        else if (score == 21)
            StartCoroutine(FinPartie(ResultatPartie.Victoire));
    }

    // -----------------------------------------------------------------------
    void Rester()
    {
        if (!partieEnCours) return;

        boutonTirer.interactable = false;
        boutonRester.interactable = false;

        StartCoroutine(TourCroupier());
    }

    IEnumerator TourCroupier()
    {
        // Reveler la carte cachee
        AfficherMains(croupierCache: false);
        yield return new WaitForSeconds(0.8f);

        // Le croupier tire jusqu'a >= 17
        while (CalculerScore(mainCroupier) < SCORE_CROUPIER_MIN)
        {
            mainCroupier.Add(TirerCarte());
            AfficherMains(croupierCache: false);
            yield return new WaitForSeconds(0.6f);
        }

        // Determiner le resultat
        int scorejoueur = CalculerScore(mainJoueur);
        int scoreCroupier = CalculerScore(mainCroupier);

        ResultatPartie resultat;
        if (scoreCroupier > 21)
            resultat = ResultatPartie.Victoire;
        else if (scorejoueur > scoreCroupier)
            resultat = ResultatPartie.Victoire;
        else if (scorejoueur == scoreCroupier)
            resultat = ResultatPartie.Egalite;
        else
            resultat = ResultatPartie.Defaite;

        StartCoroutine(FinPartie(resultat));
    }

    // -----------------------------------------------------------------------
    IEnumerator FinPartie(ResultatPartie resultat)
    {
        yield return new WaitForSeconds(0.3f);

        partieEnCours = false;
        if (boutonTirer  != null) boutonTirer.interactable = false;
        if (boutonRester != null) boutonRester.interactable = false;
        AfficherMains(croupierCache: false);
        if (zoneResultat != null) zoneResultat.SetActive(true);

        bool indiceTriche = false;

        switch (resultat)
        {
            case ResultatPartie.Blackjack:
                if (texteResultat != null) { texteResultat.text = "🃏 BLACKJACK ! +$" + (miseActuelle * 2); texteResultat.color = Color.yellow; }
                GameManager.Instance.AjouterArgent(miseActuelle * 2);
                GameManager.Instance.AjouterFolie(6f);
                break;

            case ResultatPartie.Victoire:
                if (texteResultat != null) { texteResultat.text = "✅ Victoire ! +$" + (miseActuelle * 2); texteResultat.color = Color.green; }
                GameManager.Instance.AjouterArgent(miseActuelle * 2);
                GameManager.Instance.AjouterFolie(4f);
                break;

            case ResultatPartie.Egalite:
                if (texteResultat != null) { texteResultat.text = "🤝 Égalité — mise remboursée"; texteResultat.color = Color.cyan; }
                GameManager.Instance.AjouterArgent(miseActuelle);
                GameManager.Instance.AjouterFolie(2f);
                break;

            case ResultatPartie.Bust:
                if (texteResultat != null) { texteResultat.text = "💥 Bust ! Vous dépassez 21."; texteResultat.color = Color.red; }
                indiceTriche = AppliquerDefaite();
                break;

            case ResultatPartie.Defaite:
                if (texteResultat != null) { texteResultat.text = "❌ Perdu ! Le croupier gagne."; texteResultat.color = Color.red; }
                indiceTriche = AppliquerDefaite();
                break;
        }

        MettreAJourArgent();

        // Trop de defaites : le croupier glisse un indice.
        if (indiceTriche)
        {
            yield return new WaitForSeconds(1.5f);
            FermerPanneau();
            if (SystemeDialogue.Instance != null)
                SystemeDialogue.Instance.Afficher("Le croupier",
                    "Un conseil... la prochaine fois, passe donc à droite de ma table avant de jouer. Ça t'éviterait de perdre autant.");
            yield break;
        }

        if (boutonRejouer != null) boutonRejouer.gameObject.SetActive(true);
    }

    /// <summary>Applique la defaite. Retourne true si un indice de triche doit etre montre.</summary>
    bool AppliquerDefaite()
    {
        GameManager.Instance.AjouterFolie(5f);

        if (partiesProtegees > 0)
        {
            GameManager.Instance.AjouterArgent(miseActuelle);
            partiesProtegees--;
            if (texteResultat != null)
                texteResultat.text += "\n🛡 Bouclier utilisé — mise remboursée !";
            MettreAJourProtections();
            return false; // perte annulee : pas d'indice
        }

        return GameManager.Instance.DefaiteBlackjack();
    }

    // -----------------------------------------------------------------------
    /// <summary>
    /// Soudoie le croupier. Appele depuis le cube "Soudoyer" en scene.
    /// Retourne false si le joueur n'a pas assez d'argent ou est deja protege.
    /// </summary>
    public bool SoudoyerCroupier()
    {
        if (partiesProtegees > 0)
        {
            if (texteErreurMise != null)
            {
                texteErreurMise.text = "Le croupier est deja soudoyé.";
                texteErreurMise.color = Color.yellow;
            }
            return false;
        }

        if (GameManager.Instance.argent < COUT_SOUDOYER)
        {
            if (texteErreurMise != null)
            {
                texteErreurMise.text = "Il faut $" + COUT_SOUDOYER + " pour soudoyer le croupier !";
                texteErreurMise.color = Color.red;
            }
            return false;
        }

        GameManager.Instance.RetirerArgent(COUT_SOUDOYER);
        partiesProtegees = PARTIES_PROTEGEES;

        MettreAJourArgent();
        MettreAJourProtections();

        if (texteErreurMise != null)
        {
            texteErreurMise.text = "💰 Croupier soudoyé ! 3 parties protégées.";
            texteErreurMise.color = Color.green;
        }

        GameManager.Instance.AjouterFolie(8f);
        return true;
    }

    // -----------------------------------------------------------------------
    /// <summary>
    /// Tire une carte aleatoire (deck infini).
    /// </summary>
    Carte TirerCarte()
    {
        string[] symboles = { "?", "?", "?", "?" };
        string symbole = symboles[Random.Range(0, symboles.Length)];

        int tirage = Random.Range(1, 14);

        string nom;
        int valeur;

        switch (tirage)
        {
            case 1: nom = "A"; valeur = 11; break;
            case 11: nom = "V"; valeur = 10; break;
            case 12: nom = "D"; valeur = 10; break;
            case 13: nom = "R"; valeur = 10; break;
            default: nom = tirage.ToString(); valeur = tirage; break;
        }

        return new Carte(nom, symbole, valeur);
    }

    /// <summary>
    /// Calcule le score d'une main en gerant les As (11 ou 1).
    /// </summary>
    int CalculerScore(List<Carte> main)
    {
        int score = 0;
        int nombreAs = 0;

        foreach (Carte c in main)
        {
            score += c.valeur;
            if (c.nom == "A") nombreAs++;
        }

        // Reduire les As de 11 a 1 si on depasse 21
        while (score > 21 && nombreAs > 0)
        {
            score -= 10;
            nombreAs--;
        }

        return score;
    }

    // -----------------------------------------------------------------------
    void AfficherMains(bool croupierCache)
    {
        // Main joueur
        texteMainJoueur.text = "Vous : " + MainEnTexte(mainJoueur);
        texteScoreJoueur.text = "Score : " + CalculerScore(mainJoueur);

        // Main croupier
        if (croupierCache && mainCroupier.Count >= 2)
        {
            texteMainCroupier.text = "Croupier : " + mainCroupier[0] + "  [?]";
            texteScoreCroupier.text = "Score : ?";
        }
        else
        {
            texteMainCroupier.text = "Croupier : " + MainEnTexte(mainCroupier);
            texteScoreCroupier.text = "Score : " + CalculerScore(mainCroupier);
        }
    }

    string MainEnTexte(List<Carte> main)
    {
        string resultat = "";
        foreach (Carte c in main)
            resultat += c.ToString() + "  ";
        return resultat.TrimEnd();
    }

    // -----------------------------------------------------------------------
    void FermerPanneau()
    {
        panneauBlackjack.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }
}

/// <summary>
/// Resultats possibles d'une partie de blackjack.
/// </summary>
public enum ResultatPartie
{
    Victoire,
    Defaite,
    Egalite,
    Blackjack,
    Bust
}