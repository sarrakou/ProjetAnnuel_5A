using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    private Transform camTransform;
    public Vector3 offset = new Vector3(0, 0, 2f);

    void Start()
    {
        camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.position = camTransform.position + camTransform.forward * offset.z + camTransform.up * offset.y + camTransform.right * offset.x;
        transform.rotation = camTransform.rotation;
    }
}