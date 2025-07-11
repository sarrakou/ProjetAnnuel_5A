using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [Header("Position ciblée")]
    private Vector3 targetPosition;

    [Header("Rayon de détection")]
    private float detectionRadius =0.005f;

    [Header("Couches à considérer comme mur")]
    private LayerMask wallLayer;

    private void Start()
    {
        wallLayer = LayerMask.GetMask("Mur");
        targetPosition = gameObject.transform.position + new Vector3(0, 0.05f, 0);
    }

    void Update()
    {
        // Détection des murs autour de la position cible
        Collider[] colliders = Physics.OverlapSphere(targetPosition, detectionRadius, wallLayer);

        foreach (var collider in colliders)
        {
            Destroy(collider.gameObject);
            
        }
    }

    // Affiche la sphère de détection dans la scène
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, detectionRadius);

        
    }
}