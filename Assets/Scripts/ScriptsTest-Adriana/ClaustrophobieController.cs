using UnityEngine;

public class ClaustrophobieController : MonoBehaviour
{
    [Header("Objeto que se va a escalar")]
    public Transform targetToScale;

    [Header("Escalado")]
    public Vector2 targetXYScale = new Vector2(0.5f, 0.5f); // X e Y destino
    public float duration = 10f;

    private Vector3 initialScale;
    private float timer = 0f;
    private bool isActive = false;

    void OnEnable()
    {
        if (targetToScale == null)
        {
            Debug.LogWarning("No se ha asignado el objeto a escalar.");
            return;
        }

        initialScale = targetToScale.localScale;
        timer = 0f;
        isActive = true;
    }

    void Update()
    {
        if (!isActive || targetToScale == null)
            return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);

        float newX = Mathf.Lerp(initialScale.x, targetXYScale.x, t);
        float newY = Mathf.Lerp(initialScale.y, targetXYScale.y, t);
        float z = initialScale.z;

        targetToScale.localScale = new Vector3(newX, newY, z);

        if (t >= 1f)
            isActive = false;
    }
}
