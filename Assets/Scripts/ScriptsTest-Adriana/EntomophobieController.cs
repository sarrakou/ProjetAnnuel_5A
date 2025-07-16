using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntomophobieController : MonoBehaviour
{
    [Header("Insect Setup")]
    public List<GameObject> insects;
    public float spawnInterval = 0.1f;
    public int insectsPerBatch = 3;

    [Header("Audio")]
    public AudioSource insectAudioSource;     // El AudioSource con el sonido de insectos
    public float maxVolume = 1.0f;             // Volumen final al tener todos los insectos activos

    private Coroutine spawnCoroutine;

    void OnEnable()
    {
        Debug.Log("entra insects");
        // Apagar insectos
        foreach (var insect in insects)
        {
            if (insect != null)
                insect.SetActive(false);
        }

        // Reiniciar audio
        if (insectAudioSource != null)
        {
            insectAudioSource.volume = 0f;
            insectAudioSource.Play();
        }

        spawnCoroutine = StartCoroutine(SpawnInsects());
    }

    void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (insectAudioSource != null)
        {
            insectAudioSource.Stop();
        }
    }

    IEnumerator SpawnInsects()
    {
        int index = 0;
        int total = insects.Count;

        while (index < total)
        {
            for (int i = 0; i < insectsPerBatch && index < total; i++)
            {
                if (insects[index] != null)
                    insects[index].SetActive(true);

                index++;
            }

            // Escalar volumen proporcionalmente
            if (insectAudioSource != null)
            {
                float progress = (float)index / total;
                insectAudioSource.volume = Mathf.Lerp(0f, maxVolume, progress);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
