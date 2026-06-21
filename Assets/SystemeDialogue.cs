using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Fenetre de dialogue reutilisable (un seul exemplaire en scene).
/// - Une ligne : SystemeDialogue.Instance.Afficher("Nom", "Texte...");
/// - Plusieurs lignes : SystemeDialogue.Instance.AfficherSequence("Nom", ligne1, ligne2, ...);
/// Le bouton Continuer (ou Entree) passe a la ligne suivante, puis ferme.
/// Libere le curseur et bloque le joueur pendant l'affichage.
/// </summary>
public class SystemeDialogue : MonoBehaviour
{
    private static SystemeDialogue _instance;

    public static SystemeDialogue Instance
    {
        get
        {
            if (_instance == null)
                _instance = Object.FindFirstObjectByType<SystemeDialogue>(FindObjectsInactive.Include);
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("UI")]
    public GameObject panneau;
    public TextMeshProUGUI texteNom;
    public TextMeshProUGUI texteDialogue;
    public Button boutonContinuer;

    private PlayerController playerController;
    private readonly Queue<string> file = new Queue<string>();
    private string nomCourant;

    // -----------------------------------------------------------------------
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();
        if (boutonContinuer != null) boutonContinuer.onClick.AddListener(Suivant);
        if (panneau != null) panneau.SetActive(false);
    }

    void Update()
    {
        if (panneau == null || !panneau.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Suivant();
    }

    // -----------------------------------------------------------------------
    /// <summary>Affiche une seule ligne.</summary>
    public void Afficher(string nom, string texte)
    {
        file.Clear();
        nomCourant = nom;
        AfficherLigne(texte);
    }

    /// <summary>Affiche une suite de lignes (meme interlocuteur), l'une apres l'autre.</summary>
    public void AfficherSequence(string nom, params string[] lignes)
    {
        if (lignes == null || lignes.Length == 0) return;

        file.Clear();
        nomCourant = nom;
        for (int i = 1; i < lignes.Length; i++)
            file.Enqueue(lignes[i]);

        AfficherLigne(lignes[0]);
    }

    // -----------------------------------------------------------------------
    void AfficherLigne(string texte)
    {
        if (panneau == null)
        {
            Debug.LogError("[SystemeDialogue] panneau non assigne ! Lance Tools > Dialogue > Create UI in Scene.");
            return;
        }

        panneau.SetActive(true);
        if (texteNom      != null) texteNom.text = nomCourant;
        if (texteDialogue != null) texteDialogue.text = texte;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController == null)
            playerController = Object.FindFirstObjectByType<PlayerController>();
        if (playerController != null) playerController.menuOuvert = true;
    }

    // -----------------------------------------------------------------------
    /// <summary>Ligne suivante, ou ferme si c'etait la derniere.</summary>
    public void Suivant()
    {
        if (file.Count > 0)
            AfficherLigne(file.Dequeue());
        else
            Fermer();
    }

    // -----------------------------------------------------------------------
    public void Fermer()
    {
        file.Clear();
        if (panneau != null) panneau.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }
}
