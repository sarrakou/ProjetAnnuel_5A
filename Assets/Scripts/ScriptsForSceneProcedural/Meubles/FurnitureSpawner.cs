using UnityEngine;
using System.Collections.Generic;

public class FurnitureSpawner : MonoBehaviour
{
    [Header("Meubles à instancier")]
    public GameObject bedPrefab;
    public GameObject wardrobePrefab;
    public GameObject dresserPrefab;

    [Header("Emplacements possibles")]
    public Transform[] spawnPoints; // 3 emplacements

    void Start()
    {
        SpawnFurniture();
    }

    void SpawnFurniture()
    {
        // Créer une liste pour mélanger les points
        List<Transform> availableSpots = new List<Transform>(spawnPoints);

        if (availableSpots.Count < 3)
        {
            Debug.LogError("Il faut au moins 3 emplacements dans spawnPoints !");
            return;
        }

        // Mélanger les emplacements
        Shuffle(availableSpots);

        // Instancier chaque meuble à un emplacement unique
        Instantiate(bedPrefab, availableSpots[0].position, availableSpots[0].rotation);
        Instantiate(wardrobePrefab, availableSpots[1].position, availableSpots[1].rotation);
        Instantiate(dresserPrefab, availableSpots[2].position, availableSpots[2].rotation);
    }

    void Shuffle(List<Transform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Transform temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
