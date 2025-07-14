using UnityEngine;

public class ClaustrophobieController : MonoBehaviour
{
    public Transform wallRight;   // Se moverá hacia la derecha
    public Transform wallLeft;    // Se moverá hacia la izquierda
    public Transform wallFront;   // Se moverá hacia atrás (hacia el jugador)

    public float moveDistance = 2f;
    public float moveSpeed = 1f;

    private Vector3 rightStartPos, leftStartPos, frontStartPos;
    private bool isActivated = false;

    void Start()
    {
        // Guardar las posiciones iniciales
        rightStartPos = wallRight.position;
        leftStartPos = wallLeft.position;
        frontStartPos = wallFront.position;
    }

    void Update()
    {
        if (isActivated)
        {
            wallRight.position = Vector3.MoveTowards(wallRight.position, rightStartPos + Vector3.right * moveDistance, moveSpeed * Time.deltaTime);
            wallLeft.position = Vector3.MoveTowards(wallLeft.position, leftStartPos + Vector3.left * moveDistance, moveSpeed * Time.deltaTime);
            wallFront.position = Vector3.MoveTowards(wallFront.position, frontStartPos + Vector3.back * moveDistance, moveSpeed * Time.deltaTime);
        }
    }

    public void ActivateEffect()
    {
        isActivated = true;
    }

    public void ResetEffect()
    {
        isActivated = false;
        wallRight.position = rightStartPos;
        wallLeft.position = leftStartPos;
        wallFront.position = frontStartPos;
    }
}
