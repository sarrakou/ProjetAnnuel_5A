using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 700f;
    public float minLookAngle = -90f;
    public float maxLookAngle = 90f;
    public float cameraHeight = 1.8f;
    public float cameraOffset = 0.3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Animation")]
    public Animator animator;

    private float xRotation = 0f;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 lastPosition;
    private bool isMoving = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
      
        if (animator == null)
            animator = GetComponent<Animator>();
        
        Cursor.lockState = CursorLockMode.Locked;
        lastPosition = transform.position;
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        
        // Détection du mouvement
        bool hasInput = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
        
        // Calcul de la vitesse actuelle
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        
        // Application du mouvement
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Gestion des animations
        HandleAnimations(hasInput, currentSpeed);
    }

    void HandleAnimations(bool hasInput, float currentSpeed)
    {
        if (animator == null) return;

        // Méthode 1: Utiliser un paramètre "isWalking" (bool)
        if (animator.GetBool("isWalking") != hasInput)
        {
            animator.SetBool("isWalking", hasInput);
        }

       
    }

    void LateUpdate()
    {
      
        if (cameraTransform == null) 
        {
            Debug.LogError("Camera Transform n'est pas assigné!");
            return;
        }

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

       
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);
        
      
        transform.Rotate(Vector3.up * mouseX);
        
  
        cameraTransform.rotation = transform.rotation * Quaternion.Euler(xRotation, 0f, 0f);
        
    
        Vector3 cameraPosition = transform.position + Vector3.up * cameraHeight + transform.forward * cameraOffset;
        cameraTransform.position = cameraPosition;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}