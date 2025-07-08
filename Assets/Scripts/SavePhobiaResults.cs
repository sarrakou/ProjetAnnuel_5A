using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SavePhobiaResults : MonoBehaviour
{
    [System.Serializable]
    public class SerializablePhobiaResult
    {
        public string phobiaType;
        public bool hasPhobia;
        public float confidenceScore;
        public float averageHeartRateIncrease;
        public float maxHeartRateIncrease;
        public bool heartRateIncreased; // true si pulso subió, false si bajó
        public float phobiaPercentage;  // porcentaje relativo basado en cambio absoluto ponderado
    }

    [System.Serializable]
    public class ResultsContainer
    {
        public List<SerializablePhobiaResult> results = new List<SerializablePhobiaResult>();
    }

    public static void SaveResultsToJson()
    {
        if (PhobiaDetectionAPI.instance == null)
        {
            Debug.LogError("[SavePhobiaResults] PhobiaDetectionAPI.instance no está inicializado.");
            return;
        }

        var resultsDict = PhobiaDetectionAPI.instance.GetAllResults();

        // Calculamos la suma total de cambios ponderados (valor absoluto * confianza)
        float totalWeightedChange = resultsDict.Values.Sum(r => Mathf.Abs(r.averageHeartRateIncrease) * r.confidenceScore);

        var container = new ResultsContainer();

        foreach (var entry in resultsDict)
        {
            var result = entry.Value;
            float weightedChange = Mathf.Abs(result.averageHeartRateIncrease) * result.confidenceScore;

            var serializableResult = new SerializablePhobiaResult
            {
                phobiaType = result.phobiaType.ToString(),
                hasPhobia = result.hasPhobia,
                confidenceScore = result.confidenceScore,
                averageHeartRateIncrease = result.averageHeartRateIncrease,
                maxHeartRateIncrease = result.maxHeartRateIncrease,
                heartRateIncreased = result.averageHeartRateIncrease >= 0,
                phobiaPercentage = totalWeightedChange > 0 ? (weightedChange / totalWeightedChange) * 100f : 0f
            };

            container.results.Add(serializableResult);
        }

        string json = JsonUtility.ToJson(container, true);
        string filePath = Path.Combine(Application.persistentDataPath, "phobia_results.json");

        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"[SavePhobiaResults] Resultados guardados en: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SavePhobiaResults] Error al guardar archivo JSON: {e.Message}");
        }
    }
}
