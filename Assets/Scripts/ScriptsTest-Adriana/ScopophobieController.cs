using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopophobieController : MonoBehaviour
{
    public List<Transform> statues;
    public Transform player;
    public float rotationSpeed = 1f;
    public float interval = 5f; // cada 5s gira un grupo
    public int statuesPerWave = 3;

    private int currentIndex = 0;

    void OnEnable()
    {
        StartCoroutine(ActivateStatuesInWaves());
    }

    IEnumerator ActivateStatuesInWaves()
    {
        while (currentIndex < statues.Count)
        {
            for (int i = 0; i < statuesPerWave && currentIndex < statues.Count; i++)
            {
                Transform statue = statues[currentIndex];
                StartCoroutine(LookAtPlayer(statue));
                currentIndex++;
            }
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator LookAtPlayer(Transform statue)
    {
        while (true)
        {
            Vector3 direction = player.position - statue.position;
            direction.y = 0; // solo girar en el plano horizontal
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            statue.rotation = Quaternion.Slerp(statue.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }
    }
}
