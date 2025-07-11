using UnityEngine;

public class DoorsInteractor : MonoBehaviour
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
                Doors door = hit.collider.GetComponent<Doors>();
                if (door != null)
                {
                    door.Toggle();
                }
            }
        }
    }
}
