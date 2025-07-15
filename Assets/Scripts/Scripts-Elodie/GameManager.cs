using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Quest Configuration")]
    public List<Quest> quests = new List<Quest>();
    
    [Header("Doll Event Configuration")]
    public string dollPrefabResourcePath = "Doll/DollPrefab"; 
    public AudioClip dollAudioClip; 
    public string dollAudioResourcePath = "Audio/DollSound"; 
    
    [Header("Claustrophobia Configuration")]
    public GameObject closedGameObject; 
    public float claustrophobiaActiveDuration = 5f; 
    
    [Header("Audio Configuration")]
    public AudioSource audioSource; 
    public float dollAudioVolume = 1f; 
    
    private LightManager lightManager;
    private HorrorEvents horrorEvents; 
    private GameObject spawnedDoll; 
    private int dollQuestIndex = 4; 
    private int dollSoundQuestIndex = 5; 

    void Start()
    {
        
        lightManager = FindObjectOfType<LightManager>();
        
    
        horrorEvents = FindObjectOfType<HorrorEvents>();

     
        SetupAudioSource();
        
       
        LoadDollAudio();

       
        InitializeQuests();


        CheckNyctophobiaQuest();
        
      
        
        AfficherToutesLesQuetes();
    }

    void InitializeQuests()
    {
        quests.Add(new Quest("Trouver un moyen d'allumer les lumières", "Explorer la maison pour rétablir l'électricité."));
        quests.Add(new Quest("Trouver la clé de la chambre principale", "Chercher dans les pièces accessibles."));
        quests.Add(new Quest("Trouver le journal du propriétaire", "Il pourrait contenir des indices."));
        quests.Add(new Quest("Trouver le coffre-fort", "Localiser l'endroit où il est caché."));
        quests.Add(new Quest("Récupérer la clé", "Elle est probablement dans le coffre-fort.")); 
        quests.Add(new Quest("D'où vient ce bruit ?", "Je dois récupérer cette poupée.")); 
        quests.Add(new Quest("Récupérer la poupée", "Aller chercher cette poupée mystérieuse.")); 
        quests.Add(new Quest("Trouver la porte secrète", "Quelque chose cloche dans cette maison."));
        quests.Add(new Quest("Appeler la police1", "Il faut de l'aide immédiatement."));
        quests.Add(new Quest("Trouver un moyen de faire fonctionner le téléphone", "Il est hors-service."));
        quests.Add(new Quest("Appeler la police", "Il faut de l'aide immédiatement."));
        quests.Add(new Quest("Il faut sortir. Maintenant !", "Quitte la maison au plus vite !"));
    }

    void SetupAudioSource()
    {
        if (audioSource == null)
        {
           
            audioSource = GetComponent<AudioSource>();
   
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                Debug.Log("[GameManager] AudioSource créé automatiquement");
            }
        }
        
        audioSource.playOnAwake = false;
        audioSource.volume = dollAudioVolume;
    }

    void LoadDollAudio()
    {
        if (dollAudioClip == null && !string.IsNullOrEmpty(dollAudioResourcePath))
        {
            dollAudioClip = Resources.Load<AudioClip>(dollAudioResourcePath);
            if (dollAudioClip != null)
            {
                Debug.Log($"[GameManager] Audio de la poupée chargé depuis Resources: {dollAudioResourcePath}");
            }
            else
            {
                Debug.LogWarning($"[GameManager] Impossible de charger l'audio à: Resources/{dollAudioResourcePath}");
            }
        }
    }

    void CheckNyctophobiaQuest()
    {
        if (horrorEvents != null && horrorEvents.currentPhobia == PhobiaType.Nyctophobie)
        {
            Debug.Log("🌙 Nyctophobie détectée - La quête des lumières est automatiquement passée");
            
           
            if (quests.Count > 0 && !quests[0].isCompleted)
            {
                quests[0].CompleteQuest();
                Debug.Log("✅ Quête 'Trouver un moyen d'allumer les lumières' complétée automatiquement");
                
                
            }
        }
    }

    void TriggerClaustrophobiaEffect()
    {
        if (horrorEvents != null && horrorEvents.currentPhobia == PhobiaType.Claustrophobie)
        {
            Debug.Log("🔒 Claustrophobie détectée - Activation de l'effet 'Closed'");
            
           
            if (closedGameObject == null)
            {
                closedGameObject = GameObject.Find("Closed");
                if (closedGameObject == null)
                {
                    Debug.LogError("[GameManager] GameObject 'Closed' introuvable dans la scène !");
                    return;
                }
            }
            
          
            StartCoroutine(ActivateClaustrophobiaEffect());
        }
    }

    System.Collections.IEnumerator ActivateClaustrophobiaEffect()
    {
        if (closedGameObject == null) yield break;
        
        Debug.Log("🔒 Activation de l'effet claustrophobie pendant 5 secondes");
        
    
        closedGameObject.SetActive(true);
        
       
        yield return new WaitForSeconds(claustrophobiaActiveDuration);
        
      
        closedGameObject.SetActive(false);
        
        Debug.Log("🔓 Effet claustrophobie terminé - 'Closed' désactivé");
    }

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) CompleterQuete(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) CompleterQuete(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CompleterQuete(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) CompleterQuete(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) CompleterQuete(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) CompleterQuete(5); 
        if (Input.GetKeyDown(KeyCode.Alpha7)) CompleterQuete(6); 
        if (Input.GetKeyDown(KeyCode.Alpha8)) CompleterQuete(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) CompleterQuete(8);
        if (Input.GetKeyDown(KeyCode.Alpha0)) CompleterQuete(9);
        if (Input.GetKeyDown(KeyCode.L)) CompleterQuete(10); 
    }

    void CompleterQuete(int index)
    {
        if (index >= 0 && index < quests.Count && !quests[index].isCompleted)
        {
            quests[index].CompleteQuest();
            AfficherToutesLesQuetes();

           
            if (index == dollQuestIndex)
            {
            
                PlayDollAudio();
                
               
                SpawnDoll();
            }

       
            if (index == dollSoundQuestIndex)
            {
              
                StopDollAudio();
                
               
                TriggerClaustrophobiaEffect();
            }

            
            if (index == 0 && lightManager != null)
            {
                if (horrorEvents != null && horrorEvents.currentPhobia == PhobiaType.Nyctophobie)
                {
                    Debug.Log("🌙 Nyctophobie active - Les lumières restent éteintes");
                    // Ne pas allumer les lumières
                }
                else
                {
                    lightManager.SetAllLights(true);
                    Debug.Log("💡 Lumières allumées");
                }
            }
        }
    }
    
    public void CompleteQuestByName(string questName)
    {
        Debug.Log($"🎯 Tentative de complétion de la quête : {questName}");
        
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].questName == questName && !quests[i].isCompleted)
            {
                Debug.Log($"✅ Quête trouvée et complétée : {questName}");
                quests[i].CompleteQuest();
                AfficherToutesLesQuetes();
                
               
                if (i == dollQuestIndex)
                {
                   
                    PlayDollAudio();
                    
                    
                    SpawnDoll();
                }

             
                if (i == dollSoundQuestIndex)
                {
                   
                    StopDollAudio();
                    
                   
                    TriggerClaustrophobiaEffect();
                }
                
            
                if (i == 0 && lightManager != null)
                {
                    if (horrorEvents != null && horrorEvents.currentPhobia == PhobiaType.Nyctophobie)
                    {
                        Debug.Log("🌙 Nyctophobie active - Les lumières restent éteintes");
                        // Ne pas allumer les lumières
                    }
                    else
                    {
                        lightManager.SetAllLights(true);
                        Debug.Log("💡 Lumières allumées");
                    }
                }
                return;
            }
        }
        
        Debug.LogWarning($"⚠ Quête non trouvée ou déjà complétée : {questName}");
    }

    void SpawnDoll()
    {
       
        if (spawnedDoll != null)
        {
            Debug.Log("[GameManager] Poupée déjà spawnée, destruction de l'ancienne");
            Destroy(spawnedDoll);
        }

     
        GameObject dollPrefab = Resources.Load<GameObject>(dollPrefabResourcePath);
        
        if (dollPrefab == null)
        {
            Debug.LogError($"[GameManager] Impossible de charger la poupée à: Resources/{dollPrefabResourcePath}");
            return;
        }

   
        spawnedDoll = Instantiate(dollPrefab);
        
        Debug.Log($"🪆 Poupée spawnée pour être récupérée à la prochaine quête");
    }

    void PlayDollAudio()
    {
        if (audioSource == null)
        {
            Debug.LogWarning("[GameManager] Pas d'AudioSource disponible pour jouer le son de la poupée");
            return;
        }

        if (dollAudioClip == null)
        {
            Debug.LogWarning("[GameManager] Pas d'AudioClip assigné pour la poupée");
            return;
        }

        audioSource.clip = dollAudioClip;
        audioSource.volume = dollAudioVolume;
        audioSource.Play();
        
        Debug.Log("🔊 Son de la poupée joué");
    }

    void StopDollAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("🔇 Son de la poupée arrêté");
        }
    }

    
    public void RemoveDoll()
    {
        if (spawnedDoll != null)
        {
            Destroy(spawnedDoll);
            spawnedDoll = null;
            Debug.Log("🪆 Poupée supprimée");
        }
    }

  
    public bool IsDollSpawned()
    {
        return spawnedDoll != null;
    }

 
    public GameObject GetSpawnedDoll()
    {
        return spawnedDoll;
    }

    void AfficherToutesLesQuetes()
    {
        Debug.Log("📋 Liste des quêtes :");
        for (int i = 0; i < quests.Count; i++)
        {
            string status = quests[i].isCompleted ? "✅" : "❌";
            string dollIndicator = (i == dollQuestIndex) ? " 🪆🔊" : "";
            string keyIndicator = $"[Touche {i + 1}]";
            Debug.Log($"{keyIndicator} {status} {quests[i].questName}{dollIndicator} - {quests[i].description}");
        }
    }

    void OnDestroy()
    {
     
        if (spawnedDoll != null)
        {
            Destroy(spawnedDoll);
        }
    }
}