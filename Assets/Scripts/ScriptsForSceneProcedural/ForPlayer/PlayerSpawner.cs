using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Vector3 spawnPosition = new Vector3(-0.3f, 0.033f, -0.03f);

    void Start()
    {
        Invoke(nameof(SpawnPlayer), 3f); // attend 3 secondes
    }

    void SpawnPlayer()
    {
        GameObject instance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        // S'assurer que le gameObject est actif
        instance.SetActive(true);

        // Active le CharacterController
        CharacterController controller = instance.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = true;
        }

        // Active le script de mouvement juste après le spawn
        MyPlayerMovement movementScript = instance.GetComponent<MyPlayerMovement>();
        if (movementScript == null)
        {
            movementScript = instance.GetComponentInChildren<MyPlayerMovement>();
        }

        if (movementScript != null)
        {
            movementScript.enabled = true;
            Debug.Log("MyPlayerMovement activé avec succès.");
        }
        else
        {
            Debug.LogWarning("MyPlayerMovement non trouvé sur le prefab instancié.");
        }
    }
}