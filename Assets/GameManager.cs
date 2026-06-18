using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public enum RareteChapeau { Commun, Rare }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Economie")]
    public float argent = 500f;
    public float dette = 35000f;

    [Header("Timer")]
    public float tempsRestant = 1440f; // 24h en minutes

    [Header("Folie")]
    public float folie = 0f;
    public float folieMax = 100f;

    [Header("Multiplicateur")]
    public float multiplicateurGain = 1f;

    [Header("Inventaire")]
    public int boissonEnergisante = 0;

    [Header("Inventaire cosmetiques")]
    public int chapeauxCommuns = 0;
    public int chapeauxRares = 0;

    [Header("Techniques de triche")]
    public int techniquesDisponibles = 0;

    private bool partieTerminee = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (partieTerminee) return;

        tempsRestant -= Time.deltaTime;
        if (tempsRestant <= 0)
        {
            tempsRestant = 0;
            GameOver("timer");
        }

        if (folie >= folieMax)
            GameOver("folie");

        if (argent <= 0)
            argent = 0;
    }

    // --- Argent ---

    public void AjouterArgent(float montant)
    {
        argent += montant;
        if (argent >= dette)
            Victoire();
    }

    public void RetirerArgent(float montant)
    {
        argent -= montant;
    }

    // --- Folie ---

    public void AjouterFolie(float montant)
    {
        folie = Mathf.Clamp(folie + montant, 0, folieMax);
    }

    // --- Boisson energisante ---

    public void AjouterBoisson(int quantite = 1)
    {
        boissonEnergisante += quantite;
    }

    /// <summary>
    /// Tente de consommer une boisson. Retourne true si le joueur en avait une.
    /// </summary>
    public bool UtiliserBoisson()
    {
        if (boissonEnergisante <= 0) return false;
        boissonEnergisante--;
        return true;
    }

    // --- Cosmetiques ---

    public void AjouterChapeau(RareteChapeau rarete)
    {
        if (rarete == RareteChapeau.Commun) chapeauxCommuns++;
        else chapeauxRares++;
    }

    // --- Techniques de triche ---

    public void AjouterTechniqueTriche()
    {
        techniquesDisponibles++;
    }

    // --- Fin de partie ---

    void Victoire()
    {
        partieTerminee = true;
        Debug.Log("VICTOIRE !");
        // TODO : afficher ecran de victoire
    }

    void GameOver(string raison)
    {
        partieTerminee = true;
        Debug.Log("GAME OVER : " + raison);
        // TODO : afficher ecran de game over
    }

    // --- Gestion des scenes ---

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (eventSystems.Length > 1)
            for (int i = 1; i < eventSystems.Length; i++)
                Destroy(eventSystems[i].gameObject);

        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length > 1)
            for (int i = 1; i < listeners.Length; i++)
                Destroy(listeners[i]);

        StartCoroutine(ResetJoueur());
    }

    IEnumerator ResetJoueur()
    {
        yield return null;

        PlayerController[] pcs = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        if (pcs != null && pcs.Length > 0)
        {
            PlayerController pc = pcs[0];
            pc.menuOuvert = false;
            pc.enabled = false;
            yield return null;
            pc.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}