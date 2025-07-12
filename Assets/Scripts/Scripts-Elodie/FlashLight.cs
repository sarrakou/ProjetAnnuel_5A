using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    private Light flashLight;

    void Start()
    {
        flashLight = GetComponent<Light>();
        
        if (flashLight == null)
        {
            Debug.LogWarning("Aucune lumière trouvée sur cet objet !");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (flashLight != null)
            {
                flashLight.enabled = !flashLight.enabled;
            }
        }
    }
}