using UnityEngine;

public class Touch3DButton : MonoBehaviour
{
    public string keyValue;

    void Awake()
    {
        // Si keyValue n’est pas défini dans l’inspecteur, on prend le nom du GameObject
        if (string.IsNullOrEmpty(keyValue))
            keyValue = gameObject.name;
    }

    public void OnPressed()
    {
        Debug.Log("Touche pressée : " + keyValue);
        Digicode3DManager.Instance.AddDigit(keyValue);

       
    }
}