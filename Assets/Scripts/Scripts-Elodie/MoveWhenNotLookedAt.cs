using UnityEngine;

public class MoveWhenNotLookedAt : MonoBehaviour
{
    public Transform playerCamera; 
    public float speed = 2f;
    public float lookThreshold = 0.95f; 

    private void Update()
    {
        Vector3 toObject = (transform.position - playerCamera.position).normalized;
        float dot = Vector3.Dot(playerCamera.forward, toObject);

        if (dot < lookThreshold) 
        {
            Move();
        }
        else
        {
            // Ne bouge pas
        }
    }

    private void Move()
    {
        
        Vector3 direction = (playerCamera.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}