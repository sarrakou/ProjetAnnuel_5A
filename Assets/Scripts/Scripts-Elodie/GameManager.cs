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
    
    [Header("Phone System Configuration")]
    public Transform phoneTransform; // Position du téléphone
    public AudioClip phoneRingClip; // Son de sonnerie du téléphone
    public string phoneRingResourcePath = "Audio/PhoneRing"; // Chemin vers le son
    public float phoneRingDistance = 10f; // Distance à partir de laquelle le téléphone sonne
    public float phoneCheckInterval = 1f; // Fréquence de vérification de distance
    
    [Header("Audio Configuration")]
    public AudioSource audioSource; 
    public AudioSource phoneAudioSource; // AudioSource dédié pour le téléphone
    public float dollAudioVolume = 1f; 
    public float phoneRingVolume = 0.8f;
    [Header("Final Quest Configuration")]
    public GameObject finalQuestPrefab; // Prefab à activer pour la dernière quête
    public Transform finalQuestSpawnPoint; // Point où spawner le prefab (optionnel)

    
    [Header("End Game Configuration")]
    public GameObject gameOverUI; // UI à afficher pour Game Over
    public GameObject victoryUI; // UI à afficher pour la victoire
    public AudioClip gameOverSound; // Son de Game Over
    public AudioClip victorySound; // Son de victoire
    public string gameOverSoundPath = "Audio/GameOver"; // Chemin Resources pour Game Over
    public string victorySoundPath = "Audio/Victory";
