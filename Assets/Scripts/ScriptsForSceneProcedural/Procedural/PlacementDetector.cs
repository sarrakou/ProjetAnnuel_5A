using UnityEngine;

public class PlacementDetector : MonoBehaviour
{
    [Header("Distance devant pour la détection")]
    private float detectionDistance = 0.25f;

    [Header("Rayon de la sphère de détection")]
    private float detectionRadius = 0.2f;

    [Header("Prefab de la pièce pour la détection de mesh")]
    public GameObject piecePrefabMeshTest;

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

        // Si des colliders sont détectés, vérifier s'ils sont importants
        if (colliders.Length > 0)
        {
            foreach (Collider col in colliders)
            {
                // Ignorer les triggers et les petits objets non-importants
                if (!col.isTrigger && col.gameObject.layer != LayerMask.NameToLayer("Ignore"))
                {
                    return false;
                }
            }
        }

        // Vérification principale avec le mesh du prefab (plus précise)
        return isRoomMeshFree();
    }

    private bool isClearBoxArea()
    {
        Vector3 offset = new Vector3(0, 0.05f, 0);
        Vector3 boxCenter = transform.position + transform.forward * detectionDistance + offset;

        // Augmenter légèrement la taille de base pour éviter d'être trop restrictif
        Vector3 baseSize = new Vector3(0.4003027f, 0.1f, 0.2f);
        Vector3 margin = new Vector3(0.01f, 0, 0.01f); // Réduire la marge à 1 cm
        Vector3 roomSize = baseSize - margin;

        Quaternion boxRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        Collider[] hits = Physics.OverlapBox(boxCenter, roomSize * 0.5f, boxRotation);

        // Filtrer les colliders non-importants
        foreach (Collider hit in hits)
        {
            if (!hit.isTrigger && hit.gameObject.layer != LayerMask.NameToLayer("Ignore"))
            {
                return false;
            }
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        // 1. Gizmo de la sphère de détection (bleu)
        Gizmos.color = Color.blue;
        Vector3 gizmoPos = transform.position + transform.forward * detectionDistance;
        Gizmos.DrawWireSphere(gizmoPos, detectionRadius);

        // 2. Gizmos des colliders de test du mesh (cyan clair)
        if (piecePrefabMeshTest == null) return;

        Vector3 offset = new Vector3(0, 0.05f, 0);
        Vector3 spawnPos = transform.position + transform.forward * detectionDistance + offset;
        Quaternion rotation = Quaternion.LookRotation(transform.forward, Vector3.up);

        Collider[] roomColliders = piecePrefabMeshTest.GetComponentsInChildren<Collider>();
        foreach (Collider roomCollider in roomColliders)
        {
            Vector3 center = spawnPos + rotation * roomCollider.transform.localPosition;
            Quaternion rot = rotation * roomCollider.transform.localRotation;
            Vector3 scale = roomCollider.transform.lossyScale;

            if (roomCollider is BoxCollider box)
            {
                Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, scale);
                Gizmos.color = new Color(0f, 1f, 1f, 0.6f); // cyan clair
                Matrix4x4 matrix = Matrix4x4.TRS(center, rot, Vector3.one);
                Gizmos.matrix = matrix;
                Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
            }
            else if (roomCollider is SphereCollider sphere)
            {
                float radius = sphere.radius * Mathf.Max(scale.x, scale.y, scale.z);
                Vector3 sphereCenter = center + rotation * sphere.center;
                Gizmos.color = new Color(0f, 1f, 1f, 0.6f); // cyan clair
                Gizmos.DrawWireSphere(sphereCenter, radius);
            }
        }

        // Reset Gizmos matrix
        Gizmos.matrix = Matrix4x4.identity;
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

    private bool isRoomMeshFree()
    {
        if (piecePrefabMeshTest == null)
        {
            Debug.LogWarning("Prefab de pièce manquant pour le test mesh. Utilisation de la détection basique.");
            return true; // Retourner true au lieu de false pour permettre le placement
        }

        // Préparer les paramètres de position/rotation
        Vector3 offset = new Vector3(0, 0.05f, 0);
        Vector3 spawnPos = transform.position + transform.forward * detectionDistance + offset;
        Quaternion rotation = Quaternion.LookRotation(transform.forward, Vector3.up);

        // Obtenir les colliders du prefab
        Collider[] roomColliders = piecePrefabMeshTest.GetComponentsInChildren<Collider>();

        foreach (Collider roomCollider in roomColliders)
        {
            // Ignorer les triggers
            if (roomCollider.isTrigger) continue;

            // Créer une copie de la collider transformée (simulée)
            Vector3 center = spawnPos + rotation * roomCollider.transform.localPosition;
            Quaternion rot = rotation * roomCollider.transform.localRotation;
            Vector3 scale = roomCollider.transform.lossyScale;

            if (roomCollider is BoxCollider box)
            {
                // Réduire légèrement la taille pour éviter d'être trop strict
                Vector3 halfExtents = Vector3.Scale(box.size * 0.48f, scale); // 0.48 au lieu de 0.5
                Collider[] hits = Physics.OverlapBox(center, halfExtents, rot);

                // Filtrer les colliders non-importants
                foreach (Collider hit in hits)
                {
                    if (!hit.isTrigger && hit.gameObject.layer != LayerMask.NameToLayer("Ignore"))
                    {
                        return false;
                    }
                }
            }
            else if (roomCollider is SphereCollider sphere)
            {
                float scaledRadius = sphere.radius * Mathf.Max(scale.x, scale.y, scale.z) * 0.95f; // Réduire légèrement
                Vector3 sphereCenter = center + rotation * sphere.center;
                Collider[] hits = Physics.OverlapSphere(sphereCenter, scaledRadius);

                // Filtrer les colliders non-importants
                foreach (Collider hit in hits)
                {
                    if (!hit.isTrigger && hit.gameObject.layer != LayerMask.NameToLayer("Ignore"))
                    {
                        return false;
                    }
                }
            }
            else if (roomCollider is MeshCollider meshCol)
            {
                // Pour les mesh colliders, on peut faire une vérification approximative avec une box
                Bounds bounds = meshCol.bounds;
                Vector3 halfExtents = bounds.size * 0.45f; // Encore plus petit pour compenser l'approximation
                Collider[] hits = Physics.OverlapBox(center, halfExtents, rot);

                // Filtrer les colliders non-importants
                foreach (Collider hit in hits)
                {
                    if (!hit.isTrigger && hit.gameObject.layer != LayerMask.NameToLayer("Ignore"))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}