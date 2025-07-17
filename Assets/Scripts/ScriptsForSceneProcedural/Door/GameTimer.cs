using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;



public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    public float timeLimit = 60f;
    private float currentTime;
    private bool gameEnded = false;

    public List<FlickeringMaterialLight> allLights;
    private int lastDangerLevel = -1;

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI statusText;
    public Canvas Win;
    public Canvas Lose;



    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        currentTime = timeLimit;
        //Debug.Log("Le jeu commence ! Trouve la porte rouge avant " + timeLimit + " secondes.");

        if (Win != null)
            Win.gameObject.SetActive(false);

        if (Lose != null)
            Lose.gameObject.SetActive(false);


    }

    void Update()
    {
        if (gameEnded) return;

        currentTime -= Time.deltaTime;
        //Debug.Log("Temps restant : " + Mathf.Ceil(currentTime) + " secondes");

        if (timerText != null)
            timerText.text = "Time : " + Mathf.Ceil(currentTime).ToString() + "s";


        if (currentTime <= 0)
        {
            gameEnded = true;
            //Debug.Log(" Temps écoulé ! Tu as perdu !");
            OnLose();
        }

    }


    private void OnLose()
    {
        gameEnded = true;

        if (statusText != null)
            Lose.gameObject.SetActive(true);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            MonoBehaviour movement = player.GetComponent<MyPlayerMovement>();
            if (movement != null) movement.enabled = false;
        }

        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null) audio.Play();

        StartCoroutine(RestartOnLose());
    }

    private IEnumerator RestartOnLose()
    {
        yield return new WaitForSeconds(3f); // délai pour voir le message
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void WinGame()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            if (statusText != null)
                Win.gameObject.SetActive(true);
            
        }
        StartCoroutine(ChangeSceneWin());
    }

    private IEnumerator ChangeSceneWin()
    {
        yield return new WaitForSeconds(3f); // délai pour change scene
        SceneManager.LoadScene("MainGameHorror");
    }

}
