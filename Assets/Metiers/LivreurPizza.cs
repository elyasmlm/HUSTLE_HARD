using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Metier : Livreur de Pizza
/// Mini-jeu : Labyrinthe a resoudre en moins de 1 minute.
/// Le joueur deplace un personnage (pizza) dans un labyrinthe
/// pour atteindre la sortie (maison).
/// Recompense : entre 10$ et 30$ selon le temps restant.
/// </summary>
public class LivreurPizza : MonoBehaviour
{
    [Header("UI Panneau")]
    public GameObject panneauLivreur;

    [Header("Labyrinthe")]
    public GameObject panneauLabyrinthe;
    public RectTransform joueurIcon;
    public RectTransform sortieIcon;
    public RectTransform[] murs;

    [Header("Timer")]
    public TextMeshProUGUI texteTimer;
    public TextMeshProUGUI texteInstruction;

    [Header("Resultat")]
    public TextMeshProUGUI texteResultat;
    public TextMeshProUGUI texteGain;
    public Button boutonCommencer;
    public Button boutonRejouer;
    public Button boutonFermer;

    [Header("Info joueur")]
    public TextMeshProUGUI texteArgent;

    // --- Constantes ---
    private const float DUREE_MAX = 60f;
    private const float VITESSE_JOUEUR = 150f;
    private const float TAILLE_JOUEUR = 30f;
    private const int GAIN_MIN = 10;
    private const int GAIN_MAX = 30;
    private const float DISTANCE_VICTOIRE = 40f;

    // --- Etat ---
    private float tempsRestant;
    private bool partieEnCours = false;
    private bool partieTerminee = false;

    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();

        if (boutonCommencer != null) boutonCommencer.onClick.AddListener(CommencerPartie);
        else Debug.LogWarning("[LivreurPizza] boutonCommencer est null !");
        if (boutonRejouer != null)   boutonRejouer.onClick.AddListener(ResetPartie);
        else Debug.LogWarning("[LivreurPizza] boutonRejouer est null !");
        if (boutonFermer != null)    boutonFermer.onClick.AddListener(FermerPanneau);
        else Debug.LogWarning("[LivreurPizza] boutonFermer est null !");

