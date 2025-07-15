using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractableBis
{
    [Header("Quest Configuration")]
    public string questToComplete = "Trouver un moyen d'allumer les lumières";
    
    [Header("Audio Configuration")]
    public AudioClip switchSound;
    public AudioSource audioSource;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private bool hasBeenUsed = false; 
    private GameManager gameManager;

    void Start()
    {
        
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null && showDebugLogs)
        {
            Debug.LogError("[LightSwitch] GameManager introuvable !");
        }
        
     
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f;
        
        if (showDebugLogs)
        {
            Debug.Log("[LightSwitch] Interrupteur prêt - 1 utilisation seulement");
        }
    }

   
    public void Interact()
    {
      
        if (hasBeenUsed) 
        {
            return;
        }

        hasBeenUsed = true;
        
      
        if (audioSource != null && switchSound != null)
        {
            audioSource.clip = switchSound;
            audioSource.Play();
        }
        

        if (gameManager != null)
        {
            gameManager.CompleteQuestByName(questToComplete);
            if (showDebugLogs)
            {
                Debug.Log($"[LightSwitch] ✅ '{questToComplete}' - TERMINÉ !");
            }
        }
        
       
        this.enabled = false;
        
        if (showDebugLogs)
        {
            Debug.Log("[LightSwitch] 💡 Interrupteur activé - Plus d'interaction possible !");
        }
    }


    public bool HasBeenUsed()
    {
        return hasBeenUsed;
    }
}