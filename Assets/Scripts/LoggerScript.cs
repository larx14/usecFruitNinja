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
        if (!_isLogging) return;

        var timestamp = (DateTime.UtcNow - _epochStart).TotalMilliseconds;
        var events = string.Join("| ", _eventsTriggered);

        string lineToWrite =
            $"{Time.frameCount}{Delim}" +
            $"{Time.realtimeSinceStartup}{Delim}" +
            $"{timestamp}{Delim}" +
            $"{events}{Delim}" +
            $"{_repetitionCount}{Delim}";

        // ---------------- HMD ----------------
        var hmdPos = newcamera.transform.position;
        var hmdRot = newcamera.transform.rotation;

        lineToWrite +=
            $"{hmdPos.x}{Delim}{hmdPos.y}{Delim}{hmdPos.z}{Delim}" +
            $"{hmdRot.eulerAngles.x}{Delim}{hmdRot.eulerAngles.y}{Delim}{hmdRot.eulerAngles.z}{Delim}" +
            $"{hmdRot.x}{Delim}{hmdRot.y}{Delim}{hmdRot.z}{Delim}{hmdRot.w}{Delim}";

        // ---------------- LEFT CONTROLLER ----------------
        if (leftController != null)
        {
            var p = leftController.position;
            var r = leftController.rotation;

            lineToWrite +=
                $"{p.x}{Delim}{p.y}{Delim}{p.z}{Delim}" +
                $"{r.eulerAngles.x}{Delim}{r.eulerAngles.y}{Delim}{r.eulerAngles.z}{Delim}" +
                $"{r.x}{Delim}{r.y}{Delim}{r.z}{Delim}{r.w}{Delim}";
        }
        else
        {
            lineToWrite += new string(Delim, 10);
        }

        // ---------------- RIGHT CONTROLLER ----------------
        if (rightController != null)
        {
            var p = rightController.position;
            var r = rightController.rotation;

            lineToWrite +=
                $"{p.x}{Delim}{p.y}{Delim}{p.z}{Delim}" +
                $"{r.eulerAngles.x}{Delim}{r.eulerAngles.y}{Delim}{r.eulerAngles.z}{Delim}" +
                $"{r.x}{Delim}{r.y}{Delim}{r.z}{Delim}{r.w}{Delim}";
        }
        else
        {
            lineToWrite += new string(Delim, 10);
        }

        // ---------------- PREFABS ----------------
        foreach (var tag in prefabTags)
        {
            var instances = GameObject.FindGameObjectsWithTag(tag);
            foreach (var obj in instances)
            {
                var p = obj.transform.position;
                var r = obj.transform.rotation;
                var ls = obj.transform.localScale;
                var lossy = obj.transform.lossyScale;

                lineToWrite +=
                    $"{p.x}{Delim}{p.y}{Delim}{p.z}{Delim}" +
                    $"{r.eulerAngles.x}{Delim}{r.eulerAngles.y}{Delim}{r.eulerAngles.z}{Delim}" +
                    $"{r.x}{Delim}{r.y}{Delim}{r.z}{Delim}{r.w}{Delim}" +
                    $"{ls.x}{Delim}{ls.y}{Delim}{ls.z}{Delim}" +
                    $"{lossy.x}{Delim}{lossy.y}{Delim}{lossy.z}{Delim}";
            }
        }

        // ✅ FAILSAFE repetition at the END (never shifts)
        lineToWrite += $"{_repetitionCount}";

        _streamWriter.WriteLine(lineToWrite);
        _eventsTriggered.Clear();
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
        return
            $"Unity.frameCount{Delim}" +
            $"Unity.realtimeSinceStartup{Delim}" +
            $"Unity.unixTimestamp{Delim}" +
            $"Unity.Event{Delim}" +
            $"Unity.Repetition{Delim}";
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