        if (panneauLivreur != null) panneauLivreur.SetActive(false);
        else Debug.LogWarning("[LivreurPizza] panneauLivreur est null !");
    }

    // -----------------------------------------------------------------------
    private bool PeutJouer()
    {
        if (panneauLivreur == null)    { Debug.LogWarning("[LivreurPizza] panneauLivreur manquant.");    return false; }
        if (joueurIcon == null)         { Debug.LogWarning("[LivreurPizza] joueurIcon manquant.");         return false; }
        if (sortieIcon == null)         { Debug.LogWarning("[LivreurPizza] sortieIcon manquant.");         return false; }
        if (panneauLabyrinthe == null)  { Debug.LogWarning("[LivreurPizza] panneauLabyrinthe manquant."); return false; }
        if (texteTimer == null)         { Debug.LogWarning("[LivreurPizza] texteTimer manquant.");         return false; }
        return true;
    }

    // -----------------------------------------------------------------------
    public void OuvrirPanneau()
    {
        if (!PeutJouer()) return;

        panneauLivreur.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;

        ResetPartie();
    }

    // -----------------------------------------------------------------------
    void ResetPartie()
    {
        partieEnCours = false;
        partieTerminee = false;
        tempsRestant = DUREE_MAX;

        if (panneauLabyrinthe != null) panneauLabyrinthe.SetActive(false);
        if (texteResultat    != null) texteResultat.text    = "";
        if (texteGain        != null) texteGain.text        = "";
        if (texteTimer       != null) texteTimer.text       = "1:00";
        if (texteInstruction != null) texteInstruction.text = "Livrez la pizza avant la fin du temps !";

        if (boutonCommencer != null) boutonCommencer.gameObject.SetActive(true);
        if (boutonRejouer   != null) boutonRejouer.gameObject.SetActive(false);

        if (joueurIcon != null)
            joueurIcon.anchoredPosition = EntreeLabyrinthe();

        MettreAJourArgent();
    }

    void MettreAJourArgent()
    {
        if (texteArgent != null && GameManager.Instance != null)
            texteArgent.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");
    }

    // -----------------------------------------------------------------------
    void CommencerPartie()
    {
        boutonCommencer.gameObject.SetActive(false);
        panneauLabyrinthe.SetActive(true);
        partieEnCours = true;
        partieTerminee = false;
        tempsRestant = DUREE_MAX;

        joueurIcon.anchoredPosition = EntreeLabyrinthe();
    }

    // -----------------------------------------------------------------------
    void Update()
    {
        if (panneauLivreur != null && panneauLivreur.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            FermerPanneau();
            return;
        }

        if (!partieEnCours || partieTerminee) return;

        // --- Timer ---
        tempsRestant -= Time.deltaTime;
        AfficherTimer();

        if (tempsRestant <= 0)
        {
            tempsRestant = 0;
            StartCoroutine(FinPartie(false));
            return;
        }

        // --- Deplacement joueur (fleches ou ZQSD) ---
        Vector2 direction = Vector2.zero;
        if (Input.GetKey(KeyCode.UpArrow)    || Input.GetKey(KeyCode.W)) direction += Vector2.up;
        if (Input.GetKey(KeyCode.DownArrow)  || Input.GetKey(KeyCode.S)) direction += Vector2.down;
        if (Input.GetKey(KeyCode.LeftArrow)  || Input.GetKey(KeyCode.A)) direction += Vector2.left;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) direction += Vector2.right;

        if (direction != Vector2.zero)
        {
            direction.Normalize();
            Vector2 nouvellePos = joueurIcon.anchoredPosition + direction * VITESSE_JOUEUR * Time.deltaTime;

            // Verifier collision avec les murs
            if (!CollisionMur(nouvellePos))
                joueurIcon.anchoredPosition = nouvellePos;
        }

        // --- Verifier si le joueur a atteint la sortie ---
        float distance = Vector2.Distance(joueurIcon.anchoredPosition, sortieIcon.anchoredPosition);
        if (distance < DISTANCE_VICTOIRE)
            StartCoroutine(FinPartie(true));
    }

    // -----------------------------------------------------------------------
    bool CollisionMur(Vector2 nouvellePos)
    {
        if (murs == null) return false;

        Rect joueurRect = new Rect(
            nouvellePos.x - TAILLE_JOUEUR / 2f,
            nouvellePos.y - TAILLE_JOUEUR / 2f,
            TAILLE_JOUEUR,
            TAILLE_JOUEUR
        );

        foreach (RectTransform mur in murs)
        {
            if (mur == null) continue;

            Rect murRect = new Rect(
                mur.anchoredPosition.x - mur.sizeDelta.x / 2f,
                mur.anchoredPosition.y - mur.sizeDelta.y / 2f,
                mur.sizeDelta.x,
                mur.sizeDelta.y
            );

            if (joueurRect.Overlaps(murRect))
                return true;
        }

        return false;
    }

    // -----------------------------------------------------------------------
    IEnumerator FinPartie(bool victoire)
    {
        partieEnCours = false;
        partieTerminee = true;

        yield return new WaitForSeconds(0.2f);

        panneauLabyrinthe.SetActive(false);
        boutonRejouer.gameObject.SetActive(true);

        if (victoire)
        {
            // Gain proportionnel au temps restant
            float ratio = tempsRestant / DUREE_MAX;
            int gain = Mathf.RoundToInt(Mathf.Lerp(GAIN_MIN, GAIN_MAX, ratio));

            GameManager.Instance.AjouterArgent(gain);
            GameManager.Instance.ResetFolie();
            MettreAJourArgent();

            texteResultat.text = "Pizza livree !";
            texteResultat.color = Color.green;
            texteGain.text = "+ $" + gain + " (temps restant : " + Mathf.CeilToInt(tempsRestant) + "s)";
            texteGain.color = Color.green;
        }
        else
        {
            texteResultat.text = "Temps ecoule ! Pizza froide...";
            texteResultat.color = Color.red;
            texteGain.text = "Aucune recompense.";
            texteGain.color = Color.gray;
        }
    }

    // -----------------------------------------------------------------------
    void AfficherTimer()
    {
        int minutes = Mathf.FloorToInt(tempsRestant / 60f);
        int secondes = Mathf.CeilToInt(tempsRestant % 60f);
        texteTimer.text = string.Format("{0}:{1:00}", minutes, secondes);
        texteTimer.color = tempsRestant < 15f ? Color.red : Color.white;
    }

    /// <summary>
    /// Position de depart du joueur (coin entree du labyrinthe).
    /// A ajuster selon la taille de ton Canvas dans l'Inspector.
    /// </summary>
    Vector2 EntreeLabyrinthe()
    {
        return new Vector2(-300f, 165f);
    }

    // -----------------------------------------------------------------------
    public void FermerPanneau()
    {
        partieEnCours = false;
        panneauLivreur.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }
}