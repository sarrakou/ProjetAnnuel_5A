using UnityEngine;
using System.Collections;

public class MonsterAppearanceManager : MonoBehaviour
{
    [Header("Configuration du Monstre")]
    [Tooltip("Nom du prefab monstre dans Resources")]
    public string monsterPrefabName = "Monster";
    
    [Tooltip("AudioClip du son de monstre")]
    public AudioClip monsterSound;
    
    [Header("Timing des Apparitions")]
    [Tooltip("Temps minimum entre les apparitions (secondes)")]
    public float minTimeBetweenAppearances = 45f;
    
    [Tooltip("Temps maximum entre les apparitions (secondes)")]
    public float maxTimeBetweenAppearances = 90f;
    
    [Header("Position d'Apparition")]
    [Tooltip("Distance derrière le joueur")]
    public float distanceBehindPlayer = 5f;
    
    [Tooltip("Variation aléatoire de position (±)")]
    public float positionVariation = 2f;
    
    [Header("Durée de Vie")]
    [Tooltip("Temps avant que le monstre disparaisse")]
    public float monsterLifetime = 3f;
    
    [Header("Probabilités (%)")]
    [Range(0, 100)]
    [Tooltip("Chance d'apparition avec monstre + son")]
    public int chanceMonsterWithSound = 40;
    
    [Range(0, 100)]
    [Tooltip("Chance d'apparition avec monstre sans son")]
    public int chanceMonsterWithoutSound = 30;
    
    [Range(0, 100)]
    [Tooltip("Chance de son seulement (sans monstre)")]
    public int chanceSoundOnly = 30;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [Tooltip("Volume du son")]
    public float soundVolume = 0.8f;
    
    private Transform player;
    private AudioSource audioSource;
    private bool isActive = true;
    
    public enum AppearanceType
    {
        MonsterWithSound,    // Monstre visible + son
        MonsterWithoutSound, // Monstre visible sans son
        SoundOnly           // Son seulement, pas de monstre
    }

    private void Start()
    {
        // Trouver le joueur
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("MonsterAppearanceManager: Joueur avec tag 'Player' non trouvé!");
            return;
        }
        
        // Créer AudioSource si nécessaire
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configurer l'AudioSource
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
        
