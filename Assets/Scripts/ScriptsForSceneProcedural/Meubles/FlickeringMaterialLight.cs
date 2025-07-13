using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class FlickeringMaterialLight : MonoBehaviour
{
    public Light lightSource;

    public Color normalColor = Color.white;
    public Color dangerColor = new Color(1f, 0.1f, 0.1f);

    public float baseIntensity = 0.005f;
    private bool alarmStarted = false;

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
                lightSource.color = normalColor;
                lightSource.intensity = Random.Range(baseIntensity * 0.4f, baseIntensity);
            }
            else
            {
                if (!alarmStarted)
                {
                    alarmStarted = true;
                    StartCoroutine(AlarmRoutine());
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator AlarmRoutine()
    {
        while (true)
        {
            lightSource.color = dangerColor;

            // Phase 1 - faible rouge
            lightSource.intensity = 0.2f;
            yield return new WaitForSeconds(0.1f);

            // Phase 2 - flash rouge plus fort
            lightSource.intensity = 1.0f;
            yield return new WaitForSeconds(0.1f);
        }
    }
}