// Variables privées à ajouter
    private GameObject spawnedFinalQuestObject;
    private int finalQuestIndex = 10;
    private LightManager lightManager;
    private HorrorEvents horrorEvents; 
    private GameObject spawnedDoll; 
    private int dollQuestIndex = 4; 
    private int dollSoundQuestIndex = 5;
    private int policeQuest1Index = 7; 
    private int phoneWorkingQuestIndex = 8; 
    private int policeQuest2Index = 9; // 
    
    private Transform player;
    private bool isMonitoringPhoneDistance = false;
    private bool hasPhoneRung = false;
    private float lastPhoneCheck = 0f;

    void Start()
    {
        lightManager = FindObjectOfType<LightManager>();
        horrorEvents = FindObjectOfType<HorrorEvents>();
        player = FindObjectOfType<CharacterMovement>()?.transform; 
        SetupAudioSources();
        LoadAudioClips();
        InitializeQuests();
        CheckNyctophobiaQuest();
        AfficherToutesLesQuetes();
        
     
        if (phoneTransform == null)
        {
            GameObject phoneObject = GameObject.FindWithTag("Phone"); 
            if (phoneObject != null)
            {
                phoneTransform = phoneObject.transform;
                Debug.Log(" Téléphone trouvé automatiquement");
            }
        }
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

       
        if (isMonitoringPhoneDistance && !hasPhoneRung)
        {
            CheckPhoneDistance();
        }
    }

    void SetupAudioSources()
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
        
      
        if (phoneAudioSource == null)
        {
            phoneAudioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[GameManager] AudioSource téléphone créé automatiquement");
        }
        phoneAudioSource.playOnAwake = false;
        phoneAudioSource.volume = phoneRingVolume;
        phoneAudioSource.loop = true; 
    }

    void LoadAudioClips()
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
        
      
        if (phoneRingClip == null && !string.IsNullOrEmpty(phoneRingResourcePath))
        {
            phoneRingClip = Resources.Load<AudioClip>(phoneRingResourcePath);
            if (phoneRingClip != null)
            {
                Debug.Log($"[GameManager] Audio téléphone chargé depuis Resources: {phoneRingResourcePath}");
            }
            else
            {
                Debug.LogWarning($"[GameManager] Impossible de charger l'audio téléphone à: Resources/{phoneRingResourcePath}");
            }
        }
    }
    public void CheckEndGameCondition()
    {
        Debug.Log("🎯 Vérification des conditions de fin de jeu...");
    
   
        bool policeQuestCompleted = quests[policeQuest2Index].isCompleted;
    
        if (policeQuestCompleted)
        {
            Debug.Log("📞 La police a été appelée - GAME OVER !");
            TriggerGameOver();
        }
        else
        {
            Debug.Log("🎉 Vous avez réussi à vous échapper sans appeler la police - VICTOIRE !");
            TriggerVictory();
        }
    }

    private void TriggerGameOver()
    {
        Debug.Log("💀 GAME OVER - Vous avez appelé la police mais il était trop tard...");
    
   
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            Debug.Log("🖥️ UI Game Over activée");
        }
    
       
    }


    private void TriggerVictory()
    {
        Debug.Log("🏆 VICTOIRE - Vous avez réussi à vous échapper à temps !");
    
       
        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
            Debug.Log("🖥️ UI Victoire activée");
        }
        
    }

    void CheckPhoneDistance()
    {
        if (player == null || phoneTransform == null) return;
        
      
        if (Time.time - lastPhoneCheck < phoneCheckInterval) return;
        lastPhoneCheck = Time.time;
        
        float distanceToPhone = Vector3.Distance(player.position, phoneTransform.position);
        
        Debug.Log($" Distance au téléphone: {distanceToPhone:F1}m (Seuil: {phoneRingDistance}m)");
        
        if (distanceToPhone > phoneRingDistance && !hasPhoneRung)
        {
            StartPhoneRinging();
        }
    }

    void StartPhoneRinging()
    {
        if (hasPhoneRung) return;
        
        hasPhoneRung = true;
        isMonitoringPhoneDistance = false; 
        
        Debug.Log(" Le téléphone sonne ! Distance suffisante atteinte.");
        
      
        if (phoneAudioSource != null && phoneRingClip != null)
        {
            phoneAudioSource.clip = phoneRingClip;
            phoneAudioSource.Play();
            Debug.Log(" Sonnerie du téléphone lancée");
        }
        
       
        if (phoneWorkingQuestIndex < quests.Count && !quests[phoneWorkingQuestIndex].isCompleted)
        {
            quests[phoneWorkingQuestIndex].CompleteQuest();
            AfficherToutesLesQuetes();
            Debug.Log("✅ Quête 'Trouver un moyen de faire fonctionner le téléphone' complétée automatiquement !");
        }
    }

    public void StopPhoneRinging()
    {
        if (phoneAudioSource != null && phoneAudioSource.isPlaying)
        {
            phoneAudioSource.Stop();
            Debug.Log(" Sonnerie du téléphone arrêtée");
        }
    }

    void InitializeQuests()
    {
        quests.Add(new Quest("Trouver un moyen d'allumer les lumières", "Explorer la maison pour rétablir l'électricité."));
        quests.Add(new Quest("Trouver la clé de la chambre principale", "Chercher dans les pièces accessibles."));
        quests.Add(new Quest("Trouver le journal du propriétaire", "Il pourrait contenir des indices."));
        quests.Add(new Quest("Trouver le coffre-fort", "Localiser l'endroit où il est caché."));
        quests.Add(new Quest("Récupérer la clé", "Elle est probablement dans le coffre-fort.")); 
        quests.Add(new Quest("D'où vient ce bruit ?", "Je dois récupérer cette poupée.")); 
        quests.Add(new Quest("Trouver la porte secrète", "Quelque chose cloche dans cette maison."));
        quests.Add(new Quest("Appeler la police1", "Il faut de l'aide immédiatement."));
        quests.Add(new Quest("Trouver un moyen de faire fonctionner le téléphone", "Il est hors-service."));
        quests.Add(new Quest("Appeler la police", "Il faut de l'aide immédiatement."));
        quests.Add(new Quest("Il faut sortir. Maintenant !", "Quitte la maison au plus vite !"));
    }

    void CheckNyctophobiaQuest()
    {
        if (horrorEvents != null && horrorEvents.currentPhobia == PhobiaType.Nyctophobie)
        {
            Debug.Log(" Nyctophobie détectée - La quête des lumières est automatiquement passée");
            
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
            Debug.Log(" Claustrophobie détectée - Activation de l'effet 'Closed'");
            
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
        
        Debug.Log(" Activation de l'effet claustrophobie pendant 5 secondes");
        closedGameObject.SetActive(true);
        yield return new WaitForSeconds(claustrophobiaActiveDuration);
        closedGameObject.SetActive(false);
        Debug.Log(" Effet claustrophobie terminé - 'Closed' désactivé");
    }

  void CompleterQuete(int index)
{
    if (index >= 0 && index < quests.Count && !quests[index].isCompleted)
    {
       
        if (!CanCompleteQuest(index))
        {
            if (index == finalQuestIndex)
            {
                Debug.LogWarning($"⚠️ Impossible de compléter la quête finale. Vous devez d'abord compléter 'Appeler la police1' !");
            }
            else
            {
                Debug.LogWarning($"⚠️ Impossible de compléter la quête {index + 1}. Vous devez d'abord compléter les quêtes précédentes !");
            }
            ShowQuestProgression(index);
            return;
        }

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

        if (index == policeQuest1Index)
        {
            Debug.Log("📞 Quête 'Appeler la police1' complétée - Début de la surveillance du téléphone");
            isMonitoringPhoneDistance = true;
            hasPhoneRung = false;
            
            
            ActivateFinalQuestPrefab();
        }

        if (index == policeQuest2Index)
        {
            Debug.Log("📞 Quête 'Appeler la police' complétée - Arrêt de la sonnerie du téléphone");
            StopPhoneRinging();
            isMonitoringPhoneDistance = false;
        }

      
        if (index == finalQuestIndex)
        {
            Debug.Log("🏁 Quête finale complétée - Vérification des conditions de fin...");
            CheckEndGameCondition();
        }

       
        if (index == 0 && lightManager != null)
        {
            if (horrorEvents != null && horrorEvents.currentPhobia == PhobiaType.Nyctophobie)
            {
                Debug.Log("🌑 Nyctophobie active - Les lumières restent éteintes");
            }
            else
            {
                lightManager.SetAllLights(true);
                Debug.Log("💡 Lumières allumées");
            }
        }
    }
    else if (index >= 0 && index < quests.Count && quests[index].isCompleted)
    {
        Debug.Log($"✅ La quête {index + 1} est déjà complétée !");
    }
}

    
 
    
    bool CanCompleteQuest(int questIndex)
    {
      
        
        if (questIndex == 0) return true;
    
        
        
        if (questIndex == finalQuestIndex) 
            
        {
            return quests[policeQuest1Index].isCompleted; 
            
        }
    
       
        
        for (int i = 0; i < questIndex; i++)
        {
            if (!quests[i].isCompleted)
            {
                return false;
            }
        }

        return true;
    }

 
    void ShowQuestProgression(int attemptedIndex)
    {
        Debug.Log($"📋 Progression des quêtes jusqu'à la quête {attemptedIndex + 1} :");
        
        for (int i = 0; i <= attemptedIndex; i++)
        {
            string status = quests[i].isCompleted ? "✅" : "❌";
            string indicator = "";
            
            if (!quests[i].isCompleted)
            {
             
                if (i == 0 || (i > 0 && quests[i - 1].isCompleted))
                {
                    indicator = " ← PROCHAINE QUÊTE";
                }
                else
                {
                    indicator = " (bloquée)";
                }
            }
            
            Debug.Log($"   {i + 1}. {status} {quests[i].questName}{indicator}");
        }
    }
    
    public void CompleteQuestByName(string questName)
    {
        Debug.Log($"🎯 Tentative de complétion de la quête : {questName}");
    
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].questName == questName && !quests[i].isCompleted)
            {
                
                if (!CanCompleteQuest(i))
                {
                    if (i == finalQuestIndex)
                    {
                        Debug.LogWarning($"⚠ Impossible de compléter '{questName}'. Vous devez d'abord compléter 'Appeler la police1' !");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠ Impossible de compléter '{questName}'. Progression séquentielle requise !");
                        ShowQuestProgression(i);
                    }
                    return;
                }
            
                Debug.Log($"✅ Quête trouvée et complétée : {questName}");
                CompleterQuete(i);
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
        Debug.Log($" Poupée spawnée pour être récupérée à la prochaine quête");
    }

    void PlayDollAudio()
    {
        if (audioSource == null || dollAudioClip == null)
        {
            Debug.LogWarning("[GameManager] AudioSource ou AudioClip manquant pour la poupée");
            return;
        }

        audioSource.clip = dollAudioClip;
        audioSource.volume = dollAudioVolume;
        audioSource.Play();
        Debug.Log(" Son de la poupée joué");
    }

    void StopDollAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log(" Son de la poupée arrêté");
        }
    }

    public void RemoveDoll()
    {
        if (spawnedDoll != null)
        {
            Destroy(spawnedDoll);
            spawnedDoll = null;
            Debug.Log(" Poupée supprimée");
        }
    }
