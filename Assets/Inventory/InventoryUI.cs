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
    [Header("UI References")]
    public GameObject inventoryPanel; // Le panneau d'inventaire UI
    public Transform itemContainer; // Container pour les items dans l'UI
    public GameObject itemPrefab; // Prefab pour afficher un item
    public TMP_Text notificationText; // Texte pour notifications de pickup (optionnel)

    [Header("Configuration")]
    public KeyCode inventoryKey = KeyCode.I; // Touche pour ouvrir/fermer l'inventaire
    public float notificationDuration = 2f; // Durée d'affichage des notifications

    [Header("Item Icons")]
    public List<ItemIcon> itemIcons = new List<ItemIcon>(); // Liste des icônes pour chaque type d'objet
    public Sprite defaultIcon; // Icône par défaut si aucune icône spécifique n'est trouvée

    private bool isInventoryOpen = false;
    private Inventory inventory;
    private List<GameObject> itemUIElements = new List<GameObject>(); // Liste des éléments UI créés

    void Start()
    {
        // Trouver l'inventaire dans la scène
        inventory = FindFirstObjectByType<Inventory>();

        if (inventory == null)
        {
            Debug.LogError("[InventoryUI] Aucun Inventory trouvé dans la scène !");
        }
        else
        {
            Debug.Log("[InventoryUI] Inventory trouvé avec succès !");
        }

        // Vérifier que le panneau d'inventaire est assigné
        if (inventoryPanel == null)
        {
            Debug.LogError("[InventoryUI] Inventory Panel n'est pas assigné !");
        }
        else
        {
            Debug.Log("[InventoryUI] Inventory Panel assigné !");
        }

        // Vérifier que le container des items est assigné
        if (itemContainer == null)
        {
            Debug.LogError("[InventoryUI] Item Container n'est pas assigné !");
        }
        else
        {
            Debug.Log($"[InventoryUI] Item Container assigné : {itemContainer.name}");
        }


        // S'assurer que l'inventaire est fermé au démarrage
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
            Debug.Log("[InventoryUI] Inventory UI fermé au démarrage");
        }
    }
    void Update()
    {
        // Écouter la même touche que votre Inventory original
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

        // Gestion du curseur et de la pause
        if (isInventoryOpen)
        {
            Time.timeScale = 0f; // Pause le jeu
            Cursor.lockState = CursorLockMode.None; // Libère le curseur
            Cursor.visible = true;

            // Mettre à jour l'affichage des items
            UpdateInventoryDisplay();
        }
        else
        {
            Time.timeScale = 1f; // Reprend le jeu
            Cursor.lockState = CursorLockMode.Locked; // Verrouille le curseur
            Cursor.visible = false;
        }
    }

    public void OpenInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = true;
        inventoryPanel.SetActive(true);
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
        if (inventory == null || itemContainer == null) return;

        // Nettoyer les anciens éléments UI
        ClearInventoryDisplay();

        // Récupérer les items de l'inventaire existant
        List<string> items = inventory.GetItems();

        // Debug pour voir ce qui se passe
        Debug.Log($"[InventoryUI] Updating display. Found {items.Count} items in inventory:");
        for (int i = 0; i < items.Count; i++)
        {
            Debug.Log($"[InventoryUI] Item {i + 1}: '{items[i]}'");
        }

        // Vérifications avant de créer les UI
        Debug.Log($"[InventoryUI] itemContainer: {(itemContainer != null ? itemContainer.name : "NULL")}");
        Debug.Log($"[InventoryUI] itemPrefab: {(itemPrefab != null ? itemPrefab.name : "NULL")}");

        // Créer un élément UI pour chaque item
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

        // Activer temporairement le prefab pour l'instantiation
        bool wasActive = itemPrefab.activeSelf;
        itemPrefab.SetActive(true);

        // Instantier le prefab dans le container
        GameObject itemUI = Instantiate(itemPrefab, itemContainer);

        // Réactiver le template comme il était
        itemPrefab.SetActive(wasActive);

        // S'assurer que l'instance est active
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

        // Configurer le texte
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

        // Configurer la taille
        RectTransform rectTransform = itemUI.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(200, 60);
        }

        // Forcer le refresh du layout
        if (itemContainer.GetComponent<GridLayoutGroup>() != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemContainer.GetComponent<RectTransform>());
        }

        // Ajouter à la liste pour le nettoyage
        itemUIElements.Add(itemUI);
        Debug.Log($"[InventoryUI] Élément ajouté à la liste. Total : {itemUIElements.Count}");
        Debug.Log($"[InventoryUI] Enfants dans itemContainer : {itemContainer.childCount}");
    }

    Sprite GetIconForItem(string itemName)
    {
        // Chercher une icône spécifique pour cet item
        foreach (ItemIcon itemIcon in itemIcons)
        {
            // Comparaison flexible (ignore la casse et les espaces)
            if (string.Equals(itemIcon.itemName.Trim(), itemName.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return itemIcon.icon;
            }

            // Vérifier si le nom de l'item contient le nom configuré (pour plus de flexibilité)
            if (itemName.ToLower().Contains(itemIcon.itemName.ToLower().Trim()))
            {
                return itemIcon.icon;
            }
        }

        // Retourner l'icône par défaut si aucune correspondance
        return defaultIcon;
    }

    // Méthode publique pour forcer la mise à jour depuis d'autres scripts
    public void RefreshInventoryDisplay()
    {
        if (isInventoryOpen)
        {
            UpdateInventoryDisplay();
        }
    }

    // Méthode pour détecter quand un item est ajouté (appelée depuis l'extérieur)
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

        // Afficher une notification de pickup (optionnel)
        ShowPickupNotification();
    }

    // Nouvelle méthode pour forcer la mise à jour immédiatement
    public void ForceRefreshInventoryDisplay()
    {
        UpdateInventoryDisplay();
    }

    // Méthode pour afficher une notification quand un item est ramassé
    public void OnItemAdded(string itemName)
    {
        // Mettre à jour l'UI si elle est ouverte
        if (isInventoryOpen)
        {
            UpdateInventoryDisplay();
        }

        // Afficher une notification avec le nom de l'item
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
        // Remettre le temps à la normale au cas où
        Time.timeScale = 1f;

        // Nettoyer les éléments UI
        ClearInventoryDisplay();
    }
}