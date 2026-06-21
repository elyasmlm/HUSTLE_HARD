using UnityEngine;
using System.Collections;

/// <summary>
/// Joue l'appel d'intro de Marco Milkercrocs au lancement d'une partie.
/// Ne se declenche que lorsqu'une nouvelle partie est lancee (flag "IntroAVoir"),
/// pas a chaque retour de batiment.
/// </summary>
public class IntroAppel : MonoBehaviour
{
    private const string CLE_INTRO = "IntroAVoir";

    [Tooltip("Petit delai avant l'appel, le temps que la scene se mette en place.")]
    public float delaiAvantAppel = 0.6f;

    void Start()
    {
        // GetInt par defaut 1 => l'intro joue aussi si on lance SampleScene directement (test).
        if (PlayerPrefs.GetInt(CLE_INTRO, 1) != 1) return;

        PlayerPrefs.SetInt(CLE_INTRO, 0);
        PlayerPrefs.Save();
        StartCoroutine(JouerAppel());
    }

    IEnumerator JouerAppel()
    {
        yield return new WaitForSeconds(delaiAvantAppel);

        if (SystemeDialogue.Instance == null)
        {
            Debug.LogWarning("[IntroAppel] Aucun SystemeDialogue dans la scene. Lance Tools > Dialogue > Create UI in Scene.");
            yield break;
        }

        SystemeDialogue.Instance.AfficherSequence("Marco Milkercrocs",
            "Eh. C'est Marco. Marco Milkercrocs. Décroche et écoute-moi bien, c'est pas pour rigoler.",
            "T'es à sec, mon pote. Complètement à sec. Et t'as une énorme dette à rembourser à la banque tous tes scams du passé, faut payer l'addition maintenant. Et t'en as pour 35 000$",
            "Et t'en sortir comme un honnête citoyen, oublie. Les deux seuls boulots de la ville, Livreur de Pizzas et Nettoyeur de Voitures, ça rapporte que dalle. Jamais tu rembourseras avec ça.",
            "Non. Toi, tu vas devoir sortir les gros moyens : le gambling. C'est là qu'est le vrai fric.",
            "Inspire-toi du monument de la ville, The Dice. Y'a rien de mieux que le hasard pour se faire de l'oseille, crois-moi.",
            "Commence tranquille du côté de la station essence. Quand tu seras chaud, tu pourras tenter le casino... et même claquer ta thune dans ta chambre, sur ton propre ordi. Allez, au boulot.", 
            "N'oublie surtout pas de déposer l'argent dans ton compte dans le temps imparti ! Tu trouveras un distributeur à coté de la station essence.");
    }

    /// <summary>A appeler quand on demarre une nouvelle partie pour rejouer l'intro.</summary>
    public static void ReinitialiserIntro()
    {
        PlayerPrefs.SetInt(CLE_INTRO, 1);
        PlayerPrefs.Save();
    }
}
