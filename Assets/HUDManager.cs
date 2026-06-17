using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Argent")]
    public TextMeshProUGUI texteArgent;

    [Header("Timer")]
    public TextMeshProUGUI texteTimer;

    [Header("Folie")]
    public RectTransform barreFolieFill;
    public TextMeshProUGUI texteFolie;

    private float largeurMaxBarre = 200f;

    private float argent = 1250f;
    private float tempsRestant = 1440f;
    private float folie = 35f;

    void Update()
    {
        tempsRestant -= Time.deltaTime;
        if (tempsRestant < 0) tempsRestant = 0;

        MettreAJourHUD();
    }

    void MettreAJourHUD()
    {
        if (GameManager.Instance == null) return;

        texteArgent.text = "$" + GameManager.Instance.argent.ToString("N0");

        float t = GameManager.Instance.tempsRestant;
        int heures = Mathf.FloorToInt(t / 60f);
        int minutes = Mathf.FloorToInt(t % 60f);
        texteTimer.text = string.Format("{0:00}:{1:00}", heures, minutes);
        texteTimer.color = t < 120f ? Color.red : Color.white;

        float folie = GameManager.Instance.folie;
        float largeur = (folie / 100f) * largeurMaxBarre;
        barreFolieFill.sizeDelta = new Vector2(largeur, barreFolieFill.sizeDelta.y);
        texteFolie.text = "Folie : " + Mathf.RoundToInt(folie) + "%";

        Image imgFolie = barreFolieFill.GetComponent<Image>();
        if (folie < 40f) imgFolie.color = Color.green;
        else if (folie < 70f) imgFolie.color = Color.yellow;
        else imgFolie.color = Color.red;
    }
}