using UnityEngine;

public class Persistent : MonoBehaviour
{
    public static Persistent Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // détruit le doublon au retour dans SampleScene
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}