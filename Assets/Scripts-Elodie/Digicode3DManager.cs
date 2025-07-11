using UnityEngine;

public class Digicode3DManager : MonoBehaviour
{
    public static Digicode3DManager Instance;

    [Header("Configuration")]
    public string correctCode = "1998";
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip incorrectSound;
    public AudioClip buttonSound;
    
    [Header("Animation")]
    public Animator safeAnimator; // Pour animer l'ouverture du coffre
    public GameObject safeContent; // Contenu du coffre (clé, etc.)
    
    private string inputCode = "";
    private bool isUnlocked = false;
    private GameManager gameManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        // Cacher le contenu du coffre au début
        if (safeContent != null)
            safeContent.SetActive(false);
    }

    public void AddDigit(string digit)
    {
        if (isUnlocked) return; 
        
        inputCode += digit;
        Debug.Log("Code actuel : " + inputCode);
        
        
        PlaySound(buttonSound);

        if (inputCode.Length >= correctCode.Length)
        {
            if (inputCode == correctCode)
            {
                Debug.Log(" Code correct !");
                UnlockSafe();
            }
            else
            {
                Debug.Log(" Code incorrect.");
                PlaySound(incorrectSound);
                inputCode = ""; // reset après échec
            }
        }
    }
    
    void UnlockSafe()
    {
        isUnlocked = true;
        PlaySound(correctSound);
        
        // Animation d'ouverture
        if (safeAnimator != null)
            safeAnimator.SetTrigger("Open");
        
        // Activer le contenu du coffre
        if (safeContent != null)
            safeContent.SetActive(true);
            
        // Compléter la quête du coffre-fort
        if (gameManager != null)
            gameManager.CompleteQuestByName("Trouver le coffre-fort");
            
        Debug.Log(" Coffre-fort ouvert !");
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
    
   
    public void ResetCode()
    {
        inputCode = "";
    }
    
    // Getter pour vérifier si le coffre est ouvert
    public bool IsUnlocked()
    {
        return isUnlocked;
    }
}