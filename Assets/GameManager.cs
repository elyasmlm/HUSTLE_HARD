using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum RareteChapeau { Commun, Rare }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Economie")]
    public float argent = 500f;
    public float compte = 0f;
    public float dette = 35000f;
    [HideInInspector] public float detteInitiale = 35000f;

    [Header("Timer")]
    public float tempsRestant = 1440f;

    [Header("Folie")]
    public float folie = 0f;
    public float folieMax = 100f;

    [Header("Multiplicateur")]
    public float multiplicateurGain = 1f;

    [Header("Inventaire")]
    public int boissonEnergisante = 0;
    public int zdeh = 0;
    public int aimants = 0;
    public int viagra = 0;

    [Header("Inventaire cosmetiques")]
    public int chapeauxCommuns = 0;
    public int chapeauxRares = 0;

    [Header("Techniques de triche")]
    public int techniquesDisponibles = 0;

    private const float ZDEH_REDUCTION_PAR_SECONDE = 10f;
    private bool enTrainDeFumer = false;

    private bool partieTerminee = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        detteInitiale = dette;
        PlayerPrefs.SetString("SpawnPoint", "SpawnPointDepart");
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

        if (argent <= 0)
            argent = 0;
    }

    // --- Argent ---

    public void AjouterArgent(float montant)
    {
        argent += montant;
    }

    public void RetirerArgent(float montant)
    {
        argent -= montant;
        if (argent < 0f) argent = 0f;
    }

    public void AjouterCompte(float montant)
    {
        compte += montant;
        if (compte >= dette)
            Victoire();
    }

    public void RetirerCompte(float montant)
    {
        compte -= montant;
        if (compte < 0f) compte = 0f;
    }


    // --- Folie ---

    public void AjouterFolie(float montant)
    {
        folie = Mathf.Clamp(folie + montant, 0, folieMax);
    }

    /// <summary>
    /// Verifie si le joueur peut jouer (folie < 100%).
    /// </summary>
    public bool PeutJouer()
    {
        return folie < folieMax;
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

    // --- Zdeh (calme la folie progressivement) ---

    public void AjouterZdeh(int quantite = 1)
    {
        zdeh += quantite;
    }

    /// <summary>
    /// Fume un zdeh : reduit la folie progressivement sur la duree donnee.
    /// Retourne false si le joueur n'en a pas.
    /// </summary>
    public bool UtiliserZdeh()
    {
        if (zdeh <= 0) return false;
        zdeh--;
        StartCoroutine(EffetZdeh());
        return true;
    }

    IEnumerator EffetZdeh()
    {
        enTrainDeFumer = true;

        while (folie > 0)
        {
            folie = Mathf.Max(0, folie - ZDEH_REDUCTION_PAR_SECONDE * Time.deltaTime);
            yield return null;
        }

        folie = 0;
        enTrainDeFumer = false;
    }

    // --- Aimant (triche roulette) ---

    public void AjouterAimant(int quantite = 1)
    {
        aimants += quantite;
    }

    public const int VIAGRA_MAX = 1;

    public void AjouterViagra(int quantite = 1)
    {
        viagra = Mathf.Min(viagra + quantite, VIAGRA_MAX);
    }

    public bool UtiliserViagra()
    {
        if (viagra <= 0) return false;
        viagra--;
        return true;
    }

    // --- Poulets dopes (triche combat de coq) ---
    // index 0 = MasterPoulet, 1 = PouletBraise

    [HideInInspector] public bool[] pouletsDopes = new bool[2];

    public void DoperPoulet(int index)
    {
        if (index >= 0 && index < pouletsDopes.Length)
            pouletsDopes[index] = true;
    }

    public bool EstPouletDope(int index)
    {
        return index >= 0 && index < pouletsDopes.Length && pouletsDopes[index];
    }

    public void ResetPouletDope(int index)
    {
        if (index >= 0 && index < pouletsDopes.Length)
            pouletsDopes[index] = false;
    }

    /// <summary>
    /// Utilise un aimant. Retourne false si le joueur n'en a pas.
    /// </summary>
    public bool UtiliserAimant()
    {
        if (aimants <= 0) return false;
        aimants--;
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