using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 세이브/로드 총괄 매니저
/// JSON 직렬화, 3슬롯 지원
/// </summary>
public class SaveManager : SingletonMonoBehaviour<SaveManager>
{
    public event Action<int> OnSaveCompleted;
    public event Action<int> OnLoadCompleted;

    [SerializeField] private int maxSlots = 3;

    private SaveData currentSaveData;
    private float sessionStartTime;

    private const string SAVE_FILE_PREFIX = "save_slot_";
    private const string SAVE_FILE_EXTENSION = ".json";

    public SaveData CurrentSaveData => currentSaveData;
    public int MaxSlots => maxSlots;

    protected override bool Persist => true;

    protected override void OnSingletonAwake()
    {
        currentSaveData = new SaveData();
        sessionStartTime = Time.realtimeSinceStartup;
    }

    // -- Public API --

    public void Save(int slotIndex)
    {
        if (!ValidateSlotIndex(slotIndex))
            return;

        CaptureAllStates();
        UpdateMetadata(slotIndex);

        string json = JsonUtility.ToJson(currentSaveData, true);
        string filePath = GetSaveFilePath(slotIndex);

        try
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, json);
            OnSaveCompleted?.Invoke(slotIndex);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Save failed (slot {slotIndex}): {e.Message}");
        }
    }

    public bool Load(int slotIndex)
    {
        if (!ValidateSlotIndex(slotIndex))
            return false;

        string filePath = GetSaveFilePath(slotIndex);

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[SaveManager] Save file not found: {filePath}");
            return false;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            currentSaveData = JsonUtility.FromJson<SaveData>(json);

            RestoreAllStates();
            sessionStartTime = Time.realtimeSinceStartup - currentSaveData.metadata.playTime;
            OnLoadCompleted?.Invoke(slotIndex);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load failed (slot {slotIndex}): {e.Message}");
            return false;
        }
    }

    public bool SlotExists(int slotIndex)
    {
        if (!ValidateSlotIndex(slotIndex))
            return false;

        return File.Exists(GetSaveFilePath(slotIndex));
    }

    public SaveMetadata GetSlotMetadata(int slotIndex)
    {
        if (!SlotExists(slotIndex))
            return null;

        try
        {
            string json = File.ReadAllText(GetSaveFilePath(slotIndex));
            var data = JsonUtility.FromJson<SaveData>(json);
            return data.metadata;
        }
        catch
        {
            return null;
        }
    }

    public void DeleteSlot(int slotIndex)
    {
        if (!ValidateSlotIndex(slotIndex))
            return;

        string filePath = GetSaveFilePath(slotIndex);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    // -- Internal --

    private void CaptureAllStates()
    {
        var saveables = FindObjectsOfType<MonoBehaviour>();
        foreach (var mb in saveables)
        {
            if (mb is ISaveable saveable)
            {
                saveable.CaptureState(currentSaveData);
            }
        }
    }

    private void RestoreAllStates()
    {
        var saveables = FindObjectsOfType<MonoBehaviour>();
        foreach (var mb in saveables)
        {
            if (mb is ISaveable saveable)
            {
                saveable.RestoreState(currentSaveData);
            }
        }
    }

    private void UpdateMetadata(int slotIndex)
    {
        currentSaveData.metadata.saveName = $"Slot {slotIndex + 1}";
        currentSaveData.metadata.UpdateTimestamp();
        currentSaveData.metadata.playTime = Time.realtimeSinceStartup - sessionStartTime;
    }

    private bool ValidateSlotIndex(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots)
        {
            Debug.LogError($"[SaveManager] Invalid slot index: {slotIndex}");
            return false;
        }

        return true;
    }

    private string GetSaveFilePath(int slotIndex)
    {
        return Path.Combine(
            Application.persistentDataPath,
            SAVE_FILE_PREFIX + slotIndex + SAVE_FILE_EXTENSION
        );
    }
}
