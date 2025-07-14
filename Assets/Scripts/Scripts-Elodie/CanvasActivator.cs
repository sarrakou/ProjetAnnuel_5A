using UnityEngine;
using System.Collections;

public class CanvasActivator : MonoBehaviour
{
    public enum CanvasEventType
    {
        Hallucination,
        Eyes,
    }

    [Header("Paramètres de l'événement")]
    public CanvasEventType eventType = CanvasEventType.Hallucination;

    [Tooltip("Nom du prefab Canvas dans le dossier Resources")]
    public string canvasResourceName;

    [Tooltip("Durée totale de l'effet en secondes")]
    public float totalDuration = 1f;

    [Tooltip("Nombre de clignotements (on/off alternés)")]
    public int flashCount = 2;

    [Tooltip("Vitesse de déplacement du canvas si Eyes")]
    public float moveSpeed = 1f;

    private GameObject canvasInstance;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(FlashCanvas());
        }
    }

    private IEnumerator FlashCanvas()
    {
        if (string.IsNullOrEmpty(canvasResourceName))
        {
            Debug.LogError("CanvasActivator: Nom de prefab vide.");
            yield break;
        }

        GameObject canvasPrefab = Resources.Load<GameObject>(canvasResourceName);
        if (canvasPrefab == null)
        {
            Debug.LogError("CanvasActivator: Aucun prefab trouvé dans Resources avec le nom : " + canvasResourceName);
            yield break;
        }

        // Désactiver toutes les lumières si Eyes
        Light[] allLights = null;
        if (eventType == CanvasEventType.Eyes)
        {
            allLights = GameObject.FindObjectsOfType<Light>();
            foreach (var light in allLights)
                light.enabled = false;
        }

        // Créer le canvas
        canvasInstance = Instantiate(canvasPrefab);
        canvasInstance.SetActive(false);

        float flashInterval = totalDuration / (flashCount * 2f);

        // Démarre le déplacement si Eyes
        Coroutine moveCoroutine = null;
        if (eventType == CanvasEventType.Eyes)
        {
            moveCoroutine = StartCoroutine(MoveCanvasForward(canvasInstance.transform));
        }

        for (int i = 0; i < flashCount; i++)
        {
            canvasInstance.SetActive(true);
            yield return new WaitForSeconds(flashInterval);
            canvasInstance.SetActive(false);
            yield return new WaitForSeconds(flashInterval);
        }

        // Stop déplacement
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        Destroy(canvasInstance);

        // Rallumer les lumières
        if (allLights != null)
        {
            foreach (var light in allLights)
                light.enabled = true;
        }
    }

    // Mouvement progressif vers l'avant
    private IEnumerator MoveCanvasForward(Transform canvasTransform)
    {
        while (true)
        {
            Vector3 forwardDir = -Camera.main.transform.forward; // vers la caméra
            canvasTransform.position += forwardDir * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }

}
