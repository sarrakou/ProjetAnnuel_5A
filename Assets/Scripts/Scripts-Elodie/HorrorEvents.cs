using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[System.Serializable]
public class PhobiaResult
{
    public string phobiaType;
    public bool hasPhobia;
    public float confidenceScore;
    public float averageHeartRateIncrease;
    public float maxHeartRateIncrease;
    public bool heartRateIncreased;
    public float phobiaPercentage;
}

[System.Serializable]
public class PhobiaResultsData
{
    public PhobiaResult[] results;
}
public enum PhobiaType
{
    Entomophobie,     // Peur des insectes
    Nyctophobie,      // Peur du noir
    Scopophobie,      // Peur du regard des autres
    Claustrophobie    // Peur des espaces clos
}

public class HorrorEvents : MonoBehaviour
{
    [Header("Configuration")]
    public PhobiaType currentPhobia = PhobiaType.Nyctophobie;
    
    [Header("Références")]
    public Canvas spiderCanvas;
    public AnxietySystem anxietySystem; // Référence au système d'anxiété
    [Header("Configuration JSON")]
    public string jsonFileName = "phobia_results.json";
    public PhobiaType fallbackPhobia = PhobiaType.Nyctophobie;
    private Camera playerCamera;
    private Dictionary<PhobiaType, List<Action<Vector3>>> phobiaEvents;
    private HashSet<string> playedEvents = new HashSet<string>();
    
    [Header("Prefabs Resources Paths")]
    public string entomophobiePrefabPath = "EventsEntomophobie_";
    public string nyctophobiePrefabPath = "EventsNyctophobie";
    public string scopophobiePrefabPath = "EventsScopophobie";
    public string claustrophobiePrefabPath = "EventsClaustrophobie";
    private GameObject activatedPrefab;
    private void Awake()
    {
        playerCamera = Camera.main;
        
        if (anxietySystem == null)
        {
            anxietySystem = FindObjectOfType<AnxietySystem>();
        }
        
        // Lecture unique du JSON au démarrage
        LoadPhobiaFromJson();
        ActivatePhobiaPrefab();
        InitializePhobiaEvents();
        
        Debug.Log($"🎯 Système initialisé avec la phobie : {currentPhobia}");
    }
private void LoadPhobiaFromJson()
    {
        try
        {
            string userName = System.Environment.UserName;
            string jsonPath = $"C:/Users/{userName}/AppData/LocalLow/DefaultCompany/ProjetAnnuel_5A/{jsonFileName}";
            
            Debug.Log($"🔍 Lecture du fichier JSON : {jsonPath}");
            
            if (!File.Exists(jsonPath))
            {
                Debug.LogWarning($"⚠️ Fichier JSON non trouvé, utilisation de la phobie par défaut : {fallbackPhobia}");
                currentPhobia = fallbackPhobia;
                return;
            }
            
            string jsonContent = File.ReadAllText(jsonPath);
            PhobiaResultsData phobiaData = JsonUtility.FromJson<PhobiaResultsData>(jsonContent);
            
            if (phobiaData?.results == null || phobiaData.results.Length == 0)
            {
                Debug.LogWarning("⚠️ Aucun résultat trouvé dans le JSON, utilisation de la phobie par défaut.");
                currentPhobia = fallbackPhobia;
                return;
            }
            
            // Trouver la phobie avec le pourcentage le plus élevé
            PhobiaResult highestPhobia = null;
            float highestPercentage = -1f;
            
            Debug.Log("📊 === ANALYSE DES PHOBIES ===");
            foreach (PhobiaResult result in phobiaData.results)
            {
                Debug.Log($"   {result.phobiaType}: {result.phobiaPercentage:F1}%");
                
                if (result.phobiaPercentage > highestPercentage)
                {
                    highestPercentage = result.phobiaPercentage;
                    highestPhobia = result;
                }
            }
            
            if (highestPhobia != null)
            {
                currentPhobia = ConvertJsonPhobiaToEnum(highestPhobia.phobiaType);
                Debug.Log($"🎯 Phobie sélectionnée : {currentPhobia} ({highestPercentage:F1}%)");
            }
            else
            {
                Debug.LogWarning("⚠️ Aucune phobie valide trouvée, utilisation de la phobie par défaut.");
                currentPhobia = fallbackPhobia;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Erreur lors de la lecture du JSON : {e.Message}");
            Debug.LogWarning($"Utilisation de la phobie par défaut : {fallbackPhobia}");
            currentPhobia = fallbackPhobia;
        }
    }
    
    private PhobiaType ConvertJsonPhobiaToEnum(string jsonPhobiaType)
    {
        switch (jsonPhobiaType.ToLower())
        {
            case "entomophobia":
                return PhobiaType.Entomophobie;
            case "nyctophobia":
                return PhobiaType.Nyctophobie;
            case "scopophobia":
                return PhobiaType.Scopophobie;
            case "claustrophobia":
                return PhobiaType.Claustrophobie;
            default:
                Debug.LogWarning($"⚠️ Type de phobie inconnu : {jsonPhobiaType}. Utilisation de la phobie par défaut.");
                return fallbackPhobia;
        }
    }
    

    private void InitializePhobiaEvents()
    {
        phobiaEvents = new Dictionary<PhobiaType, List<Action<Vector3>>>();

        // Entomophobie - Peur des insectes
        phobiaEvents[PhobiaType.Entomophobie] = new List<Action<Vector3>>
        {
            Event_InsectOnScreen,
            Event_InsectSound,
            Event_Randomsounds,
           
            
        };

        phobiaEvents[PhobiaType.Nyctophobie] = new List<Action<Vector3>>
        { 
            Event_FlickeringLights,
            Event_TeleportToEmptyRoom,
            Event_PlayCreepyAudio,
              Event_Crying, 
              Event_Nycto
        };


        // Scopophobie - Peur du regard des autres
        phobiaEvents[PhobiaType.Scopophobie] = new List<Action<Vector3>>
        {
            Event_PlayCreepyAudio,
            Event_Crying,
            Event_TeleportWithMannequins,
            Event_Scopo
            
        };

        // Claustrophobie - Peur des espaces clos
        phobiaEvents[PhobiaType.Claustrophobie] = new List<Action<Vector3>>
        {
           Event_ClaustroFOV,
           Event_TeleportToEmptyRoom, 
           Event_FlickeringLights, 
           Event_TightSpace
        };
    }
    private void ActivatePhobiaPrefab()
    {
        string prefabPath = GetPrefabPath(currentPhobia);
        
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogWarning($"⚠️ Aucun chemin de prefab défini pour {currentPhobia}");
            return;
        }
        
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        
        if (prefab == null)
        {
            Debug.LogError($"❌ Prefab non trouvé dans Resources : {prefabPath}");
            return;
        }
        
        // Instancier le prefab
        activatedPrefab = Instantiate(prefab);
        
        Debug.Log($"✅ Prefab activé pour {currentPhobia} : {prefab.name}");
    }
    
