using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DirectionalSoundEmitter : MonoBehaviour
{
    [Header("Référence au joueur")]
    public Transform player;

    [Header("Clips audio")]
    public List<AudioClip> soundClips;

    [Header("Paramètres de timing")]
    public float minDelay = 3f;
    public float maxDelay = 8f;

    [Header("Rayon autour du joueur")]
    public float distanceFromPlayer = 8f;

    [Header("Volume & stéréo")]
    public float volume = 1f;
    [Range(0f, 1f)] public float stereoChance = 0.2f; // 20% des sons sont non-spatialisés

    private string lastDirection = "";

    private readonly string[] directions = new string[] { "front", "back", "left", "right" };

    void Start()
    {
        if (player == null || soundClips.Count == 0)
        {
            Debug.LogWarning("Player ou AudioClips non assignés.");
            return;
        }

        StartCoroutine(PlayDirectionalSounds());
    }

    IEnumerator PlayDirectionalSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            // Choisir un clip aléatoire
            AudioClip clip = soundClips[Random.Range(0, soundClips.Count)];

            // Décider si stéréo ou directionnel
            bool isStereo = Random.value < stereoChance;
            if (isStereo)
            {
                AudioSource.PlayClipAtPoint(clip, player.position, volume); // Son "dans la tête"
                continue;
            }

            // Choisir une direction différente de la précédente
            string chosenDirection;
            do
            {
                chosenDirection = directions[Random.Range(0, directions.Length)];
            } while (chosenDirection == lastDirection);
            lastDirection = chosenDirection;

            // Calculer la position relative
            Vector3 dirOffset = Vector3.zero;

            switch (chosenDirection)
            {
                case "front": dirOffset = player.forward; break;
                case "back": dirOffset = -player.forward; break;
                case "left": dirOffset = -player.right; break;
                case "right": dirOffset = player.right; break;
            }

            Vector3 soundPos = player.position + dirOffset.normalized * distanceFromPlayer;

            // Jouer le son dans la direction choisie
            AudioSource.PlayClipAtPoint(clip, soundPos, volume);
        }
    }
}
