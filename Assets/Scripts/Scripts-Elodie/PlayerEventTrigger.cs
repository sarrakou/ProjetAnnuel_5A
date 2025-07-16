using UnityEngine;

public class PlayerEventTrigger : MonoBehaviour
{
    [Header("Références")]
    public GameObject coffreFortGameObject;
    public DigiCodeManager digiCodeManager; 

    private HorrorEvents horrorEvents;
    private GameManager gameManager;
    private bool coffreMissionValidee = false; // Pour éviter de valider plusieurs fois
    private bool finMissionValidee = false; // Pour éviter de valider plusieurs fois la mission finale

    private void Start()
    {
        horrorEvents = FindObjectOfType<HorrorEvents>();
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("❌ GameManager non trouvé !");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("EventZone"))
        {
            Debug.Log("🎯 Zone d'event touchée, déclenchement !");
            Vector3 eventPos = other.transform.position;
            horrorEvents.TriggerRandomEvent(eventPos);
            other.gameObject.SetActive(false);
        }

        if (other.CompareTag("CoffreFort") || 
            (coffreFortGameObject != null && other.gameObject == coffreFortGameObject) ||
            other.gameObject.name.ToLower().Contains("coffre"))
        {
            Debug.Log("🔐 Coffre-fort détecté !");

            if (!coffreMissionValidee && gameManager != null)
            {
                gameManager.CompleteQuestByName("Trouver le coffre-fort");
                coffreMissionValidee = true;
            }

            
            if (digiCodeManager != null)
            {
                digiCodeManager.ShowDigiCode();
            }
        }
        

        if (other.CompareTag("Library") || 
            other.gameObject.name.ToLower().Contains("library"))
        {
            Debug.Log("📚 Librairie détectée !");

            if (!coffreMissionValidee && gameManager != null)
            {
                gameManager.CompleteQuestByName("Trouver le coffre-fort");
                coffreMissionValidee = true;
            }

          
            if (digiCodeManager != null)
            {
                digiCodeManager.ShowDigiCode();
            }
        }

       
        if (other.CompareTag("Fin"))
        {
            Debug.Log("🚪 Zone de fin détectée - Validation de la mission finale !");
            
            if (!finMissionValidee && gameManager != null)
            {
                gameManager.CompleteQuestByName("Il faut sortir. Maintenant !");
                finMissionValidee = true;
                
                
            }
            else if (finMissionValidee)
            {
                Debug.Log("✅ Mission finale déjà validée !");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
   
        if (other.CompareTag("CoffreFort") ||
            (coffreFortGameObject != null && other.gameObject == coffreFortGameObject) ||
            other.gameObject.name.ToLower().Contains("coffre"))
        {
            if (digiCodeManager != null)
            {
                digiCodeManager.HideDigiCode();
            }
        }

       
        if (other.CompareTag("Library") ||
            other.gameObject.name.ToLower().Contains("library"))
        {
            if (digiCodeManager != null)
            {
                digiCodeManager.HideDigiCode();
            }
        }
    }

  
    private System.Collections.IEnumerator DisableFinObjectAfterDelay(GameObject finObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        finObject.SetActive(false);
        Debug.Log("🚪 Objet de fin désactivé après " + delay + " secondes");
    }

 
    public void ResetMissionStates()
    {
        coffreMissionValidee = false;
        finMissionValidee = false;
        Debug.Log("🔄 États des missions réinitialisés");
    }
}