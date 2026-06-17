using UnityEngine;
using UnityEngine.SceneManagement;

public class PorteTransition : MonoBehaviour
{
    public string nomScene;
    public string nomSpawnPoint = "SpawnPoint"; // nom du SpawnPoint à utiliser

    public void Entrer()
    {
        // Reset rotation caméra
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            pc.menuOuvert = false;
            var cam = Camera.main;
            if (cam != null)
                cam.transform.localRotation = Quaternion.identity;
            // Accès direct à rotationX
            pc.SendMessage("ResetRotation");
        }

        PlayerPrefs.SetString("SpawnPoint", nomSpawnPoint);

        GameObject player = GameObject.Find("Player");
        if (player != null) DontDestroyOnLoad(player);

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null) DontDestroyOnLoad(canvas);

        GameObject hudManager = GameObject.Find("HUDManager");
        if (hudManager != null) DontDestroyOnLoad(hudManager);

        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem != null) DontDestroyOnLoad(eventSystem);

        DontDestroyOnLoad(GameManager.Instance.gameObject);
        SceneManager.LoadScene(nomScene);
    }
}