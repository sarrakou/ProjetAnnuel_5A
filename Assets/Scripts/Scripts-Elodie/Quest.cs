using UnityEngine;

[System.Serializable]
public class Quest
{
    public string questName;
    public string description;
    public bool isCompleted;

    public Quest(string name, string desc)
    {
        questName = name;
        description = desc;
        isCompleted = false;
    }

    public void CompleteQuest()
    {
        isCompleted = true;
        Debug.Log($"Quête complétée : {questName}");
    }
}