using UnityEngine;

public class DoorInteractor2 : MonoBehaviour
{
    public float interactionDistance = 2f;
    public KeyCode interactKey = KeyCode.E;
    public LayerMask doorLayer;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, doorLayer))
            {
                Door2 door = hit.collider.GetComponent<Door2>();
                if (door != null)
                {
                    door.Toggle();
                }
            }
        }
    }
}
