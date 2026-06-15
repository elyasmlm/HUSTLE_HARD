using UnityEngine;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    [Header("Raycast")]
    public float porteeInteraction = 3f;
    public LayerMask masqueInteraction;

    [Header("UI")]
    public TextMeshProUGUI texteInteraction;

    private Camera cam;
    private GameObject objetCible;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        DetecterObjet();

        if (Input.GetKeyDown(KeyCode.E) && objetCible != null)
            Interagir(objetCible);
    }

    void DetecterObjet()
    {
        Ray rayon = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit touche;

        if (Physics.Raycast(rayon, out touche, porteeInteraction))
        {
            if (touche.collider.CompareTag("Interactable"))
            {
                objetCible = touche.collider.gameObject;
                texteInteraction.text = "[E] Interagir avec " + objetCible.name;
                return;
            }
        }

        objetCible = null;
        texteInteraction.text = "";
    }

    void Interagir(GameObject objet)
    {
        TicketGrattage ticket = Object.FindFirstObjectByType<TicketGrattage>();
        if (ticket != null)
            ticket.OuvrirPanneau();
    }
}