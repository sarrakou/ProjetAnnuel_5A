using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExciteOMeter;

/*
* Enhanced SimpleExperimentManager with integrated phobia detection API.
* This script manages the experiment flow and coordinates with the PhobiasDetectionAPI
* to collect heart rate data and detect phobias during the test scenarios.
*/

public class SimpleExperimentManager : MonoBehaviour
{
    public enum ExperimentState
    {
        NotRunning,
        Training,
        Baseline,
        RestBetweenStages,
        InExperiment,
    }

    [Header("Setup experiment times")]
    public float baselineTimeSeconds = 30.0f;
    public float restingTimeSeconds = 20.0f;
    public float experimentalStageSeconds = 10.0f;

    [Header("Setup stages")]
    public GameObject trainingGameObject;
    public List<GameObject> experimentStages;
    public bool randomizeStages = true;

    [Header("Setup Materials")]
    public Material restingSkybox;
    public Material experimentSkybox;

    [Header("UI setup")]
    public TextMeshProUGUI remainingStagesText;
    public TextMeshProUGUI currentStateText;
    public TextMeshProUGUI phobiaResultsText; // New: Display phobia results

    public GameObject buttonForceStopExperiment;
    public GameObject popupMessages;
    public TextMeshProUGUI popupMessageText;
    public GameObject popupForceEndOfExperiment;

    [Header("Phobia Detection")]
    [SerializeField] private bool enablePhobiaDetection = true;
    [SerializeField] private bool showPhobiaDebugInfo = false;

    // Phobia mapping - maps experiment stage index to phobia type
    // IMPORTANT: Make sure you have 4 GameObjects in the experimentStages list!
    private Dictionary<int, PhobiaDetectionAPI.PhobiaType> stagePhobiaMap = new Dictionary<int, PhobiaDetectionAPI.PhobiaType>()
    {
        { 0, PhobiaDetectionAPI.PhobiaType.Entomophobia },   // Stage 0: Insects
        { 1, PhobiaDetectionAPI.PhobiaType.Nyctophobia },    // Stage 1: Darkness
        { 2, PhobiaDetectionAPI.PhobiaType.Scopophobia },    // Stage 2: Being watched
        { 3, PhobiaDetectionAPI.PhobiaType.Claustrophobia }  // Stage 3: Confined spaces
    };

    // Keeps track of which experimental stages have been finished in this session
    private GameObject currentExperimentalStage;
    private List<int> remainingPositions = new List<int>();
    int playingPositionIdx = 0;

    private bool stageLoadedAndPlaying = false; // Useful for loading assets asynchronously

    private ExperimentState currentState = ExperimentState.NotRunning;

    public ExperimentState CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
            if (currentStateText != null)
                currentStateText.text = CurrentState.ToString();

