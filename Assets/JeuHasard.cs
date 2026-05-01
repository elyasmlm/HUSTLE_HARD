using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class JeuHasard : MonoBehaviour
{
    [Header("UI")]
    public GameObject panneauJeu;
    public TextMeshProUGUI titreJeu;
    public TextMeshProUGUI texteResultat;
    public Button boutonA;
    public Button boutonB;
    public Button boutonFermer;

    private string choixJoueur = "";
    private bool enAttente = false;
    private PlayerController playerController;

    void Start()
    {
        boutonA.onClick.AddListener(() => ChoisirOption("A"));
        boutonB.onClick.AddListener(() => ChoisirOption("B"));
        boutonFermer.onClick.AddListener(FermerMenu);

        panneauJeu.SetActive(false);

        playerController = Object.FindFirstObjectByType<PlayerController>();
    }

    public void OuvrirMenu()
    {
        panneauJeu.SetActive(true);
        texteResultat.text = "";
        choixJoueur = "";
        enAttente = false;

        boutonA.interactable = true;
        boutonB.interactable = true;

        titreJeu.text = "Choisissez votre camp :";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;
    }

    void ChoisirOption(string choix)
    {
        if (enAttente) return;

        choixJoueur = choix;
        enAttente = true;

        boutonA.interactable = false;
        boutonB.interactable = false;

        texteResultat.text = "Tirage en cours...";
        titreJeu.text = "Vous avez choisi : " + choix;

        StartCoroutine(TirageAuSort());
    }

    IEnumerator TirageAuSort()
    {
        yield return new WaitForSeconds(1.5f);

        string resultatTirage = Random.value > 0.5f ? "A" : "B";

        if (choixJoueur == resultatTirage)
        {
            texteResultat.text = resultatTirage + " gagne !\nVous avez GAGNÉ !";
            texteResultat.color = Color.green;
        }
        else
        {
            texteResultat.text = resultatTirage + " gagne !\nVous avez PERDU...";
            texteResultat.color = Color.red;
        }

        enAttente = false;
    }

    void FermerMenu()
    {
        panneauJeu.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }
}