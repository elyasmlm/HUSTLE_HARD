using UnityEngine;

/// <summary>
/// A placer sur l'objet "AimantCaché" en scene.
/// Quand le joueur le ramasse, il recoit des aimants (triche roulette)
/// puis l'objet disparait.
/// </summary>
public class RamassageAimant : MonoBehaviour
{
    [Tooltip("Nombre d'aimants donnes au ramassage.")]
    public int quantite = 5;

    public void Ramasser()
    {
        GameManager.Instance.AjouterAimant(quantite);
        Debug.Log("[RamassageAimant] +" + quantite + " aimants. Total : " + GameManager.Instance.aimants);
        gameObject.SetActive(false);
    }
}
