using UnityEngine;
using UnityEngine.SceneManagement;

public class PorteTransition : MonoBehaviour
{
    public string nomScene;
    public string nomSpawnPoint = "SpawnPoint";
    public string nomAffichage = "Entrer";

    public void Entrer()
    {
        PlayerPrefs.SetString("SpawnPoint", nomSpawnPoint);
        SceneManager.LoadScene(nomScene);
    }
}