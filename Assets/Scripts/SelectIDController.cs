using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SelectIDController : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    private int sessionNumber = 2; 

    void Start()
    {
        var data = ParticipantStorage.Load();
        dropdown.ClearOptions();
        dropdown.AddOptions(data.ids);
    }

    public void StartSession()
    {
        string selectedID = dropdown.options[dropdown.value].text;
        PlayerPrefs.SetString("CurrentParticipantID", selectedID);
        
        // Subscribe to sceneLoaded event
        SceneManager.sceneLoaded += OnGameSceneLoaded;

        // Load the Game scene
        SceneManager.LoadScene("Game");
}

private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            // Find the LoggerScript instance
            var logger = FindFirstObjectByType<LoggerScript>();
            if (logger != null)
            {
                // Reassign XR references in the new scene
                logger.AssignXRReferences();
                // Start logging
                logger.StartLogging(PlayerPrefs.GetString("CurrentParticipantID"), sessionNumber);
            }

            // Unsubscribe so this only runs once
            SceneManager.sceneLoaded -= OnGameSceneLoaded;
        }
    }
}