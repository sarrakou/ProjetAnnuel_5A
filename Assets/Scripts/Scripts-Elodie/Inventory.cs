using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public GameObject inventoryCanvas; 
    private bool isInventoryOpen = false;
    
    [Header("Image Display System")]
    public GameObject imageDisplayCanvas; // Canvas pour afficher les images
    public Image displayImage; // Component Image qui va afficher l'image
    public float imageDisplayDuration = 3f; // Durée d'affichage de l'image
    public string imageResourcePath = "Images/"; // Chemin vers les images dans Resources
    
    [Header("Objects with Images")]
    public List<string> objectsWithImages = new List<string>(); // Liste des objets qui ont une image à afficher
    
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
        
        // Désactiver le canvas d'affichage des images au démarrage
        if (imageDisplayCanvas != null)
        {
            imageDisplayCanvas.SetActive(false);
        }
        
        gameManager = FindObjectOfType<GameManager>();
        
        Debug.Log("🎒 Inventaire initialisé. Appuyez sur 'I' pour l'ouvrir.");
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
            
            DisplayInventory();
        }
        
        // Fermer l'affichage d'image avec Echap ou clic
        if (imageDisplayCanvas != null && imageDisplayCanvas.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
            {
                HideItemImage();
            }
        }
    }
    
    // Ajouter un objet à l'inventaire avec vérification de quête et affichage d'image
    public void AddItem(string itemName)
    {
        if (!items.Contains(itemName))
        {
            // Vérifier si le joueur peut prendre cet objet
            if (!CanTakeItem(itemName))
            {
                return; // Sortir si l'objet ne peut pas être pris
            }
            
            items.Add(itemName);
            Debug.Log($"✅ Objet ajouté à l'inventaire : {itemName}");
            
            // Afficher l'image de l'objet récupéré
            ShowItemImage(itemName);
            
            DisplayInventory();
            
            // Vérifier si cet objet complète une quête
            CheckQuestCompletion(itemName);
        }
        else
        {
            Debug.Log($"⚠️ Objet déjà dans l'inventaire : {itemName}");
        }
    }
    
    // Afficher l'image correspondant à l'objet récupéré
    private void ShowItemImage(string itemName)
    {
        // Vérifier si cet objet doit afficher une image
        if (!ShouldShowImage(itemName))
        {
            Debug.Log($"📷 Pas d'image configurée pour : {itemName}");
            return;
        }
        
        if (imageDisplayCanvas == null || displayImage == null)
        {
            Debug.LogWarning("⚠️ Canvas ou Image component manquant pour l'affichage !");
            return;
        }
        
        // Charger l'image depuis Resources
        string imagePath = imageResourcePath + itemName;
        Sprite itemSprite = Resources.Load<Sprite>(imagePath);
        
        if (itemSprite != null)
        {
            // Afficher l'image
            displayImage.sprite = itemSprite;
            imageDisplayCanvas.SetActive(true);
            
            Debug.Log($"📷 Image affichée pour : {itemName}");
            
            // Cacher automatiquement après la durée définie
            Invoke(nameof(HideItemImage), imageDisplayDuration);
        }
        else
        {
            Debug.LogWarning($"⚠️ Image non trouvée à : Resources/{imagePath}");
            
            // Fallback : afficher le canvas avec une image par défaut ou vide
            imageDisplayCanvas.SetActive(true);
            Invoke(nameof(HideItemImage), imageDisplayDuration);
        }
    }
    
    // Vérifier si un objet doit afficher une image
    private bool ShouldShowImage(string itemName)
    {
        // Vérifier si l'objet est dans la liste des objets avec images
        foreach (string objectWithImage in objectsWithImages)
        {
            if (string.Equals(itemName, objectWithImage, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
    
    // Cacher l'affichage de l'image
    private void HideItemImage()
    {
        if (imageDisplayCanvas != null)
        {
            imageDisplayCanvas.SetActive(false);
            Debug.Log("📷 Image cachée");
        }
        
        // Annuler l'invoke automatique si l'utilisateur ferme manuellement
        CancelInvoke(nameof(HideItemImage));
    }
    
    // Vérifier si le joueur peut prendre un objet selon les quêtes disponibles
    private bool CanTakeItem(string itemName)
    {
        if (gameManager == null) 
        {
            Debug.LogError("❌ GameManager non trouvé !");
            return false;
        }
        
        string itemLower = itemName.ToLower();
        
        // Vérifications pour chaque type d'objet
        
        // 1. Clé de la chambre principale (Quête 2)
        if (itemLower.Contains("clé") || itemLower.Contains("cle"))
        {
            if (!IsQuestAvailable("Trouver la clé de la chambre principale"))
            {
                Debug.Log("🔒 Vous ne pouvez pas encore prendre cette clé. Explorez d'abord la maison !");
                return false;
            }
        }
        
        // 2. Journal du propriétaire (Quête 3)
        if (itemLower.Contains("journal"))
        {
            if (!IsQuestAvailable("Trouver le journal du propriétaire"))
            {
                Debug.Log("🔒 Vous ne pouvez pas encore prendre ce journal. Trouvez d'abord la clé de la chambre !");
                return false;
            }
        }
        
        // 3. Clé du coffre-fort / Clé secrète (Quête 5)
        if (itemLower.Contains("chambresecrete") || itemLower.Contains("coffre"))
        {
            if (!IsQuestAvailable("Récupérer la clé"))
            {
                Debug.Log("🔒 Vous ne pouvez pas encore accéder à cette clé. Trouvez d'abord le coffre-fort !");
                return false;
            }
        }
        
        // 4. Poupée Annabelle (Quête 6)
        if (itemLower.Contains("annabelle") || itemLower.Contains("poupée") || itemLower.Contains("poupee"))
        {
            if (!IsQuestAvailable("D'où vient ce bruit ?"))
            {
                Debug.Log("🔒 Cette poupée vous fait peur... Vous n'osez pas la toucher maintenant.");
                return false;
            }
        }
        
        // 5. Objets génériques (toujours autorisés)
        // Comme les indices, notes, etc.
        
        Debug.Log($"✅ Vous pouvez prendre : {itemName}");
        return true;
    }
    
    // Vérifier si une quête est disponible (peut être complétée)
    private bool IsQuestAvailable(string questName)
    {
        if (gameManager == null || gameManager.quests == null) return false;
        
        for (int i = 0; i < gameManager.quests.Count; i++)
        {
            if (gameManager.quests[i].questName == questName)
            {
                // Vérifier que toutes les quêtes précédentes sont complétées
                for (int j = 0; j < i; j++)
                {
                    if (!gameManager.quests[j].isCompleted)
                    {
                        return false; // Une quête précédente n'est pas complétée
                    }
                }
                
                // La quête est disponible si elle n'est pas déjà complétée
                return !gameManager.quests[i].isCompleted;
            }
        }
        
        return false; // Quête non trouvée
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
            Debug.Log($"➖ Objet retiré de l'inventaire : {itemName}");
            DisplayInventory();
        }
    }
    
    // Afficher l'inventaire dans la console (seulement si demandé)
    private void DisplayInventory()
    {
        if (isInventoryOpen) // Afficher seulement si l'inventaire est ouvert
        {
            Debug.Log("🎒 === INVENTAIRE ===");
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
            Debug.LogError("❌ GameManager non trouvé !");
            return;
        }
        
        Debug.Log($"🎯 Vérification de quête pour l'objet : {itemName}");
        
        string itemLower = itemName.ToLower();
        
        // Clé de la chambre principale
        if (itemLower.Contains("clé") || itemLower.Contains("cle"))
        {
            Debug.Log("🗝️ Clé détectée ! Tentative de complétion de la quête 2...");
            gameManager.CompleteQuestByName("Trouver la clé de la chambre principale");
        }
        
        // Journal du propriétaire
        if (itemLower.Contains("journal"))
        {
            Debug.Log("📖 Journal détecté ! Tentative de complétion de la quête 3...");
            gameManager.CompleteQuestByName("Trouver le journal du propriétaire");
        }
        
        // Clé du coffre-fort
        if (itemLower.Contains("chambresecrete") || itemLower.Contains("coffre"))
        {
            Debug.Log("🔑 Clé secrète détectée ! Tentative de complétion de la quête 5...");
            gameManager.CompleteQuestByName("Récupérer la clé");
        }
        
        // Poupée Annabelle
        if (itemLower.Contains("annabelle") || itemLower.Contains("poupée") || itemLower.Contains("poupee"))
        {
            Debug.Log("🪆 Poupée Annabelle détectée ! Tentative de complétion de la quête 6...");
            gameManager.CompleteQuestByName("D'où vient ce bruit ?");
        }
    }
    
   
    public bool CanPlayerTakeItem(string itemName)
    {
        return CanTakeItem(itemName);
    }
    
    public List<string> GetItems()
    {
        return new List<string>(items); 
    }
    
    
    public string GetItemRestrictionMessage(string itemName)
    {
        if (CanTakeItem(itemName)) return "";
        
        string itemLower = itemName.ToLower();
        
        if (itemLower.Contains("clé") || itemLower.Contains("cle"))
        {
            return "🔒 Vous devez d'abord explorer la maison et allumer les lumières.";
        }
        
        if (itemLower.Contains("journal"))
        {
            return "🔒 Vous devez d'abord trouver la clé de la chambre principale.";
        }
        
        if (itemLower.Contains("chambresecrete") || itemLower.Contains("coffre"))
        {
            return "🔒 Vous devez d'abord trouver le coffre-fort.";
        }
        
        if (itemLower.Contains("annabelle") || itemLower.Contains("poupée") || itemLower.Contains("poupee"))
        {
            return "🔒 Cette poupée vous effraie... Attendez d'entendre quelque chose.";
        }
        
        return "🔒 Vous ne pouvez pas prendre cet objet maintenant.";
    }
}