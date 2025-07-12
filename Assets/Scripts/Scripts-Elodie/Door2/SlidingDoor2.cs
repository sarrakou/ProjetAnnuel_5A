using UnityEngine;

public class SlidingDoor2: MonoBehaviour
{
    public Vector3 openOffset = new Vector3(0f, 0f, 2f); 
    public float speed = 2f;
    public KeyCode interactKey = KeyCode.E;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private bool playerNearby = false;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(interactKey))
        {
            isOpen = !isOpen;
        }

        Vector3 target = isOpen ? openPosition : closedPosition;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}
