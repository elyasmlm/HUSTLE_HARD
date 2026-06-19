using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Économie")]
    public float argent = 500f;
    public float dette = 35000f;

    [Header("Timer")]
    public float tempsRestant = 1440f; // 24h en minutes

    [Header("Folie")]
    public float folie = 0f;
    public float folieMax = 100f;

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

        if (folie >= folieMax)
            GameOver("folie");

        if (argent <= 0)
            argent = 0;
    }

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

    public void AjouterFolie(float montant)
    {
        folie = Mathf.Clamp(folie + montant, 0, folieMax);
    }

    void Victoire()
    {
        partieTerminee = true;
        Debug.Log("VICTOIRE !");
        // TODO : afficher écran de victoire
    }

    void GameOver(string raison)
    {
        partieTerminee = true;
        Debug.Log("GAME OVER : " + raison);
        // TODO : afficher écran de game over
    }
}