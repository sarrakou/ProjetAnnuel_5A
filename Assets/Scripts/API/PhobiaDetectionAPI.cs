using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ExciteOMeter;

public class PhobiaDetectionAPI : MonoBehaviour
{
    public enum PhobiaType
    {
        Entomophobia,   // Fear of insects
        Nyctophobia,    // Fear of darkness/night
        Scopophobia,    // Fear of being watched/observed
        Claustrophobia  // Fear of closed/confined spaces
    }

    [System.Serializable]
    public class PhobiaResult
    {
        public PhobiaType phobiaType;
        public bool hasPhobia;
        public float confidenceScore; // 0-1, how confident we are about the result
        public float averageHeartRateIncrease; // Percentage increase from baseline
        public float maxHeartRateIncrease; // Maximum increase observed
    }

    [System.Serializable]
    public class HeartRateData
    {
        public float timestamp;
        public float heartRate;

        public HeartRateData(float time, float hr)
        {
            timestamp = time;
            heartRate = hr;
        }
    }

    [Header("Detection Settings")]
    [SerializeField] private float phobiaThreshold = 15f; // Minimum % increase to consider phobia
    [SerializeField] private float highConfidenceThreshold = 25f; // % increase for high confidence
    [SerializeField] private float sustainedResponseTime = 3f; // Seconds of sustained response needed
    [SerializeField] private int minDataPoints = 5; // Minimum heart rate samples needed

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // Singleton instance
    public static PhobiaDetectionAPI instance;

    // Data storage
    private List<HeartRateData> baselineData = new List<HeartRateData>();
    private Dictionary<PhobiaType, List<HeartRateData>> phobiaTestData = new Dictionary<PhobiaType, List<HeartRateData>>();
    private Dictionary<PhobiaType, PhobiaResult> results = new Dictionary<PhobiaType, PhobiaResult>();

    // Current test tracking
    private PhobiaType? currentlyTesting = null;
    private bool isCollectingBaseline = false;
    private bool isCollectingTestData = false;

    // Baseline stats
    private float baselineAverageHR = 0f;
    private float baselineStandardDeviation = 0f;

