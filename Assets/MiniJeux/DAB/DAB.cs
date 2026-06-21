using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Distributeur Automatique de Billets.
/// Depot : transfère argent (poche) -> compte bancaire et rembourse la dette.
/// Retrait : transfère compte bancaire -> argent (poche).
/// Victoire quand la dette est entierement remboursee.
/// </summary>
public class DAB : MonoBehaviour
{
    [Header("UI Panneau")]
    public GameObject panneauDAB;

    [Header("Informations")]
    public TextMeshProUGUI texteArgentPoche;
    public TextMeshProUGUI texteCompte;
    public TextMeshProUGUI texteDette;
    public TextMeshProUGUI texteProgresRemboursement;

    [Header("Saisie")]
    public TMP_InputField inputMontant;
    public TextMeshProUGUI texteErreur;

    [Header("Boutons")]
    public Button boutonDeposer;
    public Button boutonRetirer;
    public Button boutonFermer;

    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();

        if (boutonDeposer != null) boutonDeposer.onClick.AddListener(Deposer);
        if (boutonRetirer != null) boutonRetirer.onClick.AddListener(Retirer);
        if (boutonFermer  != null) boutonFermer.onClick.AddListener(FermerPanneau);

        if (panneauDAB != null) panneauDAB.SetActive(false);
    }

    // -----------------------------------------------------------------------
    void Update()
    {
        if (panneauDAB == null || !panneauDAB.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Escape)) FermerPanneau();
    }

    // -----------------------------------------------------------------------
    public void OuvrirPanneau()
    {
        if (panneauDAB == null)
        {
            Debug.LogError("[DAB] panneauDAB non assigne dans l'Inspector ! Lance Tools > DAB > Create UI in Scene.");
            return;
        }

        panneauDAB.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;

        if (inputMontant != null) inputMontant.text = "";
        if (texteErreur  != null) texteErreur.text = "";

        MettreAJourUI();
    }

    // -----------------------------------------------------------------------
    void Deposer()
    {
        if (!TryParseMontant(out float montant)) return;

        if (montant > GameManager.Instance.argent)
        {
            AfficherErreur("Vous n'avez pas assez d'argent en poche.");
            return;
        }

        GameManager.Instance.RetirerArgent(montant);
        GameManager.Instance.AjouterCompte(montant);

        if (texteErreur  != null) texteErreur.text = "";
        if (inputMontant != null) inputMontant.text = "";
        MettreAJourUI();
    }

    // -----------------------------------------------------------------------
    void Retirer()
    {
        if (!TryParseMontant(out float montant)) return;

        if (montant > GameManager.Instance.compte)
        {
            AfficherErreur("Solde du compte insuffisant.");
            return;
        }

        GameManager.Instance.RetirerCompte(montant);
        GameManager.Instance.AjouterArgent(montant);

        if (texteErreur  != null) texteErreur.text = "";
        if (inputMontant != null) inputMontant.text = "";
        MettreAJourUI();
    }

    // -----------------------------------------------------------------------
    bool TryParseMontant(out float montant)
    {
        montant = 0f;

        if (inputMontant == null || !float.TryParse(inputMontant.text, out montant) || montant <= 0f)
        {
            AfficherErreur("Entrez un montant valide.");
            return false;
        }

        montant = Mathf.Floor(montant);
        return true;
    }

    void AfficherErreur(string message)
    {
        if (texteErreur != null)
        {
            texteErreur.text = message;
            texteErreur.color = Color.red;
        }
    }

    // -----------------------------------------------------------------------
    void MettreAJourUI()
    {
        if (texteArgentPoche != null)
            texteArgentPoche.text = "En poche : $" + GameManager.Instance.argent.ToString("N0");

        if (texteCompte != null)
            texteCompte.text = "Compte : $" + GameManager.Instance.compte.ToString("N0");

        float dette = GameManager.Instance.dette;

        float manquant = Mathf.Max(0f, GameManager.Instance.dette - GameManager.Instance.compte);

        if (texteDette != null)
            texteDette.text = "Manquant sur le compte : $" + manquant.ToString("N0");

        if (texteProgresRemboursement != null)
        {
            float cible = GameManager.Instance.dette;
            float depot = GameManager.Instance.compte;
            float pourcent = cible > 0f ? Mathf.Clamp01(depot / cible) * 100f : 100f;
            texteProgresRemboursement.text = string.Format("Progres : {0:N0}% (${1:N0} / ${2:N0})",
                pourcent, depot, cible);
        }

        if (boutonRetirer != null)
            boutonRetirer.interactable = GameManager.Instance.compte > 0f;
    }

    // -----------------------------------------------------------------------
    void FermerPanneau()
    {
        panneauDAB.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }
}
