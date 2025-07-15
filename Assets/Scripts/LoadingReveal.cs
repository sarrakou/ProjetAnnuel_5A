using UnityEngine;
using UnityEngine.UI;

public class LoadingReveal : MonoBehaviour
{
    public RectTransform maskTransform;
    public float duration = 10f;

    private float timeElapsed = 0f;
    private float startHeight;
    private float targetHeight;

    void Start()
    {
        targetHeight = maskTransform.rect.height;
        startHeight = 0;
        SetMaskHeight(startHeight);
    }

    void Update()
    {
        if (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float newHeight = Mathf.Lerp(startHeight, targetHeight, timeElapsed / duration);
            SetMaskHeight(newHeight);
        }
    }

    void SetMaskHeight(float height)
    {
        maskTransform.sizeDelta = new Vector2(maskTransform.sizeDelta.x, height);
    }
}
