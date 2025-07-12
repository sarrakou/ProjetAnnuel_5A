using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DigiCodeManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject digiCodeCanvas;
    public Button[] numberButtons;
    public Button clearButton;

    [Header("Settings")]
    public string correctCode = "1998";
    public int maxCodeLength = 4;
    [SerializeField] private GameObject targetObject;

    private string currentCode = "";
    private GameManager gameManager;
    private bool isCodeUIActive = false;
    private bool canInput = true;
    private bool codeValidated = false; // Pour bloquer l'ouverture du canvas après validation
    private float lastCloseTime = -10f;  // Temps réel de la dernière fermeture du digicode
    private float reopenDelay = 2f;  
    private MonoBehaviour cameraController; 

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (digiCodeCanvas != null)
            digiCodeCanvas.SetActive(false);

        SetupButtons();

    
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
          
            cameraController = mainCam.GetComponent<MonoBehaviour>(); 
        }
    }

    void SetupButtons()
    {
        foreach (Button btn in numberButtons)
        {
            if (btn != null)
            {
                string digit = btn.name;
                btn.onClick.AddListener(() => OnNumberClicked(digit));
            }
        }

        if (clearButton != null)
            clearButton.onClick.AddListener(ClearCode);
    }

    public void ShowDigiCode()
    {
        if (codeValidated) return; // Impossible d'ouvrir si déjà validé

        if (digiCodeCanvas != null)
        {
            digiCodeCanvas.SetActive(true);
            isCodeUIActive = true;
            currentCode = "";
            canInput = true;

            // Figer la caméra, pas le temps
            if (cameraController != null)
                cameraController.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void HideDigiCode()
    {
        if (digiCodeCanvas != null)
        {
            digiCodeCanvas.SetActive(false);
            isCodeUIActive = false;

            // Défiger la caméra
            if (cameraController != null)
                cameraController.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnNumberClicked(string digit)
    {
        if (!canInput) return;

        if (currentCode.Length < maxCodeLength)
        {
            currentCode += digit;
            Debug.Log($" Chiffre saisi : {digit}, Code actuel : {currentCode}");

            if (currentCode.Length == maxCodeLength)
            {
                canInput = false;
                ValidateCode();
            }
        }
    }

    void ValidateCode()
    {
        Debug.Log($" Validation du code : {currentCode}");

        if (currentCode == correctCode)
        {
            codeValidated = true;

            // Rotation à -90° sur l'axe Z
                if (targetObject != null)
                    StartCoroutine(RotateDoorOverTime(targetObject.transform, -90f, 1.5f)); 



            if (digiCodeCanvas != null)
                digiCodeCanvas.SetActive(false);

            isCodeUIActive = false;

            if (cameraController != null)
                cameraController.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        else
        {
            Debug.Log(" Code incorrect !");
            ClearCode();
        }
    }
    IEnumerator RotateDoorOverTime(Transform doorTransform, float totalAngle, float duration)
    {
        float rotated = 0f;
        float speed = totalAngle / duration;

        while (Mathf.Abs(rotated) < Mathf.Abs(totalAngle))
        {
            float rotationStep = speed * Time.deltaTime;
            doorTransform.Rotate(0f, 0f, rotationStep); // rotation en Z locale
            rotated += rotationStep;
            yield return null;
        }

        // Corriger l'angle final pour éviter le dépassement
        float correction = totalAngle - rotated;
        doorTransform.Rotate(0f, 0f, correction);
    }


    void ClearCode()
    {
        Debug.Log("🗑 Code effacé");
        currentCode = "";
        canInput = true;
    }

    void CancelCode()
    {
        Debug.Log(" Annulation du digicode");
        HideDigiCode();
    }

    void Update()
    {
        if (isCodeUIActive && Input.GetKeyDown(KeyCode.Escape))
            CancelCode();

        if (isCodeUIActive)
        {
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                    OnNumberClicked(i.ToString());
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (currentCode.Length > 0 && canInput)
                    currentCode = currentCode.Substring(0, currentCode.Length - 1);
            }
        }
    }
}
