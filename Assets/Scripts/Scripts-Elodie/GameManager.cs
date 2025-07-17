using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Quest Configuration")]
    public List<Quest> quests = new List<Quest>();
    
    [Header("Doll Event Configuration")]
    public string dollPrefabResourcePath = "annabelle"; 
    public AudioClip dollAudioClip; 
    public string dollAudioResourcePath = "Audios/BabyCrying"; 
    
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
    [Header("Black Screen Configuration")]
    public float blackScreenFadeDuration = 1f; // Durée du fade vers le noir
    public GameObject blackScreenUI; // UI Panel noir (optionnel)
    [Header("End Camera Configuration")]
    public GameObject characterPrefab; // Le prefab du joueur à désactiver
    public string victoryPrefabPath = "cercueil"; // Chemin du prefab cercueil dans Resources
    public float victoryDezoomHeight = 20f; // Hauteur finale de la caméra
    public float victoryDezoomDuration = 3f; // Durée de l'animation
    public AnimationCurve dezoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Courbe d'animation

    private GameObject spawnedVictoryPrefab;
    private Camera victoryCamera;
    [Header("Victory Game Configuration")]
    public GameObject gameOverUI; // UI à afficher pour Game Over
    public GameObject victoryUI; // UI à afficher pour la victoire


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
        Debug.Log("💀 DÉFAITE - Vous avez appelé la police mais il était trop tard...");

        FinalReportSaver reportSaver = FindObjectOfType<FinalReportSaver>();
        if (reportSaver != null)
        {
            reportSaver.SaveFinalReport();
            Debug.Log("💾 Reporte final guardado (victoria)");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró FinalReportSaver para guardar reporte");
        }
        AnxietySystem anxietyManager = FindObjectOfType<AnxietySystem>();
        if (anxietyManager != null)
        {
            anxietyManager.enabled = false;
            Debug.Log("💔 AnxietySystem désactivé");
        }
    
 
    GameObject victoryPrefab = Resources.Load<GameObject>(victoryPrefabPath);
    if (victoryPrefab != null)
    {
        Vector3 spawnPosition = player != null ? player.position : Vector3.zero;
        spawnedVictoryPrefab = Instantiate(victoryPrefab, spawnPosition, Quaternion.identity);
        
       
        Light victoryLight = spawnedVictoryPrefab.GetComponentInChildren<Light>();
        if (victoryLight != null)
        {
            victoryLight.enabled = true;
            Debug.Log("💡 Lumière du cercueil activée");
        }
        
   
        victoryCamera = spawnedVictoryPrefab.GetComponentInChildren<Camera>();
        
        if (victoryCamera != null)
        {

            victoryCamera.enabled = true;
            
          
            Camera playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
            if (playerCamera != null && playerCamera != victoryCamera)
            {
                playerCamera.enabled = false;
                Debug.Log("📹 Caméra du joueur désactivée");
            }
            
           
            if (characterPrefab != null)
            {
                characterPrefab.SetActive(false);
                Debug.Log("🚶‍♂️ Prefab joueur désactivé");
            }
            else
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerObj.SetActive(false);
                    Debug.Log("🚶‍♂️ Joueur trouvé par tag et désactivé");
                }
            }
            
          
            StartCoroutine(DefeatCameraRise());
            Debug.Log($"📹 Animation démarrée");
        }
    }
    
       
    }

private void TriggerVictory() // VICTOIRE - avec fade vers le noir
{
    Debug.Log("🏆 VICTOIRE - Vous avez réussi à vous échapper à temps !");

    FinalReportSaver reportSaver = FindObjectOfType<FinalReportSaver>();
    if (reportSaver != null)
    {
        reportSaver.SaveFinalReport();
        Debug.Log("💾 Reporte final guardado (victoria)");
    }
    else
    {
        Debug.LogWarning("⚠️ No se encontró FinalReportSaver para guardar reporte");
    }

        AnxietySystem anxietyManager = FindObjectOfType<AnxietySystem>();
    if (anxietyManager != null)
    {
        anxietyManager.enabled = false;
        Debug.Log("💔 AnxietySystem désactivé");
    }
    

    if (characterPrefab != null)
    {
        characterPrefab.SetActive(false);
        Debug.Log("🚶‍♂️ Prefab joueur désactivé");
    }
    else
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerObj.SetActive(false);
            Debug.Log("🚶‍♂️ Joueur trouvé par tag et désactivé");
        }
    }
    
  
    StartCoroutine(VictoryFadeToBlack());
}