    private void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize data structures
        InitializePhobiaData();
    }

    private void OnEnable()
    {
        // Subscribe to heart rate data events
        EoM_Events.OnDataReceived += OnHeartRateDataReceived;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        EoM_Events.OnDataReceived -= OnHeartRateDataReceived;
    }

    private void InitializePhobiaData()
    {
        // Initialize data structures for each phobia type
        foreach (PhobiaType phobiaType in Enum.GetValues(typeof(PhobiaType)))
        {
            phobiaTestData[phobiaType] = new List<HeartRateData>();
            results[phobiaType] = new PhobiaResult
            {
                phobiaType = phobiaType,
                hasPhobia = false,
                confidenceScore = 0f,
                averageHeartRateIncrease = 0f,
                maxHeartRateIncrease = 0f
            };
        }
    }

    #region Public API Methods

    /// <summary>
    /// Start collecting baseline heart rate data
    /// Call this during the baseline period before any phobia tests
    /// </summary>
    public void StartBaselineCollection()
    {
        if (enableDebugLogs)
            Debug.Log("[PhobiaAPI] Starting baseline heart rate collection");

        baselineData.Clear();
        isCollectingBaseline = true;

        ExciteOMeterManager.TriggerMarker("BaselineCollectionStarted", MarkerLabel.CUSTOM_MARKER);
    }

    /// <summary>
    /// Stop collecting baseline data and calculate baseline statistics
    /// </summary>
    public void EndBaselineCollection()
    {
        if (enableDebugLogs)
            Debug.Log("[PhobiaAPI] Ending baseline collection");

        isCollectingBaseline = false;
        CalculateBaselineStatistics();

        ExciteOMeterManager.TriggerMarker("BaselineCollectionEnded", MarkerLabel.CUSTOM_MARKER);
    }

    /// <summary>
    /// Start testing for a specific phobia
    /// </summary>
    public void StartPhobiaTest(PhobiaType phobiaType)
    {
        if (enableDebugLogs)
            Debug.Log($"[PhobiaAPI] Starting {phobiaType} test");

        currentlyTesting = phobiaType;
        isCollectingTestData = true;

        // Clear previous data for this phobia
        phobiaTestData[phobiaType].Clear();

        ExciteOMeterManager.TriggerMarker($"PhobiaTest_{phobiaType}_Started", MarkerLabel.CUSTOM_MARKER);
    }

    /// <summary>
    /// End the current phobia test and analyze the data
    /// </summary>
    public void EndPhobiaTest()
    {
        if (currentlyTesting == null)
        {
            Debug.LogWarning("[PhobiaAPI] No phobia test currently running");
            return;
        }

        if (enableDebugLogs)
            Debug.Log($"[PhobiaAPI] Ending {currentlyTesting} test");

        isCollectingTestData = false;

        // Analyze the collected data
        AnalyzePhobiaData(currentlyTesting.Value);

        ExciteOMeterManager.TriggerMarker($"PhobiaTest_{currentlyTesting}_Ended", MarkerLabel.CUSTOM_MARKER);

        currentlyTesting = null;
    }

    /// <summary>
    /// Get the result for a specific phobia
    /// </summary>
    public PhobiaResult GetPhobiaResult(PhobiaType phobiaType)
    {
        return results[phobiaType];
    }

    /// <summary>
    /// Get all phobia detection results
    /// </summary>
    public Dictionary<PhobiaType, PhobiaResult> GetAllResults()
    {
        return new Dictionary<PhobiaType, PhobiaResult>(results);
    }

    /// <summary>
    /// Get a list of detected phobias (confidence > 0.5)
    /// </summary>
    public List<PhobiaType> GetDetectedPhobias()
    {
        return results.Where(kvp => kvp.Value.hasPhobia && kvp.Value.confidenceScore > 0.5f)
                     .Select(kvp => kvp.Key)
                     .ToList();
    }

    /// <summary>
    /// Reset all collected data and results
    /// </summary>
    public void ResetResults()
    {
        if (enableDebugLogs)
            Debug.Log("[PhobiaAPI] Resetting all results");

        baselineData.Clear();
        currentlyTesting = null;
        isCollectingBaseline = false;
        isCollectingTestData = false;

        foreach (var phobiaType in phobiaTestData.Keys.ToList())
        {
            phobiaTestData[phobiaType].Clear();
        }

        InitializePhobiaData();
    }

    #endregion

    #region Data Collection

    /// <summary>
    /// Called when heart rate data is received from the sensor
    /// </summary>
    private void OnHeartRateDataReceived(DataType dataType, float timestamp, float value)
    {
        // Only process heart rate data
        if (dataType != DataType.HeartRate)
            return;

        // Validate heart rate value (typical range: 30-220 bpm)
        if (value < 30f || value > 220f)
            return;

        var heartRateData = new HeartRateData(timestamp, value);

        // Collect baseline data
        if (isCollectingBaseline)
        {
            baselineData.Add(heartRateData);
        }

        // Collect test data for current phobia
        if (isCollectingTestData && currentlyTesting.HasValue)
        {
            phobiaTestData[currentlyTesting.Value].Add(heartRateData);
        }
    }

    #endregion

    #region Data Analysis

    /// <summary>
    /// Calculate baseline heart rate statistics
    /// </summary>
    private void CalculateBaselineStatistics()
    {
        if (baselineData.Count < minDataPoints)
        {
            Debug.LogWarning($"[PhobiaAPI] Insufficient baseline data: {baselineData.Count} points (minimum {minDataPoints})");
            return;
        }

        // Calculate average
        baselineAverageHR = baselineData.Average(data => data.heartRate);

        // Calculate standard deviation
        float variance = baselineData.Average(data => Mathf.Pow(data.heartRate - baselineAverageHR, 2));
        baselineStandardDeviation = Mathf.Sqrt(variance);

        if (enableDebugLogs)
        {
            Debug.Log($"[PhobiaAPI] Baseline calculated - Average: {baselineAverageHR:F1} bpm, StdDev: {baselineStandardDeviation:F1}");
        }
    }

    /// <summary>
    /// Analyze heart rate data for a specific phobia
    /// </summary>
    private void AnalyzePhobiaData(PhobiaType phobiaType)
    {
        var testData = phobiaTestData[phobiaType];

        if (testData.Count < minDataPoints)
        {
            Debug.LogWarning($"[PhobiaAPI] Insufficient test data for {phobiaType}: {testData.Count} points");
            return;
        }

        if (baselineAverageHR <= 0)
        {
            Debug.LogWarning("[PhobiaAPI] No baseline data available for comparison");
            return;
        }

        // Calculate heart rate statistics during test
        float testAverageHR = testData.Average(data => data.heartRate);
        float maxTestHR = testData.Max(data => data.heartRate);

        // Calculate increases
        float averageIncrease = ((testAverageHR - baselineAverageHR) / baselineAverageHR) * 100f;
        float maxIncrease = ((maxTestHR - baselineAverageHR) / baselineAverageHR) * 100f;

        // Check for sustained elevated response
        float sustainedResponse = CalculateSustainedResponse(testData);

        // Determine if phobia is present
        bool hasPhobia = averageIncrease >= phobiaThreshold || sustainedResponse >= sustainedResponseTime;

        // Calculate confidence score
        float confidenceScore = CalculateConfidenceScore(averageIncrease, maxIncrease, sustainedResponse, testData.Count);

        // Store results
        results[phobiaType] = new PhobiaResult
        {
            phobiaType = phobiaType,
            hasPhobia = hasPhobia,
            confidenceScore = confidenceScore,
            averageHeartRateIncrease = averageIncrease,
            maxHeartRateIncrease = maxIncrease
        };

        if (enableDebugLogs)
        {
            Debug.Log($"[PhobiaAPI] {phobiaType} Analysis - " +
                     $"Avg Increase: {averageIncrease:F1}%, " +
                     $"Max Increase: {maxIncrease:F1}%, " +
                     $"Sustained: {sustainedResponse:F1}s, " +
                     $"Has Phobia: {hasPhobia}, " +
                     $"Confidence: {confidenceScore:F2}");
        }
    }

    /// <summary>
    /// Calculate how long the heart rate was sustained above threshold
    /// </summary>
    private float CalculateSustainedResponse(List<HeartRateData> testData)
    {
        float sustainedTime = 0f;
        float currentSustainedTime = 0f;
        float maxSustainedTime = 0f;

        float elevatedThreshold = baselineAverageHR * (1f + phobiaThreshold / 100f);

        for (int i = 1; i < testData.Count; i++)
        {
            float deltaTime = testData[i].timestamp - testData[i - 1].timestamp;

            if (testData[i].heartRate > elevatedThreshold)
            {
                currentSustainedTime += deltaTime;
            }
            else
            {
                maxSustainedTime = Mathf.Max(maxSustainedTime, currentSustainedTime);
                currentSustainedTime = 0f;
            }
        }

        return Mathf.Max(maxSustainedTime, currentSustainedTime);
    }

    /// <summary>
    /// Calculate confidence score based on multiple factors
    /// </summary>
    private float CalculateConfidenceScore(float avgIncrease, float maxIncrease, float sustainedTime, int dataPoints)
    {
        float confidence = 0f;

        // Factor 1: Average increase magnitude
        if (avgIncrease >= highConfidenceThreshold)
            confidence += 0.4f;
        else if (avgIncrease >= phobiaThreshold)
            confidence += 0.2f * (avgIncrease / phobiaThreshold);

        // Factor 2: Peak response
        if (maxIncrease >= highConfidenceThreshold * 1.5f)
            confidence += 0.3f;
        else if (maxIncrease >= phobiaThreshold)
            confidence += 0.15f * (maxIncrease / (highConfidenceThreshold * 1.5f));

        // Factor 3: Sustained response
        if (sustainedTime >= sustainedResponseTime)
            confidence += 0.2f;
        else
            confidence += 0.1f * (sustainedTime / sustainedResponseTime);

        // Factor 4: Data quality (sufficient data points)
        float dataQuality = Mathf.Min(1f, dataPoints / (minDataPoints * 2f));
        confidence += 0.1f * dataQuality;

        return Mathf.Clamp01(confidence);
    }

    #endregion

    #region Debug and Utility

    /// <summary>
    /// Get debug information about current state
    /// </summary>
    public string GetDebugInfo()
    {
        string info = $"PhobiaAPI Debug Info:\n";
        info += $"Baseline HR: {baselineAverageHR:F1} ± {baselineStandardDeviation:F1} bpm\n";
        info += $"Baseline samples: {baselineData.Count}\n";
        info += $"Currently testing: {currentlyTesting?.ToString() ?? "None"}\n";
        info += $"Collecting baseline: {isCollectingBaseline}\n";
        info += $"Collecting test data: {isCollectingTestData}\n\n";

        info += "Results:\n";
        foreach (var result in results.Values)
        {
            info += $"{result.phobiaType}: {(result.hasPhobia ? "DETECTED" : "Not detected")} " +
                   $"(Confidence: {result.confidenceScore:F2}, Increase: {result.averageHeartRateIncrease:F1}%)\n";
        }

        return info;
    }

    /// <summary>
    /// Manual trigger for testing purposes
    /// </summary>
    [ContextMenu("Print Debug Info")]
    public void PrintDebugInfo()
    {
        Debug.Log(GetDebugInfo());
    }

    #endregion
}