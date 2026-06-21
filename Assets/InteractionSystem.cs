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
    public Blackjack blackjack;
    public MiniRoulette miniRoulette;
    public CaseOpening caseOpening;
    public LivreurPizza livreurPizza;
    public LavageVoiture lavageVoiture;

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
        if (blackjack == null)
            blackjack = Object.FindFirstObjectByType<Blackjack>();
        if (miniRoulette == null)
            miniRoulette = Object.FindFirstObjectByType<MiniRoulette>();
        if (caseOpening == null)
            caseOpening = Object.FindFirstObjectByType<CaseOpening>();
        if (livreurPizza == null)
            livreurPizza = Object.FindFirstObjectByType<LivreurPizza>(FindObjectsInactive.Include);
        if (lavageVoiture == null)
            lavageVoiture = Object.FindFirstObjectByType<LavageVoiture>(FindObjectsInactive.Include);
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
        if (cam == null) { Debug.LogError("[Interaction] CAM NULL sur " + gameObject.name); return; }

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
                if (texteInteraction != null) texteInteraction.text = "[E] " + nom;
                return;
            }
            else
            {
                // Raycast touche qqch mais pas Interactable
                Debug.Log("[Interaction] Raycast touche '" + touche.collider.gameObject.name
                    + "' tag=" + touche.collider.tag + " (pas Interactable)");
            }
        }

        objetCible = null;
        if (texteInteraction != null) texteInteraction.text = "";
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
                else Debug.LogWarning("[InteractionSystem] CombatCoq non assign� dans l'Inspector !");
                return;
            }

            if (nom.Equals("TicketGrattage", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Faire un grattage", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Grattage", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Ticket Grattage", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Ticket de grattage", System.StringComparison.OrdinalIgnoreCase))
            {
                if (ticketGrattage != null) ticketGrattage.OuvrirPanneau();
                else Debug.LogWarning("[InteractionSystem] TicketGrattage non assign� dans l'Inspector !");
                return;
            }

            if (nom.Equals("Blackjack", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Black Jack", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("BlackJack", System.StringComparison.OrdinalIgnoreCase))
            {
                if (blackjack != null) blackjack.OuvrirPanneau();
                else Debug.LogWarning("[InteractionSystem] Blackjack non assign� dans l'Inspector !");
                return;
            }

            if (nom.Equals("MiniRoulette", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Roulette", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Mini-Roulette", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Roue de la fortune", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Roulette Mini", System.StringComparison.OrdinalIgnoreCase))
            {
                if (miniRoulette != null) miniRoulette.OuvrirPanneau();
                else Debug.LogWarning("[InteractionSystem] MiniRoulette non assign� dans l'Inspector !");
                return;
            }

            if (nom.Equals("CaseOpening", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Case Opening", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Caisse", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("CaisseOpening", System.StringComparison.OrdinalIgnoreCase))
            {
                if (caseOpening != null) caseOpening.OuvrirPanneau();
                else Debug.LogWarning("[InteractionSystem] CaseOpening non assign� dans l'Inspector !");
                return;
            }

            if (nom.Equals("LivreurPizza", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Livreur Pizza", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Livreur de Pizza", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Livraison", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Scooter", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Pizza", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("[Interaction] Dispatch LivreurPizza – ref=" + (livreurPizza != null ? livreurPizza.name : "NULL"));
                if (livreurPizza != null) livreurPizza.OuvrirPanneau();
                else Debug.LogWarning("[InteractionSystem] LivreurPizza non assigné ! Glissez le controller dans l'Inspector.");
                return;
            }

            if (nom.Equals("LavageVoiture", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Lavage Voiture", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Lavage de Voiture", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Lavage", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("CarWash", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Voiture", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Nettoyeur de voiture", System.StringComparison.OrdinalIgnoreCase) ||
                nom.Equals("Nettoyeur", System.StringComparison.OrdinalIgnoreCase))
            {
                if (lavageVoiture != null) lavageVoiture.OuvrirPanneau();
                else Debug.LogWarning("[InteractionSystem] LavageVoiture non assigné ! Glissez le controller dans l'Inspector.");
                return;
            }

            obj.Interagir();
        }
    }
}