private System.Collections.IEnumerator VictoryFadeToBlack()
{
  
    if (blackScreenUI != null)
    {
        blackScreenUI.SetActive(true);
        
        UnityEngine.UI.Image blackImage = blackScreenUI.GetComponent<UnityEngine.UI.Image>();
        if (blackImage != null)
        {
            Color startColor = blackImage.color;
            startColor.a = 0f;
            blackImage.color = startColor;
            
            float elapsedTime = 0f;
            while (elapsedTime < blackScreenFadeDuration)
            {
                float alpha = elapsedTime / blackScreenFadeDuration;
                Color newColor = startColor;
                newColor.a = alpha;
                blackImage.color = newColor;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            startColor.a = 1f;
            blackImage.color = startColor;
        }
        
        Debug.Log("⚫ Écran noir activé pour la victoire");
    }
    else
    {
        yield return StartCoroutine(CreateAndFadeBlackScreen());
    }
    
  
    yield return new WaitForSeconds(2f);
    

    CreateVictoryText();
        yield return new WaitForSeconds(2f); // o el tiempo que quieras

        // Cargar escena reporte
        SceneManager.LoadScene("ReportFinal");
    }

private void CreateVictoryText()
{
    Debug.Log("🎉 Création du texte VICTOIRE!");
    
    // Créer un Canvas pour le texte de victoire
    GameObject victoryCanvas = new GameObject("VictoryTextCanvas");
    Canvas canvas = victoryCanvas.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = 1100;
    
    victoryCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    

    GameObject textObj = new GameObject("VictoryText");
    textObj.transform.SetParent(victoryCanvas.transform, false);
    
    UnityEngine.UI.Text victoryText = textObj.AddComponent<UnityEngine.UI.Text>();
    victoryText.text = "VICTOIRE!";
    victoryText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    victoryText.fontSize = 72;
    victoryText.color = Color.white;
    victoryText.alignment = TextAnchor.MiddleCenter;
    
    RectTransform textRect = victoryText.rectTransform;
    textRect.anchorMin = Vector2.zero;
    textRect.anchorMax = Vector2.one;
    textRect.offsetMin = Vector2.zero;
    textRect.offsetMax = Vector2.zero;
    
    
    StartCoroutine(FadeInText(victoryText));
    
    Debug.Log("✅ Texte VICTOIRE! créé et fade démarré");
}

private void ActivateVictoryUI()
{
    if (victoryUI != null)
    {
        try
        {
           
            victoryUI.SetActive(true);
            
          
            Transform current = victoryUI.transform;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    current.gameObject.SetActive(true);
                    Debug.Log($"🔧 Activé: {current.name}");
                }
                current = current.parent;
            }
            
            Debug.Log("✅ UI Victoire activée avec succès");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Erreur lors de l'activation de l'UI: {e.Message}");
            CreateEmergencyVictoryUI();
        }
    }
    else
    {
        Debug.LogWarning("⚠️ victoryUI est null, création automatique...");
        CreateEmergencyVictoryUI();
    }
}

private void CreateEmergencyVictoryUI()
{
    Debug.Log("🚨 Création d'un UI de victoire d'urgence");
    
  
    GameObject canvasObj = new GameObject("VictoryCanvas");
    Canvas canvas = canvasObj.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = 100;
    
    canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    
 
    GameObject panelObj = new GameObject("VictoryPanel");
    panelObj.transform.SetParent(canvasObj.transform, false);
    
    UnityEngine.UI.Image panelImage = panelObj.AddComponent<UnityEngine.UI.Image>();
    panelImage.color = new Color(0, 0, 0, 0.7f);
    
    RectTransform panelRect = panelImage.rectTransform;
    panelRect.anchorMin = Vector2.zero;
    panelRect.anchorMax = Vector2.one;
    panelRect.offsetMin = Vector2.zero;
    panelRect.offsetMax = Vector2.zero;
    
    
    GameObject textObj = new GameObject("VictoryText");
    textObj.transform.SetParent(panelObj.transform, false);
    
    UnityEngine.UI.Text victoryText = textObj.AddComponent<UnityEngine.UI.Text>();
    victoryText.text = "VICTOIRE !";
    victoryText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    victoryText.fontSize = 72;
    victoryText.color = Color.white;
    victoryText.alignment = TextAnchor.MiddleCenter;
    
    RectTransform textRect = victoryText.rectTransform;
    textRect.anchorMin = Vector2.zero;
    textRect.anchorMax = Vector2.one;
    textRect.offsetMin = Vector2.zero;
    textRect.offsetMax = Vector2.zero;
    
    Debug.Log("✅ UI de victoire d'urgence créé");
}
private System.Collections.IEnumerator DefeatCameraRise()
{
    if (victoryCamera == null) yield break;
    
  
    Vector3 startPosition = victoryCamera.transform.position;
    Vector3 endPosition = new Vector3(startPosition.x, startPosition.y + victoryDezoomHeight, startPosition.z);
    
    Debug.Log($"🎬 Début de la montée de la caméra de {startPosition.y:F1} vers {endPosition.y:F1}");
    
    float elapsedTime = 0f;
    bool fadeStarted = false;
    
    while (elapsedTime < victoryDezoomDuration)
    {
        float progress = elapsedTime / victoryDezoomDuration;
        float curveValue = dezoomCurve.Evaluate(progress);
        
       
        Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, curveValue);
        victoryCamera.transform.position = currentPosition;
        
      if (!fadeStarted && progress >= 0.3f)
        {
            fadeStarted = true;
            Debug.Log("🎬 Début du fade vers le noir (30% de la montée)");
            StartCoroutine(FadeToBlack());
        }
        
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    
    
    victoryCamera.transform.position = endPosition;
    
    Debug.Log("🎬 Montée de la caméra terminée");
    
   
    if (!fadeStarted)
    {
        StartCoroutine(FadeToBlack());
    }
        yield return new WaitForSeconds(5f);

        SceneManager.LoadScene("ReportFinal");
    }

