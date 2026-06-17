using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SpawnManager : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;

        // Supprimer les doublons EventSystem
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        if (eventSystems.Length > 1)
            for (int i = 1; i < eventSystems.Length; i++)
                Destroy(eventSystems[i].gameObject);

        // Supprimer les doublons AudioListener
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        if (listeners.Length > 1)
            for (int i = 1; i < listeners.Length; i++)
                Destroy(listeners[i]);

        string spawnVoulu = PlayerPrefs.GetString("SpawnPoint", "SpawnPoint");
        if (gameObject.name != spawnVoulu) yield break;

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            player.transform.position = transform.position;
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.menuOuvert = false;
                pc.ResetRotation();
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}