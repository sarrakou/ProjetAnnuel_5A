using UnityEngine;

public class PhoneInteractable : MonoBehaviour, IInteractableBis
{
    [Header("Configuration du téléphone")]
    public string firstQuestName = "Appeler la police1"; // Mission 8 (première tentative)
    public string secondQuestName = "Trouver un moyen de faire fonctionner le téléphone"; // Mission 7
    public string thirdQuestName = "Appeler la police"; // Mission 8 (deuxième tentative, vraiment réussie)
    
    private GameManager gameManager;
    private bool firstInteractionDone = false;
    private bool secondInteractionDone = false;
    private bool thirdInteractionDone = false;

    void Start()
    {
        // Trouver le GameManager dans la scène
        gameManager = FindObjectOfType<GameManager>();
        
        if (gameManager == null)
        {
            Debug.LogError(" Aucun GameManager trouvé dans la scène !");
        }
    }

    public void Interact()
    {
        if (gameManager == null)
        {
            Debug.LogError(" GameManager non trouvé !");
            return;
        }

        // Première interaction - Tentative d'appel à la police (échec)
        if (!firstInteractionDone)
        {
            Debug.Log(" Vous décrochez le téléphone pour appeler la police...");
            Debug.Log(" *Bip bip bip* Pas de tonalité ! Le téléphone ne fonctionne pas.");
            
            // Compléter la première quête (tentative d'appel)
            gameManager.CompleteQuestByName(firstQuestName);
            firstInteractionDone = true;
            
            Debug.Log($" Première interaction terminée. Quête complétée : {firstQuestName}");
            return;
        }

        // Deuxième interaction - Réparer le téléphone
        if (firstInteractionDone && !secondInteractionDone)
        {
            Debug.Log(" Vous examinez le téléphone plus attentivement...");
            Debug.Log(" Vous trouvez le problème et réussissez à le réparer !");
            
            // Compléter la deuxième quête
            gameManager.CompleteQuestByName(secondQuestName);
            secondInteractionDone = true;
            
            Debug.Log($" Deuxième interaction terminée. Quête complétée : {secondQuestName}");
            return;
        }

        // Troisième interaction - Appeler effectivement la police
        if (firstInteractionDone && secondInteractionDone && !thirdInteractionDone)
        {
            Debug.Log(" Le téléphone fonctionne maintenant ! Vous appelez la police...");
            Debug.Log(" 'Allô ? Police ? J'ai besoin d'aide immédiatement dans cette maison !'");
            Debug.Log(" 'Nous arrivons tout de suite !' répond l'opérateur.");
            
            // Compléter la troisième quête (vrai appel à la police)
            gameManager.CompleteQuestByName(thirdQuestName);
            thirdInteractionDone = true;
            
            Debug.Log($" Troisième interaction terminée. Quête complétée : {thirdQuestName}");
            return;
        }

        // Si toutes les interactions sont terminées
        if (firstInteractionDone && secondInteractionDone && thirdInteractionDone)
        {
            Debug.Log(" Vous avez déjà appelé la police. Ils devraient arriver bientôt...");
        }
}
}