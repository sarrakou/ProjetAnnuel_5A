using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactionDistance = 3f;
    public float sphereRadius = 0.5f; 
    public LayerMask interactableLayer;
    public Transform cameraTransform; 
    
    [Header("Debug")]
    public bool showDebugSphere = true;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
           
            Vector3 rayOrigin = cameraTransform != null ? cameraTransform.position : transform.position;
            Vector3 rayDirection = cameraTransform != null ? cameraTransform.forward : transform.forward;
            
            RaycastHit hit;

            
            if (showDebugSphere)
            {
                Debug.DrawRay(rayOrigin, rayDirection * interactionDistance, Color.blue, 2f);
                Debug.Log($"🔍 SphereCast from: {rayOrigin}, Direction: {rayDirection}, Radius: {sphereRadius}");
            }

          
            if (Physics.SphereCast(rayOrigin, sphereRadius, rayDirection, out hit, interactionDistance, interactableLayer))
            {
                Debug.Log($"✅ SphereCast hit object: {hit.collider.name} on layer: {hit.collider.gameObject.layer}");
                Debug.Log($"📏 Distance: {hit.distance:F2}m");
                
                InteractableObject interactableObject = hit.collider.GetComponent<InteractableObject>();
                if (interactableObject != null)
                {
                    Debug.Log($"🎯 Interacting with InteractableObject: {hit.collider.name}");
                    interactableObject.Toggle();
                    return;
                }
                
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    Debug.Log($"🎯 Interacting with IInteractable: {hit.collider.name}");
                    interactable.Interact();
                    return;
                }
                
                Debug.Log($"⚠️ Object hit but no interactable component found: {hit.collider.name}");
            }
            else
            {
                Debug.Log("🚫 Aucun objet interactif détecté dans le spherecast.");
                
               
                Collider[] nearbyObjects = Physics.OverlapSphere(rayOrigin + rayDirection * (interactionDistance/2), interactionDistance/2);
                Debug.Log($"🔍 Objects nearby: {nearbyObjects.Length}");
                foreach (var obj in nearbyObjects)
                {
                    Debug.Log($"   - {obj.name} (Layer: {obj.gameObject.layer})");
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (showDebugSphere && cameraTransform != null)
        {
            Vector3 rayOrigin = cameraTransform.position;
            Vector3 rayDirection = cameraTransform.forward;
            
          
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rayOrigin, rayDirection * interactionDistance);
            
         
            Gizmos.color = Color.cyan;
            for (float t = 0; t <= 1; t += 0.2f)
            {
                Vector3 spherePos = rayOrigin + rayDirection * (interactionDistance * t);
                Gizmos.DrawWireSphere(spherePos, sphereRadius);
            }
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rayOrigin + rayDirection * interactionDistance, sphereRadius);
        }
    }
}