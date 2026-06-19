using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TicketGrattage : MonoBehaviour
{
    [Header("UI Panneau")]
    public GameObject panneauTicket;
    public TextMeshProUGUI texteGain;
    public TextMeshProUGUI texteSymboleRect;
    public TextMeshProUGUI texteResultat;
    public Button boutonAcheter;
    public Button boutonFermer;

    [Header("Ballons (4 boutons)")]
    public Button[] boutonsBallons;      // 4 éléments
    public TextMeshProUGUI[] textesBallons; // 4 textes enfants

    // Données internes
    private string[] symboles = { "E", "O", "D", "T", "C" };
    private float[] tauxSymboles = { 0.35f, 0.25f, 0.20f, 0.12f, 0.08f };

    private string symboleGagnant;
    private string[] symbolesBallons = new string[4];
    private bool[] ballonsGrattés = new bool[4];
    private int gainPotentiel;
    private bool rectGratté = false;
    private bool partieTerminee = false;

    private const int PRIX_TICKET = 5;

    void Start()
    {
        panneauTicket.SetActive(false);
        boutonAcheter.onClick.AddListener(AcheterTicket);
        boutonFermer.onClick.AddListener(FermerTicket);

        for (int i = 0; i < boutonsBallons.Length; i++)
        {
            int index = i;
            boutonsBallons[i].onClick.AddListener(() => GratterBallon(index));
        }

        texteSymboleRect.GetComponentInParent<Button>()?.onClick.AddListener(GratterRectangle);
    }

    public void OuvrirPanneau()
    {
        if (!GameManager.Instance.PeutJouer()) return;
        panneauTicket.SetActive(true);
        ResetTicket();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null) pc.menuOuvert = true;
    }

    void ResetTicket()
    {
        partieTerminee = false;
        rectGratté = false;
        texteResultat.text = "";
        texteGain.text = "$??";
        texteSymboleRect.text = "?";

        for (int i = 0; i < 4; i++)
        {
            ballonsGrattés[i] = false;
            textesBallons[i].text = "?";
            boutonsBallons[i].interactable = true;
        }

        boutonAcheter.gameObject.SetActive(true);
        boutonAcheter.interactable = true;
    }

    void AcheterTicket()
    {
        if (GameManager.Instance.argent < PRIX_TICKET)
        {
            // TODO : afficher message "pas assez d'argent"
            return;
        }

        GameManager.Instance.RetirerArgent(PRIX_TICKET);
        boutonAcheter.gameObject.SetActive(false);
        GenererTicket();
    }

    void GenererTicket()
    {
        // 1. Tirer victoire ou défaite
        bool victoire = Random.value < 0.25f;

        // 2. Tirer le gain potentiel
        gainPotentiel = TirerGain();

        // 3. Tirer le symbole du rectangle
        symboleGagnant = TirerSymbole();

        // 4. Générer les 4 ballons selon victoire/défaite
        if (victoire)
            GenererBallonsVictoire();
        else
            GenererBallonsDefaite();
    }

    int TirerGain()
    {
        float r = Random.value;
        if (r < 0.40f) return Random.Range(5, 11);    // 5-10$  : 40%
        if (r < 0.70f) return Random.Range(11, 26);   // 11-25$ : 30%
        if (r < 0.88f) return Random.Range(26, 51);   // 26-50$ : 18%
        if (r < 0.96f) return Random.Range(51, 101);  // 51-100$: 8%
        if (r < 0.99f) return Random.Range(101, 251); // 101-250$: 3%
        return Random.Range(251, 501);                 // 251-500$: 1%
    }

    string TirerSymbole()
    {
        float r = Random.value;
        float cumul = 0f;
        for (int i = 0; i < symboles.Length; i++)
        {
            cumul += tauxSymboles[i];
            if (r < cumul) return symboles[i];
        }
        return symboles[0];
    }

    string TirerSymboleDifferentDe(string exclu)
    {
        string s;
        int tentatives = 0;
        do
        {
            s = TirerSymbole();
            tentatives++;
        } while (s == exclu && tentatives < 20);
        return s;
    }

    void GenererBallonsVictoire()
    {
        // 2 ballons avec le symbole gagnant, 2 différents
        List<int> positions = new List<int> { 0, 1, 2, 3 };

        // Choisir 2 positions au hasard pour le symbole gagnant
        int pos1 = positions[Random.Range(0, positions.Count)];
        positions.Remove(pos1);
        int pos2 = positions[Random.Range(0, positions.Count)];
        positions.Remove(pos2);

        symbolesBallons[pos1] = symboleGagnant;
        symbolesBallons[pos2] = symboleGagnant;

        foreach (int pos in positions)
            symbolesBallons[pos] = TirerSymboleDifferentDe(symboleGagnant);
    }

    void GenererBallonsDefaite()
    {
        // Aucun ballon ne doit avoir le symbole gagnant
        // Et pas de paire identique non plus (pour éviter confusion)
        List<string> utilisés = new List<string>();

        for (int i = 0; i < 4; i++)
        {
            string s;
            int tentatives = 0;
            do
            {
                s = TirerSymboleDifferentDe(symboleGagnant);
                tentatives++;
            } while (utilisés.Contains(s) && tentatives < 20);

            symbolesBallons[i] = s;
            if (!utilisés.Contains(s)) utilisés.Add(s);
        }
    }

    void GratterBallon(int index)
    {
        if (ballonsGrattés[index] || partieTerminee) return;

        ballonsGrattés[index] = true;
        textesBallons[index].text = symbolesBallons[index];
        boutonsBallons[index].interactable = false;

        VerifierFinPartie();
    }

    public void GratterRectangle()
    {
        if (rectGratté || partieTerminee) return;
        rectGratté = true;
        texteGain.text = "$" + gainPotentiel;
        texteSymboleRect.text = symboleGagnant;

        VerifierFinPartie();
    }

    void VerifierFinPartie()
    {
        // On affiche le résultat seulement quand tout est gratté
        bool toutGratté = rectGratté;
        for (int i = 0; i < 4; i++)
            if (!ballonsGrattés[i]) toutGratté = false;

        if (!toutGratté) return;

        partieTerminee = true;

        // Compter les ballons qui matchent
        int matches = 0;
        foreach (string s in symbolesBallons)
            if (s == symboleGagnant) matches++;

        if (matches >= 2)
        {
            texteResultat.text = "GAGNÉ ! +" + gainPotentiel + "$";
            texteResultat.color = Color.green;
            GameManager.Instance.AjouterArgent(gainPotentiel);
            GameManager.Instance.AjouterFolie(5f);
        }
        else
        {
            texteResultat.text = "Perdu...";
            texteResultat.color = Color.red;
            GameManager.Instance.AjouterFolie(2f);
        }
    }

    void FermerTicket()
    {
        panneauTicket.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Bloquer la caméra
        PlayerController pc = Object.FindFirstObjectByType<PlayerController>();
        if (pc != null) pc.menuOuvert = false;
    }
}