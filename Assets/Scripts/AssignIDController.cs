using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class AssignIDController : MonoBehaviour
{
    public TMP_Text assignedIDText;
    private string currentID;
    private int sessionID =1; 
    void Start()
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier.Substring(0, 6); // shorten for readability
        var data = ParticipantStorage.Load();
        int nextIDnum = data.ids.Count + 1;
         currentID = $"{deviceID}_P{nextIDnum:000}";

        assignedIDText.text = "Ihre Teilnehmer ID: " + currentID;

        data.ids.Add(currentID);
        ParticipantStorage.Save(data);
    }

    public void StartSession()
    {
        PlayerPrefs.SetString("CurrentParticipantID", currentID);
       // SceneManager.LoadScene("Game"); // change this to your actual AR scene name
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
                logger.StartLogging(currentID, sessionID);
            }

            // Unsubscribe so this only runs once
            SceneManager.sceneLoaded -= OnGameSceneLoaded;
        }
    }
}
