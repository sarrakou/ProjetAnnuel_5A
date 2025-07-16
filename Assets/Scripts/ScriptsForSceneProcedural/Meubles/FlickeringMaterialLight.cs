using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class FlickeringMaterialLight : MonoBehaviour
{
    public Light lightSource;

    public Color normalColor = Color.white;
    public Color dangerColor = new Color(130f / 255f, 26f / 255f, 26f / 255f); // #821A1A


    public float baseIntensity = 0.002f;
    private bool alarmStarted = false;

    private bool flickerStarted = false;

    public AudioSource audioSource;
    public AudioClip flickerSound;
    public AudioClip alarmSound;


    void Start()
    {
        lightSource = GetComponent<Light>();
        lightSource.type = LightType.Point;
        lightSource.range = 0.5f;
        lightSource.intensity = baseIntensity;
        lightSource.color = normalColor;
        lightSource.shadows = LightShadows.None;
        lightSource.renderMode = LightRenderMode.ForcePixel; // Force visible

        StartCoroutine(UpdateLightBehavior());
    }

    IEnumerator UpdateLightBehavior()
    {
        while (true)
        {
            float t = Time.timeSinceLevelLoad;

            if (t < 20f)
            {
                lightSource.color = normalColor;
                lightSource.intensity = baseIntensity;
            }
            else if (t < 40f)
            {
                if (!flickerStarted)
                {
                    flickerStarted = true;
                    StartCoroutine(WhiteFlickerRoutine());
                }
            }

            else
            {
                if (!alarmStarted)
                {
                    alarmStarted = true;
                    StartCoroutine(AlarmRoutine());
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator WhiteFlickerRoutine()
    {
        while (!alarmStarted)
        {
            lightSource.color = normalColor;

            // OFF (flicker)
            lightSource.intensity = 0f;
            yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));

            // ON
            lightSource.intensity = baseIntensity;

            if (flickerSound != null && audioSource != null)
            {
                audioSource.clip = flickerSound;
                audioSource.loop = false;
                audioSource.Play();
            }

            yield return new WaitForSeconds(Random.Range(0.15f, 0.3f));
        }
    }


    IEnumerator AlarmRoutine()
    {
        // Boucle infinie
        while (true)
        {
            lightSource.color = dangerColor;

            // ✅ Lancer l’alarme en boucle à chaque clignotement, si pas déjà lancée
            if (alarmSound != null && audioSource != null && audioSource.clip != alarmSound)
            {
                audioSource.clip = alarmSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            // 🔴 OFF rouge
            lightSource.intensity = 0f;
            yield return new WaitForSeconds(Random.Range(0.1f, 0.25f));

            // 🔴 ON rouge
            lightSource.intensity = Random.Range(0.008f, 0.02f);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));

            // ❌ Aucun grésillement ici !
        }
    }


}