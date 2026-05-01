using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Mouvement")]
    public float vitesse = 5f;
    public float vitesseSprint = 8f;
    public float hauteurSaut = 1.5f;
    public float gravite = -9.81f;

    [Header("Caméra")]
    public float sensibiliteSouris = 2f;
    public float limiteVerticale = 80f;

    private CharacterController controller;
    private Transform cameraPrincipale;
    private Vector3 velociteVerticale;
    private float rotationX = 0f;

    [HideInInspector]
    public bool menuOuvert = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraPrincipale = Camera.main.transform;

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

        Vector3 direction = transform.right * x + transform.forward * z;
        direction = direction.normalized;

        controller.Move(direction * vitesseActuelle * Time.deltaTime);

        velociteVerticale.y += gravite * Time.deltaTime;
        controller.Move(velociteVerticale * Time.deltaTime);
    }

    void GererCamera()
    {
        float sourisX = Input.GetAxis("Mouse X") * sensibiliteSouris;
        float sourisY = Input.GetAxis("Mouse Y") * sensibiliteSouris;


        rotationX -= sourisY;
        rotationX = Mathf.Clamp(rotationX, -limiteVerticale, limiteVerticale);
        cameraPrincipale.localRotation = Quaternion.Euler(rotationX, 0f, 0f);


        transform.Rotate(Vector3.up * sourisX);
    }
}