    private string GetPrefabPath(PhobiaType phobia)
    {
        switch (phobia)
        {
            case PhobiaType.Entomophobie:
                return entomophobiePrefabPath;
            case PhobiaType.Nyctophobie:
                return nyctophobiePrefabPath;
            case PhobiaType.Scopophobie:
                return scopophobiePrefabPath;
            case PhobiaType.Claustrophobie:
                return claustrophobiePrefabPath;
            default:
                return null;
        }
    }
    public void TriggerRandomEvent(Vector3 position)
    {
        if (!phobiaEvents.ContainsKey(currentPhobia))
        {
            Debug.LogWarning($"Aucun événement pour la phobie : {currentPhobia}");
            return;
        }

        var events = phobiaEvents[currentPhobia];
        if (events.Count == 0)
        {
            Debug.LogWarning($"Aucun événement disponible pour {currentPhobia}");
            return;
        }

        // NOUVEAU : Filtrer les événements non encore joués
        List<Action<Vector3>> availableEvents = new List<Action<Vector3>>();
        foreach (var eventAction in events)
        {
            string eventKey = $"{currentPhobia}_{eventAction.Method.Name}";
            if (!playedEvents.Contains(eventKey))
            {
                availableEvents.Add(eventAction);
            }
        }

        // Vérifier s'il reste des événements disponibles
        if (availableEvents.Count == 0)
        {
            Debug.LogWarning($"⚠️ Tous les événements de {currentPhobia} ont déjà été joués !");
            return;
        }

        // Choisir un événement aléatoire parmi ceux disponibles
        int eventIndex = UnityEngine.Random.Range(0, availableEvents.Count);
        Action<Vector3> selectedEvent = availableEvents[eventIndex];
        
        // Marquer l'événement comme joué
        string selectedEventKey = $"{currentPhobia}_{selectedEvent.Method.Name}";
        playedEvents.Add(selectedEventKey);
        
        Debug.Log($"🎭 Déclenchement événement {currentPhobia} : {selectedEvent.Method.Name} ({availableEvents.Count - 1} restants)");
        selectedEvent.Invoke(position);
    }
    public int GetRemainingEventsCount(PhobiaType phobiaType)
    {
        if (!phobiaEvents.ContainsKey(phobiaType)) return 0;
        
        int totalEvents = phobiaEvents[phobiaType].Count;
        int playedCount = 0;
        
        foreach (string eventKey in playedEvents)
        {
            if (eventKey.StartsWith($"{phobiaType}_"))
            {
                playedCount++;
            }
        }
        
        return totalEvents - playedCount;
    }



