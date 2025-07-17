using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalReportSaver : MonoBehaviour
{
    [Serializable]
    public class FinalReport
    {
        public string phobiaDetected;
        public float phobiaConfidence;
        public float maxHeartRateDuringTest;
        public float maxHeartRateDuringGame;
        public float averageAnxietyLevel;
        public float totalPlayTimeSeconds;
        public string peakHeartRateEvent;
        public float phobiaPercentage;
    }

    public string gameSceneName = "MainGameHorror";  
    public string fileName = "final_game_report.json";

    private float gameStartTime;
    private float maxHeartRateInGame = 0f;
    private AnxietySystem anxietySystem;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        gameStartTime = Time.time;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            // Lanzar corrutina para buscar AnxietySystem
            StartCoroutine(FindAnxietySystemCoroutine());
        }
        else
        {
            anxietySystem = null;
        }
    }

    private IEnumerator FindAnxietySystemCoroutine()
    {
        float timeout = 5f;  // tiempo máximo para buscar el AnxietySystem
        float timer = 0f;

        while (anxietySystem == null && timer < timeout)
        {
            anxietySystem = FindObjectOfType<AnxietySystem>();
            if (anxietySystem != null)
            {
                Debug.Log("[FinalReportSaver] AnxietySystem encontrado.");
                yield break;
            }
            timer += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        if (anxietySystem == null)
        {
            Debug.LogWarning("[FinalReportSaver] No se encontró AnxietySystem después de esperar.");
        }
    }

    private void Update()
    {
        if (anxietySystem != null && anxietySystem.heartRateText != null)
        {
            if (float.TryParse(anxietySystem.heartRateText.text.Replace("BPM", "").Trim(), out float currentHR))
            {
                if (currentHR > maxHeartRateInGame)
                {
                    maxHeartRateInGame = currentHR;
                }
            }
        }
    }

    public void SaveFinalReport()
    {
        var report = new FinalReport();

        var results = PhobiaDetectionAPI.instance.GetAllResults();
        var sorted = results.Values.OrderByDescending(r => r.confidenceScore).ToList();

        if (sorted.Count > 0)
        {
            var main = sorted[0];
            report.phobiaDetected = main.phobiaType.ToString();
            report.phobiaConfidence = main.confidenceScore;
            report.maxHeartRateDuringTest = main.maxHeartRate;
        }
        else
        {
            report.phobiaDetected = "None";
            report.phobiaConfidence = 0f;
            report.maxHeartRateDuringTest = 0f;
        }

        report.maxHeartRateDuringGame = maxHeartRateInGame;

        if (anxietySystem != null)
        {
            report.averageAnxietyLevel = anxietySystem.anxiety;
        }

        report.totalPlayTimeSeconds = Time.time - gameStartTime;


        string path = Path.Combine(Application.persistentDataPath, "phobia_results.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SavePhobiaResults.ResultsContainer loaded = JsonUtility.FromJson<SavePhobiaResults.ResultsContainer>(json);
            var match = loaded.results.FirstOrDefault(r => r.phobiaType == report.phobiaDetected);
            if (match != null)
            {
                report.phobiaPercentage = match.phobiaPercentage;
            }
        }

        string finalJson = JsonUtility.ToJson(report, true);
        string finalPath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(finalPath, finalJson);

        Debug.Log($"[FinalReportSaver] Reporte final guardado en: {finalPath}");
    }

    private string GetCurrentEventLabel()
    {
        return "Evento desconocido";
    }
}
