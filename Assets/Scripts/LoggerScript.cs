using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;

public class LoggerScript : MonoBehaviour
{
    [Header("XR Elements")]
    public Transform newcamera;
    public Transform leftController;
    public Transform rightController;

    [SerializeField] private int repetitionLimit = 30;
    private int _repetitionCount = 0;
    public event Action OnRepetitionLimitReached;

    [Header("Elements with prefabTags to log")]
    public List<string> prefabTags = new List<string>();

    private readonly string fileNamePrefix = "Log_Data";
    private string basePath = "";
    private const char Delim = '\t';
    private StreamWriter _streamWriter;
    private readonly List<string> _eventsTriggered = new List<string>();
    private readonly DateTime _epochStart =
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private bool _isLogging;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 🔴 VERY IMPORTANT: enforce dot-decimal format
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

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

        // ✅ FAILSAFE repetition at the END (never shifts)
        lineToWrite += $"{_repetitionCount}";

        _streamWriter.WriteLine(lineToWrite);
        _eventsTriggered.Clear();
    }
    }
    public void StartLogging(string participant, int session)
    {
        _isLogging = true;

        string gameInfo = $"part~{participant}_sess~{session}";
        string fileName =
            $"{fileNamePrefix}_{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}_{gameInfo}.tsv";

        string filePath = basePath + fileName;
        _streamWriter = new StreamWriter(filePath);

        Debug.Log($"[LoggerScript] Logging to file: {filePath}");

        string header =
            $"{GetXRHeader()}{SetHeaderForPrefabs()}Unity.RepetitionSafe";

        _streamWriter.WriteLine(header);
        _streamWriter.Flush();
    }

    public void StopLogging()
    {
        _isLogging = false;
        _streamWriter?.Flush();
        _streamWriter?.Close();
    }

    public void AddEvent(string eventToLog)
    {
        _eventsTriggered.Add(eventToLog);
    }

    public void AddRepetition()
    {
        _repetitionCount++;
        AddEvent($"Repetition_{_repetitionCount}");

        Debug.Log($"[LoggerScript] Repetition {_repetitionCount} counted.");

        if (_repetitionCount >= repetitionLimit)
        {
            StopLogging();
            OnRepetitionLimitReached?.Invoke();
            Application.Quit();
        }
    }

    private string SetHeaderForPrefabs()
    {
        string h = "";
        foreach (var tag in prefabTags)
        {
            h +=
                $"Unity.GameObject.{tag}.position_x{Delim}" +
                $"Unity.GameObject.{tag}.position_y{Delim}" +
                $"Unity.GameObject.{tag}.position_z{Delim}" +
                $"Unity.GameObject.{tag}.rotation.euler_x{Delim}" +
                $"Unity.GameObject.{tag}.rotation.euler_y{Delim}" +
                $"Unity.GameObject.{tag}.rotation.euler_z{Delim}" +
                $"Unity.GameObject.{tag}.rotation.quaternion_x{Delim}" +
                $"Unity.GameObject.{tag}.rotation.quaternion_y{Delim}" +
                $"Unity.GameObject.{tag}.rotation.quaternion_z{Delim}" +
                $"Unity.GameObject.{tag}.rotation.quaternion_w{Delim}" +
                $"Unity.GameObject.{tag}.localScale_x{Delim}" +
                $"Unity.GameObject.{tag}.localScale_y{Delim}" +
                $"Unity.GameObject.{tag}.localScale_z{Delim}" +
                $"Unity.GameObject.{tag}.lossyScale_x{Delim}" +
                $"Unity.GameObject.{tag}.lossyScale_y{Delim}" +
                $"Unity.GameObject.{tag}.lossyScale_z{Delim}";
        }
        return h;
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
