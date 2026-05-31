using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public sealed class WorldData
{
    public List<TransformData> transforms = new List<TransformData>();
}

[Serializable]
public sealed class TransformData
{
    public string id;
    public Vector3 position;
    public Quaternion rotation;
}

public sealed class WorldSaveLoadManager : MonoBehaviour
{
    [SerializeField] private List<SaveableTransform> saveTargets = new List<SaveableTransform>();
    [SerializeField] private Text statusText;
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool saveOnPause = true;
    [SerializeField] private bool saveOnQuit = true;

    private const string FileName = "hw23_world_state.json";

    public string SavePath => Path.Combine(Application.persistentDataPath, FileName);
    public IReadOnlyList<SaveableTransform> SaveTargets => saveTargets;

    public void SetStatusText(Text text)
    {
        statusText = text;
    }

    private void Start()
    {
        RemoveMissingTargets();

        if (loadOnStart)
        {
            Load();
        }
        else
        {
            SetStatus("Ready. Save path: " + SavePath);
        }
    }

    public void RegisterTarget(SaveableTransform target)
    {
        if (target == null || saveTargets.Contains(target))
        {
            return;
        }

        saveTargets.Add(target);
    }

    public void Save()
    {
        RemoveMissingTargets();

        WorldData worldData = new WorldData();
        foreach (SaveableTransform target in saveTargets)
        {
            Transform targetTransform = target.transform;
            worldData.transforms.Add(new TransformData
            {
                id = target.SaveId,
                position = targetTransform.position,
                rotation = targetTransform.rotation
            });
        }

        string json = JsonUtility.ToJson(worldData, true);
        File.WriteAllText(SavePath, json);
        SetStatus("Saved " + worldData.transforms.Count + " transforms\n" + SavePath);
    }

    public void Load()
    {
        RemoveMissingTargets();

        if (!File.Exists(SavePath))
        {
            SetStatus("No save file yet\n" + SavePath);
            return;
        }

        string json = File.ReadAllText(SavePath);
        WorldData worldData = JsonUtility.FromJson<WorldData>(json);
        if (worldData == null || worldData.transforms == null)
        {
            SetStatus("Save file is empty or invalid");
            return;
        }

        Dictionary<string, SaveableTransform> targetsById = new Dictionary<string, SaveableTransform>();
        foreach (SaveableTransform target in saveTargets)
        {
            targetsById[target.SaveId] = target;
        }

        int restoredCount = 0;
        foreach (TransformData data in worldData.transforms)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.id))
            {
                continue;
            }

            if (!targetsById.TryGetValue(data.id, out SaveableTransform target))
            {
                continue;
            }

            target.transform.SetPositionAndRotation(data.position, data.rotation);
            restoredCount++;
        }

        SetStatus("Loaded " + restoredCount + " transforms\n" + SavePath);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && saveOnPause)
        {
            Save();
        }
    }

    private void OnApplicationQuit()
    {
        if (saveOnQuit)
        {
            Save();
        }
    }

    private void RemoveMissingTargets()
    {
        saveTargets.RemoveAll(target => target == null);
    }

    private void SetStatus(string message)
    {
        Debug.Log("[HW23 SaveLoad] " + message);

        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
