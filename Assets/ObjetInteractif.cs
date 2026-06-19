using UnityEngine;

public class ObjetInteractif : MonoBehaviour
{
    public string nomMinijeu = "Mini-jeu";
    public string nomAffichage = "";

    public void Interagir()
    {
        Debug.Log("Lancement : " + nomMinijeu);
        // TODO : ouvrir le mini-jeu correspondant
    }
}