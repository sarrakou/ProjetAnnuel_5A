using System.Collections;
using UnityEngine;

public class NyctophobiaLightsAudio : MonoBehaviour
{
    public Light flashlight;            // Linterna que titila
    public Light[] sceneLights;         // Array de luces tipo spotlight
   
    public float blinkInterval = 0.3f;       // Intervalo titileo linterna y spots
    public float totalBlinkTime = 5f;        // Duración titileo linterna y spots
    public float spotlightOffDelay = 1f;     // Tiempo entre apagado luces spots

    // Variables para guardar estado original
    private bool flashlightOriginalEnabled;
    private float flashlightOriginalIntensity;

    private bool[] sceneLightsOriginalEnabled;
    private float[] sceneLightsOriginalIntensity;

    private Coroutine sequenceCoroutine;

    void OnEnable()
    {
        // Guardar estados originales
        if (flashlight != null)
        {
            flashlightOriginalEnabled = flashlight.enabled;
            flashlightOriginalIntensity = flashlight.intensity;
        }

        sceneLightsOriginalEnabled = new bool[sceneLights.Length];
        sceneLightsOriginalIntensity = new float[sceneLights.Length];
        for (int i = 0; i < sceneLights.Length; i++)
        {
            if (sceneLights[i] != null)
            {
                sceneLightsOriginalEnabled[i] = sceneLights[i].enabled;
                sceneLightsOriginalIntensity[i] = sceneLights[i].intensity;
            }
        }

        // Iniciar secuencia
        sequenceCoroutine = StartCoroutine(RunNyctophobiaSequence());
    }

    void OnDisable()
    {
        // Detener la coroutine si está corriendo
        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        // Restaurar estados originales
        if (flashlight != null)
        {
            flashlight.enabled = flashlightOriginalEnabled;
            flashlight.intensity = flashlightOriginalIntensity;
        }

        for (int i = 0; i < sceneLights.Length; i++)
        {
            if (sceneLights[i] != null)
            {
                sceneLights[i].enabled = sceneLightsOriginalEnabled[i];
                sceneLights[i].intensity = sceneLightsOriginalIntensity[i];
            }
        }

        
    }

    IEnumerator RunNyctophobiaSequence()
    {
        float timer = 0f;
        while (timer < totalBlinkTime)
        {
            bool enabledState = !flashlight.enabled;

            if (flashlight != null)
                flashlight.enabled = enabledState;

            foreach (var light in sceneLights)
            {
                if (light != null)
                    light.enabled = enabledState;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        if (flashlight != null)
            flashlight.enabled = false;

        foreach (var light in sceneLights)
        {
            if (light != null)
                light.enabled = true; // las dejamos prendidas para apagar en orden
        }

        foreach (var light in sceneLights)
        {
            if (light != null)
                light.enabled = false;
            yield return new WaitForSeconds(spotlightOffDelay);
        }

    }
}
