using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Metier : Lavage de Voiture
/// Mini-jeu : Maintenir le clic et frotter la souris sur la voiture.
/// Un pourcentage de proprete monte au fur et a mesure.
/// Atteindre 100% avant la fin du timer pour gagner.
///
/// Recompense : entre 15$ et 40$ selon la vitesse.
/// Timer : 90 secondes.
/// </summary>
public class LavageVoiture : MonoBehaviour
{
    [Header("UI Panneau")]
    public GameObject panneauLavage;

    [Header("Voiture")]
    public RectTransform voitureRect;
    public Image voitureSale;
    public Image voiturePropre;

    [Header("Eponge")]
    public RectTransform epongeIcon;

    [Header("Progression")]
    public Slider barrePropirete;
    public TextMeshProUGUI textePourcentage;
    public TextMeshProUGUI texteTimer;
    public TextMeshProUGUI texteInstruction;

    [Header("Resultat")]
    public TextMeshProUGUI texteResultat;
    public TextMeshProUGUI texteGain;
    public Button boutonCommencer;
    public Button boutonRejouer;
    public Button boutonFermer;

    [Header("Info joueur")]
    public TextMeshProUGUI texteArgent;

    // --- Constantes ---
    private const float DUREE_MAX = 90f;
    private const float GAIN_MIN = 15f;
    private const float GAIN_MAX = 40f;

    // Vitesse de nettoyage par pixel parcouru avec la souris
    private const float NETTOYAGE_PAR_PIXEL = 0.015f;

    // Vitesse minimale de deplacement souris pour nettoyer (evite le spam clic fixe)
    private const float VITESSE_MIN_SOURIS = 5f;

    // --- Etat ---
    private float proprete = 0f;
    private float tempsRestant;
    private bool partieEnCours = false;
    private bool estEnTrainDeFrotter = false;
    private Vector2 dernierPosSouris;

    private PlayerController playerController;

    // -----------------------------------------------------------------------
    void Start()
    {
        playerController = Object.FindFirstObjectByType<PlayerController>();

        if (panneauLavage == null)
            Debug.LogWarning("[LavageVoiture] panneauLavage est null !");
        if (voitureRect == null)
            Debug.LogWarning("[LavageVoiture] voitureRect est null !");
        if (voitureSale == null)
            Debug.LogWarning("[LavageVoiture] voitureSale est null !");
        if (voiturePropre == null)
            Debug.LogWarning("[LavageVoiture] voiturePropre est null !");
        if (epongeIcon == null)
            Debug.LogWarning("[LavageVoiture] epongeIcon est null !");
        if (barrePropirete == null)
            Debug.LogWarning("[LavageVoiture] barrePropirete est null !");
        if (texteTimer == null)
            Debug.LogWarning("[LavageVoiture] texteTimer est null !");
        if (textePourcentage == null)
            Debug.LogWarning("[LavageVoiture] textePourcentage est null !");

        if (boutonCommencer != null) boutonCommencer.onClick.AddListener(CommencerPartie);
        else Debug.LogWarning("[LavageVoiture] boutonCommencer est null !");
        if (boutonRejouer != null) boutonRejouer.onClick.AddListener(ResetPartie);
        else Debug.LogWarning("[LavageVoiture] boutonRejouer est null !");
        if (boutonFermer != null) boutonFermer.onClick.AddListener(FermerPanneau);
        else Debug.LogWarning("[LavageVoiture] boutonFermer est null !");

        if (panneauLavage != null) panneauLavage.SetActive(false);
    }

