using UnityEngine;

public class Player_Interactor : MonoBehaviour
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
                InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    interactable.Toggle();
                }
            }
        }
    }
}
