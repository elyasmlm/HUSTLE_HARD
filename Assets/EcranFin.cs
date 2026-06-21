using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Ecran de fin de partie : victoire (dette remboursee) ou game over (temps ecoule).
/// Un seul panneau, reutilise pour les deux issues.
/// </summary>
public class EcranFin : MonoBehaviour
{
    private static EcranFin _instance;
    public static EcranFin Instance
    {
        get
        {
            if (_instance == null)
                _instance = Object.FindFirstObjectByType<EcranFin>(FindObjectsInactive.Include);
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("UI")]
    public GameObject panneau;
    public TextMeshProUGUI texteTitre;
    public TextMeshProUGUI texteSousTitre;
    public Button boutonRejouer;
    public Button boutonMenu;
    public Button boutonQuitter;

    [Header("Scenes")]
    public string sceneJeu = "SampleScene";
    public string sceneMenuPrincipal = "MainMenu";

    private static readonly Color COULEUR_VICTOIRE = new Color(0.95f, 0.80f, 0.20f, 1f);
    private static readonly Color COULEUR_DEFAITE  = new Color(0.85f, 0.10f, 0.10f, 1f);

    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();
        if (boutonRejouer  != null) boutonRejouer.onClick.AddListener(Rejouer);
        if (boutonMenu     != null) boutonMenu.onClick.AddListener(MenuPrincipal);
        if (boutonQuitter  != null) boutonQuitter.onClick.AddListener(Quitter);
        if (panneau != null) panneau.SetActive(false);
    }

    // -----------------------------------------------------------------------
    public void AfficherVictoire()
    {
        Afficher("VICTOIRE",
            "Dette remboursée ! Tu as réuni les 35 000$ sur ton compte. La banque ne t'aura pas.",
            COULEUR_VICTOIRE);
    }

    public void AfficherGameOver()
    {
        Afficher("GAME OVER",
            "Temps écoulé. Tu n'as pas réuni les 35 000$ à temps... la banque t'a rattrapé.",
            COULEUR_DEFAITE);
    }

    // -----------------------------------------------------------------------
    void Afficher(string titre, string sousTitre, Color couleur)
    {
        if (panneau == null)
        {
            Debug.LogError("[EcranFin] panneau non assigne ! Lance Tools > MenuGame > Create End Screens.");
            return;
        }

        panneau.SetActive(true);
        if (texteTitre != null) { texteTitre.text = titre; texteTitre.color = couleur; }
        if (texteSousTitre != null) texteSousTitre.text = sousTitre;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController == null)
            playerController = Object.FindFirstObjectByType<PlayerController>();
        if (playerController != null) playerController.menuOuvert = true;
    }

    // -----------------------------------------------------------------------
    void Rejouer()
    {
        Time.timeScale = 1f;
        IntroAppel.ReinitialiserIntro();
        SceneManager.LoadScene(sceneJeu);
    }

    void MenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneMenuPrincipal);
    }

    void Quitter()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
