using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Quest> quests = new List<Quest>();
    private LightManager lightManager;

    void Start()
    {
        // Référence au LightManager dans la scène
        lightManager = FindObjectOfType<LightManager>();

        // Initialisation des quêtes
        quests.Add(new Quest("Trouver un moyen d'allumer les lumières", "Explorer la maison pour rétablir l'électricité."));
        quests.Add(new Quest("Trouver la clé de la chambre principale", "Chercher dans les pièces accessibles."));
        quests.Add(new Quest("Trouver le journal du propriétaire", "Il pourrait contenir des indices."));
        quests.Add(new Quest("Trouver le coffre-fort", "Localiser l'endroit où il est caché."));
        quests.Add(new Quest("Récupérer la clé", "Elle est probablement dans le coffre-fort."));
        quests.Add(new Quest("Trouver la porte secrète", "Quelque chose cloche dans cette maison."));
        quests.Add(new Quest("Appeler la police1", "Il faut de l'aide immédiatement."));
        quests.Add(new Quest("Trouver un moyen de faire fonctionner le téléphone", "Il est hors-service."));
        quests.Add(new Quest("Appeler la police", "Il faut de l'aide immédiatement."));
        quests.Add(new Quest("Il faut sortir. Maintenant !", "Quitte la maison au plus vite !"));

        AfficherToutesLesQuetes();
    }

    void Update()
    {
        // Appuyer sur les touches 1 à 9 pour compléter les quêtes manuellement
        if (Input.GetKeyDown(KeyCode.Alpha1)) CompleterQuete(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) CompleterQuete(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CompleterQuete(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) CompleterQuete(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) CompleterQuete(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) CompleterQuete(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) CompleterQuete(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) CompleterQuete(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) CompleterQuete(8);
        if (Input.GetKeyDown(KeyCode.Alpha9)) CompleterQuete(9);
    }

    void CompleterQuete(int index)
    {
        if (index >= 0 && index < quests.Count && !quests[index].isCompleted)
        {
            quests[index].CompleteQuest();
            AfficherToutesLesQuetes();

            // Allumer les lumières si la 1ʳᵉ quête est complétée
            if (index == 0 && lightManager != null)
            {
                lightManager.SetAllLights(true);
            }
        }
    }
    
  
    public void CompleteQuestByName(string questName)
    {
        Debug.Log($" Tentative de complétion de la quête : {questName}");
        
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].questName == questName && !quests[i].isCompleted)
            {
                Debug.Log($" Quête trouvée et complétée : {questName}");
                quests[i].CompleteQuest();
                AfficherToutesLesQuetes();
                
                // Allumer les lumières si la 1ʳᵉ quête est complétée
                if (i == 0 && lightManager != null)
                {
                    lightManager.SetAllLights(true);
                }
                return;
            }
        }
        
        Debug.LogWarning($"⚠ Quête non trouvée ou déjà complétée : {questName}");
    }

    void AfficherToutesLesQuetes()
    {
        Debug.Log(" Liste des quêtes :");
        for (int i = 0; i < quests.Count; i++)
        {
            string status = quests[i].isCompleted ? "✅" : "❌";
            Debug.Log($"{i + 1}. {status} {quests[i].questName} - {quests[i].description}");
        }
    }


    
}