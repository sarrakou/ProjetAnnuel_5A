using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    private Light flashLight;

    [Header("Batterie")]
    public float maxBatteryLife =  120f;
    public float currentBatteryLife;
    public float batteryDrainRate = 1f;
    public float rechargeAmount = 30f;

    private Inventory inventory;

    void Start()
    {
        flashLight = GetComponent<Light>();
        if (flashLight == null)
        {
            Debug.LogWarning("Aucune lumière trouvée sur cet objet !");
        }

        inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Inventaire non trouvé !");
        }

        currentBatteryLife = maxBatteryLife;
    }

    void Update()
    {
        HandleFlashlightToggle();
        HandleBatteryDrain();
        HandleBatteryRecharge();
    }

    void HandleFlashlightToggle()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (flashLight != null && currentBatteryLife > 0)
            {
                flashLight.enabled = !flashLight.enabled;
            }
        }
    }

    void HandleBatteryDrain()
    {
        if (flashLight != null && flashLight.enabled)
        {
            currentBatteryLife -= batteryDrainRate * Time.deltaTime;
            currentBatteryLife = Mathf.Max(currentBatteryLife, 0f);

            if (currentBatteryLife <= 0)
            {
                flashLight.enabled = false;
            }
        }
    }

    void HandleBatteryRecharge()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (inventory != null && inventory.HasItem("pile"))
            {
                currentBatteryLife += rechargeAmount;
                currentBatteryLife = Mathf.Min(currentBatteryLife, maxBatteryLife);

                inventory.RemoveItem("pile");
                Debug.Log("🔋 Pile utilisée. Batterie rechargée !");

                if (flashLight != null && !flashLight.enabled)
                {
                    flashLight.enabled = true;
                    Debug.Log("💡 Lampe rallumée automatiquement !");
                }
            }

            else
            {
                Debug.Log("❌ Pas de pile dans l'inventaire !");
            }
        }
    }
}
