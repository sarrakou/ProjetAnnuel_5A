using UnityEngine;

public class PlaySoundPlayerContact : MonoBehaviour
{
    [Tooltip("Le son à jouer au contact du joueur")]
    public AudioClip soundToPlay;

    [Tooltip("Volume du son")]
    [Range(0f, 1f)]
    public float volume = 1f;

    private AudioSource audioSource;

    private void Start()
    {
      
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && soundToPlay != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = soundToPlay;
                audioSource.volume = volume;
                audioSource.Play();
            }
        }
    }
}