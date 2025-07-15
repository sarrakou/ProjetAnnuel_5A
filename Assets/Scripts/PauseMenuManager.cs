using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel; // Panel principal du menu pause
    public GameObject questPanel; // Panel des qu�tes � droite
    public GameObject settingsPanel;
    public GameObject menuPanel;

    [Header("Quest UI Elements")]
    public TMP_Text currentQuestTitle; // Titre de la qu�te actuelle
    public TMP_Text currentQuestDescription; // Description de la qu�te

    [Header("Menu Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button exitGameButton;

    [Header("Settings Elements")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public TMP_Dropdown qualityDropdown;
    public Button backButton;

    [Header("Audio")]
    public AudioMixerGroup masterMixer;
    public AudioMixerGroup musicMixer;

    [Header("Configuration")]
    public KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;
    private GameManager gameManager;
    private bool wasInventoryOpen = false;

    void Start()
    {
        // Trouver le GameManager
        gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogWarning("[PauseMenu] GameManager non trouv� ! Les qu�tes ne s'afficheront pas.");
        }

        // Configurer les boutons
        SetupButtons();

        // Configurer les param�tres
        SetupSettings();

        // S'assurer que le menu est ferm� au d�marrage
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        isPaused = false;
        Debug.Log("[PauseMenu] Syst�me de pause initialis�");
    }

    void SetupButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ToggleSettings);

        if (exitGameButton != null)
            exitGameButton.onClick.AddListener(ExitGame);

        if (backButton != null)
            backButton.onClick.AddListener(ToggleSettings);
    }

    void SetupSettings()
    {
        // Charger les param�tres sauvegard�s
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }
    }

    void Update()
    {
        // D�tecter la touche Escape
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        // Afficher le menu pause
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Mettre � jour l'affichage des qu�tes
        UpdateQuestDisplay();

        // Pauser le jeu
        Time.timeScale = 0f;

        // Lib�rer le curseur
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[PauseMenu] Jeu en pause");
    }

    public void ResumeGame()
    {
        isPaused = false;

        // Cacher le menu pause
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Cacher le panel des param�tres s'il est ouvert
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            menuPanel.SetActive(true);
        }

        // Reprendre le jeu
        Time.timeScale = 1f;

        // Verrouiller le curseur
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[PauseMenu] Jeu repris");
    }

    void UpdateQuestDisplay()
    {
        if (gameManager == null) return;

        // Mettre � jour la qu�te actuelle
        UpdateCurrentQuest();

    }

    void UpdateCurrentQuest()
    {
        if (currentQuestTitle == null || currentQuestDescription == null) return;

        if (gameManager == null || gameManager.quests == null || gameManager.quests.Count == 0)
        {
            currentQuestTitle.text = "Aucune qu�te disponible";
            currentQuestDescription.text = "Le syst�me de qu�tes n'est pas initialis�.";
            return;
        }

        // Trouver la premi�re qu�te non compl�t�e (qu�te actuelle)
        Quest currentQuest = null;
        foreach (Quest quest in gameManager.quests)
        {
            if (!quest.isCompleted)
            {
                currentQuest = quest;
                break;
            }
        }

        if (currentQuest != null)
        {
            currentQuestTitle.text = currentQuest.questName;
            currentQuestDescription.text = currentQuest.description;
        }
        else
        {
            // Toutes les qu�tes sont compl�t�es
            currentQuestTitle.text = "Toutes les qu�tes compl�t�es !";
            currentQuestDescription.text = "F�licitations ! Vous avez termin� toutes les qu�tes disponibles.";
        }
    }

    void AddQuestTooltip(GameObject questItem, Quest quest)
    {
        // Ajouter un composant Button pour d�tecter les clics/hover
        Button questButton = questItem.GetComponent<Button>();
        if (questButton == null)
        {
            questButton = questItem.AddComponent<Button>();
        }

        // D�sactiver l'interactivit� visuelle du bouton
        questButton.interactable = true;
        var colors = questButton.colors;
        colors.normalColor = Color.clear;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.1f);
        colors.pressedColor = Color.clear;
        questButton.colors = colors;

        // Vous pouvez ajouter ici un syst�me de tooltip pour afficher quest.description
    }


    // M�thodes pour les param�tres
    public void SetMasterVolume(float volume)
    {
        if (masterMixer != null)
        {
            masterMixer.audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (musicMixer != null)
        {
            musicMixer.audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("Quality", qualityIndex);
    }

    // M�thodes pour les boutons du menu
    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf); 
            menuPanel.SetActive(!menuPanel.activeSelf);
        }
    }

    public void ExitGame()
    {
        Debug.Log("[PauseMenu] Fermeture du jeu");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void OnDestroy()
    {
        // Nettoyer les listeners
        if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
        if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
        if (exitGameButton != null) exitGameButton.onClick.RemoveAllListeners();

        // S'assurer que le temps est normal
        Time.timeScale = 1f;
    }
}