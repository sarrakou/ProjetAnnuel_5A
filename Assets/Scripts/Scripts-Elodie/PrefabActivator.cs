using UnityEngine;
using System.Collections;

public class PrefabActivator : MonoBehaviour
{
    [Header("Prefab à instancier depuis Resources")]
    public string prefabResourceName;

    [Tooltip("Durée pendant laquelle le prefab reste actif")]
    public float activeDuration = 2f;

    [Tooltip("Vitesse de déplacement en unités/seconde")]
    public float moveSpeed = 2f;

    [Tooltip("AnimatorController à glisser (pour jouer l'animation)")]
    public RuntimeAnimatorController animatorController;

    private bool triggered = false;
    private GameObject instance;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(ActivatePrefab());
        }
    }

    private IEnumerator ActivatePrefab()
    {
        if (string.IsNullOrEmpty(prefabResourceName))
        {
            Debug.LogError("PrefabActivator : Nom de prefab vide.");
            yield break;
        }

        GameObject prefab = Resources.Load<GameObject>(prefabResourceName);
        if (prefab == null)
        {
            Debug.LogError("PrefabActivator : Aucun prefab trouvé dans Resources avec le nom : " + prefabResourceName);
            yield break;
        }

        instance = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation);

        Animator animator = instance.GetComponent<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = animatorController;
            animator.Play(animatorController.animationClips[0].name); 
            animator.applyRootMotion = false; 
        }

        float moveSpeed = 2f;
        float elapsed = 0f;

        while (elapsed < activeDuration)
        {
            instance.transform.position += instance.transform.forward * moveSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(instance);
    }

}
