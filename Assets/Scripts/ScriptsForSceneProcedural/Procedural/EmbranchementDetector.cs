using System.Collections.Generic;
using UnityEngine;

public class EmbranchementDetector : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject exitDoorPrefab;
    public string tagRecherche = "ouverture";
    public float rayonDetection = 1f;
    public LayerMask layerMask = -1; // Toutes les couches par défaut

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool showDebugLogs = true;

    private void Start()
    {
        // Optionnel : détecter et placer automatiquement au démarrage
        // Décommentez si vous voulez que ça se fasse automatiquement
        Invoke("DetecterEtPlacerPorte", 0.5f); // Petit délai pour s'assurer que tout est généré
    }

    [ContextMenu("Détecter et Placer Porte")]
    public void DetecterEtPlacerPorte()
    {
        if (showDebugLogs) Debug.Log("=== DÉBUT DÉTECTION EMBRANCHEMENTS ===");

        List<Transform> embranchements = TrouverEmbranchements();

        if (embranchements.Count > 0)
        {
            PlacerPorteSurEmbranchement(embranchements);
        }
        else
        {
            if (showDebugLogs) Debug.LogWarning("Aucun embranchement détecté pour placer la porte rouge.");
        }

        if (showDebugLogs) Debug.Log("=== FIN DÉTECTION EMBRANCHEMENTS ===");
    }

    private List<Transform> TrouverEmbranchements()
    {
        List<Transform> embranchements = new List<Transform>();

        // Trouver tous les GameObjects avec le tag "ouverture"
        GameObject[] ouvertures = GameObject.FindGameObjectsWithTag(tagRecherche);

        if (showDebugLogs) Debug.Log($"Nombre d'ouvertures trouvées : {ouvertures.Length}");

        foreach (GameObject ouverture in ouvertures)
        {
            // Vérifier si cette ouverture est "libre" (pas connectée à un autre module)
            if (EstOuvertureLibre(ouverture.transform))
            {
                embranchements.Add(ouverture.transform);
                if (showDebugLogs) Debug.Log($"✓ Embranchement libre trouvé: {ouverture.name} à {ouverture.transform.position}");
            }
            else
            {
                if (showDebugLogs) Debug.Log($"✗ Ouverture occupée: {ouverture.name}");
            }
        }

        if (showDebugLogs) Debug.Log($"Total embranchements libres : {embranchements.Count}");
        return embranchements;
    }

    private bool EstOuvertureLibre(Transform ouverture)
    {
        // Position de test devant l'ouverture
        Vector3 positionTest = ouverture.position + ouverture.forward * rayonDetection;

        // Détection avec sphère
        Collider[] colliders = Physics.OverlapSphere(positionTest, rayonDetection * 0.5f, layerMask);

        foreach (Collider col in colliders)
        {
            // Ignorer :
            // - Le GameObject de l'ouverture elle-même
            // - Le parent de l'ouverture (le module)
            // - Les autres ouvertures
            if (col.gameObject == ouverture.gameObject ||
                col.transform.IsChildOf(ouverture.parent) ||
                col.gameObject.CompareTag(tagRecherche))
            {
                continue;
            }

            // Si on trouve autre chose, l'ouverture n'est pas libre
            if (showDebugLogs) Debug.Log($"Ouverture {ouverture.name} bloquée par: {col.gameObject.name}");
            return false;
        }

        return true; // Ouverture libre
    }

    private void PlacerPorteSurEmbranchement(List<Transform> embranchements)
    {
        if (exitDoorPrefab == null)
        {
            Debug.LogError("Prefab de porte rouge non assigné dans EmbranchementDetector !");
            return;
        }

        // Choisir un embranchement au hasard
        int randomIndex = Random.Range(0, embranchements.Count);
        Transform embranchementChoisi = embranchements[randomIndex];

        // Position et rotation de la porte
        Vector3 positionPorte = embranchementChoisi.position;
        Quaternion rotationPorte = embranchementChoisi.rotation;

        // Instancier la porte
        GameObject porteRouge = Instantiate(exitDoorPrefab, positionPorte, rotationPorte);
        porteRouge.name = "Porte de Sortie (Auto-placée)";

        // Optionnel: Assurer que la porte est bien rouge
        Renderer renderer = porteRouge.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }

        if (showDebugLogs) Debug.Log($"🚪 Porte rouge placée sur embranchement libre à: {positionPorte}");

        // Optionnel: Désactiver l'ouverture originale pour éviter les conflits
        embranchementChoisi.gameObject.SetActive(false);
    }

    // Fonction publique pour forcer le placement (appelable depuis d'autres scripts)
    public void ForcerPlacementPorte()
    {
        DetecterEtPlacerPorte();
    }

    // Fonction pour nettoyer les anciennes portes avant d'en placer une nouvelle
    public void NettoyerAnciennesPortes()
    {
        GameObject[] anciennesPortes = GameObject.FindGameObjectsWithTag("ExitDoor");
        foreach (GameObject porte in anciennesPortes)
        {
            DestroyImmediate(porte);
        }
    }

    // Visualisation dans l'éditeur
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        GameObject[] ouvertures = GameObject.FindGameObjectsWithTag(tagRecherche);

        foreach (GameObject ouverture in ouvertures)
        {
            if (ouverture != null)
            {
                bool estLibre = EstOuvertureLibre(ouverture.transform);

                // Couleur différente selon si l'ouverture est libre ou pas
                Gizmos.color = estLibre ? Color.green : Color.red;

                // Sphère de détection
                Vector3 positionTest = ouverture.transform.position + ouverture.transform.forward * rayonDetection;
                Gizmos.DrawWireSphere(positionTest, rayonDetection * 0.5f);

                // Flèche indiquant la direction
                Gizmos.DrawRay(ouverture.transform.position, ouverture.transform.forward * rayonDetection);

                // Petit cube à la position de l'ouverture
                Gizmos.color = estLibre ? Color.cyan : Color.magenta;
                Gizmos.DrawWireCube(ouverture.transform.position, Vector3.one * 0.1f);
            }
        }
    }
}