using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class ItemIcon
{
    public string itemName;
    public Sprite icon;
}

public class InventoryUI : MonoBehaviour
{
    [Header("Resources Configuration")]
    public string inventoryPanelResourcePath = "Inventory/InventoryPanel"; 
    public string itemPrefabResourcePath = "Inventory/ItemPrefab";

    [Header("UI References (Assignées automatiquement)")]
    public GameObject inventoryPanel; 
    public Transform itemContainer;
    public GameObject itemPrefab;
    public TMP_Text notificationText;

    [Header("Configuration")]
    public KeyCode inventoryKey = KeyCode.I; 
    public float notificationDuration = 2f; 

    [Header("Item Icons")]
    public List<ItemIcon> itemIcons = new List<ItemIcon>(); 
    public Sprite defaultIcon; 

    private bool isInventoryOpen = false;
    private Inventory inventory;
    private List<GameObject> itemUIElements = new List<GameObject>(); 
    private GameObject instantiatedInventoryPanel; 
    private Canvas uiCanvas;

    void Start()
    {
        
        inventory = FindFirstObjectByType<Inventory>();

        if (inventory == null)
        {
            Debug.LogError("[InventoryUI] Aucun Inventory trouvé dans la scène !");
        }
        else
        {
            Debug.Log("[InventoryUI] Inventory trouvé avec succès !");
        }

     
        LoadInventoryUIFromResources();
    }

    void LoadInventoryUIFromResources()
    {
       
        uiCanvas = FindFirstObjectByType<Canvas>();
        if (uiCanvas == null)
        {
            Debug.Log("[InventoryUI] Aucun Canvas trouvé, création d'un nouveau Canvas");
            GameObject canvasGO = new GameObject("InventoryCanvas");
            uiCanvas = canvasGO.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = 100;
            
            // Ajouter CanvasScaler et GraphicRaycaster
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        
        if (inventoryPanel == null)
        {
            GameObject panelPrefab = Resources.Load<GameObject>(inventoryPanelResourcePath);
            if (panelPrefab != null)
            {
                instantiatedInventoryPanel = Instantiate(panelPrefab, uiCanvas.transform);
                inventoryPanel = instantiatedInventoryPanel;
                Debug.Log("[InventoryUI] InventoryPanel chargé et instantié depuis Resources");
            }
            else
            {
                Debug.LogError($"[InventoryUI] Impossible de charger le prefab à : Resources/{inventoryPanelResourcePath}");
                return;
            }
        }

      
        if (itemPrefab == null)
        {
            itemPrefab = Resources.Load<GameObject>(itemPrefabResourcePath);
            if (itemPrefab != null)
            {
                Debug.Log("[InventoryUI] ItemPrefab chargé depuis Resources");
            }
            else
            {
                Debug.LogError($"[InventoryUI] Impossible de charger le prefab à : Resources/{itemPrefabResourcePath}");
            }
        }

        
        if (inventoryPanel != null)
        {
            FindContentContainer();
            FindNotificationText();
        }

   
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
            Debug.Log("[InventoryUI] Inventory UI fermé au démarrage");
        }
    }

    void FindContentContainer()
    {
        if (inventoryPanel == null) 
        {
            Debug.LogError("[InventoryUI] Impossible de chercher Content : inventoryPanel est null");
            return;
        }

       
        Transform foundContent = inventoryPanel.transform.Find("Content");
        
        if (foundContent == null)
        {
           
            foundContent = FindChildRecursive(inventoryPanel.transform, "Content");
        }

        if (foundContent == null)
        {
            
            foundContent = FindChildByNameContaining(inventoryPanel.transform, "content");
        }

        if (foundContent == null)
        {
            
            ScrollRect scrollRect = inventoryPanel.GetComponentInChildren<ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
            {
                foundContent = scrollRect.content;
                Debug.Log("[InventoryUI] Content trouvé via ScrollRect");
            }
        }

        if (foundContent != null)
        {
            itemContainer = foundContent;
            Debug.Log($"[InventoryUI] Content trouvé automatiquement : {itemContainer.name} dans {itemContainer.parent.name}");
        }
        else
        {
            Debug.LogError("[InventoryUI] Impossible de trouver un GameObject 'Content' dans l'inventoryPanel !");
            Debug.Log("[InventoryUI] Hiérarchie de l'inventoryPanel :");
            LogHierarchy(inventoryPanel.transform, 0);
        }
    }

