using ExciteOMeter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeartRateReader : MonoBehaviour
{
    public static HeartRateReader Instance { get; private set; }

    [Header("Réglages")]
    public bool useSimulation = true;
    public TextMeshProUGUI heartRateText;

    [Header("Fréquence cardiaque")]
    public float currentHeartRate = 70f;

    private float targetHeartRate;
    private float simulationTimer = 0f;
    private float heartRateUpdateTimer = 0f;
    private float heartRateUpdateInterval = 1f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (useSimulation)
            SimulateHeartRate();
    }

    private void SimulateHeartRate()
    {
        simulationTimer += Time.deltaTime;
        heartRateUpdateTimer += Time.deltaTime;

        if (heartRateUpdateTimer >= heartRateUpdateInterval)
        {
            heartRateUpdateTimer = 0f;

            float baseHeartRate = 70f;

            if (simulationTimer % 10f < 2f)
            {
                targetHeartRate = baseHeartRate + 40f + Random.Range(-3f, 3f); // pic de stress
            }
            else
            {
                targetHeartRate = baseHeartRate + Random.Range(-5f, 5f);
            }
        }

        float lerpSpeed = 2f;
        currentHeartRate = Mathf.Lerp(currentHeartRate, targetHeartRate, Time.deltaTime * lerpSpeed);

        if (heartRateText != null)
            heartRateText.text = $"{Mathf.RoundToInt(currentHeartRate)} BPM";
    }

    // Permet d'écouter les vraies données (si utilisées plus tard)
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
        }
    }
}
