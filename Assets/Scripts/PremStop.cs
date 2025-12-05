using UnityEngine;

public class PremStop : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void stop()
    {
       var logger = FindFirstObjectByType<LoggerScript>();

        logger.StopLogging(); 
    }
}