    void FindNotificationText()
    {
        if (inventoryPanel == null) return;

   
        TMP_Text[] texts = inventoryPanel.GetComponentsInChildren<TMP_Text>();
        foreach (TMP_Text text in texts)
        {
            if (text.name.ToLower().Contains("notification") || 
                text.name.ToLower().Contains("pickup") ||
                text.name.ToLower().Contains("message"))
            {
                notificationText = text;
                Debug.Log($"[InventoryUI] Notification text trouvé : {notificationText.name}");
                break;
            }
        }

        if (notificationText == null)
        {
            Debug.Log("[InventoryUI] Aucun texte de notification trouvé");
        }
    }

    Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Equals(childName, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
            
            Transform result = FindChildRecursive(child, childName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    Transform FindChildByNameContaining(Transform parent, string nameContains)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains(nameContains.ToLower()))
            {
                return child;
            }
            
            Transform result = FindChildByNameContaining(child, nameContains);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    void LogHierarchy(Transform parent, int depth)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"[InventoryUI] {indent}- {parent.name}");
        
        foreach (Transform child in parent)
        {
            LogHierarchy(child, depth + 1);
        }
    }

    void Update()
    {
    
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

   
        if (isInventoryOpen)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true;

        
            if (itemContainer == null)
            {
                FindContentContainer();
            }

            UpdateInventoryDisplay();
        }
        else
        {
            Time.timeScale = 1f; 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OpenInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = true;
        inventoryPanel.SetActive(true);
        
      
        if (itemContainer == null)
        {
            FindContentContainer();
        }
        
        UpdateInventoryDisplay();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = false;
        inventoryPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UpdateInventoryDisplay()
    {
        if (inventory == null) 
        {
            Debug.LogError("[InventoryUI] Inventory est null, impossible de mettre à jour l'affichage");
            return;
        }

        if (itemContainer == null) 
        {
            Debug.LogError("[InventoryUI] itemContainer est null, impossible de mettre à jour l'affichage");
            return;
        }

      
        ClearInventoryDisplay();

      
        List<string> items = inventory.GetItems();

      
        Debug.Log($"[InventoryUI] Updating display. Found {items.Count} items in inventory:");
        for (int i = 0; i < items.Count; i++)
        {
            Debug.Log($"[InventoryUI] Item {i + 1}: '{items[i]}'");
        }

      
        Debug.Log($"[InventoryUI] itemContainer: {(itemContainer != null ? itemContainer.name : "NULL")}");
        Debug.Log($"[InventoryUI] itemPrefab: {(itemPrefab != null ? itemPrefab.name : "NULL")}");

       
        int successfulCreations = 0;
        foreach (string item in items)
        {
            Debug.Log($"[InventoryUI] Tentative de création UI pour: '{item}'");
            try
            {
                CreateItemUI(item);
                successfulCreations++;
                Debug.Log($"[InventoryUI] Succès pour: '{item}'");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InventoryUI] Erreur lors de la création UI pour '{item}': {e.Message}");
            }
        }

        Debug.Log($"[InventoryUI] Created {successfulCreations} UI elements successfully");
        Debug.Log($"[InventoryUI] itemUIElements.Count = {itemUIElements.Count}");
    }

    void ClearInventoryDisplay()
    {
        // Détruire tous les éléments UI existants
        foreach (GameObject uiElement in itemUIElements)
        {
            if (uiElement != null)
            {
                Destroy(uiElement);
            }
        }
        itemUIElements.Clear();
    }

    void CreateItemUI(string itemName)
    {
        Debug.Log($"[InventoryUI] Création d'un élément UI pour : {itemName}");

        if (itemPrefab == null)
        {
            Debug.LogError("[InventoryUI] itemPrefab est null !");
            return;
        }

        if (itemContainer == null)
        {
            Debug.LogError("[InventoryUI] itemContainer est null !");
            return;
        }

      
        bool wasActive = itemPrefab.activeSelf;
        itemPrefab.SetActive(true);

   
        GameObject itemUI = Instantiate(itemPrefab, itemContainer);

      
        itemPrefab.SetActive(wasActive);

      
        itemUI.SetActive(true);

        Debug.Log($"[InventoryUI] GameObject créé : {itemUI.name} dans {itemContainer.name}");
        Debug.Log($"[InventoryUI] Position dans hiérarchie : {itemUI.transform.parent.name}");

        // Configurer l'icône
        Transform iconTransform = itemUI.transform.Find("Icon");
        if (iconTransform != null)
        {
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                Sprite iconToUse = GetIconForItem(itemName);
                if (iconToUse != null)
                {
                    iconImage.sprite = iconToUse;
                    iconImage.color = Color.white;
                    Debug.Log($"[InventoryUI] Icône assignée pour {itemName}");
                }
                else
                {
                    iconImage.color = Color.clear;
                    Debug.Log($"[InventoryUI] Pas d'icône trouvée pour {itemName}, utilisation de l'icône par défaut");
                }
            }
        }
        else
        {
            Debug.LogWarning("[InventoryUI] Pas d'objet 'Icon' trouvé dans le prefab");
        }

       
        TMP_Text itemText = itemUI.GetComponentInChildren<TMP_Text>();
        if (itemText != null)
        {
            itemText.text = itemName;
            Debug.Log($"[InventoryUI] Texte configuré : {itemName}");
        }
        else
        {
            Debug.LogWarning("[InventoryUI] Pas de composant Text trouvé dans le prefab");
        }

        RectTransform rectTransform = itemUI.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(200, 60);
        }

      
        if (itemContainer.GetComponent<GridLayoutGroup>() != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemContainer.GetComponent<RectTransform>());
        }

       
        itemUIElements.Add(itemUI);
        Debug.Log($"[InventoryUI] Élément ajouté à la liste. Total : {itemUIElements.Count}");
        Debug.Log($"[InventoryUI] Enfants dans itemContainer : {itemContainer.childCount}");
    }

    Sprite GetIconForItem(string itemName)
    {
        
        foreach (ItemIcon itemIcon in itemIcons)
        {
            
            if (string.Equals(itemIcon.itemName.Trim(), itemName.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return itemIcon.icon;
            }

           
            if (itemName.ToLower().Contains(itemIcon.itemName.ToLower().Trim()))
            {
                return itemIcon.icon;
            }
        }

        
        return defaultIcon;
    }

  
    public void RefreshInventoryDisplay()
    {
        if (isInventoryOpen)
        {
            UpdateInventoryDisplay();
        }
    }

    public void OnItemAdded()
    {
        Debug.Log("[InventoryUI] OnItemAdded() appelée !");

        // Mettre à jour l'UI si elle est ouverte
        if (isInventoryOpen)
        {
            Debug.Log("[InventoryUI] Inventory est ouvert, mise à jour de l'affichage...");
            UpdateInventoryDisplay();
        }
        else
        {
            Debug.Log("[InventoryUI] Inventory fermé, pas de mise à jour visuelle pour le moment");
        }

     
        ShowPickupNotification();
    }

    
    public void ForceRefreshInventoryDisplay()
    {
        UpdateInventoryDisplay();
    }

    public void OnItemAdded(string itemName)
    {
       
        if (isInventoryOpen)
        {
            UpdateInventoryDisplay();
        }

      
        ShowPickupNotification(itemName);
    }

    void ShowPickupNotification(string itemName = "")
    {
        if (notificationText == null) return;

        string message = string.IsNullOrEmpty(itemName) ?
            "Item ajouté à l'inventaire!" :
            $" {itemName} ajouté!";

        StartCoroutine(DisplayNotification(message));
    }

    System.Collections.IEnumerator DisplayNotification(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(notificationDuration);

        notificationText.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
      
        Time.timeScale = 1f;

      
        ClearInventoryDisplay();

  
        if (instantiatedInventoryPanel != null)
        {
            Destroy(instantiatedInventoryPanel);
        }
    }
}