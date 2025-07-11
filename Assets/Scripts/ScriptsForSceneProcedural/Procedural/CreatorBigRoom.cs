using System.Collections.Generic;
using UnityEngine;

public class CreatorBigRoom : MonoBehaviour
{
    public int NBpieceACreer;

    [Header("Porte de sortie")]
    public GameObject exitDoorPrefab;

    public void createBigRoom()
    {
        PlacementDetector dernierDetectorUtilise = null;

        int piecesCreees = 0;
        PlacementDetector[] detectors = FindObjectsByType<PlacementDetector>(FindObjectsSortMode.None);
        List<PlacementDetector> detectorsAvecEspace = new List<PlacementDetector>();

        foreach (PlacementDetector detector in detectors)
        {

            if (detector.isTheirSpace())
            {
                detectorsAvecEspace.Add(detector);
                print("aa");

            }
        }

        // Tant qu'on doit encore cr�er des pi�ces
        while (piecesCreees < NBpieceACreer)
        {
            // Trouver tous les PlacementDetector avec espace libre AU MOMENT

            // S�il n�y a plus d�espace libre, on arr�te la boucle
            if (detectorsAvecEspace.Count == 0)
            {

                break;
            }

            // Choisir un d�tecteur au hasard dans ceux avec espace libre
            int randIndex = Random.Range(0, detectorsAvecEspace.Count);
            PlacementDetector choisi = detectorsAvecEspace[randIndex];

            // Cr�er la pi�ce
            choisi.privateCreatePiece();

            // M�moriser le dernier utilis�
            dernierDetectorUtilise = choisi;

            piecesCreees++;
        }

        //place la porte a la fin de la generation 

        if (exitDoorPrefab != null && dernierDetectorUtilise != null)
        {
            Vector3 pos = dernierDetectorUtilise.transform.position;
            Quaternion rot = Quaternion.LookRotation(Vector3.Cross(Vector3.up, dernierDetectorUtilise.transform.forward), Vector3.up);

            Instantiate(exitDoorPrefab, pos, rot);
        }
        else
        {
            Debug.LogWarning("Porte non plac�e : prefab ou dernier d�tecteur manquant.");
        }

    }
}