using UnityEngine;

public class Door2 : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed = 2f;

    private bool isOpen = false;
    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        closedRot = transform.rotation;
        openRot = transform.rotation * Quaternion.Euler(0, 0, openAngle);
    }

    void Update()
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            isOpen ? openRot : closedRot,
            Time.deltaTime * speed
        );
    }

    public void Toggle()
    {
        isOpen = !isOpen;
    }
}
