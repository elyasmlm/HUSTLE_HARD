using UnityEngine;

public class Persistent : MonoBehaviour
{
    public static Persistent Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // d�truit le doublon au retour dans SampleScene
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Detruit l'objet persistant (joueur, GameManager...) pour repartir
    /// d'un etat totalement neuf au prochain chargement de scene.
    /// A appeler avant de lancer une nouvelle partie ou de revenir au menu.
    /// </summary>
    public static void DetruireInstance()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
}