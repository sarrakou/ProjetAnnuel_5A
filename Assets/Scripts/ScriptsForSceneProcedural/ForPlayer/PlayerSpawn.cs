using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public Vector3 spawnPosition = new Vector3(-0.3f, 0.03f, -0.03f);

    void Start()
    {
        transform.position = spawnPosition;
    }
}
