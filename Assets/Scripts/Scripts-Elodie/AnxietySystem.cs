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
    public float baseBreathingRate = 12f; // Respirations par minute au repos (normal)
    public float breathingAccelerationThreshold = 90f; // Seuil où la respiration accélère
    public float maxBreathingRate = 30f; // Respirations par minute maximum (stress)
    public float breathingVolume = 0.4f; // Volume constant de la respiration
    
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

        // Setup audio sources
        SetupAudioSources();
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

        // Calculer la fréquence de respiration actuelle
        float currentBreathingRate = baseBreathingRate; // Commencer par la fréquence de base
        
        // Si le rythme cardiaque dépasse le seuil, accélérer la respiration
        if (currentHeartRate > breathingAccelerationThreshold)
        {
            float accelerationFactor = (currentHeartRate - breathingAccelerationThreshold) / (200f - breathingAccelerationThreshold);
            accelerationFactor = Mathf.Clamp01(accelerationFactor);
            currentBreathingRate = Mathf.Lerp(baseBreathingRate, maxBreathingRate, accelerationFactor);
        }
        
        // Calculer l'intervalle de respiration
        float breathingInterval = 60f / currentBreathingRate; // Intervalle en secondes
        
        // Vérifier si c'est le moment de jouer une respiration
        if (Time.time - lastBreathingTime >= breathingInterval)
        {
            lastBreathingTime = Time.time;
            
            breathingAudioSource.volume = breathingVolume;
            breathingAudioSource.clip = breathingClip;
            breathingAudioSource.pitch = 0.8f; // Pitch plus bas pour ralentir le son
            
            breathingAudioSource.Play();
            
            Debug.Log($"Playing breathing - HR: {currentHeartRate}, Rate: {currentBreathingRate}/min, Interval: {breathingInterval}s");
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

        // Update target heart rate every second
        if (heartRateUpdateTimer >= heartRateUpdateInterval)
        {
            heartRateUpdateTimer = 0f;

            // Base heart rate around 70 BPM
            float baseHeartRate = 70f;

            // Every 10 seconds, spike to high heart rate
            if (simulationTimer % 10f < 2f) // High for 4 seconds every 10 seconds
            {
                targetHeartRate = baseHeartRate + 40f + Random.Range(-3f, 3f); // Spike to ~110 BPM with slight variation
            }
            else
            {
                targetHeartRate = baseHeartRate + Random.Range(-5f, 5f); // Normal variation
            }
        }

        // Smoothly interpolate current heart rate towards target
        float lerpSpeed = 2f; // Adjust this to control how fast the heart rate changes
        currentHeartRate = Mathf.Lerp(currentHeartRate, targetHeartRate, Time.deltaTime * lerpSpeed);

        breathingRate = currentHeartRate / 4f;

        heartRateText.text = $"{Mathf.RoundToInt(currentHeartRate)} BPM";
        breathingRateText.text = $"{Mathf.RoundToInt(breathingRate)} Breaths/Min";
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