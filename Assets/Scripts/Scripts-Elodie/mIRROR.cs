using UnityEngine;

public class Mirror : MonoBehaviour
{
    public Camera mainCamera;   // Ta caméra principale, active
    public Camera mirrorCamera; // Caméra miroir, désactivée dans l'inspecteur (enabled = false)
    public Renderer mirrorRenderer;

    RenderTexture rt;

    void Start()
    {
        rt = new RenderTexture(512, 512, 16);
        rt.Create();

        mirrorCamera.targetTexture = rt;
        mirrorRenderer.material.SetTexture("_MainTex", rt);

        mirrorCamera.enabled = false; // IMPORTANT : désactive la caméra pour qu'elle ne soit pas utilisée automatiquement
    }

    void LateUpdate()
    {
        // Calcul position et rotation miroir comme avant
        Vector3 pos = mainCamera.transform.position;
        Vector3 normal = transform.up;
        Vector3 mirrorPos = pos - 2 * Vector3.Dot(pos - transform.position, normal) * normal;

        mirrorCamera.transform.position = mirrorPos;

        Vector3 forward = mainCamera.transform.forward;
        Vector3 mirrorForward = Vector3.Reflect(forward, normal);
        mirrorCamera.transform.rotation = Quaternion.LookRotation(mirrorForward, Vector3.up);

        // Rendu manuel dans la RenderTexture
        mirrorCamera.Render();
    }
}
