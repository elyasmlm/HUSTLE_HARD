using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mini-jeu : Case Opening (inspire de CS:GO)
/// Le joueur achete une caisse et obtient une arme d'une certaine rarete.
/// La valeur de l'arme est directement encaissee.
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
    public TextMeshProUGUI texteMultiplicateur;
    public TextMeshProUGUI texteErreurCo;

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

        if (boutonCaisseNormale != null)
            boutonCaisseNormale.onClick.AddListener(() => AcheterCaisse(TypeCaisse.Normale));
        if (boutonCaisseSupersonique != null)
            boutonCaisseSupersonique.onClick.AddListener(() => AcheterCaisse(TypeCaisse.Supersonique));
        if (boutonRejouer != null)
            boutonRejouer.onClick.AddListener(ResetChoix);
        if (boutonFermer != null)
            boutonFermer.onClick.AddListener(FermerPanneau);

        if (panneauCase != null) panneauCase.SetActive(false);
    }

    // -----------------------------------------------------------------------
    void Update()
    {
        if (panneauCase != null && panneauCase.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            FermerPanneau();
    }

    // -----------------------------------------------------------------------
    public void OuvrirPanneau()
    {
        if (!GameManager.Instance.PeutJouer()) return;
        if (panneauCase != null) panneauCase.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;

        ResetChoix();
    }

    // -----------------------------------------------------------------------
    void ResetChoix()
    {
        derniereArme = null;
        if (panneauAnimation != null) panneauAnimation.SetActive(false);
        if (panneauResultat != null)  panneauResultat.SetActive(false);
        if (boutonRejouer != null)    boutonRejouer.gameObject.SetActive(false);

        if (boutonCaisseNormale != null)
            boutonCaisseNormale.interactable = GameManager.Instance.argent >= PRIX_CAISSE_NORMALE;
        if (boutonCaisseSupersonique != null)
            boutonCaisseSupersonique.interactable = GameManager.Instance.argent >= PRIX_CAISSE_SUPERSONIQUE;

        if (texteDescCaisseNormale != null)
            texteDescCaisseNormale.text = "Caisse Normale\n$10 — jusqu'à 100$";
        if (texteDescCaisseSupersonique != null)
            texteDescCaisseSupersonique.text = "Caisse Supersonique\n$20 — jusqu'à 1 000$";

        if (texteErreurCo != null) texteErreurCo.text = "";
        MettreAJourArgent();
    }

    void MettreAJourArgent()
    {
        if (texteArgent != null)
            texteArgent.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");

        if (texteMultiplicateur != null)
        {
            float m = GameManager.Instance.multiplicateurGain;
            texteMultiplicateur.text = m > 1f ? "⚡ x" + m : "";
        }
    }

    // -----------------------------------------------------------------------
    void AcheterCaisse(TypeCaisse type)
    {
        if (enAnimation) return;

        int prix = type == TypeCaisse.Normale ? PRIX_CAISSE_NORMALE : PRIX_CAISSE_SUPERSONIQUE;

        if (GameManager.Instance.argent < prix)
        {
            if (texteErreurCo != null)
            {
                texteErreurCo.text = "Argent insuffisant.";
            }
            else if (texteMessageResultat != null)
            {
                texteMessageResultat.text = "Pas assez d'argent !";
            }
            return;
        }

        if (texteErreurCo != null) texteErreurCo.text = "";
        if (texteMessageResultat != null) texteMessageResultat.text = "";


        caisseChoisie = type;
        GameManager.Instance.RetirerArgent(prix);
        MettreAJourArgent();

        if (boutonCaisseNormale != null)      boutonCaisseNormale.interactable = false;
        if (boutonCaisseSupersonique != null) boutonCaisseSupersonique.interactable = false;

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
            // Choisir une rarete aleatoire pour la couleur de defilement
            RareteArme rareteAnim = TirerRareteAnimation();
            string[] nomsRar = nomsArmes[rareteAnim];
            string nomAleatoire = nomsRar[Random.Range(0, nomsRar.Length)];
            if (texteDefilement != null)
            {
                texteDefilement.text = nomAleatoire;
                texteDefilement.color = CouleurRarete(rareteAnim);
            }

            // Ralentissement progressif
            float ratio = elapsed / duree;
            if (ratio > 0.65f)
                interval = Mathf.Lerp(0.07f, 0.4f, (ratio - 0.65f) / 0.35f);

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // Afficher l'arme finale avec sa couleur
        if (texteDefilement != null)
        {
            texteDefilement.text = derniereArme.nom;
            texteDefilement.color = CouleurRarete(derniereArme.rarete);
        }
        yield return new WaitForSeconds(0.6f);

        if (panneauAnimation != null) panneauAnimation.SetActive(false);
        AfficherResultat();

        enAnimation = false;
    }

    // -----------------------------------------------------------------------
    void AfficherResultat()
    {
        if (panneauResultat != null) panneauResultat.SetActive(true);

        if (texteNomArme != null)    texteNomArme.text = "Arme : " + derniereArme.nom;
        if (texteRareteArme != null)
        {
            texteRareteArme.text  = "Rareté : " + NomRarete(derniereArme.rarete);
            texteRareteArme.color = CouleurRarete(derniereArme.rarete);
        }

        // Gain automatique : la valeur de l'arme est directement ajoutee
        float multi = GameManager.Instance.multiplicateurGain;
        float gainFinal = AppliquerMultiplicateur(derniereArme.valeur);
        GameManager.Instance.AjouterArgent(gainFinal);
        MettreAJourArgent();

        if (texteValeurArme != null)
            texteValeurArme.text = "Gain : $" + gainFinal.ToString("N0")
                + (multi > 1f ? "  (x" + multi + " appliqué)" : "");

        if (texteMessageResultat != null)
        {
            texteMessageResultat.text  = "+ $" + gainFinal.ToString("N0") + " encaissé !  |  Folie +3";
            texteMessageResultat.color = UnityEngine.Color.green;
        }

        if (boutonRejouer != null) boutonRejouer.gameObject.SetActive(true);

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

    // Tirage rapide pour la couleur d'animation (sans impact sur le resultat reel)
    RareteArme TirerRareteAnimation()
    {
        float r = Random.value;
        float cumul = 0f;
        foreach (var kvp in probas)
        {
            cumul += kvp.Value;
            if (r < cumul) return kvp.Key;
        }
        return RareteArme.Normale;
    }

    string NomRarete(RareteArme r)
    {
        switch (r)
        {
            case RareteArme.Normale:    return "Normale";
            case RareteArme.Rare:       return "Rare";
            case RareteArme.Mythique:   return "Mythique";
            case RareteArme.Legendaire: return "Légendaire";
            case RareteArme.Antique:    return "Antique";
            case RareteArme.ExtraRare:  return "★ Extra Rare";
            default: return r.ToString();
        }
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
        if (panneauCase != null) panneauCase.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }
}