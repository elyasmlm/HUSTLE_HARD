using UnityEngine;

/// <summary>
/// Achat instantane de Viagra au prix de 500$.
/// Pas d'interface : l'achat se fait au moment de l'interaction.
/// </summary>
public class AchatViagra : MonoBehaviour
{
    private const int PRIX = 200;

    public void OuvrirPanneau()
    {
        if (GameManager.Instance.viagra >= GameManager.VIAGRA_MAX)
        {
            Debug.Log("[AchatViagra] Vous avez deja un viagra sur vous.");
            return;
        }

        if (GameManager.Instance.argent < PRIX)
        {
            Debug.Log("[AchatViagra] Pas assez d'argent. (500$ requis)");
            return;
        }

        GameManager.Instance.RetirerArgent(PRIX);
        GameManager.Instance.AjouterViagra(1);
        GameManager.Instance.AjouterFolie(2f);

        Debug.Log("[AchatViagra] Viagra achete ! Stock : " + GameManager.Instance.viagra);
    }
}
