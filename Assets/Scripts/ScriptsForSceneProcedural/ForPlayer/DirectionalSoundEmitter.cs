using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    void Start()
    {
        if (player == null) player = Camera.main.transform;          // secours
        StartCoroutine(PlayLoop());
    }

    IEnumerator PlayLoop()
    {
        while (true)
        {
            /* ----------- 1.  Récupère la FC actuelle ---------------- */
            float bpm = HeartRateReader.Instance != null
                        ? HeartRateReader.Instance.currentHeartRate
                        : 70f;

            /* ----------- 2.  Choisis la liste de sons ---------------- */
            List<AudioClip> list;
            if (bpm < 85f) list = calmClips;
            else if (bpm < 120f) list = mediumClips;
            else list = intenseClips;

            if (list.Count == 0) yield break; // sécurité

            /* ----------- 3.  Calcule le délai ------------------------ */
            // bpm bas  → délai long ; bpm haut → délai court
            float t = Mathf.InverseLerp(minBPM, maxBPM, bpm);   // 0‑1
            float delay = Mathf.Lerp(8f, 2f, t);                // 8s → 2s
            yield return new WaitForSeconds(delay);

            /* ----------- 4.  Choix clip & position ------------------- */
            AudioClip clip = list[Random.Range(0, list.Count)];

            bool stereo = Random.value < stereoChance;
            if (stereo)
            {
                AudioSource.PlayClipAtPoint(clip, player.position, volume);
                continue;
            }

            // direction ≠ de la précédente
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

            AudioSource.PlayClipAtPoint(clip, pos, volume);
        }
    }
}
