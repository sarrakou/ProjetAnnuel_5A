using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralHouseScript : MonoBehaviour
{
    public GameObject[] largeRoomModules;
    public GameObject corridorModules;
    public GameObject[] coinModules;
    string tagRecherche = "ouverture"; // Remplace par le tag que tu veux
    int nbCouloir = 50;
    int nbCorridor = 5;
    int maxnbCorridor = 5;
    GameObject currentModule;
    bool iscorridor = true;
    public GameObject BigRoomCreator;
    private Stack<GameObject> corridorsEnAttente = new Stack<GameObject>();

    private void Start()
    {


        GameObject randomModule = largeRoomModules[Random.Range(0, largeRoomModules.Length)];
        GameObject firstModule = Instantiate(randomModule, new Vector3(0, 0, 0), Quaternion.identity);
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


                }

                if (nbCorridor == 0)
                {
                    iscorridor = false;
                    nbCorridor = Random.Range(2, maxnbCorridor + 1); ;
                }
            }

            nbCouloir--;

        }

        // Attente de 2 secondes après toute la génération


        Debug.Log("Génération terminée. Pause de 2 secondes effectuée.");

        BigRoomCreator.GetComponent<CreatorBigRoom>().createBigRoom();

    }
}