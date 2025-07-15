using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class FlickeringMaterialLight : MonoBehaviour
{
    public Light lightSource;

    public Color normalColor = Color.white;
    public Color dangerColor = new Color(0.509804f, 0.1f, 0.1f);

    public float baseIntensity = 0.002f;
    private bool alarmStarted = false;

    private bool flickerStarted = false;


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

            // Éteint
            lightSource.intensity = 0.0009f;
            yield return new WaitForSeconds(0.4f);

            // Allumé faible blanc
            lightSource.intensity = baseIntensity;
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator AlarmRoutine()
    {
        while (true)
        {
            lightSource.color = dangerColor;

            // Phase 1 - faible rouge
            lightSource.intensity = 0.0009f;
            yield return new WaitForSeconds(0.5f);

            // Phase 2 - flash rouge plus fort
            lightSource.intensity = 0.01f;
            yield return new WaitForSeconds(0.5f);
        }
    }
}