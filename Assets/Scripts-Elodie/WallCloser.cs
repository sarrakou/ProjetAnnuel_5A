using UnityEngine;

public class WallCloser : MonoBehaviour
{
    public Transform wallLeft;
    public Transform wallRight;
    public float closeSpeed = 1f;
    public float minDistance = 1.5f;

    private bool isClosing = false;

    void Start()
    {
        isClosing = true;
    }

    void Update()
    {
        if (!isClosing) return;

        float distance = Mathf.Abs(wallRight.position.z - wallLeft.position.z);

        if (distance > minDistance)
        {
            wallLeft.position = new Vector3(
                wallLeft.position.x,
                wallLeft.position.y,
                Mathf.MoveTowards(wallLeft.position.z, wallRight.position.z, closeSpeed * Time.deltaTime)
            );

            wallRight.position = new Vector3(
                wallRight.position.x,
                wallRight.position.y,
                Mathf.MoveTowards(wallRight.position.z, wallLeft.position.z, closeSpeed * Time.deltaTime)
            );
        }
    }
}