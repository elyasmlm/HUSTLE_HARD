using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu pause (style page d'accueil) : Reprendre, Nouvelle partie, Options, Quitter.
/// S'ouvre avec Echap quand aucun autre ecran de jeu n'est ouvert.
/// Met le jeu en pause via Time.timeScale.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("Panneaux")]
    public GameObject overlay;           // fond + groupe de boutons
    public GameObject groupeBoutons;     // les 4 boutons principaux
    public GameObject panneauOptions;    // sous-panneau options

    [Header("Scenes")]
    public string sceneMenuPrincipal = "MainMenu";
    public string sceneNouvellePartie = "SampleScene";

    private PlayerController playerController;
    private bool enPause = false;
    private bool optionsOuvert = false;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();
        if (overlay != null) overlay.SetActive(false);
        if (panneauOptions != null) panneauOptions.SetActive(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (optionsOuvert) { FermerOptions(); return; }
        if (enPause)       { Reprendre();    return; }

        // N'ouvre la pause que si aucun autre menu/jeu n'est ouvert.
        if (playerController == null || !playerController.menuOuvert)
            OuvrirPause();
    }

    // -----------------------------------------------------------------------
    public void OuvrirPause()
    {
        enPause = true;
        optionsOuvert = false;
        Time.timeScale = 0f;

        if (overlay != null) overlay.SetActive(true);
        if (groupeBoutons != null) groupeBoutons.SetActive(true);
        if (panneauOptions != null) panneauOptions.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;
    }

    public void Reprendre()
    {
        enPause = false;
        optionsOuvert = false;
        Time.timeScale = 1f;

        if (overlay != null) overlay.SetActive(false);
        if (panneauOptions != null) panneauOptions.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }

    public void NouvellePartie()
    {
        Time.timeScale = 1f;
        IntroAppel.ReinitialiserIntro();
        PlayerPrefs.SetString("SpawnPoint", "SpawnPointDepart");
        Persistent.DetruireInstance();   // repart d'un etat neuf
        SceneManager.LoadScene(sceneNouvellePartie);
    }

    public void OuvrirOptions()
    {
        optionsOuvert = true;
        if (groupeBoutons != null) groupeBoutons.SetActive(false);
        if (panneauOptions != null) panneauOptions.SetActive(true);
    }

    public void FermerOptions()
    {
        optionsOuvert = false;
        if (panneauOptions != null) panneauOptions.SetActive(false);
        if (groupeBoutons != null) groupeBoutons.SetActive(true);
    }

    public void Quitter()
    {
        // Retour a la page d'accueil : on detruit l'etat pour que la prochaine
        // partie reparte de zero.
        Time.timeScale = 1f;
        Persistent.DetruireInstance();
        SceneManager.LoadScene(sceneMenuPrincipal);
    }
}
