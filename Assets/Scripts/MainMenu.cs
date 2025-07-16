using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Global Language
public static class GlobalLanguage
{
    public static string language = "english";
}

public class MainMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer mainAudioMixer;
    public AudioMixer musicAudioMixer;

    [Header("Game Settings")]
    public string gameSceneName = "Main1";

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    // Language dropdown function
    public void SetLanguage(int index)
    {
        switch (index)
        {
            case 0:
                GlobalLanguage.language = "english";
                break;
            case 1:
                GlobalLanguage.language = "french";
                break;
            default:
                GlobalLanguage.language = "english";
                break;
        }

        PlayerPrefs.SetString("Language", GlobalLanguage.language);
        PlayerPrefs.Save();
    }

    // Quality dropdown function
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("Quality", qualityIndex);
        PlayerPrefs.Save();
    }

    // Overall volume slider function
    public void SetOverallVolume(float volume)
    {
        float dbValue = volume > 0 ? Mathf.Log10(volume) * 20f : -80f;

        if (mainAudioMixer != null)
        {
            mainAudioMixer.SetFloat("Volume", dbValue);
        }

        PlayerPrefs.SetFloat("OverallVolume", volume);
        PlayerPrefs.Save();
    }

    // Music volume slider function
    public void SetMusicVolume(float volume)
    {
        float dbValue = volume > 0 ? Mathf.Log10(volume) * 20f : -80f;

        if (musicAudioMixer != null)
        {
            musicAudioMixer.SetFloat("Volume", dbValue);
        }

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    // Settings button function
    public void Settings()
    {
        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    // Play button function
    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Exit button function
    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Back()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}