    #region Entomophobie Events
    
    void Event_InsectOnScreen(Vector3 pos)
    {
        Debug.Log(" Insecte à l'écran");
        StartCoroutine(InsectCoroutine());
    }

    void Event_InsectSwarm(Vector3 pos)
    {
        Debug.Log(" Essaim d'insectes");
        
    }

    void Event_Randomsounds(Vector3 pos)
    {
        // Charger tous les sons depuis Resources/Audios
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("Audios/AudiosInsectes");
    
        if (audioClips.Length == 0)
        {
            Debug.LogWarning("⚠️ Aucun son trouvé dans Resources/AudiosInsectes");
            return;
        }
    
        // Choisir un son aléatoire
        AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
    
        // Créer un objet temporaire pour jouer le son à la position
        GameObject tempAudioObject = new GameObject("TempSound");
        tempAudioObject.transform.position = pos;
    
        AudioSource tempAudioSource = tempAudioObject.AddComponent<AudioSource>();
        tempAudioSource.clip = randomClip;
        tempAudioSource.volume = 1f;
        tempAudioSource.spatialBlend = 1f; // 3D
        tempAudioSource.Play();
    
        // Détruire l'objet après la lecture
        Destroy(tempAudioObject, randomClip.length);
    
        Debug.Log($"🔊 Son aléatoire joué: {randomClip.name} à {pos}");
    }
    void Event_InsectSound(Vector3 pos)
    {
        Debug.Log(" Son d'insectes");
        StartCoroutine(Insectsounds());
    }



    IEnumerator InsectCoroutine()
    {
        GameObject prefab = Resources.Load<GameObject>("SpiderAnimation"); // Renommer en InsectAnimation
        if (prefab == null)
        {
            Debug.LogWarning("InsectAnimation prefab non trouvé !");
            yield break;
        }

        GameObject insectGO = Instantiate(prefab);

        if (playerCamera == null)
        {
            Debug.LogWarning("Player camera not found!");
            yield break;
        }

        insectGO.transform.SetParent(playerCamera.transform);
        insectGO.transform.localPosition = new Vector3(0f, 0f, 2f);
        insectGO.transform.localRotation = Quaternion.identity;
        insectGO.transform.localScale = Vector3.one * 0.8f;

        yield return new WaitForSeconds(5f);
        Destroy(insectGO);
    }


    IEnumerator Insectsounds()
    {
        // Charger le clip audio
        AudioClip cryingClip = Resources.Load<AudioClip>("Audios/BUG"); 
        if (cryingClip == null)
        {
            Debug.LogWarning("🔊 Audio 'Crying' non trouvé !");
            yield break;
        }

        // Créer un objet audio derrière le joueur
        GameObject audioGO = new GameObject("BUG");
        audioGO.transform.SetParent(playerCamera.transform);
        audioGO.transform.localPosition = new Vector3(0, 0, -0.5f); // Derrière la tête


        AudioSource source = audioGO.AddComponent<AudioSource>();
        source.clip = cryingClip;
        source.spatialBlend = 1f; // Son 3D
        source.volume = 0.9f;
        source.minDistance = 0.1f;
        source.maxDistance = 2f;
        source.Play();

        
        yield return new WaitForSeconds(cryingClip.length);
        Destroy(audioGO);
    }
    void Event_Spider(Vector3 pos)
    {
        Debug.Log("Insecte apparaît devant le joueur");
        StartCoroutine(SpawnInsectInFrontOfPlayer());
    }

    IEnumerator SpawnInsectInFrontOfPlayer()
    {
        GameObject prefab = Resources.Load<GameObject>("SpiderAnimation"); // Assure-toi que le prefab est bien dans Resources

        if (prefab == null)
        {
            Debug.LogWarning("Prefab 'SpiderAnimation' non trouvé !");
            yield break;
        }

        if (playerCamera == null)
        {
            Debug.LogWarning("Camera principale non trouvée !");
            yield break;
        }

        // Instanciation du prefab comme enfant de la caméra pour suivre la tête
        GameObject insectInstance = Instantiate(prefab);
        insectInstance.transform.SetParent(playerCamera.transform);

        // Positionner devant la caméra (ex: 2 mètres devant)
        insectInstance.transform.localPosition = new Vector3(0f, 0f, 2f);
        insectInstance.transform.localRotation = Quaternion.identity;
        insectInstance.transform.localScale = Vector3.one * 0.8f;

        // Afficher pendant 5 secondes puis détruire
        yield return new WaitForSeconds(5f);

        Destroy(insectInstance);
    }
    #endregion

