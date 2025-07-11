using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))] // nécessaire pour OnTriggerX
public class DetectObstacle : MonoBehaviour
{
    private bool isTouching = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            isTouching = true;
            Debug.Log($" En contact avec : {other.name}", this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger)
        {
            isTouching = false;
            Debug.Log($" Plus en contact avec : {other.name}", this);
        }
    }

    // ✅ Appelle cette méthode dans un autre script pour savoir s’il y a collision
    public bool IsTouching()
    {
        return isTouching;
    }
}