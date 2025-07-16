using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiderEventManager : MonoBehaviour
{
    public enum SpiderMode { Solo, Multi }
    public enum SpiderBehavior { Move, Jump, Descend }

    [System.Serializable]
    public class SpiderData
    {
        public string name = "Araignée";
        public GameObject spiderModelPrefab;
        public RuntimeAnimatorController animatorController;
        public Transform spawnPoint;
        public Vector3 moveDirection = Vector3.forward;
        public float moveSpeed = 2f;
        public Collider triggerZone;
        public string animationStateName = "Walk";
        public SpiderBehavior behavior = SpiderBehavior.Move;
        public SpiderMode mode = SpiderMode.Solo;

        public int numberToSpawn = 1;

        [HideInInspector] public GameObject instance;
        [HideInInspector] public bool hasSpawned = false;
    }

    [Header("Liste des araignées à gérer")]
    public List<SpiderData> spiders = new List<SpiderData>();

    void Start()
    {
        foreach (SpiderData spider in spiders)
        {
            // Ne pas instancier directement en mode Multi
            if (spider.mode == SpiderMode.Multi)
            {
                spider.instance = null;
                spider.hasSpawned = false;
            }

            // Ajout du listener de détection
            if (spider.triggerZone != null)
            {
                TriggerListener listener = spider.triggerZone.gameObject.AddComponent<TriggerListener>();
                listener.Setup(spider, this);
            }
        }
    }

    public void ActivateSpider(SpiderData spider)
    {
        if (spider.hasSpawned) return;
        spider.hasSpawned = true;

        int count = spider.mode == SpiderMode.Multi ? spider.numberToSpawn : 1;

        for (int i = 0; i < count; i++)
        {
            // Légère variation de position pour éviter le chevauchement
            Vector3 offset = new Vector3(i * 0.5f, 0, 0); // décalage horizontal
            Vector3 spawnPos = spider.spawnPoint.position;

            // Appliquer un décalage uniquement en mode Multi
            if (spider.mode == SpiderMode.Multi)
            {
                spawnPos += new Vector3(i * 1.5f, 0, 1); 
            }


            GameObject instance = Instantiate(spider.spiderModelPrefab, spawnPos, spider.spawnPoint.rotation);
            instance.SetActive(true);

            Animator animator = instance.GetComponent<Animator>();
            if (animator != null && spider.animatorController != null)
            {
                animator.runtimeAnimatorController = spider.animatorController;

                if (!string.IsNullOrEmpty(spider.animationStateName))
                {
                    animator.Play(spider.animationStateName);
                    Debug.Log($"→ Animation lancée : {spider.animationStateName}");
                }

                animator.applyRootMotion = false;
            }

            // Comportement
            switch (spider.behavior)
            {
                case SpiderBehavior.Move:
                    StartCoroutine(MoveSpider(instance, spider.moveDirection, spider.moveSpeed));
                    break;
                case SpiderBehavior.Jump:
                    StartCoroutine(JumpSpiderArc(instance));
                    break;
                case SpiderBehavior.Descend:
                    StartCoroutine(DescendSpider(instance));
                    break;
            }
        }
    }



    private IEnumerator MoveSpider(GameObject spiderObj, Vector3 direction, float speed)
    {
        while (spiderObj != null)
        {
            spiderObj.transform.position += direction.normalized * speed * Time.deltaTime;
            yield return null;
        }
    }


    private IEnumerator JumpSpiderArc(GameObject spiderObj, float height = 2f, float duration = 1f, float horizontalDistance = 2f)
    {
        Vector3 startPos = spiderObj.transform.position;
        Vector3 endPos = startPos + spiderObj.transform.forward * horizontalDistance;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 horizontal = Vector3.Lerp(startPos, endPos, t);
            float arcY = Mathf.Sin(t * Mathf.PI) * height;

            spiderObj.transform.position = new Vector3(horizontal.x, startPos.y + arcY, horizontal.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        spiderObj.transform.position = new Vector3(endPos.x, startPos.y, endPos.z);

        // 🔁 Attendre 1s puis détruire
        yield return new WaitForSeconds(1f);
        Destroy(spiderObj);
    }


    private IEnumerator DescendSpider(GameObject spiderObj, float descendDistance = 2f, float speed = 1f)
    {
        Vector3 start = spiderObj.transform.position;
        Vector3 end = start - Vector3.up * descendDistance;

        while (Vector3.Distance(spiderObj.transform.position, end) > 0.05f)
        {
            spiderObj.transform.position = Vector3.MoveTowards(spiderObj.transform.position, end, speed * Time.deltaTime);
            yield return null;
        }
    }
}
