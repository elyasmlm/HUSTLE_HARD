using UnityEngine;

/// <summary>
/// PNJ crackhead. En interagissant :
///   - si la folie est au maximum (100%) : il detend le joueur (folie a 0) + dialogue ;
///   - sinon : il ne fait rien et laisse le mini-jeu eventuel du cube se lancer.
/// A placer sur le cube/PNJ. Peut cohabiter avec un ObjetInteractif (nomMinijeu).
/// </summary>
public class Crackhead : MonoBehaviour
{
    /// <summary>
    /// Tente de detendre le joueur. Retourne true si la detente a eu lieu
    /// (folie au maximum), false s'il faut laisser le jeu se lancer.
    /// </summary>
    public bool TenterDetendre()
    {
        if (GameManager.Instance.folie < GameManager.Instance.folieMax)
            return false;

        GameManager.Instance.ResetFolie();

        if (SystemeDialogue.Instance != null)
            SystemeDialogue.Instance.Afficher("Le crackhead",
                "Eh ferme... t'as l'air à cran. Tiens, prends ce zdeh et détends-toi un coup. Ça remet les idées en place, crois-moi.");
        else
            Debug.Log("[Crackhead] Folie remise à zéro.");

        return true;
    }
}
