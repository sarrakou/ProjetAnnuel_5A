using UnityEngine;
using System.Collections.Generic;

public class FurnitureSpawner : MonoBehaviour
{
    [Header("Liste de meubles à instancier")]
    public List<GameObject> furniturePrefabs = new List<GameObject>(); // Ajoute autant de meubles que tu veux

    [Header("Emplacements disponibles")]
    public Transform[] spawnPoints; // Toujours 3 emplacements

    void Start()
    {
        SpawnFurniture();
    }

    void SpawnFurniture()
    {
        if (spawnPoints.Length < 3)
        {
            Debug.LogError("Tu dois définir au moins 3 emplacements !");
            return;
        }

        if (furniturePrefabs.Count < 3)
        {
            Debug.LogError("Tu dois fournir au moins 3 meubles !");
            return;
        }

        // Mélanger les meubles et les emplacements
        List<GameObject> shuffledFurniture = new List<GameObject>(furniturePrefabs);
        List<Transform> shuffledSpots = new List<Transform>(spawnPoints);

        Shuffle(shuffledFurniture);
        Shuffle(shuffledSpots);

        // Instancier 3 meubles à 3 emplacements
        for (int i = 0; i < 3; i++)
        {
            Instantiate(shuffledFurniture[i], shuffledSpots[i].position, shuffledSpots[i].rotation);
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int rand = Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}