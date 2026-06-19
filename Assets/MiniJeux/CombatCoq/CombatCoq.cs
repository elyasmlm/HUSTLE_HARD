using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Mini-jeu : Combat de Coq
/// Le joueur mise sur un des deux coqs. Les cotes sont generees aleatoirement.
/// Triche : donner une boisson energisante a un coq garantit sa victoire.
/// La boisson doit avoir ete achetee ailleurs et etre dans l'inventaire du joueur.
/// </summary>
public class CombatCoq : MonoBehaviour
{
    [Header("UI Panneau")]
    public GameObject panneauCombat;

    [Header("Coq A")]
    public TextMeshProUGUI texteNomCoqA;
    public TextMeshProUGUI texteCoteCoqA;
    public TextMeshProUGUI texteEffetCoqA;   // affiche "ENERGISE !" si boisson donnee
    public Button boutonMiserA;
    public Button boutonBoisssonA;           // bouton pour donner boisson energisante au coq A

    [Header("Coq B")]
    public TextMeshProUGUI texteNomCoqB;
    public TextMeshProUGUI texteCoteCoqB;
    public TextMeshProUGUI texteEffetCoqB;
    public Button boutonMiserB;
    public Button boutonBoissonB;

    [Header("Mise")]
    public TMP_InputField inputMise;
    public TextMeshProUGUI texteArgentDisponible;
    public TextMeshProUGUI texteErreurMise;

    [Header("Resultat")]
    public TextMeshProUGUI texteResultat;
    public TextMeshProUGUI texteGainResultat;
    public Button boutonLancer;
    public Button boutonRejouer;
    public Button boutonFermer;

    // --- Constantes ---
    private const int MISE_MIN = 5;

    // --- Donnees des coqs ---
    private CoqData coqA;
    private CoqData coqB;

    // --- Etat de la partie ---
    private int coqChoisi = -1;   // 0 = coq A, 1 = coq B, -1 = pas encore choisi
    private int miseActuelle = 0;
    private bool partieEnCours = false;

    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();

        boutonMiserA.onClick.AddListener(() => ChoisirCoq(0));
        boutonMiserB.onClick.AddListener(() => ChoisirCoq(1));
        boutonBoisssonA.onClick.AddListener(() => DonnerBoisson(0));
        boutonBoissonB.onClick.AddListener(() => DonnerBoisson(1));
        boutonLancer.onClick.AddListener(LancerCombat);
        boutonRejouer.onClick.AddListener(NouvellePartie);
        boutonFermer.onClick.AddListener(FermerPanneau);

        panneauCombat.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // Ouverture depuis InteractionSystem
    public void OuvrirPanneau()
    {
        if (!GameManager.Instance.PeutJouer()) return;
        panneauCombat.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;

        NouvellePartie();
    }

    // -----------------------------------------------------------------------
    void NouvellePartie()
    {
        partieEnCours = false;
        coqChoisi = -1;
        miseActuelle = 0;

        // Generer les deux coqs avec des cotes aleatoires
        coqA = GenererCoq("Coq Rouge");
        coqB = GenererCoq("Coq Noir");

        // S'assurer que les cotes sont coherentes
        NormaliserCotes();

        // Mettre a jour l'UI
        texteNomCoqA.text = coqA.nom;
        texteCoteCoqA.text = "x" + coqA.cote.ToString("F2");
        texteEffetCoqA.text = "";

        texteNomCoqB.text = coqB.nom;
        texteCoteCoqB.text = "x" + coqB.cote.ToString("F2");
        texteEffetCoqB.text = "";

        texteArgentDisponible.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");
        texteErreurMise.text = "";
        texteResultat.text = "";
        texteGainResultat.text = "";
        inputMise.text = "";

        // Boutons
        boutonMiserA.interactable = true;
        boutonMiserB.interactable = true;
        boutonBoisssonA.interactable = true;
        boutonBoissonB.interactable = true;
        boutonLancer.interactable = false;
        boutonRejouer.gameObject.SetActive(false);
    }

    // -----------------------------------------------------------------------
    CoqData GenererCoq(string nom)
    {
        float cote = Mathf.Round(Random.Range(1.2f, 4.0f) * 100f) / 100f;
        return new CoqData(nom, cote);
    }

    void NormaliserCotes()
    {
        if (Mathf.Abs(coqA.cote - coqB.cote) < 0.15f)
            coqB.cote = Mathf.Round((coqA.cote + Random.Range(0.3f, 1.0f)) * 100f) / 100f;
    }

