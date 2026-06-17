using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Mouvement")]
    public float vitesse = 9f;
    public float vitesseSprint = 15f;
    public float gravite = -9.81f;

    [Header("Caméra")]
    public Transform cameraPrincipale; // À GLISSER dans l'Inspector !
    public float sensibiliteSouris = 2f;
    public float limiteVerticale = 80f;

    private CharacterController controller;
    private Vector3 velociteVerticale;
    private float rotationX = 0f;

    [HideInInspector]
    public bool menuOuvert = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        // Fallback si pas assigné dans l'inspector
        if (cameraPrincipale == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraPrincipale = cam.transform;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None && !menuOuvert)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (!menuOuvert)
        {
            GererMouvement();
            GererCamera();
        }
    }

    void GererMouvement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        float vitesseActuelle = Input.GetKey(KeyCode.LeftShift) ? vitesseSprint : vitesse;

        Vector3 direction = (transform.right * x + transform.forward * z).normalized;
        controller.Move(direction * vitesseActuelle * Time.deltaTime);

        velociteVerticale.y += gravite * Time.deltaTime;
        controller.Move(velociteVerticale * Time.deltaTime);
    }

    void GererCamera()
    {
        if (cameraPrincipale == null) return;

        float sourisX = Input.GetAxis("Mouse X") * sensibiliteSouris;
        float sourisY = Input.GetAxis("Mouse Y") * sensibiliteSouris;

        rotationX -= sourisY;
        rotationX = Mathf.Clamp(rotationX, -limiteVerticale, limiteVerticale);
        cameraPrincipale.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        transform.Rotate(Vector3.up * sourisX);
    }

    public void ResetRotation()
    {
        rotationX = 0f;
        if (cameraPrincipale != null)
            cameraPrincipale.localRotation = Quaternion.identity;
    }
}