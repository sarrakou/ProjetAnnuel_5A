using UnityEngine;

public class MoveWhenNotLookedAt : MonoBehaviour
{
    public Transform playerCamera; 
    public float speed = 2f;
    public float lookThreshold = 0.95f;
    public float lifeTime = 30f; // Temps de vie en secondes
    
    private float timer = 0f;
    private float originalY; // Sauvegarder la hauteur Y d'origine

    private void Start()
    {
        // Trouver automatiquement la main camera
        if (playerCamera == null)
        {
            GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCam != null)
            {
                playerCamera = mainCam.transform;
            }
            else
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    playerCamera = cam.transform;
                }
                else
                {
                    Debug.LogWarning("Aucune caméra trouvée pour " + gameObject.name);
                }
            }
        }
        
        // Sauvegarder la position Y d'origine
        originalY = transform.position.y;
    }

    private void Update()
    {
        // Vérifier que la caméra est assignée
        if (playerCamera == null) return;

        // Incrémenter le timer
        timer += Time.deltaTime;
        
        // Détruire l'objet après le temps de vie
        if (timer >= lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 toObject = (transform.position - playerCamera.position).normalized;
        float dot = Vector3.Dot(playerCamera.forward, toObject);

        if (dot < lookThreshold) 
        {
            Move();
        }
        else
        {
            // Ne bouge pas
        }
    }

    private void Move()
    {
        // Calculer la direction vers la caméra mais seulement sur X et Z
        Vector3 direction = (playerCamera.position - transform.position);
        direction.y = 0; // Ignorer la composante Y
        direction = direction.normalized;
        
        // Calculer la nouvelle position
        Vector3 newPosition = transform.position + direction * speed * Time.deltaTime;
        
        // Forcer le Y à rester le même qu'à l'origine
        newPosition.y = originalY;
        
        transform.position = newPosition;
    }
}