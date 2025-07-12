using UnityEngine;

public class LibraryInteractable : MonoBehaviour, IInteractableBis
{
    [Header("Configuration de la librairie")]
    public float moveDistance = 70f; // Distance de déplacement vers la droite
    public float moveSpeed = 2f; // Vitesse de déplacement
    public string questToComplete = "Trouver la porte secrète"; // Nom de la quête à compléter
    
    private bool hasBeenMoved = false;
    private bool isMoving = false;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private GameManager gameManager;

    void Start()
    {
        // Stocker la position originale
        originalPosition = transform.position;
        targetPosition = originalPosition + Vector3.forward * -moveDistance; // Déplacement en Z vers l'arrière
        
        // Trouver le GameManager dans la scène
        gameManager = FindObjectOfType<GameManager>();
        
        if (gameManager == null)
        {
            Debug.LogError(" Aucun GameManager trouvé dans la scène !");
        }
    }

    void Update()
    {
        // Si la librairie est en train de bouger
        if (isMoving)
        {
            // Déplacer progressivement vers la position cible
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // Vérifier si on a atteint la position cible
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                Debug.Log(" La librairie a fini de se déplacer.");
            }
        }
    }

    public void Interact()
    {
        if (hasBeenMoved)
        {
            Debug.Log(" La librairie a déjà été déplacée.");
            return;
        }
        
        if (isMoving)
        {
            Debug.Log(" La librairie est déjà en train de bouger...");
            return;
        }
        
        // Commencer le déplacement
        Debug.Log(" Vous poussez la librairie...");
        isMoving = true;
        hasBeenMoved = true;
        
        // Compléter la quête
        if (gameManager != null)
        {
            gameManager.CompleteQuestByName(questToComplete);
            Debug.Log($" Quête complétée : {questToComplete}");
        }
        else
        {
            Debug.LogError(" Impossible de compléter la quête : GameManager non trouvé !");
        }
    }
}