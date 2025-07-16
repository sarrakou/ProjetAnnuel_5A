using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntomophobieController : MonoBehaviour
{
    [Header("Insect Setup")]
    public List<GameObject> insects;
    public float spawnInterval = 0.1f;
    public int insectsPerBatch = 3;

    [Header("Intro Audio")]
    public List<AudioSource> introSounds;  // Sonidos de introducción
    public float delayBeforeSpawn = 3f;    // Espera antes de iniciar spawn

    [Header("Looping Insect Audio")]
    public AudioSource insectLoopAudio;    // Sonido continuo de insectos
    public float maxVolume = 1f;

    [Header("Local Sound Effects")]
    public List<AudioSource> randomScareSounds;  // Sonidos localizados que se activan aleatoriamente
    public float scareSoundInterval = 2f;

    private Coroutine spawnCoroutine;
    private Coroutine scareCoroutine;

    void OnEnable()
    {
        Debug.Log("ENTOMOPHOBIE: Activando escena...");

        // Apagar todos los insectos
        foreach (var insect in insects)
            if (insect != null) insect.SetActive(false);

        // Detener audio principal si estaba sonando
        if (insectLoopAudio != null)
        {
            insectLoopAudio.volume = 0f;
            insectLoopAudio.Stop();
        }

        // Reproducir intro sounds
        foreach (var sound in introSounds)
            if (sound != null) sound.Play();

        // Iniciar proceso completo
        StartCoroutine(FullSequence());
    }

    void OnDisable()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        if (scareCoroutine != null)
            StopCoroutine(scareCoroutine);

        if (insectLoopAudio != null)
            insectLoopAudio.Stop();

        foreach (var sound in randomScareSounds)
            if (sound != null) sound.Stop();
    }

    IEnumerator FullSequence()
    {
        // Esperar hasta que terminen los sonidos intro o delay manual
        yield return new WaitForSeconds(delayBeforeSpawn);

        // Iniciar audio principal
        if (insectLoopAudio != null)
        {
            insectLoopAudio.Play();
            insectLoopAudio.volume = 0f;
        }

        // Iniciar aparición de insectos
        spawnCoroutine = StartCoroutine(SpawnInsects());

        // Iniciar sonidos aleatorios
        scareCoroutine = StartCoroutine(PlayScareSounds());
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

            // Escalar volumen progresivamente
            if (insectLoopAudio != null)
            {
                float progress = (float)index / total;
                insectLoopAudio.volume = Mathf.Lerp(0f, maxVolume, progress);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator PlayScareSounds()
    {
        yield return new WaitForSeconds(1f); // Pequeño delay inicial

        while (true)
        {
            if (randomScareSounds.Count > 0)
            {
                // Elegir un sonido al azar
                int randIndex = Random.Range(0, randomScareSounds.Count);
                AudioSource sound = randomScareSounds[randIndex];
                if (sound != null && !sound.isPlaying)
                {
                    sound.Play();
                }
            }

            yield return new WaitForSeconds(scareSoundInterval);
        }
    }
}
