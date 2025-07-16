using ExciteOMeter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnxietySystem : MonoBehaviour
{
    [Header("Anxiety Settings")]
    public float anxiety = 0f;
    public float maxAnxiety = 100f;
    public float increaseRate = 2f;

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
    }

    void Update()
    {
        anxiety += increaseRate * Time.deltaTime;
        anxiety = Mathf.Clamp(anxiety, 0f, maxAnxiety);

        if (useSimulation)
        {
            SimulateHeartRate();
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
