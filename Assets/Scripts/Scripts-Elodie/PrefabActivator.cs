using UnityEngine;
using System.Collections;

public class PrefabActivator : MonoBehaviour
{
    [Header("Prefab à instancier depuis Resources")]
    public string prefabResourceName;

    [Tooltip("Durée pendant laquelle le prefab reste actif")]
    public float activeDuration = 2f;

    [Tooltip("Vitesse de déplacement en unités/seconde (si pas de MoveWhenNotLookedAt)")]
    public float moveSpeed = 2f;

    [Tooltip("AnimatorController à glisser (pour jouer l'animation)")]
    public RuntimeAnimatorController animatorController;

    [Header("Comportement")]
    [Tooltip("Cocher si ce prefab doit utiliser MoveWhenNotLookedAt au lieu du mouvement basique")]
    public bool useMoveWhenNotLookedAt = false;

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
        instance = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation);
        Debug.Log("Prefab instancié à la position : " + instance.transform.position);

        // Ajouter le script MoveWhenNotLookedAt seulement si demandé
        if (useMoveWhenNotLookedAt)
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

        // Si on utilise MoveWhenNotLookedAt, on attend juste
        if (useMoveWhenNotLookedAt)
        {
            yield return new WaitForSeconds(activeDuration);
        }
        else
        {
            // Sinon, mouvement basique vers le joueur
            float elapsed = 0f;
            while (elapsed < activeDuration && instance != null && player != null)
            {
                Vector3 directionToPlayer = (player.position - instance.transform.position);
                directionToPlayer.y = 0;
                directionToPlayer = directionToPlayer.normalized;
                
                instance.transform.position += directionToPlayer * moveSpeed * Time.deltaTime;
                
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