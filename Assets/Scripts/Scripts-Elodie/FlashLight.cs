using System.Collections;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    private Light flashLight;

    [Header("Batterie")]
    public float maxBatteryLife = 120f;
    public float currentBatteryLife;
    public float batteryDrainRate = 1f;
    public float rechargeAmount = 30f;

    [Header("Musique dans le noir")]
    public float timeBeforeMusic = 10f; // Temps avant déclenchement
    public AudioClip darknessMusic;
    private AudioSource audioSource;
    private float timeInDark = 0f;
    private bool musicPlaying = false;

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

        // Initialiser AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = darknessMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        HandleFlashlightToggle();
        HandleBatteryDrain();
        HandleBatteryRecharge();
        HandleDarknessMusic(); // Gère le son selon la lumière
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
            // Trouver la première pile dans l'inventaire
            string batteryItem = FindBatteryInInventory();
            
            if (batteryItem != null)
            {
                currentBatteryLife += rechargeAmount;
                currentBatteryLife = Mathf.Min(currentBatteryLife, maxBatteryLife);

                inventory.RemoveItem(batteryItem);
                Debug.Log($"🔋 {batteryItem} utilisée. Batterie rechargée !");

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

    // Méthode pour trouver n'importe quelle pile dans l'inventaire
    private string FindBatteryInInventory()
    {
        if (inventory == null) return null;
        
        var items = inventory.GetItems();
        
        foreach (string item in items)
        {
            // Vérifier si l'objet contient "pile" (insensible à la casse)
            if (item.ToLower().Contains("pile"))
            {
                return item; // Retourner le nom exact de la pile trouvée
            }
        }
        
        return null; // Aucune pile trouvée
    }

    void HandleDarknessMusic()
    {
        if (flashLight == null || !flashLight.enabled)
        {
            timeInDark += Time.deltaTime;

            if (!musicPlaying && timeInDark >= timeBeforeMusic)
            {
                audioSource.Play();
                musicPlaying = true;
            }
        }
        else
        {
            timeInDark = 0f;

            if (musicPlaying)
            {
                audioSource.Stop();
                musicPlaying = false;
            }
        }
    }
}