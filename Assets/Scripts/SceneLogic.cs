using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenceLogic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewParticipant()
{
    PlayerPrefs.SetString("Mode", "New");
    SceneManager.LoadScene("AssignIDScene");
}

public void ReturningParticipant()
{
    PlayerPrefs.SetString("Mode", "Returning");
    SceneManager.LoadScene("SelectIDScene");
}

}
