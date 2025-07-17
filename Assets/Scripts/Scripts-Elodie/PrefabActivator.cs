using UnityEngine;
using System.Collections;

public class PrefabActivator : MonoBehaviour
{
    [Header("Prefab à instancier depuis Resources")]
    public string prefabResourceName;

    [Tooltip("Durée pendant laquelle le prefab reste actif")]
    public float activeDuration = 2f;

    [Tooltip("Vitesse de déplacement en unités/seconde (si mouvement basique)")]
    public float moveSpeed = 2f;

    [Tooltip("AnimatorController à glisser (pour jouer l'animation)")]
    public RuntimeAnimatorController animatorController;

    [Header("Position d'apparition")]
    [Tooltip("Où faire apparaître le prefab")]
    public SpawnPosition spawnPosition = SpawnPosition.PrefabOriginalPosition;
    
    [Tooltip("Distance derrière le joueur (si SpawnBehindPlayer)")]
    public float distanceBehindPlayer = 3f;

    [Header("Comportement de mouvement")]
    [Tooltip("Cocher si le prefab doit bouger")]
    public bool shouldMove = true;
    
    [Tooltip("Type de mouvement (seulement si shouldMove = true)")]
    public MovementType movementType = MovementType.BasicTowardPlayer;

    public enum SpawnPosition
    {
        PrefabOriginalPosition,  // Position d'origine du prefab
        SpawnBehindPlayer        // Derrière le joueur
    }

    public enum MovementType
    {
        BasicTowardPlayer,      // Mouvement basique vers le joueur
        MoveWhenNotLookedAt     // Script MoveWhenNotLookedAt
    }

    private bool triggered = false;
    private GameObject instance;
    private Transform player;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger activé par : " + other.name + " avec tag : " + other.tag);
        
        if (triggered) 
        {
            Debug.Log("Déjà triggered, on ignore");
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("Joueur détecté, activation du prefab !");
            player = other.transform;
            triggered = true;
            StartCoroutine(ActivatePrefab());
        }
        else
        {
            Debug.Log("Ce n'est pas le joueur, tag attendu : Player");
        }
    }

    private Vector3 GetSpawnPosition(GameObject prefab)
    {
        if (spawnPosition == SpawnPosition.SpawnBehindPlayer && player != null)
        {
            // Calculer la position derrière le joueur
            Vector3 behindDirection = -player.forward; // Direction opposée à celle du joueur
            Vector3 spawnPos = player.position + behindDirection * distanceBehindPlayer;
            
            // Garder la même hauteur Y que le joueur (ou ajuster selon vos besoins)
            spawnPos.y = player.position.y;
            
            Debug.Log("Spawn derrière le joueur à : " + spawnPos);
            return spawnPos;
        }
        else
        {
            // Position d'origine du prefab
            Debug.Log("Spawn à la position d'origine : " + prefab.transform.position);
            return prefab.transform.position;
        }
    }

    private IEnumerator ActivatePrefab()
    {
        Debug.Log("Début de ActivatePrefab()");
        
        if (string.IsNullOrEmpty(prefabResourceName))
        {
            Debug.LogError("PrefabActivator : Nom de prefab vide.");
            yield break;
        }

        Debug.Log("Chargement du prefab : " + prefabResourceName);
        GameObject prefab = Resources.Load<GameObject>(prefabResourceName);
        if (prefab == null)
        {
            Debug.LogError("PrefabActivator : Aucun prefab trouvé dans Resources avec le nom : " + prefabResourceName);
            yield break;
        }

        Debug.Log("Prefab trouvé, instanciation...");
        
        // Calculer la position d'spawn
        Vector3 spawnPos = GetSpawnPosition(prefab);
        
        instance = Instantiate(prefab, spawnPos, prefab.transform.rotation);
        Debug.Log("Prefab instancié à la position : " + instance.transform.position);

        // Ajouter le script MoveWhenNotLookedAt seulement si demandé
        if (shouldMove && movementType == MovementType.MoveWhenNotLookedAt)
        {
            MoveWhenNotLookedAt moveScript = instance.GetComponent<MoveWhenNotLookedAt>();
            if (moveScript == null)
            {
                moveScript = instance.AddComponent<MoveWhenNotLookedAt>();
                Debug.Log("Script MoveWhenNotLookedAt ajouté");
            }
        }

        Animator animator = instance.GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log("Animator trouvé, configuration...");
            animator.runtimeAnimatorController = animatorController;
            if (animatorController != null && animatorController.animationClips.Length > 0)
            {
                animator.Play(animatorController.animationClips[0].name);
            }
            animator.applyRootMotion = false;
        }
        else
        {
            Debug.Log("Pas d'Animator sur le prefab");
        }

        // Gestion du mouvement selon les paramètres
        if (!shouldMove)
        {
            // Pas de mouvement, juste attendre
            Debug.Log("Pas de mouvement, attente de " + activeDuration + " secondes");
            yield return new WaitForSeconds(activeDuration);
        }
        else if (movementType == MovementType.MoveWhenNotLookedAt)
        {
            // MoveWhenNotLookedAt s'occupe du mouvement
            Debug.Log("MoveWhenNotLookedAt gère le mouvement, attente de " + activeDuration + " secondes");
            yield return new WaitForSeconds(activeDuration);
        }
        else
        {
            // Mouvement tout droit
            Debug.Log("Mouvement tout droit");
            float elapsed = 0f;
    
            while (elapsed < activeDuration && instance != null)
            {
                // Avancer tout droit dans la direction forward de l'objet
                instance.transform.position += instance.transform.forward * moveSpeed * Time.deltaTime;
        
                elapsed += Time.deltaTime;
                yield return null;
            }
        }


        if (instance != null)
        {
            Debug.Log("Destruction du prefab après " + activeDuration + " secondes");
            Destroy(instance);
        }
    }
}