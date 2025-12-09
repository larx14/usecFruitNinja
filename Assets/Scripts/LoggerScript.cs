using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;



    public class LoggerScript : MonoBehaviour
    {
        //[Header("General")]
        [Header("XR Elements")]
        public Transform newcamera;
        public Transform leftController;
        public Transform rightController;
        [SerializeField] private int repetitionLimit = 30;
        private int _repetitionCount = 0;
        public event Action OnRepetitionLimitReached;


        // ToDo: If you have GameObjects that you want to log (besides left controller, right controller, and head), you need to give them tags
        [Header("Elements with prefabTags to log")]
        public List<string> prefabTags = new List<string>();

        private readonly string fileNamePrefix = "Log_Data";
        private string basePath = "";
        private const char Delim = '\t';
        private StreamWriter _streamWriter;
        private readonly List<string> _eventsTriggered = new List<string>();
        private readonly DateTime _epochStart = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        private bool _isLogging;


void Awake()
{
    DontDestroyOnLoad(gameObject);
}

        void Start()
        {

            Debug.Log($"[LoggerScript] Logger started! Application.persistentDataPath: {Application.persistentDataPath}");
            basePath = Application.persistentDataPath + "/";
        }

        void Update()
        {
            if (_isLogging)
            {
                var timestamp = (DateTime.UtcNow - _epochStart).TotalMilliseconds;
                var events = string.Join("| ", _eventsTriggered);
                var lineToWrite = $"{Time.frameCount}{Delim}{Time.realtimeSinceStartup}{Delim}{timestamp}{Delim}{events}{Delim}{_repetitionCount}{Delim}";


                // Log HMD position and rotation
                // ToDo: Check your Version of the XR Plugin: If you have a OVRCameraRig you need to find the main camera to assign it. If you have an XROrigin xrOrigin, you might need to call xrOrigin.Camera.transform.position etc.
                // IMPORTANT: Make sure to check that you track the correct data here as it can easily be messed up since this part changes depending on the used Toolkit for MR!
                var hmdPosition = newcamera.transform.position;
                var hmdRotation = newcamera.transform.rotation;
                lineToWrite += $"{hmdPosition.x}{Delim}{hmdPosition.y}{Delim}{hmdPosition.z}{Delim}" +
                               $"{hmdRotation.eulerAngles.x}{Delim}{hmdRotation.eulerAngles.y}{Delim}{hmdRotation.eulerAngles.z}{Delim}" +
                               $"{hmdRotation.x}{Delim}{hmdRotation.y}{Delim}{hmdRotation.z}{Delim}{hmdRotation.w}{Delim}";

                // Log left controller position and rotation
                if (leftController != null)
                {
                    var leftPosition = leftController.transform.position;
                    var leftRotation = leftController.transform.rotation;
                    lineToWrite += $"{leftPosition.x}{Delim}{leftPosition.y}{Delim}{leftPosition.z}{Delim}" +
                                   $"{leftRotation.eulerAngles.x}{Delim}{leftRotation.eulerAngles.y}{Delim}{leftRotation.eulerAngles.z}{Delim}" +
                                   $"{leftRotation.x}{Delim}{leftRotation.y}{Delim}{leftRotation.z}{Delim}{leftRotation.w}{Delim}";
                }
                else
                {
                    lineToWrite += $"{Delim}{Delim}{Delim}{Delim}{Delim}{Delim}{Delim}{Delim}{Delim}{Delim}";
                }

                // Log right controller position and rotation
                if (rightController != null)
                {
                    var rightPosition = rightController.transform.position;
                    var rightRotation = rightController.transform.rotation;
                    lineToWrite += $"{rightPosition.x}{Delim}{rightPosition.y}{Delim}{rightPosition.z}{Delim}" +
                                   $"{rightRotation.eulerAngles.x}{Delim}{rightRotation.eulerAngles.y}{Delim}{rightRotation.eulerAngles.z}{Delim}" +
                                   $"{rightRotation.x}{Delim}{rightRotation.y}{Delim}{rightRotation.z}{Delim}{rightRotation.w}{Delim}";
                }
                else
                {
                    lineToWrite += $"{Delim}{Delim}{Delim}{Delim}{Delim}{Delim}{Delim}{Delim}{Delim}{Delim}";
                }

                // Log instantiated prefabs
                foreach (var tag in prefabTags)
                {
                    var prefabInstances = GameObject.FindGameObjectsWithTag(tag);
                    foreach (var instance in prefabInstances)
                    {
                        var position = instance.transform.position;
                        var rotation = instance.transform.rotation;
                        var localScale = instance.transform.localScale;
                        var lossyScale = instance.transform.lossyScale;
                        lineToWrite += $"{position.x}{Delim}{position.y}{Delim}{position.z}{Delim}" +
                                       $"{rotation.eulerAngles.x}{Delim}{rotation.eulerAngles.y}{Delim}{rotation.eulerAngles.z}{Delim}" +
                                       $"{rotation.x}{Delim}{rotation.y}{Delim}{rotation.z}{Delim}{rotation.w}{Delim}" +
                                       $"{localScale.x}{Delim}{localScale.y}{Delim}{localScale.z}{Delim}" +
                                       $"{lossyScale.x}{Delim}{lossyScale.y}{Delim}{lossyScale.z}{Delim}";
                                        Debug.Log("Tag" + instance.tag); 
                    }
                    
                }
                
                _streamWriter.WriteLine(lineToWrite);
                _eventsTriggered.Clear();
               
            }
        }
     
        // ToDo: Change this method to log relevant information in the file name - you need a ParticipantID, maybe a SessionID if you plan to take a break between sessions, maybe a RepetitionID for muliple repetitions of your movement (or you can also get this by the timestamp later), and maybe more if you want to log game specific details
        public void StartLogging(string participant, int session)
        {
            string gameInfo = $"part~{participant}_sess~{session}";
            _isLogging = true;
            var ctime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
          /*  if (file_info != "")
            {
                file_info = "_" + file_info;
            } **/ 
            var fileName = fileNamePrefix + 
           //  file_info + 
            "_" + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss") + "_" + gameInfo + ".tsv";
            var filePath = basePath + fileName;
            Debug.Log($"[LoggerScript] Logging to file: {filePath}");
            if (File.Exists(filePath))
            {
                filePath = GenerateNextFileName(filePath);
            }
            _streamWriter = new StreamWriter(filePath);
            var prefabHeader = SetHeaderForPrefabs();

            _streamWriter.WriteLine($"{GetXRHeader()}{prefabHeader}");

            _streamWriter.Flush();
            this.Update();
        }

        public void StopLogging()
        {
            _isLogging = false;
            if (_streamWriter != null)
            {
                _streamWriter.Flush();
                _streamWriter.Close();
            }
        }

        public void AddEvent(string eventToLog)
        {
            _eventsTriggered.Add(eventToLog);
        }

        private string SetHeaderForPrefabs()
        {
            var lineToWrite = "";
            const string prefix = "Unity.GameObject.";
            const string position = ".position";
            const string euler = ".rotation.euler";
            const string quaternion = ".rotation.quaternion";
            const string localScale = ".localScale";
            const string lossyScale = ".lossyScale";
            const string x = "_x";
            const string y = "_y";
            const string z = "_z";
            const string w = "_w";

            foreach (var tag in prefabTags)
            {
                lineToWrite += $"{prefix}{tag}{position}{x}{Delim}" +
                               $"{prefix}{tag}{position}{y}{Delim}" +
                               $"{prefix}{tag}{position}{z}{Delim}" +
                               $"{prefix}{tag}{euler}{x}{Delim}" +
                               $"{prefix}{tag}{euler}{y}{Delim}" +
                               $"{prefix}{tag}{euler}{z}{Delim}" +
                               $"{prefix}{tag}{quaternion}{x}{Delim}" +
                               $"{prefix}{tag}{quaternion}{y}{Delim}" +
                               $"{prefix}{tag}{quaternion}{z}{Delim}" +
                               $"{prefix}{tag}{quaternion}{w}{Delim}" +
                               $"{prefix}{tag}{localScale}{x}{Delim}" +
                               $"{prefix}{tag}{localScale}{y}{Delim}" +
                               $"{prefix}{tag}{localScale}{z}{Delim}" +
                               $"{prefix}{tag}{lossyScale}{x}{Delim}" +
                               $"{prefix}{tag}{lossyScale}{y}{Delim}" +
                               $"{prefix}{tag}{lossyScale}{z}{Delim}";
            }
            return lineToWrite;
        }

        private string GetXRHeader()
        {
            const string prefix = "Unity.";
            const string hmdPosition = "HMDPosition";
            const string leftControllerPosition = "LeftControllerPosition";
            const string rightControllerPosition = "RightControllerPosition";

            const string position = ".position";
            const string euler = ".rotation.euler";
            const string quaternion = ".rotation.quaternion";

            const string x = "_x";
            const string y = "_y";
            const string z = "_z";
            const string w = "_w";

            return $"{prefix}frameCount{Delim}" +
                   $"{prefix}realtimeSinceStartup{Delim}" +
                   $"{prefix}unixTimestamp{Delim}" +
                   $"{prefix}Event{Delim}" +
                    $"{prefix}Repetition{Delim}" +
                   $"{prefix}{hmdPosition}{position}{x}{Delim}" +
                   $"{prefix}{hmdPosition}{position}{y}{Delim}" +
                   $"{prefix}{hmdPosition}{position}{z}{Delim}" +
                   $"{prefix}{hmdPosition}{euler}{x}{Delim}" +
                   $"{prefix}{hmdPosition}{euler}{y}{Delim}" +
                   $"{prefix}{hmdPosition}{euler}{z}{Delim}" +
                   $"{prefix}{hmdPosition}{quaternion}{x}{Delim}" +
                   $"{prefix}{hmdPosition}{quaternion}{y}{Delim}" +
                   $"{prefix}{hmdPosition}{quaternion}{z}{Delim}" +
                   $"{prefix}{hmdPosition}{quaternion}{w}{Delim}" +
                   $"{prefix}{leftControllerPosition}{position}{x}{Delim}" +
                   $"{prefix}{leftControllerPosition}{position}{y}{Delim}" +
                   $"{prefix}{leftControllerPosition}{position}{z}{Delim}" +
                   $"{prefix}{leftControllerPosition}{euler}{x}{Delim}" +
                   $"{prefix}{leftControllerPosition}{euler}{y}{Delim}" +
                   $"{prefix}{leftControllerPosition}{euler}{z}{Delim}" +
                   $"{prefix}{leftControllerPosition}{quaternion}{x}{Delim}" +
                   $"{prefix}{leftControllerPosition}{quaternion}{y}{Delim}" +
                   $"{prefix}{leftControllerPosition}{quaternion}{z}{Delim}" +
                   $"{prefix}{leftControllerPosition}{quaternion}{w}{Delim}" +
                   $"{prefix}{rightControllerPosition}{position}{x}{Delim}" +
                   $"{prefix}{rightControllerPosition}{position}{y}{Delim}" +
                   $"{prefix}{rightControllerPosition}{position}{z}{Delim}" +
                   $"{prefix}{rightControllerPosition}{euler}{x}{Delim}" +
                   $"{prefix}{rightControllerPosition}{euler}{y}{Delim}" +
                   $"{prefix}{rightControllerPosition}{euler}{z}{Delim}" +
                   $"{prefix}{rightControllerPosition}{quaternion}{x}{Delim}" +
                   $"{prefix}{rightControllerPosition}{quaternion}{y}{Delim}" +
                   $"{prefix}{rightControllerPosition}{quaternion}{z}{Delim}" +
                   $"{prefix}{rightControllerPosition}{quaternion}{w}{Delim}";
        }

        private static string GenerateNextFileName(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            var i = 0;
            while (File.Exists(filePath))
            {
                filePath = i == 0
                    ? filePath.Replace(extension, "(" + ++i + ")" + extension)
                    : filePath.Replace("(" + i + ")" + extension, "(" + ++i + ")" + extension);
            }
            return filePath;
        }

     public void AddRepetition()
{
    _repetitionCount++;
    AddEvent($"Repetition_{_repetitionCount}");
    Debug.Log($"[LoggerScript] Repetition {_repetitionCount} counted.");

    if (_repetitionCount >= repetitionLimit)
    {
        Debug.Log("[LoggerScript] Repetition limit reached. Stopping logging...");
        StopLogging();
        OnRepetitionLimitReached?.Invoke();   // notify game logic
         Application.Quit(); // exits the application
    }
}
public void AssignXRReferences()
{
    var rig = GameObject.Find("[BuildingBlock] Camera Rig");
    if (rig != null)
    {
        newcamera = rig.transform.Find("TrackingSpace/CenterEyeAnchor");
        leftController = rig.transform.Find("TrackingSpace/LeftHandAnchor");
        rightController = rig.transform.Find("TrackingSpace/RightHandAnchor");
        Debug.Log($"[LoggerScript] XR references reassigned. Head: {newcamera}, Left: {leftController}, Right: {rightController}");
    }
    else
    {
        Debug.LogWarning("[LoggerScript] Camera Rig not found!");
    }
}



    }
