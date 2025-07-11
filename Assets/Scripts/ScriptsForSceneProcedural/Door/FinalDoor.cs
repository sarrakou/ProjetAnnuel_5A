using UnityEngine;

public class FinalDoor : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed = 2f;

    private bool isOpen = false;
    private Quaternion closedRot;
    private Quaternion openRot;

    void Start()
    {
        closedRot = transform.rotation;
        openRot = transform.rotation * Quaternion.Euler(0, 0, openAngle);
    }

    void Update()
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            isOpen ? openRot : closedRot,
            Time.deltaTime * speed
        );
    }

    public void Toggle()
    {
        if (!isOpen)
        {
            isOpen = true;
            Debug.Log("!!!!!!! Gagné ! Tu as trouvé la sortie !");
            // Tu peux aussi déclencher ici d'autres effets (sons, fin de jeu, etc.)
        }
    }
}