    // -----------------------------------------------------------------------
    private bool PeutJouer()
    {
        if (panneauLavage == null)   { Debug.LogWarning("[LavageVoiture] panneauLavage manquant.");   return false; }
        if (voitureRect == null)     { Debug.LogWarning("[LavageVoiture] voitureRect manquant.");     return false; }
        if (voitureSale == null)     { Debug.LogWarning("[LavageVoiture] voitureSale manquant.");     return false; }
        if (voiturePropre == null)   { Debug.LogWarning("[LavageVoiture] voiturePropre manquant.");   return false; }
        if (epongeIcon == null)      { Debug.LogWarning("[LavageVoiture] epongeIcon manquant.");      return false; }
        if (barrePropirete == null)  { Debug.LogWarning("[LavageVoiture] barrePropirete manquant."); return false; }
        if (texteTimer == null)      { Debug.LogWarning("[LavageVoiture] texteTimer manquant.");      return false; }
        if (textePourcentage == null){ Debug.LogWarning("[LavageVoiture] textePourcentage manquant."); return false; }
        return true;
    }

    // -----------------------------------------------------------------------
    public void OuvrirPanneau()
    {
        if (!PeutJouer()) return;
        panneauLavage.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null) playerController.menuOuvert = true;

        MettreAJourArgent();
        ResetPartie();
    }

    // -----------------------------------------------------------------------
    void ResetPartie()
    {
        partieEnCours = false;
        estEnTrainDeFrotter = false;
        proprete = 0f;
        tempsRestant = DUREE_MAX;

        if (voitureSale != null)
        {
            Color c = voitureSale.color;
            c.a = 1f;
            voitureSale.color = c;
        }
        if (voiturePropre != null)
        {
            Color c = voiturePropre.color;
            c.a = 0f;
            voiturePropre.color = c;
        }
        if (barrePropirete != null)
        {
            barrePropirete.minValue = 0;
            barrePropirete.maxValue = 100;
            barrePropirete.value = 0;
        }
        if (epongeIcon != null)
            epongeIcon.gameObject.SetActive(false);

        if (textePourcentage != null) textePourcentage.text  = "0%";
        if (texteTimer != null)       { texteTimer.text = "1:30"; texteTimer.color = Color.white; }
        if (texteResultat != null)    texteResultat.text    = "";
        if (texteGain != null)        texteGain.text        = "";
        if (texteInstruction != null) texteInstruction.text = "Maintenez le clic et frottez la voiture !";

        if (boutonCommencer != null) boutonCommencer.gameObject.SetActive(true);
        if (boutonRejouer   != null) boutonRejouer.gameObject.SetActive(false);

        MettreAJourArgent();
    }

    void MettreAJourArgent()
    {
        if (texteArgent != null && GameManager.Instance != null)
            texteArgent.text = "Argent : $" + GameManager.Instance.argent.ToString("N0");
    }

    // -----------------------------------------------------------------------
    void CommencerPartie()
    {
        boutonCommencer.gameObject.SetActive(false);
        partieEnCours = true;
        dernierPosSouris = Input.mousePosition;
    }

    // -----------------------------------------------------------------------
    void Update()
    {
        if (panneauLavage != null && panneauLavage.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            FermerPanneau();
            return;
        }

        if (!partieEnCours) return;

        tempsRestant -= Time.deltaTime;
        AfficherTimer();

        if (tempsRestant <= 0)
        {
            tempsRestant = 0;
            StartCoroutine(FinPartie(false));
            return;
        }

        Vector2 posSouris = Input.mousePosition;

        if (Input.GetMouseButton(0) && SourisSurVoiture(posSouris))
        {
            estEnTrainDeFrotter = true;

            if (epongeIcon != null)
            {
                epongeIcon.gameObject.SetActive(true);
                epongeIcon.position = posSouris;
            }

            float distanceSouris = Vector2.Distance(posSouris, dernierPosSouris);

            if (distanceSouris > VITESSE_MIN_SOURIS)
            {
                float nettoyage = distanceSouris * NETTOYAGE_PAR_PIXEL;
                proprete = Mathf.Clamp(proprete + nettoyage, 0f, 100f);

                MettreAJourVisuel();

                if (proprete >= 100f)
                {
                    StartCoroutine(FinPartie(true));
                    return;
                }
            }
        }
        else
        {
            estEnTrainDeFrotter = false;
            if (epongeIcon != null)
                epongeIcon.gameObject.SetActive(false);
        }

        dernierPosSouris = posSouris;
    }

    // -----------------------------------------------------------------------
    bool SourisSurVoiture(Vector2 posSouris)
    {
        if (voitureRect == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(voitureRect, posSouris);
    }

    // -----------------------------------------------------------------------
    void MettreAJourVisuel()
    {
        if (barrePropirete != null)
            barrePropirete.value = proprete;
        if (textePourcentage != null)
            textePourcentage.text = Mathf.RoundToInt(proprete) + "%";

        float ratio = proprete / 100f;
        if (voitureSale != null)
        {
            Color c = voitureSale.color;
            c.a = 1f - ratio;
            voitureSale.color = c;
        }
        if (voiturePropre != null)
        {
            Color c = voiturePropre.color;
            c.a = ratio;
            voiturePropre.color = c;
        }

        if (texteInstruction != null)
        {
            if (proprete < 30f)
                texteInstruction.text = "Continuez a frotter !";
            else if (proprete < 70f)
                texteInstruction.text = "Bien ! Continuez...";
            else if (proprete < 100f)
                texteInstruction.text = "Presque propre !";
        }
    }

    // -----------------------------------------------------------------------
    void AfficherTimer()
    {
        if (texteTimer == null) return;
        int minutes = Mathf.FloorToInt(tempsRestant / 60f);
        int secondes = Mathf.CeilToInt(tempsRestant % 60f);
        texteTimer.text = string.Format("{0}:{1:00}", minutes, secondes);
        texteTimer.color = tempsRestant < 20f ? Color.red : Color.white;
    }

    // -----------------------------------------------------------------------
    IEnumerator FinPartie(bool victoire)
    {
        partieEnCours = false;
        estEnTrainDeFrotter = false;

        if (epongeIcon != null)
            epongeIcon.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        if (boutonRejouer != null) boutonRejouer.gameObject.SetActive(true);

        if (victoire)
        {
            float ratio = tempsRestant / DUREE_MAX;
            int gain = Mathf.RoundToInt(Mathf.Lerp(GAIN_MIN, GAIN_MAX, ratio));

            if (GameManager.Instance != null) GameManager.Instance.AjouterArgent(gain);
            MettreAJourArgent();

            if (texteResultat != null) { texteResultat.text = "Voiture impeccable !"; texteResultat.color = Color.green; }
            if (texteGain     != null) { texteGain.text = "+ $" + gain + " (temps restant : " + Mathf.CeilToInt(tempsRestant) + "s)"; texteGain.color = Color.green; }
        }
        else
        {
            if (proprete >= 50f)
            {
                int gainPartiel = Mathf.RoundToInt(GAIN_MIN / 2f);
                if (GameManager.Instance != null) GameManager.Instance.AjouterArgent(gainPartiel);
                MettreAJourArgent();

                if (texteResultat != null) { texteResultat.text = "Temps ecoule ! Voiture a moitie propre."; texteResultat.color = Color.yellow; }
                if (texteGain     != null) { texteGain.text = "+ $" + gainPartiel + " (travail incomplet)"; texteGain.color = Color.yellow; }
            }
            else
            {
                if (texteResultat != null) { texteResultat.text = "Temps ecoule ! Voiture encore sale."; texteResultat.color = Color.red; }
                if (texteGain     != null) { texteGain.text = "Aucune recompense."; texteGain.color = Color.gray; }
            }
        }
    }

    // -----------------------------------------------------------------------
    public void FermerPanneau()
    {
        partieEnCours = false;
        estEnTrainDeFrotter = false;
        if (epongeIcon != null) epongeIcon.gameObject.SetActive(false);
        if (panneauLavage != null) panneauLavage.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null) playerController.menuOuvert = false;
    }
}