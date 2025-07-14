using UnityEngine;

public class InsectSwarmSpawner : MonoBehaviour
{
    [Header("Prefab d'insecte à cloner depuis Resources")]
    public string insectPrefabResourceName;

    [Header("Nombre d'insectes à créer")]
    public int insectCount = 10;

    [Header("Rayon autour du prefab pour spawn")]
    public float spawnRadius = 5f;

    private GameObject insectPrefab;

    private void Start()
    {
        insectPrefab = Resources.Load<GameObject>(insectPrefabResourceName);
        if (insectPrefab == null)
        {
            Debug.LogError("InsectSwarmSpawner : prefab introuvable dans Resources : " + insectPrefabResourceName);
            return;
        }

        SpawnInsects();
    }

    private void SpawnInsects()
    {
        Vector3 prefabPosition = insectPrefab.transform.position; 

        for (int i = 0; i < insectCount; i++)
        {
           
            Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(
                prefabPosition.x + randomPos.x,
                prefabPosition.y,     
                prefabPosition.z + randomPos.y
            );

            Instantiate(insectPrefab, spawnPos, Quaternion.identity);
        }
    }
}