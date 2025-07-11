using UnityEngine;

public class MissionObjects : MonoBehaviour, IInteractableBis
{
    [Header("Configuration de l'objet")]
    public string objectName = "Objet"; // Nom de l'objet à afficher dans l'inventaire
    public bool canBePickedUp = true; // Si l'objet peut être ramassé
    public bool destroyAfterPickup = true; // Si l'objet doit être détruit après ramassage
    
    private Inventory inventory;
    private bool hasBeenPickedUp = false;

    void Start()
    {
        // Trouver l'inventaire dans la scène
        inventory = FindObjectOfType<Inventory>();
        
        if (inventory == null)
        {
            Debug.LogError(" Aucun Inventory trouvé dans la scène !");
        }
    }

    public void Interact()
    {
        if (!canBePickedUp)
        {
            Debug.Log($" {objectName} ne peut pas être ramassé.");
            return;
        }
        
        if (hasBeenPickedUp)
        {
            Debug.Log($" {objectName} a déjà été ramassé.");
            return;
        }
        
        if (inventory != null)
        {
            
            string itemName = objectName;
            if (objectName == "Objet")
            {
                itemName = gameObject.name; 
            }
            
            inventory.AddItem(itemName);
            hasBeenPickedUp = true;
            
            Debug.Log($"✅ Vous avez ramassé : {objectName}");
            
            // Détruire l'objet si configuré pour cela
            if (destroyAfterPickup)
            {
                Destroy(gameObject);
            }
            else
            {
               
                GetComponent<Renderer>().enabled = false;
                GetComponent<Collider>().enabled = false;
            }
        }
        else
        {
            Debug.LogError("Impossible de trouver l'inventaire !");
        }
    }
}