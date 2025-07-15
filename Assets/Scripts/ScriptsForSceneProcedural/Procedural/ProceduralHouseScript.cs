using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralHouseScript : MonoBehaviour
{
    public GameObject[] largeRoomModules;
    public GameObject corridorModules;
    public GameObject CouloirDebut;
    public GameObject[] coinModules;
    string tagRecherche = "ouverture"; 
    int nbCouloir = 50;
    int nbCorridor = 5;
    int maxnbCorridor = 5;
    GameObject currentModule;
    bool iscorridor = true;
    public GameObject BigRoomCreator;
    private Stack<GameObject> corridorsEnAttente = new Stack<GameObject>();

    private GameObject dernierCouloir;
    public GameObject DoorRed;

    private void Start()
    {


        GameObject randomModule = largeRoomModules[Random.Range(0, largeRoomModules.Length)];
        GameObject firstModule = Instantiate(CouloirDebut, new Vector3(0, 0, 0), Quaternion.identity);

        corridorsEnAttente.Push(firstModule);

        

        while (nbCouloir > 0)
        {
            currentModule = corridorsEnAttente.Pop();

            foreach (Transform child in currentModule.transform)
            {
                if (child.CompareTag(tagRecherche))
                {

                    if (iscorridor)
                    {
                        randomModule = corridorModules;
                        nbCorridor--;

                    }
                    else
                    {
                        randomModule = coinModules[Random.Range(0, coinModules.Length)];
                        iscorridor = true;
                    }

                    GameObject corridor = Instantiate(randomModule, child.position, child.rotation);



                    corridorsEnAttente.Push(corridor);

                    dernierCouloir = corridor;
                }

                if (nbCorridor == 0)
                {
                    iscorridor = false;
                    nbCorridor = Random.Range(2, maxnbCorridor + 1); ;
                }
            }

            

            nbCouloir--;

        }

        if (dernierCouloir != null)
        {
            foreach (Transform child in dernierCouloir.transform)
            {
                if (child.CompareTag(tagRecherche))
                {
                    // Ici tu instancies ta prefab à l'ouverture
                    GameObject nouvellePrefab = DoorRed; // remplace par ce que tu veux instancier
                    Instantiate(nouvellePrefab, child.position, child.rotation);
                    break; // On ne le fait qu'une fois
                }
            }
        }



        Debug.Log("Génération terminée. Pause de 2 secondes effectuée.");

        BigRoomCreator.GetComponent<CreatorBigRoom>().createBigRoom();

    }
}