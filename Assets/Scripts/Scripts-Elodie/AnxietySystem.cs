using UnityEngine;
using UnityEngine.UI;

public class AnxietySystem : MonoBehaviour
{
    [Header("Anxiety Settings")]
    public float anxiety = 0f;
    public float maxAnxiety = 100f;
    public float increaseRate = 5f;

    [Header("Camera Shake Settings")]
    public Camera playerCamera;
    public float maxShakeAmount = 0.1f; 
    private Vector3 originalLocalPos;
    private Vector3 targetShakeOffset;
    private Vector3 currentShakeOffset;

    [Header("Pill Settings")]
    public float pillEffectAmount = 100f; 
    public string pillItemName = "pillule"; 
    private Inventory inventory;

    public CharacterMovement characterMovement;

    void Start()
    {
        if (playerCamera != null)
            originalLocalPos = playerCamera.transform.localPosition;
        else
            Debug.LogError("playerCamera n'est pas assignée !");

        inventory = FindObjectOfType<Inventory>();
        if (inventory == null)
            Debug.LogError("Inventaire non trouvé !");
    }

    void Update()
    {
        anxiety += increaseRate * Time.deltaTime;
        anxiety = Mathf.Clamp(anxiety, 0f, maxAnxiety);

        

        if (anxiety > 70f)
        {
            if (characterMovement != null)
                characterMovement.SetCameraRepositioning(false);

            ApplyCameraShake();
        }
        else
        {
            ResetCameraEffects();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TryTakePill();
        }
    }


    void ApplyCameraShake()
    {
        float shakeIntensity = (anxiety - 70f) / (maxAnxiety - 70f);
        shakeIntensity = Mathf.Clamp01(shakeIntensity);

        float shakeAmount = Mathf.Lerp(0.005f, maxShakeAmount, shakeIntensity); 

        targetShakeOffset = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0f
        ) * shakeAmount;

        currentShakeOffset = Vector3.Lerp(currentShakeOffset, targetShakeOffset, Time.deltaTime * 10f);
        playerCamera.transform.localPosition = originalLocalPos + currentShakeOffset;
    }

    void ResetCameraEffects()
    {
        if (playerCamera != null)
            playerCamera.transform.localPosition = originalLocalPos;

        if (characterMovement != null)
            characterMovement.SetCameraRepositioning(true);
    }

    void TryTakePill()
    {
        if (inventory != null && inventory.HasItem(pillItemName))
        {
            anxiety -= pillEffectAmount;
            anxiety = Mathf.Clamp(anxiety, 0f, maxAnxiety);

            inventory.RemoveItem(pillItemName);
            Debug.Log("Pilule utilisée. Anxiété réduite !");
        }
        else
        {
            Debug.Log("Pas de pilule dans l'inventaire !");
        }
    }

    
}
