using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FinalReportDisplay : MonoBehaviour
{
    [System.Serializable]
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

    [Header("Élément UI")]
    public TextMeshProUGUI reportText;

    [Header("Fichier JSON")]
    public string fileName = "final_game_report.json";

    void Start()
    {
        Debug.Log("Ruta de Application.persistentDataPath: " + Application.persistentDataPath);
        string path = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            FinalReport report = JsonUtility.FromJson<FinalReport>(json);
            AfficherRapport(report);
        }
        else
        {
            if (reportText != null)
                reportText.text = "Aucun rapport final trouvé.";
            Debug.LogWarning($"[FinalReportDisplayCondensed_FR] Fichier introuvable : {path}");
        }
    }

    void AfficherRapport(FinalReport r)
    {
        if (reportText == null) return;

        reportText.text =
            $"<b>Phobie détectée :</b> \n{r.phobiaDetected}\n\n" +
            $"<b>Confiance :</b> \n{r.phobiaConfidence:F2}\n\n" +
            $"<b>% de peur relative :</b> \n{r.phobiaPercentage:F1}%\n\n" +
            $"<b>Fréquence max pendant le test :</b> \n{r.maxHeartRateDuringTest} BPM\n\n" +
            $"<b>Fréquence max pendant le jeu :</b> \n{r.maxHeartRateDuringGame} BPM\n\n" +
            $"<b>Niveau moyen d’anxiété :</b> \n{r.averageAnxietyLevel:F2}\n\n" +
            $"<b>Temps total de jeu :</b> \n{r.totalPlayTimeSeconds:F0} s\n\n";
    }
    public void backToMainMenu()
    {
        Debug.Log("Finish");
        SceneManager.LoadScene("MainMenu");
    }
}
