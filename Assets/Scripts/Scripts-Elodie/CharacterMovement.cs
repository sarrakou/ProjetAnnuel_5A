using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2f; 
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 700f;
    public float minLookAngle = -90f;
    public float maxLookAngle = 90f;
    public float cameraHeight = 1.8f;
    public float crouchCameraHeight = 0.9f;
    public float cameraOffset = 0.3f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Animation")]
    public Animator animator;

    [Header("Crouch Settings")]
    public float crouchTransitionSpeed = 8f;  
    public float standingHeight = 2f;       
    public float crouchHeight = 1f;         

    private float xRotation = 0f;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector3 lastPosition;
    private bool isMoving = false;
    private bool isCrouching = false;        
    private float targetCameraHeight;         
    private float currentCameraHeight;       
    
   
    private bool allowCameraRepositioning = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
      
        if (animator == null)
            animator = GetComponent<Animator>();
        
        Cursor.lockState = CursorLockMode.Locked;
        lastPosition = transform.position;
        
   
        targetCameraHeight = cameraHeight;
        currentCameraHeight = cameraHeight;
        
        
        controller.height = standingHeight;
        controller.center = new Vector3(0, standingHeight / 2f, 0);
    }

    void Update()
    {
       
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

       
        HandleCrouch();

        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        
     
        bool hasInput = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
        
     
        float currentSpeed = GetCurrentSpeed();
        
       
        controller.Move(move * currentSpeed * Time.deltaTime);

      
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

       
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

       
        HandleAnimations(hasInput, currentSpeed);
        
       
        UpdateCameraHeight();
    }

    void HandleCrouch()
    {
       
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            
           
            if (isCrouching)
            {
                
                controller.height = crouchHeight;
                controller.center = new Vector3(0, crouchHeight / 2f, 0);
                
                targetCameraHeight = crouchCameraHeight;
            }
            else
            {
             
                if (CanStandUp())
                {
                   
                    controller.height = standingHeight;
                    controller.center = new Vector3(0, standingHeight / 2f, 0);
                    
                    targetCameraHeight = cameraHeight;
                }
                else
                {
                    isCrouching = true; 
                }
            }
        }
    }

    bool CanStandUp()
    {
        
        float checkDistance = standingHeight - crouchHeight;
        Vector3 checkPosition = transform.position + Vector3.up * (crouchHeight + checkDistance/2);
        
        return !Physics.CheckSphere(checkPosition, controller.radius, groundMask);
    }

    float GetCurrentSpeed()
    {
        if (isCrouching)
            return crouchSpeed;
        else if (Input.GetKey(KeyCode.LeftShift))
            return sprintSpeed;
        else
            return walkSpeed;
    }

    void UpdateCameraHeight()
    {
        
        currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, 
                                       crouchTransitionSpeed * Time.deltaTime);
    }

    void HandleAnimations(bool hasInput, float currentSpeed)
    {
        if (animator == null) return;

        if (animator.GetBool("isWalking") != hasInput)
        {
            animator.SetBool("isWalking", hasInput);
        }
        
        
        if (animator.GetBool("isCrouching") != isCrouching)
        {
            animator.SetBool("isCrouching", isCrouching);
        }
        
        
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && hasInput && !isCrouching;
        if (animator.GetBool("isSprinting") != isSprinting)
        {
            animator.SetBool("isSprinting", isSprinting);
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
        
        
        if (allowCameraRepositioning)
        {
            
            Vector3 cameraPosition = transform.position + Vector3.up * currentCameraHeight + transform.forward * (cameraOffset);

            cameraTransform.position = cameraPosition;
        }
    }

  
    public void SetCameraRepositioning(bool enabled)
    {
        allowCameraRepositioning = enabled;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        
   
        if (isCrouching)
        {
            Gizmos.color = Color.red;
            float checkDistance = standingHeight - crouchHeight;
            Vector3 checkPosition = transform.position + Vector3.up * (crouchHeight + checkDistance/2);
            Gizmos.DrawWireSphere(checkPosition, controller.radius);
        }
    }
}