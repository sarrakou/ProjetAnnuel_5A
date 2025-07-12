using UnityEngine;
using UnityEngine.UI; // Pour Image/Text
using TMPro; 

interface IInteractableBis
{
    void Interact();
}

public class Interactor : MonoBehaviour
{
    public Transform InteractorSource;
    public float InteractRange = 2f;
    public float SphereRadius = 0.3f;

    public GameObject interactPromptUI; // ← assigné dans l’inspecteur
    private bool isLookingAtInteractable;

    void Update()
    {
        bool foundInteractable = false;

        // SphereCast pour interaction plus facile
        if (Physics.SphereCast(InteractorSource.position, SphereRadius, InteractorSource.forward, out RaycastHit hit, InteractRange))
        {
            if (hit.collider.TryGetComponent(out IInteractableBis interactObj))
            {
                foundInteractable = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactObj.Interact();
                    Debug.Log($"✅ OK: {hit.collider.gameObject.name}");

                }
            }
        }

        // Activer ou désactiver le prompt E
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(foundInteractable);
        }
    }
}