public void CleanupVictoryPrefab()
{
    if (spawnedVictoryPrefab != null)
    {
        Destroy(spawnedVictoryPrefab);
        spawnedVictoryPrefab = null;
        victoryCamera = null;
        Debug.Log("🗑️ Prefab cercueil supprimé");
    }
}
private System.Collections.IEnumerator FadeToBlack()
{
    if (blackScreenUI != null)
    {
        blackScreenUI.SetActive(true);
        
        UnityEngine.UI.Image blackImage = blackScreenUI.GetComponent<UnityEngine.UI.Image>();
        if (blackImage != null)
        {
            Color startColor = blackImage.color;
            startColor.a = 0f;
            blackImage.color = startColor;
            
            float elapsedTime = 0f;
            while (elapsedTime < blackScreenFadeDuration)
            {
                float alpha = elapsedTime / blackScreenFadeDuration;
                Color newColor = startColor;
                newColor.a = alpha;
                blackImage.color = newColor;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            startColor.a = 1f;
            blackImage.color = startColor;
        }
        
        Debug.Log("⚫ Écran noir activé");
    }
    else
    {
        yield return StartCoroutine(CreateAndFadeBlackScreen());
    }
    
    
    yield return new WaitForSeconds(5f);
    
   
    if (spawnedVictoryPrefab != null)
    {
        UnityEngine.UI.Text victoryText = spawnedVictoryPrefab.GetComponentInChildren<UnityEngine.UI.Text>();
        if (victoryText != null)
        {
            Debug.Log("📝 Texte trouvé dans le prefab, début du fade in");
            yield return StartCoroutine(FadeInText(victoryText));
        }
        else
        {
            Debug.LogWarning("⚠️ Aucun texte trouvé dans le prefab cercueil");
        }
    }
 
    if (victoryUI != null)
    {
        victoryUI.SetActive(true);
        Debug.Log("✅ UI Victoire de fallback activée");
    }
}

private System.Collections.IEnumerator FadeInText(UnityEngine.UI.Text textComponent)
{
  
    textComponent.gameObject.SetActive(true);
    

    Color textColor = textComponent.color;
    textColor.a = 0f;
    textComponent.color = textColor;
    
    Debug.Log($"📝 Fade in du texte: '{textComponent.text}'");
    
    float fadeDuration = 2f; 
    float elapsedTime = 0f;
    
    while (elapsedTime < fadeDuration)
    {
        float alpha = elapsedTime / fadeDuration;
        Color newColor = textColor;
        newColor.a = alpha;
        textComponent.color = newColor;
        
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    
    
    textColor.a = 1f;
    textComponent.color = textColor;
    
    Debug.Log("✅ Fade in du texte terminé");
}
private System.Collections.IEnumerator CreateAndFadeBlackScreen()
{
   
    GameObject blackScreenCanvas = new GameObject("BlackScreenCanvas");
    Canvas canvas = blackScreenCanvas.AddComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.sortingOrder = 1000; 
    
    blackScreenCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    
    
    GameObject blackPanel = new GameObject("BlackPanel");
    blackPanel.transform.SetParent(blackScreenCanvas.transform, false);
    
    UnityEngine.UI.Image blackImage = blackPanel.AddComponent<UnityEngine.UI.Image>();
    blackImage.color = new Color(0, 0, 0, 0);
    
    
    RectTransform rectTransform = blackImage.rectTransform;
    rectTransform.anchorMin = Vector2.zero;
    rectTransform.anchorMax = Vector2.one;
    rectTransform.offsetMin = Vector2.zero;
    rectTransform.offsetMax = Vector2.zero;
    
   
    float elapsedTime = 0f;
    while (elapsedTime < blackScreenFadeDuration)
    {
        float alpha = elapsedTime / blackScreenFadeDuration;
        blackImage.color = new Color(0, 0, 0, alpha);
        
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    
   
    blackImage.color = Color.black;
    
    Debug.Log("⚫ Écran noir créé et fade terminé");
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