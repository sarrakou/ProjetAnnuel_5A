using UnityEngine;

public class TriggerListener : MonoBehaviour
{
    private SpiderEventManager.SpiderData spider;
    private SpiderEventManager manager;

    public void Setup(SpiderEventManager.SpiderData spiderData, SpiderEventManager managerRef)
    {
        spider = spiderData;
        manager = managerRef;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Modifier si besoin
        {
            manager.ActivateSpider(spider);
        }
    }
}
