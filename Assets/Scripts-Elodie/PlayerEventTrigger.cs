using UnityEngine;

public class PlayerEventTrigger : MonoBehaviour
{
    [Header("Références")]
    public GameObject coffreFortGameObject;
    public DigiCodeManager digiCodeManager; 

    private HorrorEvents horrorEvents;
    private GameManager gameManager;
    private bool coffreMissionValidee = false; // Pour éviter de valider plusieurs fois

    private void Start()
    {
        horrorEvents = FindObjectOfType<HorrorEvents>();
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError(" GameManager non trouvé !");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Zone d'horreur
        if (other.CompareTag("EventZone"))
        {
            Vector3 eventPos = other.transform.position;
            horrorEvents.TriggerRandomEvent(eventPos);
            other.gameObject.SetActive(false);
        }

        // Zone du coffre-fort
        if (other.CompareTag("CoffreFort") || 
            (coffreFortGameObject != null && other.gameObject == coffreFortGameObject) ||
            other.gameObject.name.ToLower().Contains("coffre"))
        {
            Debug.Log(" Coffre-fort détecté !");

            if (!coffreMissionValidee && gameManager != null)
            {
                gameManager.CompleteQuestByName("Trouver le coffre-fort");
                coffreMissionValidee = true;
            }

            //  Activer le Digicode
            if (digiCodeManager != null)
            {
                digiCodeManager.ShowDigiCode();
            }
        }
        
        if (other.CompareTag("Library") || 
            (coffreFortGameObject != null && other.gameObject == coffreFortGameObject) ||
            other.gameObject.name.ToLower().Contains("library"))
        {
            Debug.Log(" Librairie détecté !");

            if (!coffreMissionValidee && gameManager != null)
            {
                gameManager.CompleteQuestByName("Trouver le coffre-fort");
                coffreMissionValidee = true;
            }

            //  Activer le Digicode
            if (digiCodeManager != null)
            {
                digiCodeManager.ShowDigiCode();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //  Fermer le Digicode quand on sort
        if (other.CompareTag("CoffreFort") ||
            (coffreFortGameObject != null && other.gameObject == coffreFortGameObject) ||
            other.gameObject.name.ToLower().Contains("coffre"))
        {
            if (digiCodeManager != null)
            {
                digiCodeManager.HideDigiCode();
            }
        }
    }
}
