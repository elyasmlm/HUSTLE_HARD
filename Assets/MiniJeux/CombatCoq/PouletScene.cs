using UnityEngine;

/// <summary>
/// A placer sur chaque poulet 3D dans la scene.
/// indexCoq : 0 = MasterPoulet (CoqA), 1 = PouletBraise (CoqB).
/// modelNormal : le GameObject du modele de base (actif au depart).
/// modelBooste : le GameObject du modele PouletBoosté (inactif au depart).
///
/// L'etat "dope" est stocke dans le GameManager : pas besoin de reference
/// vers CombatCoq, ce qui evite tout cablage fragile dans l'Inspector.
/// </summary>
public class PouletScene : MonoBehaviour
{
    [Header("Identite")]
    [Tooltip("0 = MasterPoulet, 1 = PouletBraise")]
    public int indexCoq = 0;

    [Header("Modeles")]
    public GameObject modelNormal;
    public GameObject modelBooste;

    private bool visuelDope = false;

    // -----------------------------------------------------------------------
    void Start()
    {
        // Synchronise l'affichage avec l'etat global (utile si on revient dans la scene).
        visuelDope = GameManager.Instance != null && GameManager.Instance.EstPouletDope(indexCoq);
        AppliquerVisuel(visuelDope);
    }

    // Le dopage peut etre consomme par CombatCoq : on resynchronise le modele.
    void Update()
    {
        if (GameManager.Instance == null) return;

        bool dope = GameManager.Instance.EstPouletDope(indexCoq);
        if (dope != visuelDope)
        {
            visuelDope = dope;
            AppliquerVisuel(dope);
        }
    }

    // -----------------------------------------------------------------------
    /// <summary>
    /// Appele par InteractionSystem quand le joueur utilise le Viagra sur ce poulet.
    /// </summary>
    public bool TenterDoper()
    {
        if (GameManager.Instance.EstPouletDope(indexCoq))
        {
            Debug.Log("[PouletScene] Ce poulet est deja dope.");
            return false;
        }

        if (!GameManager.Instance.UtiliserViagra())
        {
            Debug.Log("[PouletScene] Pas de viagra en inventaire.");
            return false;
        }

        GameManager.Instance.DoperPoulet(indexCoq);
        visuelDope = true;
        AppliquerVisuel(true);

        Debug.Log("[PouletScene] " + (indexCoq == 0 ? CombatCoq.NOM_COQ_A : CombatCoq.NOM_COQ_B) + " est maintenant dope !");
        return true;
    }

    // -----------------------------------------------------------------------
    void AppliquerVisuel(bool dope)
    {
        if (modelNormal != null) modelNormal.SetActive(!dope);
        if (modelBooste != null) modelBooste.SetActive(dope);
    }

    public bool EstDope() => GameManager.Instance != null && GameManager.Instance.EstPouletDope(indexCoq);
}
