using UnityEngine;

public class MissionObjects : MonoBehaviour, IInteractableBis
{
    [Header("Configuration de l'objet")]
    public string objectName = "Objet"; 
    public bool canBePickedUp = true; 
    public bool destroyAfterPickup = true; 
    
    private Inventory inventory;
    private bool hasBeenPickedUp = false;

    void Start()
    {
      
        inventory = FindFirstObjectByType<Inventory>();
        
        if (inventory == null)
        {
            Debug.LogError("❌ Aucun Inventory trouvé dans la scène !");
        }
    }

    public void Interact()
    {
        if (!canBePickedUp)
        {
            Debug.Log($"⚠️ {objectName} ne peut pas être ramassé.");
            return;
        }
        
        if (hasBeenPickedUp)
        {
            Debug.Log($"⚠️ {objectName} a déjà été ramassé.");
            return;
        }
        
        if (inventory != null)
        {
            // Déterminer le nom de l'objet
            string itemName = objectName;
            if (objectName == "Objet")
            {
                itemName = gameObject.name; 
            }
            
         
            if (!inventory.CanPlayerTakeItem(itemName))
            {
                string restrictionMessage = inventory.GetItemRestrictionMessage(itemName);
                Debug.Log($"🚫 {restrictionMessage}");
                
              
                StartCoroutine(ShowCantTakeEffect());
                
                return; 
            }
            
        
            inventory.AddItem(itemName);
            hasBeenPickedUp = true;
            
            Debug.Log($"✅ Vous avez ramassé : {objectName}");
            
           
            if (destroyAfterPickup)
            {
                Destroy(gameObject);
            }
            else
            {
               
                if (GetComponent<Renderer>() != null)
                    GetComponent<Renderer>().enabled = false;
                if (GetComponent<Collider>() != null)
                    GetComponent<Collider>().enabled = false;
            }
        }
        else
        {
            Debug.LogError("❌ Impossible de trouver l'inventaire !");
        }
    }
    
   
    private System.Collections.IEnumerator ShowCantTakeEffect()
    {
        Renderer objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            Color originalColor = objectRenderer.material.color;
            
           
            for (int i = 0; i < 3; i++)
            {
                objectRenderer.material.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                objectRenderer.material.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
 
    public bool CanBePickedUpNow()
    {
        if (!canBePickedUp || hasBeenPickedUp || inventory == null)
            return false;
            
        string itemName = objectName;
        if (objectName == "Objet")
        {
            itemName = gameObject.name;
        }
        
        return inventory.CanPlayerTakeItem(itemName);
    }
    
    
    public string GetRestrictionMessage()
    {
        if (inventory == null) return "";
        
        string itemName = objectName;
        if (objectName == "Objet")
        {
            itemName = gameObject.name;
        }
        
        return inventory.GetItemRestrictionMessage(itemName);
    }
}