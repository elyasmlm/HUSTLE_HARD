using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mini-jeu : Case Opening (inspire de CS:GO)
/// Le joueur achete une caisse et obtient une arme d'une certaine rarete.
/// Il peut ensuite vendre l'arme pour recuperer de l'argent.
///
/// Caisse normale      : 10$  - armes valant jusqu'a 100$
/// Caisse supersonique : 20$  - armes valant jusqu'a 1000$
///
/// Raretes et chances :
///   Normale     60%
///   Rare        30%
///   Mythique     5%
///   Legendaire   3%
///   Antique      1.5%
///   Extra Rare   0.5%
/// </summary>
public class CaseOpening : MonoBehaviour
{
    [Header("UI Panneau")]
    public GameObject panneauCase;

    [Header("Choix de caisse")]
    public Button boutonCaisseNormale;
    public Button boutonCaisseSupersonique;
    public TextMeshProUGUI texteDescCaisseNormale;
    public TextMeshProUGUI texteDescCaisseSupersonique;

    [Header("Animation ouverture")]
    public GameObject panneauAnimation;
    public TextMeshProUGUI texteDefilement;   // nom des armes qui defilent

    [Header("Resultat")]
    public GameObject panneauResultat;
    public TextMeshProUGUI texteNomArme;
    public TextMeshProUGUI texteRareteArme;
    public TextMeshProUGUI texteValeurArme;
    public TextMeshProUGUI texteMessageResultat;

    [Header("Actions apres ouverture")]
    public Button boutonRejouer;
    public Button boutonFermer;

    [Header("Infos joueur")]
    public TextMeshProUGUI texteArgent;

    // --- Constantes ---
    private const int PRIX_CAISSE_NORMALE = 10;
    private const int PRIX_CAISSE_SUPERSONIQUE = 20;

    // --- Probabilites des raretes (doivent sommer a 1) ---
    private static readonly Dictionary<RareteArme, float> probas = new Dictionary<RareteArme, float>
    {
        { RareteArme.Normale,     0.600f },
        { RareteArme.Rare,        0.300f },
        { RareteArme.Mythique,    0.050f },
        { RareteArme.Legendaire,  0.030f },
        { RareteArme.Antique,     0.015f },
        { RareteArme.ExtraRare,   0.005f },
    };

    // --- Noms d'armes par rarete (pour la variete) ---
    private static readonly Dictionary<RareteArme, string[]> nomsArmes = new Dictionary<RareteArme, string[]>
    {
        { RareteArme.Normale,    new[] { "Pistolet Rouille",   "Couteau Basique",    "Fusil Fatigue",      "Revolver Commun"    } },
        { RareteArme.Rare,       new[] { "Desert Eagle Bleu",  "AK-47 Forêt",        "MP5 Chrome",         "Glock Tactique"     } },
        { RareteArme.Mythique,   new[] { "AWP Serpent",        "M4A4 Flammes",       "USP Fantôme",        "P250 Dunes"         } },
        { RareteArme.Legendaire, new[] { "AK-47 Sang Dragon",  "AWP Meduse",         "M4A1-S Indien",      "Deagle Or Noir"     } },
        { RareteArme.Antique,    new[] { "AWP Dragon Lore",    "AK-47 Feu Sauvage",  "M4A4 Howl",          "Karambit Doppler"   } },
        { RareteArme.ExtraRare,  new[] { "★ Karambit Fade",    "★ Butterfly Gem",    "★ Stiletto Sapphire","★ Talon Ruby"       } },
    };

    // --- Plages de valeurs par rarete et type de caisse ---
    // Caisse normale : max 100$
    private static readonly Dictionary<RareteArme, (float min, float max)> valeursNormale =
        new Dictionary<RareteArme, (float, float)>
    {
        { RareteArme.Normale,     (1f,   8f)   },
        { RareteArme.Rare,        (8f,   25f)  },
        { RareteArme.Mythique,    (25f,  50f)  },
        { RareteArme.Legendaire,  (50f,  75f)  },
        { RareteArme.Antique,     (75f,  90f)  },
        { RareteArme.ExtraRare,   (90f,  100f) },
    };

    // Caisse supersonique : max 1000$
    private static readonly Dictionary<RareteArme, (float min, float max)> valeursSupersonique =
        new Dictionary<RareteArme, (float, float)>
    {
        { RareteArme.Normale,     (1f,    20f)   },
        { RareteArme.Rare,        (20f,   80f)   },
        { RareteArme.Mythique,    (80f,   200f)  },
        { RareteArme.Legendaire,  (200f,  400f)  },
        { RareteArme.Antique,     (400f,  700f)  },
        { RareteArme.ExtraRare,   (700f,  1000f) },
    };

    // --- Etat ---
    private Arme derniereArme = null;
    private TypeCaisse caisseChoisie;
    private bool enAnimation = false;

    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();

        boutonCaisseNormale.onClick.AddListener(() => AcheterCaisse(TypeCaisse.Normale));
        boutonCaisseSupersonique.onClick.AddListener(() => AcheterCaisse(TypeCaisse.Supersonique));
        boutonRejouer.onClick.AddListener(ResetChoix);
        boutonFermer.onClick.AddListener(FermerPanneau);

