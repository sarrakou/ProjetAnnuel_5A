using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject inventoryCanvas; 
    private bool isInventoryOpen = false;
    
    // Liste des objets dans l'inventaire
    private List<string> items = new List<string>();
    
    // Référence au GameManager pour valider les quêtes
    private GameManager gameManager;

    private void Start()
    {
        if (inventoryCanvas != null)
        {
            inventoryCanvas.SetActive(false);
        }
        gameManager = FindObjectOfType<GameManager>();
        
        // Ne pas afficher l'inventaire au démarrage
        Debug.Log(" Inventaire initialisé. Appuyez sur 'I' pour l'ouvrir.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            isInventoryOpen = !isInventoryOpen;
            if (inventoryCanvas != null)
            {
                inventoryCanvas.SetActive(isInventoryOpen);
            }
            
            // Afficher l'inventaire dans la console quand on l'ouvre
            DisplayInventory();
        }
    }
    
    // Ajouter un objet à l'inventaire
    public void AddItem(string itemName)
    {
        if (!items.Contains(itemName))
        {
            items.Add(itemName);
            Debug.Log($" Objet ajouté à l'inventaire : {itemName}");
            DisplayInventory();
            
            // Vérifier si cet objet complète une quête
            CheckQuestCompletion(itemName);
        }
        else
        {
            Debug.Log($"️ Objet déjà dans l'inventaire : {itemName}");
        }
    }
    
    // Vérifier si un objet est dans l'inventaire
    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }
    
    // Supprimer un objet de l'inventaire
    public void RemoveItem(string itemName)
    {
        if (items.Contains(itemName))
        {
            items.Remove(itemName);
            Debug.Log($"️ Objet retiré de l'inventaire : {itemName}");
            DisplayInventory();
        }
    }
    
    // Afficher l'inventaire dans la console (seulement si demandé)
    private void DisplayInventory()
    {
        if (isInventoryOpen) // Afficher seulement si l'inventaire est ouvert
        {
            Debug.Log(" === INVENTAIRE ===");
            if (items.Count == 0)
            {
                Debug.Log("   (Vide)");
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    Debug.Log($"   {i + 1}. {items[i]}");
                }
            }
            Debug.Log("==================");
        }
    }
    
    // Vérifier si un objet complète une quête
    private void CheckQuestCompletion(string itemName)
    {
        if (gameManager == null) 
        {
            Debug.LogError(" GameManager non trouvé !");
            return;
        }
        
        Debug.Log($" Vérification de quête pour l'objet : {itemName}");
        
       
        if (itemName.ToLower().Contains("clé") || itemName.ToLower().Contains("cle"))
        {
            Debug.Log("️ Clé détectée ! Tentative de complétion de la quête 2...");
            gameManager.CompleteQuestByName("Trouver la clé de la chambre principale");
        }
        
        if (itemName.ToLower().Contains("journal"))
        {
            gameManager.CompleteQuestByName("Trouver le journal du propriétaire");
        }
        if (itemName.ToLower().Contains("chambresecrete"))
        {
            gameManager.CompleteQuestByName("Récupérer la clé");
        }
    }
    
    // Getter pour la liste des objets (utile pour d'autres scripts)
    public List<string> GetItems()
    {
        return new List<string>(items); // Retourne une copie pour éviter les modifications externes
    }
}