using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class AnxietySystem : MonoBehaviour
{
    [Header("Anxiety Settings")]
    public float anxiety = 0f;
    public float maxAnxiety = 100f;
    public float increaseRate = 10f;

    [Header("UI")]
    public Slider anxietySlider;

    [Header("Effects")]
    public Camera playerCamera;
    public float shakeAmount = 0.1f;
    private Vector3 originalCamPos;

    public PostProcessVolume blurVolume;
    private DepthOfField depthOfField;

    void Start()
    {
        if (playerCamera != null)
            originalCamPos = playerCamera.transform.localPosition;

        if (blurVolume != null)
        {
            blurVolume.profile.TryGetSettings(out depthOfField);
        
            
            if (depthOfField != null)
            {
                depthOfField.enabled.value = true;
                depthOfField.focusDistance.value = 0.1f;
                depthOfField.aperture.value = 0.1f; 
            }
        }
    }
    void Update()
    {
    
        anxiety += increaseRate * Time.deltaTime;
        anxiety = Mathf.Clamp(anxiety, 0f, maxAnxiety);

   
        if (anxietySlider != null)
            anxietySlider.value = anxiety / maxAnxiety;

       
        if (anxiety > 70f)
            ApplyCameraEffects();
        else
            ResetCameraEffects();

      
        if (depthOfField != null)
        {
           
            float minFocus = 0.5f;  // Distance proche
            float maxFocus = 10f;   // Distance loin
            depthOfField.focusDistance.value = Mathf.Lerp(maxFocus, minFocus, anxiety / maxAnxiety);
            
            depthOfField.aperture.value = Mathf.Lerp(32f, 0.1f, anxiety / maxAnxiety);
        }
    }

    void ApplyCameraEffects()
    {
        if (playerCamera != null)
        {
            Vector3 shake = Random.insideUnitSphere * shakeAmount;
            playerCamera.transform.localPosition = originalCamPos + shake;
        }

        if (blurVolume != null)
            blurVolume.enabled = true;
    }

    void ResetCameraEffects()
    {
        if (playerCamera != null)
            playerCamera.transform.localPosition = originalCamPos;

        if (blurVolume != null)
            blurVolume.enabled = false;
    }
}
