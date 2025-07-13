using UnityEngine;

public class PlacementDetector : MonoBehaviour
{
    [Header("Distance devant pour la détection")]
    private float detectionDistance = 0.25f;

    [Header("Rayon de la sphère de détection")]
    private float detectionRadius = 0.2f;


    public GameObject BigPiece;


    // Position calculée de la sphère de détection
    private Vector3 spherePosition;

    private void Start()
    {
        // Optionnel : calcul initial (utile si tu veux garder targetPosition)
        spherePosition = transform.position + new Vector3(0, 0.06f, 0) + transform.forward * detectionDistance;


    }

    public bool isTheirSpace()
    {
        // Calculer la position devant le GameObject
        spherePosition = transform.position + transform.forward * detectionDistance;

        // OverlapSphere pour détecter les collisions dans cette zone
        Collider[] colliders = Physics.OverlapSphere(spherePosition, detectionRadius);


        foreach (Collider collider in colliders)
        {
            // Ignorer son propre collider
            if (collider.transform == transform)
            {
                continue;
            }

            // S'il y a un collider valide, l'espace n'est pas libre
            return false;
        }

        // True si aucun collider détecté (zone libre)
        return true;
    }

    private void OnDrawGizmos()
    {
        // Couleur selon l'état
        Gizmos.color = isTheirSpace() ? Color.green : Color.red; 

        // Calculer position de la sphère
        Vector3 gizmoPos = transform.position + transform.forward * detectionDistance;

        // Dessiner la sphère de détection
        Gizmos.DrawWireSphere(gizmoPos, detectionRadius);
    }

    public void privateCreatePiece()
    {



        GameObject pieceToInstantiate = BigPiece;

        // Direction de la flèche bleue (forward) du GameObject d'origine
        Vector3 forward = transform.forward;

        // Construire une rotation pour que l'axe right de l'objet instancié soit aligné avec forward
        // On crée un quaternion avec right = forward, up = Vector3.up (ou autre vecteur vertical)
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, forward), Vector3.up);

        // Instancier avec cette rotation
        Instantiate(pieceToInstantiate, transform.position, targetRotation);
    }
}