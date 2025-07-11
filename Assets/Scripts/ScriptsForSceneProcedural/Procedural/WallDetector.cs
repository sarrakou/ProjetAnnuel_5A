using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [Header("Position cibl�e")]
    private Vector3 targetPosition;

    [Header("Rayon de d�tection")]
    private float detectionRadius =0.005f;

    [Header("Couches � consid�rer comme mur")]
    private LayerMask wallLayer;

    private void Start()
    {
        wallLayer = LayerMask.GetMask("Mur");
        targetPosition = gameObject.transform.position + new Vector3(0, 0.05f, 0);
    }

    void Update()
    {
        // D�tection des murs autour de la position cible
        Collider[] colliders = Physics.OverlapSphere(targetPosition, detectionRadius, wallLayer);

        foreach (var collider in colliders)
        {
            Destroy(collider.gameObject);
            
        }
    }

    // Affiche la sph�re de d�tection dans la sc�ne
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, detectionRadius);

        
    }
}