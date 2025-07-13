using UnityEngine;
using System.Collections;

public class CanvasActivator : MonoBehaviour
{
    public enum CanvasEventType
    {
        Hallucination,
        Info,
        Warning,
        Custom
    }

    [Header("Paramètres de l'événement")]
    public CanvasEventType eventType = CanvasEventType.Hallucination;

    [Tooltip("Nom du prefab Canvas dans le dossier Resources")]
    public string canvasResourceName;

    [Tooltip("Durée totale de l'effet en secondes")]
    public float totalDuration = 1f;

    [Tooltip("Nombre de clignotements (on/off alternés)")]
    public int flashCount = 2;

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

        canvasInstance = Instantiate(canvasPrefab);
        canvasInstance.SetActive(false); // On commence éteint

        float flashInterval = totalDuration / (flashCount * 2f); // On/off = 2 phases

        for (int i = 0; i < flashCount; i++)
        {
            canvasInstance.SetActive(true);
            yield return new WaitForSeconds(flashInterval);
            canvasInstance.SetActive(false);
            yield return new WaitForSeconds(flashInterval);
        }

        Destroy(canvasInstance);
    }
}