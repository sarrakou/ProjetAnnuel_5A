using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionalAudioSpawner : MonoBehaviour
{
    [Header("Sound Settings")]
    public List<AudioClip> scareClips;
    public float minDelay = 3f;
    public float maxDelay = 7f;
    public float soundRadius = 5f;
    public float fadeOutDuration = 2f;

    [Header("References")]
    public Transform player;
    public GameObject audioPrefab;

    private void OnEnable()
    {
        StartCoroutine(SpawnScarySounds());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator SpawnScarySounds()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            if (player != null && scareClips.Count > 0)
            {
                Vector3 spawnPos = player.position + Random.onUnitSphere * soundRadius;
                spawnPos.y = player.position.y;

                GameObject soundObj = Instantiate(audioPrefab, spawnPos, Quaternion.identity);
                AudioSource source = soundObj.GetComponent<AudioSource>();

                if (source != null)
                {
                    AudioClip clip = scareClips[Random.Range(0, scareClips.Count)];
                    source.clip = clip;
                    source.volume = 1f;
                    source.loop = false;
                    source.Play();

                    // Espera la duración del clip menos el fade out
                    float playTimeBeforeFade = Mathf.Max(0f, clip.length - fadeOutDuration);
                    StartCoroutine(FadeOutAndDestroy(source, playTimeBeforeFade));
                }
            }
        }
    }

    IEnumerator FadeOutAndDestroy(AudioSource source, float waitBeforeFade)
    {
        yield return new WaitForSeconds(waitBeforeFade);

        float startVolume = source.volume;
        float t = 0f;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
            yield return null;
        }

        Destroy(source.gameObject);
    }
}
