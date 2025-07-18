using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
public class DirectionalSoundEmitter : MonoBehaviour
{
    [Header("Référence au joueur")]
    public Transform player;

    [Header("Clips audio (stress faible / moyen / fort)")]
    public List<AudioClip> calmClips;
    public List<AudioClip> mediumClips;
    public List<AudioClip> intenseClips;

    [Header("Rayon autour du joueur")]
    public float distanceFromPlayer = 8f;

    [Header("Volume & stéréo")]
    public float volume = 1f;
    [Range(0, 1)] public float stereoChance = 0.2f;

    // bornes de BPM pour mapping
    public float minBPM = 60f;
    public float maxBPM = 160f;

    private string lastDir = "";
    private readonly string[] dirs = { "front", "back", "left", "right" };

    // Nouveau système : score par clip audio
    private Dictionary<AudioClip, float> clipFearScores = new();
    private List<AudioClip> allClips = new();

    private bool initialTestDone = false;

    void Start()
    {
        if (player == null) player = Camera.main.transform;

        // Rassembler tous les clips
        allClips.AddRange(calmClips);
        allClips.AddRange(mediumClips);
        allClips.AddRange(intenseClips);

        // Initialiser les scores à 0
        foreach (var clip in allClips)
        {
            if (clip != null)
                clipFearScores[clip] = 0f;
        }

        StartCoroutine(InitialSoundTest());
    }

    IEnumerator InitialSoundTest()
    {
        // Tester chaque clip individuellement
        foreach (var clip in allClips)
        {
            if (clip != null)
            {
                yield return TestSound(clip);
                yield return new WaitForSeconds(2f);
            }
        }

        initialTestDone = true;
        StartCoroutine(PlayLoop());
    }

    IEnumerator TestSound(AudioClip clip)
    {
        float bpmBefore = HeartRateReader.Instance != null ? HeartRateReader.Instance.currentHeartRate : 70f;

        AudioSource.PlayClipAtPoint(clip, player.position, volume);

        yield return new WaitForSeconds(4f); // Attendre réaction

        float bpmAfter = HeartRateReader.Instance != null ? HeartRateReader.Instance.currentHeartRate : bpmBefore;
        float delta = Mathf.Max(0f, bpmAfter - bpmBefore); // On garde que les hausses

        clipFearScores[clip] = delta;
    }

    IEnumerator PlayLoop()
    {
        while (true)
        {
            float bpm = HeartRateReader.Instance != null ? HeartRateReader.Instance.currentHeartRate : 70f;

            // Délai basé sur le BPM
            float t = Mathf.InverseLerp(minBPM, maxBPM, bpm);
            float delay = Mathf.Lerp(8f, 2f, t);
            yield return new WaitForSeconds(delay);

            // Sélection pondérée du clip
            AudioClip selectedClip = ChooseWeightedClip();

            if (selectedClip == null) continue;

            bool stereo = Random.value < stereoChance;
            if (stereo)
            {
                AudioSource.PlayClipAtPoint(selectedClip, player.position, volume);
                StartCoroutine(AnalyzeReactionAfterSound(selectedClip));
                continue;
            }

            string dir;
            do { dir = dirs[Random.Range(0, dirs.Length)]; } while (dir == lastDir);
            lastDir = dir;

            Vector3 offset = dir switch
            {
                "front" => player.forward,
                "back" => -player.forward,
                "left" => -player.right,
                _ => player.right
            };
            Vector3 pos = player.position + offset.normalized * distanceFromPlayer;

            AudioSource.PlayClipAtPoint(selectedClip, pos, volume);
            StartCoroutine(AnalyzeReactionAfterSound(selectedClip));
        }
    }

    IEnumerator AnalyzeReactionAfterSound(AudioClip clip, float waitTime = 3f)
    {
        float bpmBefore = HeartRateReader.Instance != null ? HeartRateReader.Instance.currentHeartRate : 70f;
        yield return new WaitForSeconds(waitTime);
        float bpmAfter = HeartRateReader.Instance != null ? HeartRateReader.Instance.currentHeartRate : bpmBefore;

        float delta = Mathf.Max(0f, bpmAfter - bpmBefore);

        // Mettre à jour le score du clip avec une moyenne pondérée
        if (clipFearScores.ContainsKey(clip))
        {
            clipFearScores[clip] = Mathf.Lerp(clipFearScores[clip], delta, 0.5f);
        }
    }

    AudioClip ChooseWeightedClip()
    {
        // Si pas de données, choisir aléatoirement
        if (!initialTestDone || clipFearScores.Count == 0)
        {
            if (allClips.Count > 0)
                return allClips[Random.Range(0, allClips.Count)];
            return null;
        }

        // Calculer la somme totale des scores
        float totalScore = clipFearScores.Values.Sum();
        totalScore = Mathf.Max(totalScore, 0.01f); // éviter division par zéro

        // Créer une liste pondérée basée sur les scores de peur
        List<WeightedClip> weightedClips = new();

        foreach (var kvp in clipFearScores)
        {
            AudioClip clip = kvp.Key;
            float score = kvp.Value;

            if (clip != null)
            {
                // Plus le score est élevé, plus le clip a de chances d'être sélectionné
                // Ajouter un petit pourcentage pour maintenir de la variété
                float weight = (score / totalScore) * 0.8f + 0.2f / clipFearScores.Count;
                weightedClips.Add(new WeightedClip(clip, weight));
            }
        }

        // Sélection aléatoire pondérée
        return SelectWeightedRandom(weightedClips);
    }

    AudioClip SelectWeightedRandom(List<WeightedClip> weightedClips)
    {
        if (weightedClips.Count == 0) return null;

        float totalWeight = weightedClips.Sum(w => w.weight);
        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;

        foreach (var weightedClip in weightedClips)
        {
            currentWeight += weightedClip.weight;
            if (randomValue <= currentWeight)
            {
                return weightedClip.clip;
            }
        }

        // Fallback au dernier clip
        return weightedClips[weightedClips.Count - 1].clip;
    }

    // Classe helper pour la sélection pondérée
    private class WeightedClip
    {
        public AudioClip clip;
        public float weight;

        public WeightedClip(AudioClip clip, float weight)
        {
            this.clip = clip;
            this.weight = weight;
        }
    }

    // Méthode pour debug - afficher les scores actuels
    [ContextMenu("Debug Clip Scores")]
    void DebugClipScores()
    {
        Debug.Log("=== Scores des clips ===");
        foreach (var kvp in clipFearScores)
        {
            Debug.Log($"{kvp.Key.name}: {kvp.Value:F2}");
        }
    }
}