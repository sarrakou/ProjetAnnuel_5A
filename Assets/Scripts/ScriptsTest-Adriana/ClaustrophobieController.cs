using System.Collections;
using UnityEngine;

public class ClaustrophobieController : MonoBehaviour
{
    [Header("Paredes")]
    public Transform wallRight;
    public Transform wallLeft;
    public Transform wallFront;
    public Transform wallBack;

    [Header("Objetivos")]
    public Transform targetRight;
    public Transform targetLeft;
    public Transform targetFront;
    public Transform targetBack;

    [Header("Movimiento")]
    public float moveSpeed = 1f;

    [Header("Luces")]
    public Light[] claustroLights;
    public float flickerMinInterval = 0.1f;
    public float flickerMaxInterval = 0.8f;

    private Vector3 rightStartPos, leftStartPos, frontStartPos, backStartPos;
    private bool hasStarted = false;
    private Coroutine flickerCoroutine;
    private bool[] originalLightStates;

    void Start()
    {
        // Guardar las posiciones iniciales
        rightStartPos = wallRight.position;
        leftStartPos = wallLeft.position;
        frontStartPos = wallFront.position;
        backStartPos = wallBack.position;

        // Guardar el estado inicial de las luces
        if (claustroLights != null && claustroLights.Length > 0)
        {
            originalLightStates = new bool[claustroLights.Length];
            for (int i = 0; i < claustroLights.Length; i++)
                originalLightStates[i] = claustroLights[i].enabled;
        }
    }

    void OnEnable()
    {
        if (!hasStarted)
        {
            hasStarted = true;

            // Iniciar movimiento de las paredes
            StartCoroutine(MoveWall(wallRight, targetRight.position));
            StartCoroutine(MoveWall(wallLeft, targetLeft.position));
            StartCoroutine(MoveWall(wallFront, targetFront.position));
            StartCoroutine(MoveWall(wallBack, targetBack.position));

            // Iniciar parpadeo de luces
            if (claustroLights != null && claustroLights.Length > 0)
                flickerCoroutine = StartCoroutine(FlickerLights());
        }
    }

    void OnDisable()
    {
        // Restaurar posiciones de las paredes
        wallRight.position = rightStartPos;
        wallLeft.position = leftStartPos;
        wallFront.position = frontStartPos;
        wallBack.position = backStartPos;
        hasStarted = false;

        // Detener parpadeo de luces
        if (flickerCoroutine != null)
            StopCoroutine(flickerCoroutine);

        // Restaurar estado original de las luces
        if (claustroLights != null)
        {
            for (int i = 0; i < claustroLights.Length; i++)
            {
                if (claustroLights[i] != null)
                    claustroLights[i].enabled = originalLightStates[i];
            }
        }
    }

    IEnumerator MoveWall(Transform wall, Vector3 targetPosition)
    {
        while (Vector3.Distance(wall.position, targetPosition) > 0.01f)
        {
            wall.position = Vector3.MoveTowards(wall.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator FlickerLights()
    {
        while (true)
        {
            foreach (var light in claustroLights)
            {
                if (light != null)
                    light.enabled = !light.enabled;
            }

            float waitTime = Random.Range(flickerMinInterval, flickerMaxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
