using UnityEngine;

public class InsectSwarmSpawner : MonoBehaviour
{
    [Header("Prefab d'insecte à cloner")]
    public GameObject insectPrefab;

    [Header("Nombre d'insectes")]
    public int insectCount = 2;

    [Header("Rayon autour du point de spawn")]
    public float spawnRadius = 5f;

    [Header("Vitesse de déplacement des insectes (optionnel)")]
    public float moveSpeed = 1f;

    private GameObject[] insects;

    void Start()
    {
        if (insectPrefab == null)
        {
            Debug.LogError("InsectSwarmSpawner : Pas de prefab d'insecte assigné !");
            return;
        }

        insects = new GameObject[insectCount];

        for (int i = 0; i < insectCount; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = transform.position.y; 

            GameObject insect = Instantiate(insectPrefab, randomPos, Quaternion.identity);
            insects[i] = insect;
        }
    }

    

}