    #region Scopophobie Events

    void Event_WatchingEyes(Vector3 pos)
    {
        Debug.Log("👁️ Yeux qui observent");
        StartCoroutine(WatchingEyesCoroutine());
    }

    void Event_ShadowySilhouette(Vector3 pos)
    {
        Debug.Log("👁️ Silhouette qui observe");
        // Implémenter une silhouette dans l'ombre
    }

    void Event_StareEffect(Vector3 pos)
    {
        Debug.Log("👁️ Effet de regard fixe");
        // Implémenter l'effet de regard intense
    }

    void Event_MultipleEyes(Vector3 pos)
    {
        Debug.Log("👁️ Multiples yeux");
        StartCoroutine(MultipleEyesCoroutine());
    }
    void Event_Scopo(Vector3 pos)
    {
        // Charger tous les sons depuis Resources/Audios
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("Audios/Scopophobie");
    
        if (audioClips.Length == 0)
        {
            Debug.LogWarning("⚠️ Aucun son trouvé dans ");
            return;
        }
    
        // Choisir un son aléatoire
        AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
    
        // Créer un objet temporaire pour jouer le son à la position
        GameObject tempAudioObject = new GameObject("TempSound");
        tempAudioObject.transform.position = pos;
    
        AudioSource tempAudioSource = tempAudioObject.AddComponent<AudioSource>();
        tempAudioSource.clip = randomClip;
        tempAudioSource.volume = 1f;
        tempAudioSource.spatialBlend = 1f; // 3D
        tempAudioSource.Play();
    
        // Détruire l'objet après la lecture
        Destroy(tempAudioObject, randomClip.length);
    
        Debug.Log($"🔊 Son aléatoire joué: {randomClip.name} à {pos}");
    }
void Event_TeleportWithMannequins(Vector3 pos)
{
    Debug.Log("👁️ Téléportation avec mannequins observateurs");
    StartCoroutine(TeleportWithMannequinsCoroutine());
}

IEnumerator TeleportWithMannequinsCoroutine()
{
    
    GameObject emptyRoomPrefab = Resources.Load<GameObject>("ChambreVide");
    if (emptyRoomPrefab == null)
    {
        Debug.LogWarning("Prefab 'ChambreVide' non trouvé dans Resources !");
        yield break;
    }

    // Charger le prefab Mannequin depuis Resources
    GameObject mannequinPrefab = Resources.Load<GameObject>("mannequin");
    if (mannequinPrefab == null)
    {
        Debug.LogWarning("Prefab 'mannequin' non trouvé dans Resources !");
        yield break;
    }

    // Trouver le joueur
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null)
    {
        Debug.LogWarning("Joueur non trouvé dans la scène !");
        yield break;
    }

    Vector3 originalPosition = player.transform.position;
    Vector3 roomPosition = new Vector3(1000, -19.6f, 1000);

    // Instancier la pièce à sa position définie
    GameObject emptyRoomInstance = Instantiate(emptyRoomPrefab, roomPosition, Quaternion.identity);
    Vector3 teleportOffset = roomPosition - originalPosition;

    // Désactiver temporairement le CharacterController
    CharacterController controller = player.GetComponent<CharacterController>();
    bool wasEnabled = controller != null && controller.enabled;
    if (controller != null) controller.enabled = false;

    // Déplacer le joueur
    player.transform.position += teleportOffset;

    if (controller != null && wasEnabled) controller.enabled = true;

 
    yield return new WaitForSeconds(0.5f);

  
    List<GameObject> mannequins = new List<GameObject>();
    List<Vector3> usedPositions = new List<Vector3>();
    int mannequinCount = 40;
    float spawnRadius = 15.0f; 
    float minDistance = 2.0f;
    
    for (int i = 0; i < mannequinCount; i++)
    {
        Vector3 mannequinPosition;
        int attempts = 0;
        
       
        do
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            
            mannequinPosition = new Vector3(
                player.transform.position.x + randomCircle.x,
                -39.2999992f, // Hauteur fixe
                player.transform.position.z + randomCircle.y
            );
            
            attempts++;
        }
        while (IsPositionTooClose(mannequinPosition, usedPositions, minDistance) && attempts < 50);
        
        // Ajouter la position à la liste des positions utilisées
        usedPositions.Add(mannequinPosition);

      
        float randomZRotation = Random.Range(0f, 360f);
        Quaternion randomRotation = Quaternion.Euler(
            mannequinPrefab.transform.rotation.eulerAngles.x,
            mannequinPrefab.transform.rotation.eulerAngles.y,
            randomZRotation
        );

        // Instancier avec rotation aléatoire sur Z
        GameObject mannequin = Instantiate(mannequinPrefab, 
            mannequinPosition, 
            randomRotation);