void ActivateFinalQuestPrefab()
{
    Debug.Log(" Activation du prefab pour la quête finale - Il faut sortir !");
    
    
    if (finalQuestPrefab != null)
    {
        finalQuestPrefab.SetActive(true);
        Debug.Log(" Prefab final activé dans la scène");
        return;
    }
    
    
}


public void RemoveFinalQuestPrefab()
{
    if (spawnedFinalQuestObject != null)
    {
        Destroy(spawnedFinalQuestObject);
        spawnedFinalQuestObject = null;
        Debug.Log("🗑️ Prefab final supprimé");
    }
    
    if (finalQuestPrefab != null)
    {
        finalQuestPrefab.SetActive(false);
        Debug.Log("🗑️ Prefab final désactivé");
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
        string phoneIndicator = (i == policeQuest1Index) ? " 📞" : "";
        
   
        string keyIndicator;
        if (i == 10)
        {
            keyIndicator = "[Touche L]";
        }
        else if (i < 9)
        {
            keyIndicator = $"[Touche {i + 1}]";
        }
        else if (i == 9)
        {
            keyIndicator = "[Touche 0]";
        }
        else
        {
            keyIndicator = $"[Touche {i + 1}]";
        }
        
        
        string availabilityIndicator = "";
        if (!quests[i].isCompleted)
        {
           
            if (i == 10) 
            {
                if (CanCompleteFinalQuest())
                {
                    availabilityIndicator = "  DISPONIBLE (Sortie d'urgence)";
                }
                else
                {
                    availabilityIndicator = "  VERROUILLÉE (Complétez 'Appeler la police1' d'abord)";
                }
            }
            else
            { 
                if (CanCompleteQuest(i))
                {
                    availabilityIndicator = "  DISPONIBLE";
                }
                else
                {
                    availabilityIndicator = "  VERROUILLÉE";
                }
            }
        }
        
        
        string finalQuestIndicator = (i == 10) ? " 🏃‍♂️" : "";
        
        Debug.Log($"{keyIndicator} {status} {quests[i].questName}{dollIndicator}{phoneIndicator}{finalQuestIndicator}{availabilityIndicator} - {quests[i].description}");
    }
}
    private bool CanCompleteFinalQuest()
    {
        
        return quests[policeQuest1Index].isCompleted;
    }
    void OnDestroy()
    {
        if (spawnedDoll != null)
        {
            Destroy(spawnedDoll);
        }
    
        if (spawnedFinalQuestObject != null)
        {
            Destroy(spawnedFinalQuestObject);
        }
    
        StopPhoneRinging();
    }
}