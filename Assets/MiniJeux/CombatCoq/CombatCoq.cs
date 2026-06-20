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
    public TextMeshProUGUI texteEffetCoqA;
    public Button boutonMiserA;
    public Image panneauCoqA;

    [Header("Coq B")]
    public TextMeshProUGUI texteNomCoqB;
    public TextMeshProUGUI texteCoteCoqB;
    public TextMeshProUGUI texteEffetCoqB;
    public Button boutonMiserB;
    public Image panneauCoqB;

    [Header("Mise")]
    public TMP_InputField inputMise;
    public TextMeshProUGUI texteArgentDisponible;
    public TextMeshProUGUI texteErreurMise;

    [Header("Resultat")]
    public GameObject zoneResultat;
    public TextMeshProUGUI texteResultat;
    public TextMeshProUGUI texteGainResultat;
    public Button boutonLancer;
    public Button boutonRejouer;
    public Button boutonFermer;

    // --- Couleurs surbrillance ---
    private static readonly Color COULEUR_COQ_A_NORMAL = new Color(0.25f, 0.06f, 0.06f, 1f);
    private static readonly Color COULEUR_COQ_A_SELEC  = new Color(0.70f, 0.08f, 0.08f, 1f);
    private static readonly Color COULEUR_COQ_B_NORMAL = new Color(0.08f, 0.08f, 0.08f, 1f);
    private static readonly Color COULEUR_COQ_B_SELEC  = new Color(0.28f, 0.28f, 0.28f, 1f);

    // --- Constantes ---
    private const int MISE_MIN = 5;

    // --- Donnees des coqs ---
    private CoqData coqA;
    private CoqData coqB;

    // --- Etat de la partie ---
    private int coqChoisi = -1;
    private int miseActuelle = 0;

    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();

        if (boutonMiserA != null) boutonMiserA.onClick.AddListener(() => ChoisirCoq(0));
        if (boutonMiserB != null) boutonMiserB.onClick.AddListener(() => ChoisirCoq(1));
        if (boutonLancer != null) boutonLancer.onClick.AddListener(LancerCombat);
        if (boutonRejouer != null) boutonRejouer.onClick.AddListener(NouvellePartie);
        if (boutonFermer != null) boutonFermer.onClick.AddListener(FermerPanneau);
        if (inputMise != null) inputMise.onValueChanged.AddListener(_ => ActualiserBoutonLancer());

        if (panneauCombat != null) panneauCombat.SetActive(false);
    }

    // -----------------------------------------------------------------------
    void Update()
    {
        if (!panneauCombat.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            FermerPanneau();
    }

    // -----------------------------------------------------------------------
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
    public const string NOM_COQ_A = "MasterPoulet";
    public const string NOM_COQ_B = "PouletBraise";

    void NouvellePartie()
    {
        coqChoisi = -1;
        miseActuelle = 0;

        coqA = GenererCoq(NOM_COQ_A);
        coqB = GenererCoq(NOM_COQ_B);
        NormaliserCotes();

        coqA.dope = GameManager.Instance.EstPouletDope(0);
        coqB.dope = GameManager.Instance.EstPouletDope(1);

        texteNomCoqA.text = coqA.nom;
        texteCoteCoqA.text = "x" + coqA.cote.ToString("F2");
        texteEffetCoqA.text = coqA.dope ? "DOPE !" : "";
        texteNomCoqB.text = coqB.nom;
        texteCoteCoqB.text = "x" + coqB.cote.ToString("F2");
        texteEffetCoqB.text = coqB.dope ? "DOPE !" : "";

        texteArgentDisponible.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");
        texteErreurMise.text = "";
        texteResultat.text = "";
        texteGainResultat.text = "";
        inputMise.text = "";

        boutonMiserA.interactable = true;
        boutonMiserB.interactable = true;
        boutonLancer.interactable = false;
        boutonRejouer.gameObject.SetActive(false);

        if (zoneResultat != null) zoneResultat.SetActive(false);

        AppliquerSurbrillance(-1);
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
        AppliquerSurbrillance(index);
        texteErreurMise.text = (index == 0 ? coqA.nom : coqB.nom) + " sélectionné !";
        texteErreurMise.color = Color.cyan;
        ActualiserBoutonLancer();
    }

    void AppliquerSurbrillance(int index)
    {
        if (panneauCoqA != null)
            panneauCoqA.color = (index == 0) ? COULEUR_COQ_A_SELEC : COULEUR_COQ_A_NORMAL;
        if (panneauCoqB != null)
            panneauCoqB.color = (index == 1) ? COULEUR_COQ_B_SELEC : COULEUR_COQ_B_NORMAL;
    }

    // Active le bouton Lancer dès que la mise est valide, même sans coq choisi
    void ActualiserBoutonLancer()
    {
        if (!int.TryParse(inputMise.text, out int mise) || mise < MISE_MIN)
        {
            boutonLancer.interactable = false;
            return;
        }

        if (mise > GameManager.Instance.argent)
        {
            boutonLancer.interactable = false;
            return;
        }

        boutonLancer.interactable = true;
    }

    bool ValiderMise(out int mise)
    {
        mise = 0;

        if (!int.TryParse(inputMise.text, out mise) || mise < MISE_MIN)
        {
            texteErreurMise.text = "Mise minimum : $" + MISE_MIN;
            texteErreurMise.color = Color.red;
            return false;
        }

        if (mise > GameManager.Instance.argent)
        {
            texteErreurMise.text = "Pas assez d'argent !";
            texteErreurMise.color = Color.red;
            return false;
        }

        return true;
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

        boutonMiserA.interactable = false;
        boutonMiserB.interactable = false;
        boutonLancer.interactable = false;

        GameManager.Instance.RetirerArgent(miseActuelle);
        texteArgentDisponible.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");

        StartCoroutine(AnimerCombat());
    }

    IEnumerator AnimerCombat()
    {
        texteResultat.text = "Combat en cours...";
        texteResultat.color = Color.white;
        texteGainResultat.text = "";

        if (zoneResultat != null) zoneResultat.SetActive(true);

        yield return new WaitForSeconds(2f);

        int indexGagnant = DeterminerVainqueur();
        CoqData gagnant = indexGagnant == 0 ? coqA : coqB;
        bool joueurAGagne = (coqChoisi == indexGagnant);
        float gain = joueurAGagne ? Mathf.Floor(miseActuelle * gagnant.cote) : 0f;

        texteResultat.text = "🏆 " + gagnant.nom + " remporte le combat !";

        if (joueurAGagne)
        {
            texteGainResultat.text = "GAGNÉ ! +" + gain + "$";
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

        // Le dopage est consomme : les poulets redeviennent normaux apres le combat.
        GameManager.Instance.ResetPouletDope(0);
        GameManager.Instance.ResetPouletDope(1);
        texteEffetCoqA.text = "";
        texteEffetCoqB.text = "";

        boutonRejouer.gameObject.SetActive(true);
    }

    // -----------------------------------------------------------------------
    int DeterminerVainqueur()
    {
        if (coqA.dope && !coqB.dope) return 0;
        if (coqB.dope && !coqA.dope) return 1;
        if (coqA.energise && !coqB.energise) return 0;
        if (coqB.energise && !coqA.energise) return 1;

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
