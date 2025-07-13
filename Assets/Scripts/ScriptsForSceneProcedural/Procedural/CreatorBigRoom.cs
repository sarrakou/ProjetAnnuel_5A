using System.Collections.Generic;
using UnityEngine;

public class CreatorBigRoom : MonoBehaviour
{
    public int NBpieceACreer;

    public PlayerSpawner Spawner;
    //[Header("Porte de sortie")]
    //public GameObject exitDoorPrefab;

    List<GameObject> piecesCreeesList = new List<GameObject>();


    public void createBigRoom()
    {
        PlacementDetector dernierDetectorUtilise = null;

        // Compter les pièces déjà existantes (BigPiece)
        GameObject[] existingPieces = GameObject.FindGameObjectsWithTag("BigPiece"); 
        int piecesDejaExistantes = existingPieces.Length;

        int piecesCreees = piecesDejaExistantes; // Commencer avec les pièces déjà présentes

        Debug.Log("Pièces déjà existantes : " + piecesDejaExistantes);

        // Tant qu'on doit encore créer des pièces
        while (piecesCreees < NBpieceACreer)
        {
            // À chaque itération, chercher TOUS les détecteurs avec espace libre
            PlacementDetector[] detectors = FindObjectsByType<PlacementDetector>(FindObjectsSortMode.None);
            List<PlacementDetector> detectorsAvecEspace = new List<PlacementDetector>();

            foreach (PlacementDetector detector in detectors)
            {
                if (detector.isTheirSpace())
                {
                    detectorsAvecEspace.Add(detector);
                }
            }

            // S'il n'y a plus d'espace libre, on arrête la boucle
            if (detectorsAvecEspace.Count == 0)
            {
                Debug.Log("Plus d'espace libre pour créer des pièces. Pièces créées : " + piecesCreees);
                break;
            }

            // Choisir un détecteur au hasard dans ceux avec espace libre
            int randIndex = Random.Range(0, detectorsAvecEspace.Count);
            PlacementDetector choisi = detectorsAvecEspace[randIndex];

            // Créer la pièce
            //GameObject roomSpawn = choisi.privateCreatePiece();

            GameObject newRoom = choisi.privateCreatePiece();
            piecesCreeesList.Add(newRoom);


            // Mémoriser le dernier utilisé
            dernierDetectorUtilise = choisi;

            piecesCreees++;
            //Spawner.SpawnPlayer(roomSpawn.transform.position);
        }


        if (piecesCreeesList.Count > 0)
        {
            int indexRandom = Random.Range(0, piecesCreeesList.Count);
            Vector3 spawnPos = piecesCreeesList[indexRandom].transform.position;
            spawnPos.y = Mathf.Max(spawnPos.y, 0.02f);


            Spawner.SpawnPlayer(spawnPos);

            Debug.Log("Joueur spawné dans une pièce aléatoire !");
        }
        else
        {
            Debug.LogWarning("Aucune pièce créée, impossible de spawn le joueur.");
        }

        //place la porte a la fin de la generation 

        /*if (exitDoorPrefab != null && dernierDetectorUtilise != null)
        {
            Vector3 pos = dernierDetectorUtilise.transform.position;
            Quaternion rot = Quaternion.LookRotation(Vector3.Cross(Vector3.up, dernierDetectorUtilise.transform.forward), Vector3.up);

            Instantiate(exitDoorPrefab, pos, rot);
        }
        else
        {
            Debug.LogWarning("Porte non placée : prefab ou dernier détecteur manquant.");
        }*/

    }
}
