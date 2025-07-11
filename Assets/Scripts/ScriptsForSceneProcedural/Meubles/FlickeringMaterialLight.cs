using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlickeringMaterialLight : MonoBehaviour
{
    public Light lightSource;
    public Color normalColor = Color.white;
    public Color dangerColor = Color.red;

    [Header("Flicker Settings")]
    public float minIntensity = 0.6f;
    public float maxIntensity = 1.5f;
    public float flickerIntervalMin = 0.1f;
    public float flickerIntervalMax = 0.4f;

    [Header("Danger Color")]
    public float dangerChance = 0.05f; // 5% chance
    public float dangerDuration = 2f;

    private bool isInDangerMode = false;

    void Start()
    {
        if (lightSource == null)
            lightSource = GetComponent<Light>();

        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (!isInDangerMode)
            {
                // Clignotement blanc
                lightSource.intensity = Random.Range(minIntensity, maxIntensity);
                lightSource.color = normalColor;

                // Chance d'entrer en mode rouge
                if (Random.value < dangerChance)
                {
                    StartCoroutine(DangerRoutine());
                }
            }

            yield return new WaitForSeconds(Random.Range(flickerIntervalMin, flickerIntervalMax));
        }
    }

    IEnumerator DangerRoutine()
    {
        isInDangerMode = true;
        lightSource.color = dangerColor;
        lightSource.intensity = 1f;

        yield return new WaitForSeconds(dangerDuration);

        lightSource.color = normalColor;
        isInDangerMode = false;
    }
}