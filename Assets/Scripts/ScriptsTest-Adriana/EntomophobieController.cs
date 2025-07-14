using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntomophobieController : MonoBehaviour
{
    [Header("Insect Setup")]
    public List<GameObject> insects;       // Insectos que se irán activando
    public float spawnInterval = 0.5f;     // Tiempo entre cada aparición
    public int insectsPerBatch = 2;        // Cuántos insectos aparecen por intervalo

    private bool started = false;

    void OnEnable()
    {
        if (!started)
        {

            // Desactivar todos los insectos al inicio
            foreach (var insect in insects)
            {
                if (insect != null)
                    insect.SetActive(false);
            }
            started = true;
            StartCoroutine(SpawnInsects());
        }
    }

    IEnumerator SpawnInsects()
    {
        int index = 0;

        while (index < insects.Count)
        {
            for (int i = 0; i < insectsPerBatch && index < insects.Count; i++)
            {
                var insect = insects[index];
                if (insect != null)
                    insect.SetActive(true);

                index++;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

}