        // Démarrer le cycle d'apparitions
        if (isActive)
        {
            StartCoroutine(AppearanceCycle());
        }
    }
    
    private IEnumerator AppearanceCycle()
    {
        while (isActive)
        {
            // Attendre un temps aléatoire
            float waitTime = Random.Range(minTimeBetweenAppearances, maxTimeBetweenAppearances);
            Debug.Log($"MonsterManager: Prochaine apparition dans {waitTime:F1} secondes");
            yield return new WaitForSeconds(waitTime);
            
            if (!isActive || player == null) break;
            
            // Déterminer le type d'apparition
            AppearanceType appearanceType = DetermineAppearanceType();
            
            // Exécuter l'apparition
            yield return StartCoroutine(ExecuteAppearance(appearanceType));
        }
    }
    
    private AppearanceType DetermineAppearanceType()
    {
        int totalChance = chanceMonsterWithSound + chanceMonsterWithoutSound + chanceSoundOnly;
        if (totalChance == 0)
        {
            Debug.LogWarning("MonsterManager: Toutes les probabilités sont à 0!");
            return AppearanceType.MonsterWithSound;
        }
        
        int randomValue = Random.Range(0, totalChance);
        
        if (randomValue < chanceMonsterWithSound)
        {
            return AppearanceType.MonsterWithSound;
        }
        else if (randomValue < chanceMonsterWithSound + chanceMonsterWithoutSound)
        {
            return AppearanceType.MonsterWithoutSound;
        }
        else
        {
            return AppearanceType.SoundOnly;
        }
    }
    
    private IEnumerator ExecuteAppearance(AppearanceType type)
    {
        Debug.Log($"MonsterManager: Exécution de {type}");
        
        switch (type)
        {
            case AppearanceType.MonsterWithSound:
                PlayMonsterSound();
                yield return StartCoroutine(SpawnMonster());
                break;
                
            case AppearanceType.MonsterWithoutSound:
                yield return StartCoroutine(SpawnMonster());
                break;
                
            case AppearanceType.SoundOnly:
                PlayMonsterSound();
                break;
        }
    }
    
    private void PlayMonsterSound()
    {
        if (monsterSound != null && audioSource != null)
        {
            audioSource.clip = monsterSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
            Debug.Log("MonsterManager: Son de monstre joué");
        }
        else
        {
            Debug.LogWarning("MonsterManager: Son de monstre ou AudioSource manquant!");
        }
    }
    
    private IEnumerator SpawnMonster()
    {
       
        GameObject monsterPrefab = Resources.Load<GameObject>(monsterPrefabName);
        if (monsterPrefab == null)
        {
            Debug.LogError($"MonsterManager: Prefab '{monsterPrefabName}' non trouvé dans Resources!");
            yield break;
        }
        
       
        Vector3 spawnPosition = GetSpawnPosition();
        
       
        GameObject monster = Instantiate(monsterPrefab, spawnPosition, GetSpawnRotation(spawnPosition));
        Debug.Log($"MonsterManager: Monstre spawné à {spawnPosition}");
        
        
        AddMonsterBehavior(monster);
        
        // Attendre puis détruire
        yield return new WaitForSeconds(monsterLifetime);
        
        if (monster != null)
        {
            Destroy(monster);
            Debug.Log("MonsterManager: Monstre détruit");
        }
    }
    
    private Vector3 GetSpawnPosition()
    {
        
        Vector3 behindDirection = -player.forward;
        Vector3 basePosition = player.position + behindDirection * distanceBehindPlayer;
        
        
        Vector3 randomOffset = new Vector3(
            Random.Range(-positionVariation, positionVariation),
            0f,
            Random.Range(-positionVariation, positionVariation)
        );
        
        Vector3 finalPosition = basePosition + randomOffset;
        finalPosition.y = player.position.y; 
        
        return finalPosition;
    }
    
    private Quaternion GetSpawnRotation(Vector3 spawnPosition)
    {
        // Faire regarder le monstre vers le joueur
        Vector3 directionToPlayer = (player.position - spawnPosition).normalized;
        directionToPlayer.y = 0; // Pas de rotation verticale
    
        if (directionToPlayer != Vector3.zero)
        {
            return Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(0, 90, 0);
        }
    
        return Quaternion.identity;
    }
    
    private void AddMonsterBehavior(GameObject monster)
    {
       
        MoveWhenNotLookedAt moveScript = monster.GetComponent<MoveWhenNotLookedAt>();
        if (moveScript == null)
        {
            moveScript = monster.AddComponent<MoveWhenNotLookedAt>();
            
            moveScript.speed = 1.5f;
            moveScript.lookThreshold = 0.8f;
            moveScript.lifeTime = monsterLifetime;
        }
    }
    
    #region Contrôles Publics
    
    [ContextMenu("Forcer Apparition Monstre + Son")]
    public void ForceMonsterWithSound()
    {
        if (player != null)
        {
            StartCoroutine(ExecuteAppearance(AppearanceType.MonsterWithSound));
        }
    }
    
    [ContextMenu("Forcer Apparition Monstre Sans Son")]
    public void ForceMonsterWithoutSound()
    {
        if (player != null)
        {
            StartCoroutine(ExecuteAppearance(AppearanceType.MonsterWithoutSound));
        }
    }
    
    [ContextMenu("Forcer Son Seulement")]
    public void ForceSoundOnly()
    {
        if (player != null)
        {
            StartCoroutine(ExecuteAppearance(AppearanceType.SoundOnly));
        }
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        if (active && player != null)
        {
            StartCoroutine(AppearanceCycle());
        }
    }
    
    public void StopAllAppearances()
    {
        isActive = false;
        StopAllCoroutines();
    }
    
    #endregion
}