        // Forcer l'échelle d'origine du prefab
        mannequin.transform.localScale = mannequinPrefab.transform.localScale;

        mannequins.Add(mannequin);
    }

    // Attendre 10 secondes
    yield return new WaitForSeconds(10f);

    // Revenir à la position d'origine
    if (controller != null) controller.enabled = false;
    player.transform.position = originalPosition;
    if (controller != null && wasEnabled) controller.enabled = true;

    // Détruire les mannequins
    foreach (GameObject mannequin in mannequins)
    {
        if (mannequin != null) Destroy(mannequin);
    }

    // Détruire la pièce
    Destroy(emptyRoomInstance);
}

// Fonction pour vérifier si une position est trop proche des autres
bool IsPositionTooClose(Vector3 newPosition, List<Vector3> usedPositions, float minDistance)
{
    foreach (Vector3 usedPos in usedPositions)
    {
        if (Vector3.Distance(newPosition, usedPos) < minDistance)
        {
            return true;
        }
    }
    return false;
}

    IEnumerator WatchingEyesCoroutine()
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

        // Créer des yeux aux coins de l'écran
        List<GameObject> eyes = new List<GameObject>();
        Vector2[] eyePositions = {
            new Vector2(0.1f, 0.9f),  // Coin haut gauche
            new Vector2(0.9f, 0.9f),  // Coin haut droite
            new Vector2(0.1f, 0.1f),  // Coin bas gauche
            new Vector2(0.9f, 0.1f)   // Coin bas droite
        };

        foreach (Vector2 pos in eyePositions)
        {
            GameObject eyeGO = new GameObject("WatchingEye");
            eyeGO.transform.SetParent(canvas.transform, false);
            
            Image eyeImage = eyeGO.AddComponent<Image>();
            eyeImage.color = Color.red;
            eyeImage.rectTransform.anchorMin = pos;
            eyeImage.rectTransform.anchorMax = pos;
            eyeImage.rectTransform.sizeDelta = new Vector2(50, 50);
            
            eyes.Add(eyeGO);
        }

        yield return new WaitForSeconds(6f);

        // Détruire les yeux
        foreach (GameObject eye in eyes)
        {
            if (eye != null) Destroy(eye);
        }
    }

    IEnumerator MultipleEyesCoroutine()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) yield break;

        List<GameObject> eyes = new List<GameObject>();
        
        // Créer des yeux aléatoires sur tout l'écran
        for (int i = 0; i < 10; i++)
        {
            GameObject eyeGO = new GameObject("Eye");
            eyeGO.transform.SetParent(canvas.transform, false);
            
            Image eyeImage = eyeGO.AddComponent<Image>();
            eyeImage.color = Color.red;
            
            Vector2 randomPos = new Vector2(
                UnityEngine.Random.Range(0.1f, 0.9f),
                UnityEngine.Random.Range(0.1f, 0.9f)
            );
            
            eyeImage.rectTransform.anchorMin = randomPos;
            eyeImage.rectTransform.anchorMax = randomPos;
            eyeImage.rectTransform.sizeDelta = new Vector2(30, 30);
            
            eyes.Add(eyeGO);
        }

        yield return new WaitForSeconds(8f);

        foreach (GameObject eye in eyes)
        {
            if (eye != null) Destroy(eye);
        }
    }

    #endregion

    #region Claustrophobie Events

    void Event_ClaustroFOV(Vector3 pos)
    {
        Debug.Log("🚪 Réduction FOV claustrophobe");
        if (playerCamera != null)
        {
            StartCoroutine(ClaustroFOVCoroutine());
        }
    }

    void Event_WallsClosing(Vector3 pos)
    {
        Debug.Log("🚪 Murs qui se rapprochent");
        // Implémenter l'effet de murs
    }

    void Event_TightSpace(Vector3 pos)
    {
        Debug.Log("🚪 Espace réduit");
        StartCoroutine(TeleportElevator()); 
    }
    IEnumerator TeleportElevator()
    {
        // Charger le prefab ChambreVide depuis Resources
        GameObject emptyRoomPrefab = Resources.Load<GameObject>("ChambreVide");
        if (emptyRoomPrefab == null)
        {
            Debug.LogWarning("Prefab 'ChambreVide' non trouvé dans Resources !");
            yield break;
        }

        // Trouver le joueur
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Joueur non trouvé dans la scène !");
            yield break;
        }

   
        Vector3 originalPosition = player.transform.position;
    

        Vector3 roomPosition = new Vector3(1000, 0, 1000);
    
  
        GameObject emptyRoomInstance = Instantiate(emptyRoomPrefab, roomPosition, Quaternion.identity);

    
        Vector3 teleportOffset = roomPosition - originalPosition + new Vector3(0, 1, 0);
    
   
        CharacterController controller = player.GetComponent<CharacterController>();
        bool wasEnabled = controller != null && controller.enabled;
        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.Translate(teleportOffset, Space.World);
    
        Debug.Log($"Joueur téléporté vers {player.transform.position}");

 
        if (controller != null && wasEnabled)
        {
            controller.enabled = true;
        }

        // Désactiver les lumières
        Light[] lights = emptyRoomInstance.GetComponentsInChildren<Light>();
        foreach (Light light in lights)
        {
            light.enabled = false;
        }

        yield return new WaitForSeconds(8f);

        // Retour à la position originale
        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position = originalPosition;

        if (controller != null && wasEnabled)
        {
            controller.enabled = true;
        }

        // Détruire la chambre
        Destroy(emptyRoomInstance);

        Debug.Log("Fin de la téléportation dans ChambreVide");
    }

    IEnumerator ClaustroFOVCoroutine()
    {
        float normalFOV = 60f;
        float claustroFOV = 30f;
        float duration = 7f;
        float timer = 0f;

        // Réduction FOV
        while (timer < duration / 2f)
        {
            timer += Time.deltaTime;
            playerCamera.fieldOfView = Mathf.Lerp(normalFOV, claustroFOV, timer / (duration / 2f));
            yield return null;
        }

        // Maintien FOV réduit
        timer = 0f;
        while (timer < duration / 4f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Retour normal
        timer = 0f;
        while (timer < duration / 4f)
        {
            timer += Time.deltaTime;
            playerCamera.fieldOfView = Mathf.Lerp(claustroFOV, normalFOV, timer / (duration / 4f));
            yield return null;
        }

        playerCamera.fieldOfView = normalFOV;
    }

    #endregion

    #region Nyctophobie Events
    void Event_TeleportToEmptyRoom(Vector3 pos)
    {
        Debug.Log(" Téléportation dans ChambreVide");
        StartCoroutine(TeleportToEmptyRoomCoroutine());
    }

   IEnumerator TeleportToEmptyRoomCoroutine()
{
    // Charger le prefab ChambreVide depuis Resources
    GameObject emptyRoomPrefab = Resources.Load<GameObject>("ChambreVide");
    if (emptyRoomPrefab == null)
    {
        Debug.LogWarning("Prefab 'ChambreVide' non trouvé dans Resources !");
        yield break;
    }

    // Trouver le joueur
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null)
    {
        Debug.LogWarning("Joueur non trouvé dans la scène !");
        yield break;
    }

   
    Vector3 originalPosition = player.transform.position;
    

    Vector3 roomPosition = new Vector3(1000, 0, 1000);
    
  
    GameObject emptyRoomInstance = Instantiate(emptyRoomPrefab, roomPosition, Quaternion.identity);

    
    Vector3 teleportOffset = roomPosition - originalPosition + new Vector3(0, 1, 0);
    
   
    CharacterController controller = player.GetComponent<CharacterController>();
    bool wasEnabled = controller != null && controller.enabled;
    if (controller != null)
    {
        controller.enabled = false;
    }

    player.transform.Translate(teleportOffset, Space.World);
    
    Debug.Log($"Joueur téléporté vers {player.transform.position}");

 
    if (controller != null && wasEnabled)
    {
        controller.enabled = true;
    }

    // Désactiver les lumières
    Light[] lights = emptyRoomInstance.GetComponentsInChildren<Light>();
    foreach (Light light in lights)
    {
        light.enabled = false;
    }

    yield return new WaitForSeconds(8f);

    // Retour à la position originale
    if (controller != null)
    {
        controller.enabled = false;
    }

    player.transform.position = originalPosition;

    if (controller != null && wasEnabled)
    {
        controller.enabled = true;
    }

    // Détruire la chambre
    Destroy(emptyRoomInstance);

    Debug.Log("Fin de la téléportation dans ChambreVide");
}

    void Event_DarknessEffect(Vector3 pos)
    {
        Debug.Log("🌑 Effet d'obscurité");
        StartCoroutine(DarknessCoroutine());
    }

    void Event_FlickeringLights(Vector3 pos)
    {
        Debug.Log(" Lumières qui clignotent");
        StartCoroutine(FlickeringLightsCoroutine());
    }

   
    void Event_LightFailure(Vector3 pos)
    {
        Debug.Log(" Panne de lumière");
        StartCoroutine(LightFailureCoroutine());
    }
    void Event_Nycto(Vector3 pos)
    {
        // Charger tous les sons depuis Resources/Audios
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("Audios/Nycto");
    
        if (audioClips.Length == 0)
        {
            Debug.LogWarning("⚠️ Aucun son trouvé dans ");
            return;
        }
    
        // Choisir un son aléatoire
        AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
    
        // Créer un objet temporaire pour jouer le son à la position
        GameObject tempAudioObject = new GameObject("TempSound");
        tempAudioObject.transform.position = pos;
    
        AudioSource tempAudioSource = tempAudioObject.AddComponent<AudioSource>();
        tempAudioSource.clip = randomClip;
        tempAudioSource.volume = 1f;
        tempAudioSource.spatialBlend = 1f; // 3D
        tempAudioSource.Play();
    
        // Détruire l'objet après la lecture
        Destroy(tempAudioObject, randomClip.length);
    
        Debug.Log($"🔊 Son aléatoire joué: {randomClip.name} à {pos}");
    }
    IEnumerator DarknessCoroutine()
    {
        // Obscurité progressive plus intense que la vignette
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        Image darknessImage = null;
        Transform existing = canvas.transform.Find("DarknessImage");
        if (existing != null)
        {
            darknessImage = existing.GetComponent<Image>();
        }
        else
        {
            GameObject imgGO = new GameObject("DarknessImage");
            imgGO.transform.SetParent(canvas.transform, false);
            darknessImage = imgGO.AddComponent<Image>();
            darknessImage.color = new Color(0, 0, 0, 0);
            darknessImage.rectTransform.anchorMin = Vector2.zero;
            darknessImage.rectTransform.anchorMax = Vector2.one;
            darknessImage.rectTransform.offsetMin = Vector2.zero;
            darknessImage.rectTransform.offsetMax = Vector2.zero;
        }

        float duration = 6f;
        float maxAlpha = 0.95f; // Très sombre
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, maxAlpha, timer / duration);
            darknessImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Retour progressif à la normale
        timer = 0f;
        while (timer < duration / 2f)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(maxAlpha, 0f, timer / (duration / 2f));
            darknessImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        Destroy(darknessImage.gameObject);
    }

    IEnumerator FlickeringLightsCoroutine()
    {
        Debug.Log(" Début du clignotement progressif...");
        Light[] lights = FindObjectsOfType<Light>();
        Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();

        // Enregistre l'intensité de base
        foreach (var light in lights)
            originalIntensities[light] = light.intensity;

        float duration = 8f;
        float timer = 0f;
        float flickerInterval = 0.2f;

        while (timer < duration)
        {
            // Diminue intensité
            foreach (var light in lights)
                light.intensity = originalIntensities[light] * 0.2f;

            yield return new WaitForSeconds(flickerInterval);

            // Restaure l'intensité
            foreach (var light in lights)
                light.intensity = originalIntensities[light];

            yield return new WaitForSeconds(flickerInterval);

            timer += flickerInterval * 2;
        }

        Debug.Log(" Fin du clignotement progressif");
    }

    void Event_PlayCreepyAudio(Vector3 pos)
    {
        Debug.Log(" Lecture d'un son angoissant (Nyctophobie)");
        StartCoroutine(PlayCreepyAudioCoroutine());
    }
    void Event_Crying(Vector3 pos)
    {
        Debug.Log(" Respiration dans le cou");
        StartCoroutine(CryingCoroutine());
    }

    IEnumerator CryingCoroutine()
    {
        // Charger le clip audio
        AudioClip cryingClip = Resources.Load<AudioClip>("Audios/Crying"); 
        if (cryingClip == null)
        {
            Debug.LogWarning("🔊 Audio 'Crying' non trouvé !");
            yield break;
        }

        // Créer un objet audio derrière le joueur
        GameObject audioGO = new GameObject("CryingAudio");
        audioGO.transform.SetParent(playerCamera.transform);
        audioGO.transform.localPosition = new Vector3(0, 0, -0.5f); // Derrière la tête


        AudioSource source = audioGO.AddComponent<AudioSource>();
        source.clip = cryingClip;
        source.spatialBlend = 1f; // Son 3D
        source.volume = 0.9f;
        source.minDistance = 0.1f;
        source.maxDistance = 2f;
        source.Play();

        
        yield return new WaitForSeconds(cryingClip.length);
        Destroy(audioGO);
    }

  


    IEnumerator PlayCreepyAudioCoroutine()
    {
       
        AudioClip creepyClip = Resources.Load<AudioClip>("Audios/whisper5-94457"); 
        if (creepyClip == null)
        {
            Debug.LogWarning(" Audio 'NyctophobieAmbiance' introuvable !");
            yield break;
        }

        
        GameObject audioGO = new GameObject("CreepyAudioSource");
        AudioSource source = audioGO.AddComponent<AudioSource>();
        source.clip = creepyClip;
        source.loop = false;
        source.spatialBlend = 0f; // Son 2D
        source.volume = 0.8f;

       
        source.Play();
        Debug.Log(" Son angoissant en cours...");

        yield return new WaitForSeconds(creepyClip.length);

     
        Destroy(audioGO);
    }


   

    IEnumerator LightFailureCoroutine()
    {
        Light[] lights = FindObjectsOfType<Light>();
        float[] originalIntensities = new float[lights.Length];
        
        for (int i = 0; i < lights.Length; i++)
        {
            originalIntensities[i] = lights[i].intensity;
        }

        // Extinction progressive
        float duration = 2f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float intensity = Mathf.Lerp(1f, 0f, timer / duration);
            
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = originalIntensities[i] * intensity;
            }
            
            yield return null;
        }

        // Maintenir l'obscurité
        yield return new WaitForSeconds(3f);

        // Rallumage progressif
        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float intensity = Mathf.Lerp(0f, 1f, timer / duration);
            
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = originalIntensities[i] * intensity;
            }
            
            yield return null;
        }

        // Restaurer les intensités
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = originalIntensities[i];
        }
    }

    #endregion

    #region Shared Events

    void Event_Vignette(Vector3 pos)
    {
        Debug.Log(" Vignette activée");
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

    #endregion
    public void LogEventStatus()
    {
        Debug.Log($"📊 Status des événements (Phobie active: {currentPhobia}) :");
        foreach (var phobia in phobiaEvents.Keys)
        {
            int remaining = GetRemainingEventsCount(phobia);
            int total = phobiaEvents[phobia].Count;
            string indicator = phobia == currentPhobia ? "👑 " : "   ";
            Debug.Log($"{indicator}{phobia}: {remaining}/{total} événements restants");
        }
    }

    // Méthode pour changer manuellement la phobie (pour tests ou événements spéciaux)
    public void SetPhobia(PhobiaType newPhobia)
    {
        currentPhobia = newPhobia;
        Debug.Log($"🎯 Phobie changée manuellement pour : {newPhobia}");
    }

    // Méthode pour recharger la phobie depuis le JSON (si le fichier a changé)
    public void ReloadPhobiaFromJson()
    {
        LoadPhobiaFromJson();
        Debug.Log($"🔄 Phobie rechargée depuis JSON : {currentPhobia}");
    }

    // Getter pour accéder à la phobie actuelle depuis l'extérieur
    public PhobiaType GetCurrentPhobia()
    {
        return currentPhobia;
    }

    // Méthode pour afficher les statistiques complètes
    public void LogPhobiaStatistics()
    {
        try
        {
            string userName = System.Environment.UserName;
            string jsonPath = $"C:/Users/{userName}/AppData/LocalLow/DefaultCompany/ProjetAnnuel_5A/{jsonFileName}";
            
            if (!File.Exists(jsonPath))
            {
                Debug.LogWarning("📊 Fichier JSON non trouvé pour les statistiques.");
                return;
            }
            
            string jsonContent = File.ReadAllText(jsonPath);
            PhobiaResultsData phobiaData = JsonUtility.FromJson<PhobiaResultsData>(jsonContent);
            
            Debug.Log($"📊 === STATISTIQUES COMPLÈTES - Phobie active: {currentPhobia} ===");
            foreach (PhobiaResult result in phobiaData.results)
            {
                PhobiaType phobiaType = ConvertJsonPhobiaToEnum(result.phobiaType);
                string indicator = phobiaType == currentPhobia ? "👑 " : "   ";
                Debug.Log($"{indicator}{result.phobiaType}:");
                Debug.Log($"     - Pourcentage: {result.phobiaPercentage:F1}%");
                Debug.Log($"     - A une phobie: {result.hasPhobia}");
                Debug.Log($"     - Rythme cardiaque augmenté: {result.heartRateIncreased}");
                Debug.Log($"     - Augmentation moyenne RC: {result.averageHeartRateIncrease:F2}");
            }
            Debug.Log("=====================================");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Erreur lors de l'affichage des statistiques : {e.Message}");
        }
    }

    [ContextMenu("Afficher Phobie Actuelle")]
    public void ShowCurrentPhobiaContextMenu()
    {
        Debug.Log($"🎯 Phobie actuellement active : {currentPhobia}");
    }
    
    [ContextMenu("Recharger depuis JSON")]
    public void ReloadPhobiaFromJsonContextMenu()
    {
        ReloadPhobiaFromJson();
    }
    
    [ContextMenu("Afficher Statistiques Complètes")]
    public void LogPhobiaStatisticsContextMenu()
    {
        LogPhobiaStatistics();
    }

    [ContextMenu("Afficher Status des Événements")]
    public void LogEventStatusContextMenu()
    {
        LogEventStatus();
    }
    
   
    
}