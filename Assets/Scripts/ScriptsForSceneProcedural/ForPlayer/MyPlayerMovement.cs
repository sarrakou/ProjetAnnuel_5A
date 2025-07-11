using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MyPlayerMovement : MonoBehaviour
{
    [Header("Déplacement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 200f;
    public Transform cameraTransform;

    private Rigidbody rb;
    private float xRotation = 0f;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.freezeRotation = true;
        rb.useGravity = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Rotation souris
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);

        // Entrée joueur
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveInput = (transform.right * x + transform.forward * z).normalized * moveSpeed;
    }

    void FixedUpdate()
    {
        // Mouvement physique fluide
        Vector3 targetPosition = rb.position + moveInput * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }
}