    // -----------------------------------------------------------------------
    void ChoisirCoq(int index)
    {
        coqChoisi = index;

        if (!ValiderMise(out miseActuelle)) return;

        boutonLancer.interactable = true;
        texteErreurMise.text = (index == 0 ? coqA.nom : coqB.nom) + " selectionne !";
        texteErreurMise.color = Color.cyan;
    }

    bool ValiderMise(out int mise)
    {
        mise = 0;

        if (!int.TryParse(inputMise.text, out mise) || mise < MISE_MIN)
        {
            texteErreurMise.text = "Mise minimum : $" + MISE_MIN;
            texteErreurMise.color = Color.red;
            boutonLancer.interactable = false;
            return false;
        }

        if (mise > GameManager.Instance.argent)
        {
            texteErreurMise.text = "Pas assez d'argent !";
            texteErreurMise.color = Color.red;
            boutonLancer.interactable = false;
            return false;
        }

        texteErreurMise.text = "";
        return true;
    }

    // -----------------------------------------------------------------------
    void DonnerBoisson(int index)
    {
        // Verifier que le joueur a une boisson dans son inventaire
        if (!GameManager.Instance.UtiliserBoisson())
        {
            texteErreurMise.text = "Vous n'avez pas de boisson energisante !";
            texteErreurMise.color = Color.red;
            return;
        }

        if (index == 0)
        {
            coqA.energise = true;
            texteEffetCoqA.text = "⚡ ENERGISE !";
            boutonBoisssonA.interactable = false;
        }
        else
        {
            coqB.energise = true;
            texteEffetCoqB.text = "⚡ ENERGISE !";
            boutonBoissonB.interactable = false;
        }

        texteErreurMise.text = "";
        // Ajouter un peu de folie (tricher c'est stressant)
        GameManager.Instance.AjouterFolie(3f);
    }

    // -----------------------------------------------------------------------
    void LancerCombat()
    {
        if (!ValiderMise(out miseActuelle)) return;
        if (coqChoisi == -1)
        {
            texteErreurMise.text = "Choisissez un coq !";
            texteErreurMise.color = Color.red;
            return;
        }

        partieEnCours = true;
        boutonMiserA.interactable = false;
        boutonMiserB.interactable = false;
        boutonBoisssonA.interactable = false;
        boutonBoissonB.interactable = false;
        boutonLancer.interactable = false;

        GameManager.Instance.RetirerArgent(miseActuelle);
        texteArgentDisponible.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");

        StartCoroutine(AnimerCombat());
    }

    IEnumerator AnimerCombat()
    {
        texteResultat.text = "Combat en cours...";
        texteResultat.color = Color.white;

        yield return new WaitForSeconds(2f);

        int indexGagnant = DeterminerVainqueur();
        CoqData gagnant = indexGagnant == 0 ? coqA : coqB;

        bool joueurAGagne = (coqChoisi == indexGagnant);
        float gain = joueurAGagne ? Mathf.Floor(miseActuelle * gagnant.cote) : 0f;

        texteResultat.text = "🏆 " + gagnant.nom + " remporte le combat !";

        if (joueurAGagne)
        {
            texteGainResultat.text = "GAGNE ! +" + gain + "$";
            texteGainResultat.color = Color.green;
            GameManager.Instance.AjouterArgent(gain);
            GameManager.Instance.AjouterFolie(4f);
        }
        else
        {
            texteGainResultat.text = "Perdu... -" + miseActuelle + "$";
            texteGainResultat.color = Color.red;
            GameManager.Instance.AjouterFolie(2f);
        }

        texteArgentDisponible.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");
        boutonRejouer.gameObject.SetActive(true);
        partieEnCours = false;
    }

    // -----------------------------------------------------------------------
    int DeterminerVainqueur()
    {
        // Regle de triche : coq energise gagne automatiquement
        if (coqA.energise && !coqB.energise) return 0;
        if (coqB.energise && !coqA.energise) return 1;

        // Si les deux sont energises ou aucun : probabilites basees sur les cotes
        float probA = 1f / coqA.cote;
        float probB = 1f / coqB.cote;
        float seuil = probA / (probA + probB);

        return Random.value < seuil ? 0 : 1;
    }

    // -----------------------------------------------------------------------
    void FermerPanneau()
    {
        panneauCombat.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }
}