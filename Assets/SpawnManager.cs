using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null;

        string spawnVoulu = PlayerPrefs.GetString("SpawnPoint", "SpawnPointDepart");
        if (gameObject.name != spawnVoulu) yield break;

        GameObject player = GameObject.Find("Player");
        if (player == null) yield break;

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