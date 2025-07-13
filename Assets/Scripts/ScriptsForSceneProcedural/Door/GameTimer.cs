using UnityEngine;
using System.Collections.Generic;


public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    public float timeLimit = 120f;
    private float currentTime;
    private bool gameEnded = false;

    public List<FlickeringMaterialLight> allLights;
    private int lastDangerLevel = -1;


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
    }

    void Update()
    {
        if (gameEnded) return;

        currentTime -= Time.deltaTime;
        //Debug.Log("Temps restant : " + Mathf.Ceil(currentTime) + " secondes");

        if (currentTime <= 0)
        {
            gameEnded = true;
            Debug.Log(" Temps écoulé ! Tu as perdu !");
            OnLose();
        }

    }


    private void OnLose()
    {
        // Désactive le contrôle joueur
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            MonoBehaviour movement = player.GetComponent<MyPlayerMovement>(); 
            if (movement != null) movement.enabled = false;
        }

        // Joue un son de défaite (voir ci-dessous)
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null) audio.Play();
    }


    public void WinGame()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            Debug.Log(" Gagné ! Tu as trouvé la sortie à temps !");
        }
    }
}
