using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class DollSoundLoop : MonoBehaviour
{
    public AudioClip[] sounds;           // Sons possibles à jouer
    public float interval = 3f;          // Temps entre chaque son
    public float volume = 1f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayLoop());
    }

    IEnumerator PlayLoop()
    {
        while (true)
        {
            if (sounds.Length > 0)
            {
                AudioClip clip = sounds[Random.Range(0, sounds.Length)];
                audioSource.PlayOneShot(clip, volume);
            }

            yield return new WaitForSeconds(interval);
        }
    }
}
