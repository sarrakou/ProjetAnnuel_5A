using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    private Light[] allLights;
    private List<Material> emissiveMaterials = new List<Material>();

    public float fadeDuration = 2f;

    void Start()
    {
        // Récupère toutes les lumières de la scène
        allLights = FindObjectsOfType<Light>();

        foreach (Light l in allLights)
        {
            if (l.gameObject.name == "FlashLight")
                continue;

            l.intensity = 0f;
            l.enabled = false;
        }

        // Récupère tous les matériaux émissifs dans la scène (sur MeshRenderer)
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.IsKeywordEnabled("_EMISSION"))
                {
                    emissiveMaterials.Add(mat);
                    mat.DisableKeyword("_EMISSION"); // désactive au départ
                }
            }
        }
    }

    public void SetAllLights(bool state)
    {
        foreach (Light l in allLights)
        {
            if (l.gameObject.name == "FlashLight")
                continue;

            if (state)
                StartCoroutine(FadeInLight(l));
            else
                l.enabled = false;
        }

        foreach (Material mat in emissiveMaterials)
        {
            if (state)
                mat.EnableKeyword("_EMISSION");
            else
                mat.DisableKeyword("_EMISSION");
        }

        Debug.Log($"💡 Lumières {(state ? "allumées avec fondu" : "éteintes")} + Matériaux émissifs {(state ? "activés" : "désactivés")}");
    }

    IEnumerator FadeInLight(Light light)
    {
        light.enabled = true;

        float elapsed = 0f;
        float targetIntensity = 1f;
        light.intensity = 0f;

        while (elapsed < fadeDuration)
        {
            light.intensity = Mathf.Lerp(0f, targetIntensity, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        light.intensity = targetIntensity;
    }
}
