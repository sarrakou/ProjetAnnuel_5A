using System.Collections;
using UnityEngine;

public class NyctophobieController : MonoBehaviour
{
    public Light[] sceneLights;
    public float fadeDuration = 5f;

    public AudioSource scarySounds;
    public GameObject shadowObject;

    public Light redLight;                     //  Luz roja al final
    public float redLightFlashSpeed = 5f;

    public float cameraShakeDuration = 2f;     // Temblor de cámara
    public float cameraShakeMagnitude = 0.1f;

    private Transform camTransform;
    private Vector3 originalCamPosition;
    private bool started = false;

    void OnEnable()
    {
        if (!started)
        {
            started = true;
            camTransform = Camera.main.transform;
            originalCamPosition = camTransform.position;
            StartCoroutine(RunNyctophobiaSequence());
        }
    }

    IEnumerator RunNyctophobiaSequence()
    {
        // 0s–5s: Atenuar luces
        float timer = 0f;
        float[] originalIntensities = new float[sceneLights.Length];
        for (int i = 0; i < sceneLights.Length; i++)
            originalIntensities[i] = sceneLights[i].intensity;

        while (timer < fadeDuration)
        {
            float t = 1f - (timer / fadeDuration);
            for (int i = 0; i < sceneLights.Length; i++)
                sceneLights[i].intensity = originalIntensities[i] * t;
            timer += Time.deltaTime;
            yield return null;
        }

        // 5s–15s: Apagar luces una a una
        for (int i = 0; i < sceneLights.Length; i++)
        {
            sceneLights[i].enabled = false;
            yield return new WaitForSeconds(3.3f);
        }

        // 15s–25s: Sonidos en oscuridad
        if (scarySounds != null)
            scarySounds.Play();

        yield return new WaitForSeconds(10f);

        // 25s–30s: Aparece sombra, luz roja y tiemblan cosas
        if (shadowObject != null)
            shadowObject.SetActive(true);

        if (redLight != null)
            StartCoroutine(FlashRedLight());

        if (camTransform != null)
            StartCoroutine(ShakeCamera());

        yield return new WaitForSeconds(5f);

        // Restaurar cámara al final
        camTransform.position = originalCamPosition;
    }

    IEnumerator FlashRedLight()
    {
        redLight.enabled = true;
        float time = 0f;
        while (time < 5f)
        {
            redLight.intensity = Mathf.PingPong(Time.time * redLightFlashSpeed, 1f);
            time += Time.deltaTime;
            yield return null;
        }
        redLight.enabled = false;
    }

    IEnumerator ShakeCamera()
    {
        float elapsed = 0.0f;
        while (elapsed < cameraShakeDuration)
        {
            Vector3 randomPoint = originalCamPosition + Random.insideUnitSphere * cameraShakeMagnitude;
            camTransform.position = randomPoint;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