        panneauCase.SetActive(false);
    }

    // -----------------------------------------------------------------------
    public void OuvrirPanneau()
    {
        if (!GameManager.Instance.PeutJouer()) return;
        panneauCase.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;

        ResetChoix();
    }

    // -----------------------------------------------------------------------
    void ResetChoix()
    {
        derniereArme = null;
        panneauAnimation.SetActive(false);
        panneauResultat.SetActive(false);

        boutonCaisseNormale.interactable = GameManager.Instance.argent >= PRIX_CAISSE_NORMALE;
        boutonCaisseSupersonique.interactable = GameManager.Instance.argent >= PRIX_CAISSE_SUPERSONIQUE;

        texteDescCaisseNormale.text = "Caisse Normale\n10$\nArmes jusqu'a 100$";
        texteDescCaisseSupersonique.text = "Caisse Supersonique\n20$\nArmes jusqu'a 1 000$";

        MettreAJourArgent();
    }

    void MettreAJourArgent()
    {
        texteArgent.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");
    }

    // -----------------------------------------------------------------------
    void AcheterCaisse(TypeCaisse type)
    {
        if (enAnimation) return;

        int prix = type == TypeCaisse.Normale ? PRIX_CAISSE_NORMALE : PRIX_CAISSE_SUPERSONIQUE;

        if (GameManager.Instance.argent < prix)
        {
            texteMessageResultat.text = "Pas assez d'argent !";
            return;
        }

        caisseChoisie = type;
        GameManager.Instance.RetirerArgent(prix);
        MettreAJourArgent();

        boutonCaisseNormale.interactable = false;
        boutonCaisseSupersonique.interactable = false;

        StartCoroutine(AnimerOuverture());
    }

    // -----------------------------------------------------------------------
    IEnumerator AnimerOuverture()
    {
        enAnimation = true;
        panneauAnimation.SetActive(true);
        panneauResultat.SetActive(false);

        // Tirer la rarete et l'arme finale maintenant (mais ne pas l'afficher)
        RareteArme rareteTiree = TirerRarete();
        derniereArme = GenererArme(rareteTiree, caisseChoisie);

        // Animation : defiler des noms d'armes aleatoires
        float duree = 3f;
        float elapsed = 0f;
        float interval = 0.07f;

        List<string> tousLesNoms = ObtenirTousLesNoms();

        while (elapsed < duree)
        {
            string nomAleatoire = tousLesNoms[Random.Range(0, tousLesNoms.Count)];
            texteDefilement.text = nomAleatoire;

            // Ralentissement progressif
            float ratio = elapsed / duree;
            if (ratio > 0.65f)
                interval = Mathf.Lerp(0.07f, 0.4f, (ratio - 0.65f) / 0.35f);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // Afficher l'arme finale
        texteDefilement.text = derniereArme.nom;
        yield return new WaitForSeconds(0.4f);

        panneauAnimation.SetActive(false);
        AfficherResultat();

        enAnimation = false;
    }

    // -----------------------------------------------------------------------
    void AfficherResultat()
    {
        panneauResultat.SetActive(true);

        texteNomArme.text = derniereArme.nom;
        texteRareteArme.text = derniereArme.rarete.ToString().ToUpper();
        texteRareteArme.color = CouleurRarete(derniereArme.rarete);

        // Gain automatique : la valeur de l'arme est directement ajoutee
        float gainFinal = AppliquerMultiplicateur(derniereArme.valeur);
        GameManager.Instance.AjouterArgent(gainFinal);
        MettreAJourArgent();

        texteValeurArme.text = "Valeur : $" + gainFinal.ToString("N0");
        texteMessageResultat.text = "+ $" + gainFinal.ToString("N0") + " encaisse !";
        texteMessageResultat.color = UnityEngine.Color.green;

        boutonRejouer.gameObject.SetActive(true);

        GameManager.Instance.AjouterFolie(3f);
        derniereArme = null;
    }

    // -----------------------------------------------------------------------
    RareteArme TirerRarete()
    {
        float r = Random.value;
        float cumul = 0f;

        foreach (var kvp in probas)
        {
            cumul += kvp.Value;
            if (r < cumul) return kvp.Key;
        }

        return RareteArme.Normale; // fallback
    }

    Arme GenererArme(RareteArme rarete, TypeCaisse type)
    {
        string[] noms = nomsArmes[rarete];
        string nom = noms[Random.Range(0, noms.Length)];

        var plage = type == TypeCaisse.Normale ? valeursNormale[rarete] : valeursSupersonique[rarete];
        float valeur = Mathf.Floor(Random.Range(plage.min, plage.max));

        return new Arme(nom, rarete, valeur, type);
    }

    float AppliquerMultiplicateur(float valeur)
    {
        float multi = GameManager.Instance.multiplicateurGain;
        if (multi > 1f)
        {
            valeur *= multi;
            GameManager.Instance.multiplicateurGain = 1f;
        }
        return Mathf.Floor(valeur);
    }

    List<string> ObtenirTousLesNoms()
    {
        var liste = new List<string>();
        foreach (var kvp in nomsArmes)
            liste.AddRange(kvp.Value);
        return liste;
    }

    UnityEngine.Color CouleurRarete(RareteArme rarete)
    {
        switch (rarete)
        {
            case RareteArme.Normale: return UnityEngine.Color.gray;
            case RareteArme.Rare: return UnityEngine.Color.blue;
            case RareteArme.Mythique: return new UnityEngine.Color(0.5f, 0f, 1f);   // violet
            case RareteArme.Legendaire: return new UnityEngine.Color(1f, 0.5f, 0f);   // orange
            case RareteArme.Antique: return UnityEngine.Color.red;
            case RareteArme.ExtraRare: return new UnityEngine.Color(1f, 0.84f, 0f);  // or
            default: return UnityEngine.Color.white;
        }
    }

    // -----------------------------------------------------------------------
    void FermerPanneau()
    {
        panneauCase.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }
}