            // Show/Hide buttons
            if (buttonForceStopExperiment != null)
            {
                buttonForceStopExperiment.SetActive(experimentRunning);
            }
        }
    }

    private bool stopExperimentWasForced = false;
    public bool experimentRunning
    {
        get { return CurrentState != ExperimentState.NotRunning && CurrentState != ExperimentState.Training; }
    }

    public static SimpleExperimentManager instance;

    /// <summary>
    /// Singleton
    /// </summary>
    private void Awake()
    {
        // Check singleton, each time the menu scene is loaded, the instance is replaced with the newest script
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        if (popupMessages != null)
            popupMessages.SetActive(false);

        if (popupForceEndOfExperiment != null)
            popupForceEndOfExperiment.SetActive(false);

        CurrentState = ExperimentState.NotRunning;
        ConfigureDefaultEnvironment();
        UpdateExperimentalStagesCount();
        UpdatePhobiaResultsDisplay();

        Debug.Log($"[ExperimentManager] Setup complete: {experimentStages.Count} stages configured");
        for (int i = 0; i < experimentStages.Count; i++)
        {
            if (stagePhobiaMap.ContainsKey(i))
            {
                Debug.Log($"Stage {i}: {experimentStages[i]?.name} -> {stagePhobiaMap[i]}");
            }
        }
    }

    void OnEnable()
    {
        EoM_Events.OnLoggingStateChanged += OnLoggingStateChanged;
    }

    public void OnDisable()
    {
        ConfigureDefaultEnvironment();
        CurrentState = ExperimentState.NotRunning;

        EoM_Events.OnLoggingStateChanged -= OnLoggingStateChanged;
    }

    public void StartStopTrainingStage()
    {
        if (trainingGameObject == null)
        {
            Debug.LogWarning("No training game object was set.");
            return;
        }

        // In case training video was set...
        if (CurrentState == ExperimentState.NotRunning)
        {
            CurrentState = ExperimentState.Training;

            // Setup training
            currentExperimentalStage = trainingGameObject;
            OnLoadStageCompleted();
        }
        else if (CurrentState == ExperimentState.Training)
        {
            // Force early stop of player
            ExperimentStageHasEnded();
        }
        else
        {
            ShowPopupMessage("Experiment running, not possible to play training video");
        }
    }

    public void StartExperiment()
    {
        if (CurrentState == ExperimentState.NotRunning)
        {
            Debug.Log("New experiment started...");

            // Reset phobia detection results
            if (enablePhobiaDetection && PhobiaDetectionAPI.instance != null)
            {
                PhobiaDetectionAPI.instance.ResetResults();
            }

            ExciteOMeterManager.TriggerMarker("ExperimentStarted", MarkerLabel.CUSTOM_MARKER);

            for (int i = 0; i < experimentStages.Count; i++)
            {
                remainingPositions.Add(i);
            }

            // Setup flags
            stageLoadedAndPlaying = false;
            stopExperimentWasForced = false;

            // Setup environment for default state
            ConfigureDefaultEnvironment();

            // Wait baseline time before starting videos
            CurrentState = ExperimentState.Baseline;

            // Start baseline heart rate collection
            if (enablePhobiaDetection && PhobiaDetectionAPI.instance != null)
            {
                PhobiaDetectionAPI.instance.StartBaselineCollection();
                ShowPopupMessage($"Collecting baseline heart rate data for {baselineTimeSeconds} seconds...");
            }

            // Update Video Count
            UpdateExperimentalStagesCount();

            StartCoroutine(WaitingTime(baselineTimeSeconds, OnBaselineCompleted));
        }
        else
        {
            Debug.LogWarning("Experiment already running");
            ShowPopupMessage("Experiment already running.");
        }
    }

    /// <summary>
    /// Called when baseline collection period ends
    /// </summary>
    private void OnBaselineCompleted()
    {
        // End baseline collection
        if (enablePhobiaDetection && PhobiaDetectionAPI.instance != null)
        {
            PhobiaDetectionAPI.instance.EndBaselineCollection();
        }

        // Start the first experimental stage
        LoadAndPlayExperimentalStage();
    }

    private void UpdateExperimentalStagesCount()
    {
        // Show Text
        if (remainingStagesText != null)
            remainingStagesText.text = (experimentStages.Count - remainingPositions.Count).ToString() + "/" + experimentStages.Count.ToString();
    }

    private void UpdatePhobiaResultsDisplay()
    {
        if (phobiaResultsText == null || !enablePhobiaDetection || PhobiaDetectionAPI.instance == null)
            return;

        if (showPhobiaDebugInfo)
        {
            phobiaResultsText.text = PhobiaDetectionAPI.instance.GetDebugInfo();
        }
        else
        {
            var detectedPhobias = PhobiaDetectionAPI.instance.GetDetectedPhobias();
            if (detectedPhobias.Count > 0)
            {
                phobiaResultsText.text = "Detected Phobias:\n" + string.Join(", ", detectedPhobias);
            }
            else
            {
                phobiaResultsText.text = "No phobias detected yet...";
            }
        }
    }

    private void EndExperiment()
    {
        Debug.Log("End of the experiment");
        ExciteOMeterManager.TriggerMarker("ExperimentEnded", MarkerLabel.CUSTOM_MARKER);
        ConfigureDefaultEnvironment();
        CurrentState = ExperimentState.NotRunning;
        LoggerController.instance.StopLogSession();

        // Display final phobia results
        if (enablePhobiaDetection && PhobiaDetectionAPI.instance != null)
        {
            UpdatePhobiaResultsDisplay();
            var results = PhobiaDetectionAPI.instance.GetAllResults();

            string resultMessage = "Experiment completed!\n\nPhobia Detection Results:\n";
            foreach (var result in results.Values)
            {
                string status = result.hasPhobia ? "DETECTED" : "Not detected";
                resultMessage += $"{result.phobiaType}: {status} (Confidence: {result.confidenceScore:F2})\n";
            }

            ShowPopupMessage(resultMessage);
        }

        if (!stopExperimentWasForced && popupMessages != null)
        {
            // Show popup with message of successful end of session
            // this is not shown if experimenter forces end of session
            if (!enablePhobiaDetection)
            {
                ShowPopupMessage("The experiment has finished successfully. Data can be accessed in the offline analysis.");
            }
        }
    }

    public void Update()
    {
        // Update phobia results display periodically
        if (enablePhobiaDetection && Time.frameCount % 60 == 0) // Update once per second approximately
        {
            UpdatePhobiaResultsDisplay();
        }
    }

    void ConfigureDefaultEnvironment()
    {
        //// In case the skybox changes between running and 
        /// not running (like in videos360), or putting back
        /// the objects in their initial transforms...
        RenderSettings.skybox = restingSkybox;

        if (trainingGameObject != null) trainingGameObject.SetActive(false);
        foreach (GameObject go in experimentStages)
            if (go != null) go.SetActive(false);
    }

    public GameObject GetExperimentalStage()
    {
        // Take a random index from the remaining videos

        // All videos have been watched
        if (remainingPositions.Count == 0)
        {
            return null;
        }

        // Extract a new random unseen video from the list
        if (randomizeStages)
            playingPositionIdx = remainingPositions[UnityEngine.Random.Range(0, remainingPositions.Count)];
        else
            playingPositionIdx = remainingPositions[0]; // Sequential stages

        remainingPositions.Remove(playingPositionIdx);

        // Show text of video count
        // Update Video Count
        UpdateExperimentalStagesCount();

        string _str = "[";
        for (int i = 0; i < remainingPositions.Count; i++)
        {
            _str += " " + remainingPositions[i].ToString() + ",";
        }

        Debug.Log("Random stage " + playingPositionIdx + ": Remaining stages " + remainingPositions.Count + " = " + _str);

        return experimentStages[playingPositionIdx];
    }

    public void LoadAndPlayExperimentalStage()
    {
        // Get an experimental stage from the ones that are available.
        currentExperimentalStage = GetExperimentalStage();

        if (currentExperimentalStage == null)
        {
            // Notify End Session
            EndExperiment();
            return;
        }

        // If any stage is remaining, execute it.
        // Use asyncload if there are large files such as videoclips from resources folder
        OnLoadStageCompleted();
    }

    void OnLoadStageCompleted()
    {
        // Setup flags
        stageLoadedAndPlaying = true;

        // Actions specific for the stage, playing a video, etc.
        ActivateExperimentalStage();

        //Notify Start of Video // If not in training video
        if (CurrentState != ExperimentState.Training)
            NotifyStartExperimentStage();
        else
            ShowPopupMessage("Training stage has started!");
    }

    void ActivateExperimentalStage()
    {
        // The experiment is forced after a fixed amount of time.
        // This might not be needed, for example when playing videos, because
        // each stage has different length.
        StartCoroutine(WaitingTime(experimentalStageSeconds, DeactivateExperimentalStage));

        // Activating experimental objects
        RenderSettings.skybox = experimentSkybox;
        currentExperimentalStage.SetActive(true);
    }

    void DeactivateExperimentalStage()
    {
        if (stageLoadedAndPlaying)
        {
            // Deactivate experimental objects
            currentExperimentalStage.SetActive(false);

            // Setup for the rest of the logic
            ExperimentStageHasEnded();
        }
    }

    void ExperimentStageHasEnded()
    {
        stageLoadedAndPlaying = false;

        // NOTIFY END OF VIDEO // If not in training video
        if (CurrentState != ExperimentState.Training)
            NotifyEndExperimentStage();
        else
            NotifyEndTrainingStage();
    }

    private void NotifyStartExperimentStage()
    {
        CurrentState = ExperimentState.InExperiment;
        Debug.Log("Experimental stage has started");

        // Start heart-rate-based phobia test
        if (enablePhobiaDetection && PhobiaDetectionAPI.instance != null && stagePhobiaMap.ContainsKey(playingPositionIdx))
        {
            var phobiaType = stagePhobiaMap[playingPositionIdx];
            PhobiaDetectionAPI.instance.StartPhobiaTest(phobiaType);

            if (showPhobiaDebugInfo)
                Debug.Log($"[ExperimentManager] Starting phobia test for {phobiaType}");
        }

        ExciteOMeterManager.TriggerMarker("ExperimentalStageStarted:" + playingPositionIdx.ToString(), MarkerLabel.CUSTOM_MARKER);
    }

    private void NotifyEndExperimentStage()
    {
        Debug.Log("Experimental stage has ended");

        // End phobia test
        if (enablePhobiaDetection && PhobiaDetectionAPI.instance != null)
        {
            PhobiaDetectionAPI.instance.EndPhobiaTest();

            // Show current results if in debug mode
            if (showPhobiaDebugInfo && stagePhobiaMap.ContainsKey(playingPositionIdx))
            {
                var phobiaType = stagePhobiaMap[playingPositionIdx];
                var result = PhobiaDetectionAPI.instance.GetPhobiaResult(phobiaType);
                Debug.Log($"[ExperimentManager] {phobiaType} test result: {(result.hasPhobia ? "DETECTED" : "Not detected")} " +
                         $"(Confidence: {result.confidenceScore:F2}, HR Increase: {result.averageHeartRateIncrease:F1}%)");
            }
        }

        ExciteOMeterManager.TriggerMarker("ExperimentalStageEnded:" + playingPositionIdx.ToString(), MarkerLabel.CUSTOM_MARKER);

        // Go to resting stage if there are upcoming stages
        if (remainingPositions.Count == 0)
        {
            // Skip resting stage
            ConfigureDefaultEnvironment();
            // Check loading remaining stages and end it!
            LoadAndPlayExperimentalStage();
        }
        else
        {
            // Interim resting stage
            RestingStage();
        }
    }

    void NotifyEndTrainingStage()
    {
        trainingGameObject.SetActive(false);

        ConfigureDefaultEnvironment();

        // Go back to beginning of experiment
        CurrentState = ExperimentState.NotRunning;
        ShowPopupMessage("Training stage was stopped...");
    }

    void RestingStage()
    {
        CurrentState = ExperimentState.RestBetweenStages;
        ConfigureDefaultEnvironment();
        StartCoroutine(WaitingTime(restingTimeSeconds, LoadAndPlayExperimentalStage));
    }

    IEnumerator WaitingTime(float time, Action action)
    {
        yield return new WaitForSecondsRealtime(time);

        // Execute action
        action();
        yield return null;
    }

    ////////////////////////
    // UI Button to control experiment
    ////////////////////////

    public void OnLoggingStateChanged(bool isStartingExperiment)
    {
        if (isStartingExperiment)
        {
            // Start Experiment
            StartExperiment();
        }
        else
        {
            // Stopped Experiment
        }
    }

    public void ForceStopExperiment()
    {
        if (experimentRunning && popupForceEndOfExperiment != null)
        {
            // Show warning
            popupForceEndOfExperiment.SetActive(true);
        }
    }

    public void ForceStopExperimentConfirmed()
    {
        stopExperimentWasForced = true;

        Debug.Log("Experiment was forced to stop");

        ExperimentStageHasEnded();

        StopAllCoroutines();

        // End this experiment
        EndExperiment();
    }

    // UI button to skip watching the whole video (For testing only)
    public void SkipCurrentExperimentStage()
    {
        if (CurrentState == ExperimentState.InExperiment)
            ExperimentStageHasEnded();
        else
            ShowPopupMessage("Not in an experimental stage right now.\n Current stage = " + CurrentState.ToString());
    }

    void ShowPopupMessage(string text)
    {
        popupMessageText.text = text;
        popupMessages.SetActive(true);
    }

    ////////////////////////
    // Public API for accessing phobia results
    ////////////////////////

    /// <summary>
    /// Get the detected phobias for use in the main game
    /// </summary>
    public List<PhobiaDetectionAPI.PhobiaType> GetDetectedPhobias()
    {
        if (enablePhobiaDetection && PhobiaDetectionAPI.instance != null)
        {
            return PhobiaDetectionAPI.instance.GetDetectedPhobias();
        }
        return new List<PhobiaDetectionAPI.PhobiaType>();
    }

    /// <summary>
    /// Check if a specific phobia was detected
    /// </summary>
    public bool HasPhobia(PhobiaDetectionAPI.PhobiaType phobiaType)
    {
        if (enablePhobiaDetection && PhobiaDetectionAPI.instance != null)
        {
            var result = PhobiaDetectionAPI.instance.GetPhobiaResult(phobiaType);
            return result.hasPhobia && result.confidenceScore > 0.5f;
        }
        return false;
    }

    /// <summary>
    /// Get detailed result for a specific phobia
    /// </summary>
    public PhobiaDetectionAPI.PhobiaResult GetPhobiaResult(PhobiaDetectionAPI.PhobiaType phobiaType)
    {
        if (enablePhobiaDetection && PhobiaDetectionAPI.instance != null)
        {
            return PhobiaDetectionAPI.instance.GetPhobiaResult(phobiaType);
        }
        return null;
    }
}