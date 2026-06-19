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
        cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        DetecterObjet();

        if (Input.GetKeyDown(KeyCode.E) && objetCible != null)
            Interagir(objetCible);
    }

    void DetecterObjet()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Ray rayon = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit touche;

        if (Physics.Raycast(rayon, out touche, porteeInteraction))
        {
            if (touche.collider.CompareTag("Interactable"))
            {
                objetCible = touche.collider.gameObject; // assigner D'ABORD
                ObjetInteractif obj = objetCible.GetComponent<ObjetInteractif>();
                PorteTransition porte = objetCible.GetComponent<PorteTransition>();
                string nom;
                if (porte != null && porte.nomAffichage != "")
                    nom = porte.nomAffichage;
                else if (obj != null && obj.nomAffichage != "")
                    nom = obj.nomAffichage;
                else
                    nom = objetCible.name;
                texteInteraction.text = "[E] " + nom;
                return;
            }
        }

        objetCible = null;
        texteInteraction.text = "";
    }

    void Interagir(GameObject objet)
    {
        // Porte
        PorteTransition porte = objet.GetComponent<PorteTransition>();
        if (porte != null)
        {
            porte.Entrer();
            return;
        }

        // Ticket grattage
        TicketGrattage ticket = Object.FindFirstObjectByType<TicketGrattage>();
        if (ticket != null)
            ticket.OuvrirPanneau();

        // Objet interactif générique
        ObjetInteractif obj = objet.GetComponent<ObjetInteractif>();
        if (obj != null) { obj.Interagir(); return; }
    }
}