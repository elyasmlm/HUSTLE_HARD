using UnityEngine;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    [Header("Raycast")]
    public float porteeInteraction = 3f;
    public LayerMask masqueInteraction;

    [Header("UI")]
    public TextMeshProUGUI texteInteraction;

    [Header("Mini-jeux")]
    public CombatCoq combatCoq;
    public TicketGrattage ticketGrattage;

    private Camera cam;
    private GameObject objetCible;
    private PlayerController playerController;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
            playerController = Object.FindFirstObjectByType<PlayerController>();

        if (combatCoq == null)
            combatCoq = Object.FindFirstObjectByType<CombatCoq>();
        if (ticketGrattage == null)
            ticketGrattage = Object.FindFirstObjectByType<TicketGrattage>();
    }

    void Update()
    {
        if (playerController != null && playerController.menuOuvert) return;

        DetecterObjet();

        if (Input.GetKeyDown(KeyCode.E) && objetCible != null)
            Interagir(objetCible);
    }

    void DetecterObjet()
    {
        if (cam == null) cam = GetComponentInChildren<Camera>();
        if (cam == null) { Debug.Log("CAM NULL"); return; }

        Ray rayon = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit touche;
        if (Physics.Raycast(rayon, out touche, porteeInteraction))
        {
            if (touche.collider.CompareTag("Interactable"))
            {
                objetCible = touche.collider.gameObject;
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

        // Dispatch par nomMinijeu
        ObjetInteractif obj = objet.GetComponent<ObjetInteractif>();
        if (obj != null)
        {
            string nom = obj.nomMinijeu.Trim();

            if (nom.Equals("CombatCoq", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Combat de coqs", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Combat de coq", System.StringComparison.OrdinalIgnoreCase))
            {
                if (combatCoq != null) combatCoq.OuvrirPanneau();
                else Debug.LogWarning("[InteractionSystem] CombatCoq non assigné dans l'Inspector !");
                return;
            }

            if (nom.Equals("TicketGrattage", System.StringComparison.OrdinalIgnoreCase))
            {
                if (ticketGrattage != null) ticketGrattage.OuvrirPanneau();
                else Debug.LogWarning("[InteractionSystem] TicketGrattage non assigné dans l'Inspector !");
                return;
            }

            obj.Interagir();
        }
    }
}
