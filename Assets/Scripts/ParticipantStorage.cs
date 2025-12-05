using System.IO;
using UnityEngine;

public static class ParticipantStorage
{
    private static string Path => Application.persistentDataPath + "/participants.json";

    public static ParticipantData Load()
    {
        if (!File.Exists(Path))
            return new ParticipantData();

        string json = File.ReadAllText(Path);
        return JsonUtility.FromJson<ParticipantData>(json);
    }

    public static void Save(ParticipantData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path, json);
    }
}
