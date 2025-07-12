using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorrorEvents : MonoBehaviour
{
    public List<Action<Vector3>> eventList = new List<Action<Vector3>>();
    public Canvas spiderCanvas;
    private Camera playerCamera;

    private void Awake()
    {
        playerCamera = Camera.main;

        // eventList.Add(Event_SpawnEnemy);
        // eventList.Add(Event_SpawnChest);
        // eventList.Add(Event_TriggerTrap);
        // eventList.Add(Event_Vignette);
        // eventList.Add(Event_ClaustroFOV);
        eventList.Add(Event_InsecteSurEcran);
    }

    public void TriggerRandomEvent(Vector3 position)
    {
        if (eventList.Count == 0) return;
        int eventIndex = UnityEngine.Random.Range(0, eventList.Count);
        eventList[eventIndex].Invoke(position);
    }

    void Event_SpawnEnemy(Vector3 pos)
    {
        Debug.Log(" Ennemi spawn à " + pos);
    }

    void Event_SpawnChest(Vector3 pos)
    {
        Debug.Log(" Coffre spawn à " + pos);
    }

    void Event_TriggerTrap(Vector3 pos)
    {
        Debug.Log(" Piège activé à " + pos);
    }

    void Event_Vignette(Vector3 pos)
    {
        Debug.Log(" Vignette activée à " + pos);
        StartCoroutine(VignetteCoroutine());
    }

    IEnumerator VignetteCoroutine()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        Image vignetteImage = null;
        Transform existing = canvas.transform.Find("VignetteImage");
        if (existing != null)
        {
            vignetteImage = existing.GetComponent<Image>();
        }
        else
        {
            GameObject imgGO = new GameObject("VignetteImage");
            imgGO.transform.SetParent(canvas.transform, false);
            vignetteImage = imgGO.AddComponent<Image>();
            vignetteImage.color = new Color(0, 0, 0, 0);
            vignetteImage.rectTransform.anchorMin = Vector2.zero;
            vignetteImage.rectTransform.anchorMax = Vector2.one;
            vignetteImage.rectTransform.offsetMin = Vector2.zero;
            vignetteImage.rectTransform.offsetMax = Vector2.zero;
        }

        float duration = 5f;
        float maxAlpha = 0.8f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, maxAlpha, timer / duration);
            vignetteImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    void Event_ClaustroFOV(Vector3 pos)
    {
        Debug.Log(" Claustrophobie FOV activé à " + pos);
        if (playerCamera != null)
        {
            StartCoroutine(ClaustroFOVCoroutine());
        }
        else
        {
            Debug.LogWarning("Player camera not found!");
        }
    }

    IEnumerator ClaustroFOVCoroutine()
    {
        float normalFOV = 60f;
        float claustroFOV = 30f;
        float duration = 7f;
        float timer = 0f;

        while (timer < duration / 2f)
        {
            timer += Time.deltaTime;
            playerCamera.fieldOfView = Mathf.Lerp(normalFOV, claustroFOV, timer / (duration / 2f));
            yield return null;
        }

        timer = 0f;
        while (timer < duration / 4f)
        {
            timer += Time.deltaTime;
            playerCamera.fieldOfView = claustroFOV;
            yield return null;
        }

        timer = 0f;
        while (timer < duration / 4f)
        {
            timer += Time.deltaTime;
            playerCamera.fieldOfView = Mathf.Lerp(claustroFOV, normalFOV, timer / (duration / 4f));
            yield return null;
        }

        playerCamera.fieldOfView = normalFOV;
    }

    // Nouvel event : insecte animé rampant à l’écran
    void Event_InsecteSurEcran(Vector3 pos)
    {
        Debug.Log("🐜 Insecte rampant à l'écran à " + pos);
        StartCoroutine(InsecteCoroutine());
    }

    IEnumerator InsecteCoroutine()
    {
        GameObject prefab = Resources.Load<GameObject>("SpiderAnimation");
        if (prefab == null)
        {
            Debug.LogWarning(" InsectePrefab non trouvé dans Resources !");
            yield break;
        }

        GameObject insectGO = Instantiate(prefab);

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Player camera not found!");
            yield break;
        }

        insectGO.transform.SetParent(cam.transform);
        insectGO.transform.localPosition = new Vector3(0f, 0f, 2f);
        insectGO.transform.localRotation = Quaternion.identity;
        insectGO.transform.localScale = Vector3.one * 0.8f;


        yield return new WaitForSeconds(5f);

        Destroy(insectGO);
    }




}
