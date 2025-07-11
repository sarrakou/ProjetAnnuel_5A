using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactionDistance = 3f;
    public LayerMask interactableLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
            {
               
                InteractableObject interactableObject = hit.collider.GetComponent<InteractableObject>();
                if (interactableObject != null)
                {
                    interactableObject.Toggle();
                    return;
                }
                
                
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                    return;
                }
                
                Debug.Log("🚫 Aucun objet interactif détecté.");
            }
        }
    }
}