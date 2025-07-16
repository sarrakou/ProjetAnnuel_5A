using ExciteOMeter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnxietySystem : MonoBehaviour
{
    [Header("Anxiety Settings")]
    public float anxiety = 0f;
    public float maxAnxiety = 100f;
    public float baseIncreaseRate = 0.5f; // Augmentation de base plus lente
    public float acceleratedIncreaseRate = 4f; // Augmentation rapide quand le rythme est élevé
    public float heartRateThreshold = 90f; // Seuil à partir duquel l'anxiété monte plus vite

    [Header("Camera Shake Settings")]
    public Camera playerCamera;
    public float maxShakeAmount = 0.1f; 
    private Vector3 originalLocalPos;
    private Vector3 targetShakeOffset;
    private Vector3 currentShakeOffset;

    [Header("Screen Pulse Settings")]
    public Image screenPulseOverlay; // Image UI qui couvre tout l'écran
    public float pulseStartHeartRate = 80f; // Rythme cardiaque où les pulsions commencent
    public float maxPulseOpacity = 0.3f; // Opacité maximum de la pulsion
    public Color pulseColor = Color.red; // Couleur de la pulsion
    private float lastPulseTime = 0f;
    private bool isPulsing = false;
    private float pulseTimer = 0f;
    private float pulseDuration = 0.3f; // Durée d'une pulsion
    
    [Header("Light Flicker Settings")]
    public float lightFlickerStartHeartRate = 85f; // Rythme cardiaque où les lumières commencent à vaciller
    public float maxFlickerIntensity = 0.4f; // Intensité maximum du vacillement (0-1)
    public float flickerSpeed = 2f; // Vitesse du vacillement
    public bool autoFindLights = true; // Trouve automatiquement toutes les lumières
    public Light[] customLights; // Lumières personnalisées à affecter
    private Light[] allLights;
    private float[] originalLightIntensities;
    private bool lightsInitialized = false;
    
    [Header("Pill Settings")]
    public float pillEffectAmount = 100f; 
    public string pillItemName = "pillule"; 
    private Inventory inventory;

    [Header("Critical Danger Settings")]
    public float criticalHeartRate = 150f; // Seuil de danger critique
    public GameObject criticalDangerPanel; // Panel noir avec le message
    public TMP_Text criticalDangerText; // Texte du message
    public float normalHeartRate = 110f; // Seuil pour revenir à la normale
    private bool isInCriticalDanger = false;
    private bool gameWasPaused = false;

    [Header("Audio Settings")]
    public AudioSource heartbeatAudioSource; // Source audio pour les battements
    public AudioClip heartbeatClip; // Son d'un battement de cœur
    public float heartbeatVolumeThreshold = 85f; // Seuil où les battements deviennent audibles
    public float maxHeartbeatVolume = 0.8f; // Volume maximum des battements
    
    public AudioSource breathingAudioSource; // Source audio pour la respiration
    public AudioClip breathingClip; // Son de respiration
    public float baseBreathingRate = 8f; // Respirations par minute au repos (plus lent)
    public float breathingAccelerationThreshold = 80f; // Seuil où la respiration commence à accélérer
    public float maxBreathingRate = 25f; // Respirations par minute maximum (stress)
    public float baseBreathingVolume = 0.05f; // Volume de base très discret
    public float maxBreathingVolume = 0.25f; // Volume maximum sous stress
    public float breathingPitchMin = 0.6f; // Pitch minimum (plus grave au repos)
    public float breathingPitchMax = 1.0f; // Pitch maximum (plus aigu sous stress)
    
    private float lastHeartbeatTime = 0f;
    private float lastBreathingTime = 0f;

    public CharacterMovement characterMovement;

    public TMP_Text heartRateText;
    public TMP_Text breathingRateText;
    private float currentHeartRate = 0f;
    private float targetHeartRate = 70f;
    private float breathingRate = 0f;

    public bool useSimulation = false; 

    private float simulationTimer = 0f;
    private float heartRateUpdateTimer = 0f;
    private float heartRateUpdateInterval = 2f; 

    void Start()
    {
        heartRateText.text = "0 BPM";
        breathingRateText.text = "0 Breaths/Min";

        if (playerCamera != null)
            originalLocalPos = playerCamera.transform.localPosition;
        else
            Debug.LogError("playerCamera n'est pas assignée !");

        inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
            Debug.LogError("Inventaire non trouvé !");

        // Initialize heart rate values
        currentHeartRate = 70f;
        targetHeartRate = 70f;

        // Initialize critical danger UI
        if (criticalDangerPanel != null)
            criticalDangerPanel.SetActive(false);
        
        if (criticalDangerText != null)
            criticalDangerText.text = "Le jeu reprendra lorsque votre rythme sera revenu à la normale";

        // Initialize screen pulse overlay
        SetupScreenPulseOverlay();

        // Initialize light system
        SetupLightFlicker();

        // Setup audio sources
        SetupAudioSources();
    }

    void SetupScreenPulseOverlay()
    {
        if (screenPulseOverlay == null)
        {
            // Créer automatiquement l'overlay de pulsion
            CreateScreenPulseOverlay();
        }
        
        if (screenPulseOverlay != null)
        {
            // Configurer l'overlay de pulsion
            screenPulseOverlay.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 0f);
            screenPulseOverlay.raycastTarget = false; // Pour éviter qu'il interfère avec les autres UI
        }
    }
    
    void CreateScreenPulseOverlay()
    {
        // Trouver ou créer un Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("PulseCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // S'assurer qu'il est au-dessus de tout
            
            // Ajouter CanvasScaler et GraphicRaycaster
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Créer l'GameObject pour l'overlay
        GameObject overlayObj = new GameObject("ScreenPulseOverlay");
        overlayObj.transform.SetParent(canvas.transform, false);
        
        // Ajouter le composant Image
        screenPulseOverlay = overlayObj.AddComponent<Image>();
        
        // Configurer le RectTransform pour couvrir tout l'écran
        RectTransform rectTransform = screenPulseOverlay.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Créer une texture simple pour l'overlay
        Texture2D overlayTexture = new Texture2D(1, 1);
        overlayTexture.SetPixel(0, 0, Color.white);
        overlayTexture.Apply();
        
        // Créer un sprite à partir de la texture
        Sprite overlaySprite = Sprite.Create(overlayTexture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
        screenPulseOverlay.sprite = overlaySprite;
        
        // S'assurer que l'overlay est au bon endroit dans la hiérarchie
        // (juste avant le panel de danger critique s'il existe)
        if (criticalDangerPanel != null && criticalDangerPanel.transform.parent == canvas.transform)
        {
            overlayObj.transform.SetSiblingIndex(criticalDangerPanel.transform.GetSiblingIndex());
        }
        
        Debug.Log("Overlay de pulsion créé automatiquement !");
    }

    void SetupLightFlicker()
    {
        if (autoFindLights)
        {
            // Trouver automatiquement toutes les lumières dans la scène
            allLights = FindObjectsOfType<Light>();
            Debug.Log($"Trouvé {allLights.Length} lumières dans la scène pour le vacillement");
        }
        else if (customLights != null && customLights.Length > 0)
        {
            // Utiliser les lumières personnalisées
            allLights = customLights;
            Debug.Log($"Utilisation de {allLights.Length} lumières personnalisées pour le vacillement");
        }
        else
        {
            Debug.LogWarning("Aucune lumière trouvée pour le système de vacillement !");
            return;
        }

        // Sauvegarder les intensités originales
        if (allLights != null && allLights.Length > 0)
        {
            originalLightIntensities = new float[allLights.Length];
            for (int i = 0; i < allLights.Length; i++)
            {
                if (allLights[i] != null)
                {
                    originalLightIntensities[i] = allLights[i].intensity;
                }
            }
            lightsInitialized = true;
        }
    }

    private void UpdateLightFlicker()
    {
        if (!lightsInitialized || allLights == null || allLights.Length == 0) return;

        // Calculer l'intensité du vacillement basée sur le rythme cardiaque
        float flickerIntensity = 0f;
        
        if (currentHeartRate > lightFlickerStartHeartRate)
        {
            // Calculer l'intensité progressive
            flickerIntensity = (currentHeartRate - lightFlickerStartHeartRate) / (200f - lightFlickerStartHeartRate);
            flickerIntensity = Mathf.Clamp01(flickerIntensity) * maxFlickerIntensity;
        }

        // Appliquer le vacillement à toutes les lumières
        for (int i = 0; i < allLights.Length; i++)
        {
            if (allLights[i] != null && i < originalLightIntensities.Length)
            {
                if (flickerIntensity > 0f)
                {
                    // Créer un pattern de vacillement unique pour chaque lumière
                    float timeOffset = i * 0.5f; // Décalage pour que toutes les lumières ne vacillent pas en même temps
                    float flickerPattern = Mathf.PerlinNoise(Time.time * flickerSpeed + timeOffset, 0f);
                    
                    // Synchroniser partiellement avec le rythme cardiaque
                    float heartbeatSync = Mathf.Sin(Time.time * (currentHeartRate / 60f) * Mathf.PI * 2f + timeOffset);
                    heartbeatSync = (heartbeatSync + 1f) * 0.5f; // Normaliser entre 0 et 1
                    
                    // Mélanger le pattern aléatoire et la synchronisation cardiaque
                    float combinedPattern = Mathf.Lerp(flickerPattern, heartbeatSync, 0.3f);
                    
                    // Calculer la variation d'intensité
                    float intensityVariation = (combinedPattern - 0.5f) * 2f * flickerIntensity;
                    
                    // Appliquer la nouvelle intensité
                    float newIntensity = originalLightIntensities[i] + (originalLightIntensities[i] * intensityVariation);
                    newIntensity = Mathf.Max(0f, newIntensity); // Éviter les intensités négatives
                    
                    allLights[i].intensity = newIntensity;
                }
                else
                {
                    // Restaurer l'intensité originale quand le rythme cardiaque est normal
                    allLights[i].intensity = Mathf.Lerp(allLights[i].intensity, originalLightIntensities[i], Time.deltaTime * 2f);
                }
            }
        }
    }

    void SetupAudioSources()
    {
        // Setup heartbeat audio source
        if (heartbeatAudioSource == null)
        {
            GameObject heartbeatObj = new GameObject("HeartbeatAudio");
            heartbeatObj.transform.SetParent(transform);
            heartbeatAudioSource = heartbeatObj.AddComponent<AudioSource>();
        }
        
        heartbeatAudioSource.playOnAwake = false;
        heartbeatAudioSource.loop = false;
        heartbeatAudioSource.volume = 0f;
        heartbeatAudioSource.clip = heartbeatClip;

        // Setup breathing audio source
        if (breathingAudioSource == null)
        {
            GameObject breathingObj = new GameObject("BreathingAudio");
            breathingObj.transform.SetParent(transform);
            breathingAudioSource = breathingObj.AddComponent<AudioSource>();
        }
        
        breathingAudioSource.playOnAwake = false;
        breathingAudioSource.loop = false;
        breathingAudioSource.volume = 0f;
    }

    void Update()
    {
        // Calculer le taux d'augmentation de l'anxiété en fonction du rythme cardiaque
        float currentIncreaseRate = CalculateAnxietyIncreaseRate();
        
        anxiety += currentIncreaseRate * Time.deltaTime;
        anxiety = Mathf.Clamp(anxiety, 0f, maxAnxiety);

        if (useSimulation)
        {
            SimulateHeartRate();
        }

        // Gérer les sons audio
        UpdateAudioEffects();

        // Gérer les pulsions d'écran
        UpdateScreenPulse();

        // Gérer le vacillement des lumières
        UpdateLightFlicker();

        // Vérifier le danger critique
        CheckCriticalDanger();

        // Si on est en danger critique, on arrête le reste du gameplay
        if (isInCriticalDanger)
        {
            return;
        }

        if (anxiety > 70f)
        {
            if (characterMovement != null)
                characterMovement.SetCameraRepositioning(false);

            ApplyCameraShake();
        }
        else
        {
            ResetCameraEffects();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TryTakePill();
        }
    }

    private void UpdateScreenPulse()
    {
        if (screenPulseOverlay == null) return;

        // Calculer l'intervalle entre les pulsions basé sur le rythme cardiaque
        float pulseInterval = 60f / currentHeartRate; // Intervalle en secondes

        // Vérifier si le rythme cardiaque est assez élevé pour déclencher les pulsions
        if (currentHeartRate > pulseStartHeartRate)
        {
            // Déclencher une nouvelle pulsion si l'intervalle est écoulé
            if (Time.time - lastPulseTime >= pulseInterval)
            {
                lastPulseTime = Time.time;
                StartPulse();
            }
        }

        // Animer la pulsion en cours
        if (isPulsing)
        {
            pulseTimer += Time.deltaTime;
            
            // Calculer l'intensité de la pulsion basée sur le rythme cardiaque
            float pulseIntensity = (currentHeartRate - pulseStartHeartRate) / (200f - pulseStartHeartRate);
            pulseIntensity = Mathf.Clamp01(pulseIntensity);
            
            // Utiliser une courbe d'animation pour la pulsion (rapide montée, descente plus lente)
            float pulseProgress = pulseTimer / pulseDuration;
            float pulseAlpha = 0f;
            
            if (pulseProgress < 0.2f) // Montée rapide (20% du temps)
            {
                pulseAlpha = Mathf.Lerp(0f, maxPulseOpacity * pulseIntensity, pulseProgress / 0.2f);
            }
            else // Descente plus lente (80% du temps)
            {
                float fadeProgress = (pulseProgress - 0.2f) / 0.8f;
                pulseAlpha = Mathf.Lerp(maxPulseOpacity * pulseIntensity, 0f, fadeProgress);
            }
            
            // Appliquer la couleur avec l'alpha calculé
            screenPulseOverlay.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, pulseAlpha);
            
            // Arrêter la pulsion quand elle est terminée
            if (pulseTimer >= pulseDuration)
            {
                isPulsing = false;
                pulseTimer = 0f;
                screenPulseOverlay.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 0f);
            }
        }
        else if (currentHeartRate <= pulseStartHeartRate)
        {
            // Assurer que l'overlay est transparent quand le rythme cardiaque est normal
            screenPulseOverlay.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 0f);
        }
    }

    private void StartPulse()
    {
        isPulsing = true;
        pulseTimer = 0f;
    }

    private void UpdateAudioEffects()
    {
        UpdateHeartbeatAudio();
        UpdateBreathingAudio();
    }

    private void UpdateHeartbeatAudio()
    {
        if (heartbeatAudioSource == null || heartbeatClip == null) return;

        // Calculer l'intervalle entre les battements basé sur le BPM
        float heartbeatInterval = 60f / currentHeartRate; // Intervalle en secondes
        
        // Vérifier si c'est le moment de jouer un battement
        if (Time.time - lastHeartbeatTime >= heartbeatInterval)
        {
            lastHeartbeatTime = Time.time;
            
            // Jouer le battement seulement si le rythme dépasse le seuil
            if (currentHeartRate > heartbeatVolumeThreshold)
            {
                // Calculer le volume basé sur le rythme cardiaque
                float volumeIntensity = (currentHeartRate - heartbeatVolumeThreshold) / (200f - heartbeatVolumeThreshold);
                volumeIntensity = Mathf.Clamp01(volumeIntensity);
                
                heartbeatAudioSource.volume = volumeIntensity * maxHeartbeatVolume;
                heartbeatAudioSource.pitch = 1f + (volumeIntensity * 0.3f); // Légère augmentation du pitch
                heartbeatAudioSource.Play();
            }
        }
    }

    private void UpdateBreathingAudio()
    {
        if (breathingAudioSource == null)
        {
            Debug.LogWarning("breathingAudioSource is null!");
            return;
        }
        
        if (breathingClip == null)
        {
            Debug.LogWarning("breathingClip is null! Assigne ton clip audio dans l'inspector.");
            return;
        }

        // Calculer la fréquence de respiration avec progression douce
        float currentBreathingRate = baseBreathingRate; // Commencer par la fréquence de base (plus lente)
        float currentBreathingVolume = baseBreathingVolume; // Volume de base très discret
        float currentBreathingPitch = breathingPitchMin; // Pitch grave au repos
        
        // Accélération progressive dès que le seuil est dépassé
        if (currentHeartRate > breathingAccelerationThreshold)
        {
            // Facteur d'accélération progressif et plus sensible
            float accelerationFactor = (currentHeartRate - breathingAccelerationThreshold) / (180f - breathingAccelerationThreshold);
            accelerationFactor = Mathf.Clamp01(accelerationFactor);
            
            // Courbe d'accélération plus douce au début, plus rapide à la fin
            float smoothAcceleration = Mathf.Pow(accelerationFactor, 1.5f); // Courbe exponentielle douce
            
            // Appliquer l'accélération à tous les paramètres
            currentBreathingRate = Mathf.Lerp(baseBreathingRate, maxBreathingRate, smoothAcceleration);
            currentBreathingVolume = Mathf.Lerp(baseBreathingVolume, maxBreathingVolume, smoothAcceleration);
            currentBreathingPitch = Mathf.Lerp(breathingPitchMin, breathingPitchMax, smoothAcceleration * 0.8f); // Pitch change plus subtil
        }
        
        // Calculer l'intervalle de respiration
        float breathingInterval = 60f / currentBreathingRate; // Intervalle en secondes
        
        // Vérifier si c'est le moment de jouer une respiration
        if (Time.time - lastBreathingTime >= breathingInterval)
        {
            lastBreathingTime = Time.time;
            
            // Appliquer les paramètres calculés
            breathingAudioSource.volume = currentBreathingVolume;
            breathingAudioSource.clip = breathingClip;
            breathingAudioSource.pitch = currentBreathingPitch;
            
            breathingAudioSource.Play();
            
            Debug.Log($"Playing breathing - HR: {currentHeartRate:F1}, Rate: {currentBreathingRate:F1}/min, Volume: {currentBreathingVolume:F2}, Pitch: {currentBreathingPitch:F2}");
        }
    }

    private float CalculateAnxietyIncreaseRate()
    {
        // Si le rythme cardiaque dépasse le seuil, augmentation accélérée
        if (currentHeartRate > heartRateThreshold)
        {
            // Plus le rythme cardiaque est élevé, plus l'anxiété monte vite
            float exceedAmount = currentHeartRate - heartRateThreshold;
            float multiplier = 1f + (exceedAmount / 50f); // Multiplier qui augmente progressivement
            return baseIncreaseRate + (acceleratedIncreaseRate * multiplier);
        }
        else
        {
            // Augmentation normale (plus lente)
            return baseIncreaseRate;
        }
    }

    private void CheckCriticalDanger()
    {
        if (!isInCriticalDanger && currentHeartRate >= criticalHeartRate)
        {
            // Entrer en danger critique
            isInCriticalDanger = true;
            gameWasPaused = Time.timeScale > 0;
            
            // Pause le jeu
            Time.timeScale = 0f;
            
            // Afficher l'écran noir
            if (criticalDangerPanel != null)
                criticalDangerPanel.SetActive(true);
            
            // Désactiver les contrôles du joueur
            if (characterMovement != null)
                characterMovement.enabled = false;
            
            Debug.Log("DANGER CRITIQUE! Rythme cardiaque trop élevé: " + currentHeartRate);
        }
        else if (isInCriticalDanger && currentHeartRate <= normalHeartRate)
        {
            // Sortir du danger critique
            isInCriticalDanger = false;
            
            // Reprendre le jeu si il était en cours
            if (gameWasPaused)
                Time.timeScale = 1f;
            
            // Masquer l'écran noir
            if (criticalDangerPanel != null)
                criticalDangerPanel.SetActive(false);
            
            // Réactiver les contrôles du joueur
            if (characterMovement != null)
                characterMovement.enabled = true;
            
            Debug.Log("Retour à la normale. Rythme cardiaque: " + currentHeartRate);
        }
    }

    private void OnEnable()
    {
        EoM_Events.OnDataReceived += UpdateHeartRateDisplay;
    }

    private void OnDisable()
    {
        EoM_Events.OnDataReceived -= UpdateHeartRateDisplay;
        
        // Réinitialiser tous les effets quand le script est désactivé
        ResetAllEffects();
    }

    private void UpdateHeartRateDisplay(DataType dataType, float timestamp, float value)
    {
        if (!useSimulation && dataType == DataType.HeartRate && value >= 30f && value <= 220f)
        {
            currentHeartRate = value;
            heartRateText.text = $"{Mathf.RoundToInt(currentHeartRate)} BPM";

            breathingRate = currentHeartRate / 4f;
            breathingRateText.text = $"{Mathf.RoundToInt(breathingRate)} Breaths/Min";
        }
    }

    private void SimulateHeartRate()
    {
        simulationTimer += Time.deltaTime;
        heartRateUpdateTimer += Time.deltaTime;

        // Update target heart rate every 3 seconds (plus lent)
        if (heartRateUpdateTimer >= 3f)
        {
            heartRateUpdateTimer = 0f;

            // Base heart rate around 70 BPM
            float baseHeartRate = 70f;

            // Cycle plus long et modéré sur 30 secondes
            float currentCycleTime = simulationTimer % 30f;
            
            if (currentCycleTime < 8f) // Repos prolongé (8 secondes)
            {
                targetHeartRate = baseHeartRate + Random.Range(-3f, 3f); // 67-73 BPM
            }
            else if (currentCycleTime < 15f) // Stress très léger (7 secondes)
            {
                targetHeartRate = baseHeartRate + 15f + Random.Range(-3f, 3f); // 82-88 BPM
            }
            else if (currentCycleTime < 22f) // Stress modéré (7 secondes)
            {
                targetHeartRate = baseHeartRate + 30f + Random.Range(-5f, 5f); // 95-105 BPM
            }
            else if (currentCycleTime < 26f) // Pic de stress (4 secondes)
            {
                targetHeartRate = baseHeartRate + 50f + Random.Range(-5f, 10f); // 115-130 BPM
            }
            else // Retour progressif au calme (4 secondes)
            {
                targetHeartRate = baseHeartRate + Random.Range(-5f, 5f); // 65-75 BPM
            }
            
            // Limites plus conservatrices
            targetHeartRate = Mathf.Clamp(targetHeartRate, 60f, 140f);
        }

        // Transition encore plus lente et douce
        float lerpSpeed = 0.8f; // Beaucoup plus lent pour des transitions très progressives
        currentHeartRate = Mathf.Lerp(currentHeartRate, targetHeartRate, Time.deltaTime * lerpSpeed);

        breathingRate = currentHeartRate / 4f;

        heartRateText.text = $"{Mathf.RoundToInt(currentHeartRate)} BPM";
        breathingRateText.text = $"{Mathf.RoundToInt(breathingRate)} Breaths/Min";
        
        // Debug pour voir les phases
        string currentPhase = "";
        float debugCycleTime = simulationTimer % 30f;
        if (debugCycleTime < 8f) currentPhase = "Repos";
        else if (debugCycleTime < 15f) currentPhase = "Stress Léger";
        else if (debugCycleTime < 22f) currentPhase = "Stress Modéré";
        else if (debugCycleTime < 26f) currentPhase = "Pic de Stress";
        else currentPhase = "Retour au Calme";
        
        if (Time.frameCount % 120 == 0) // Afficher toutes les 2 secondes
        {
            Debug.Log($"Simulation - Phase: {currentPhase}, Current: {currentHeartRate:F1} BPM, Target: {targetHeartRate:F1} BPM");
        }
    }

    void ApplyCameraShake()
    {
        float shakeIntensity = (anxiety - 70f) / (maxAnxiety - 70f);
        shakeIntensity = Mathf.Clamp01(shakeIntensity);

        float shakeAmount = Mathf.Lerp(0.005f, maxShakeAmount, shakeIntensity); 

        targetShakeOffset = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0f
        ) * shakeAmount;

        currentShakeOffset = Vector3.Lerp(currentShakeOffset, targetShakeOffset, Time.deltaTime * 10f);
        playerCamera.transform.localPosition = originalLocalPos + currentShakeOffset;
    }

    void ResetCameraEffects()
    {
        if (playerCamera != null)
            playerCamera.transform.localPosition = originalLocalPos;

        if (characterMovement != null)
            characterMovement.SetCameraRepositioning(true);
    }

    void ResetAllEffects()
    {
        // Réinitialiser les intensités des lumières
        if (lightsInitialized && allLights != null && originalLightIntensities != null)
        {
            for (int i = 0; i < allLights.Length && i < originalLightIntensities.Length; i++)
            {
                if (allLights[i] != null)
                {
                    allLights[i].intensity = originalLightIntensities[i];
                }
            }
        }
    }

    void TryTakePill()
    {
        if (inventory != null && inventory.HasItem(pillItemName))
        {
            anxiety -= pillEffectAmount;
            anxiety = Mathf.Clamp(anxiety, 0f, maxAnxiety);

            inventory.RemoveItem(pillItemName);
            Debug.Log("Pilule utilisée. Anxiété réduite !");
        }
        else
        {
            Debug.Log("Pas de pilule dans l'inventaire !");
        }
    }
}