using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

    // ISaveable 레지스트리.
    // FindObjectsOfType는 비활성 오브젝트를 누락하므로, 구현체가 직접 등록/해제한다.
    private static readonly List<ISaveable> saveables = new List<ISaveable>();

    /// <summary>
    /// Enter Play Mode(도메인 리로드 비활성) 옵션 대비 정적 상태 초기화
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        saveables.Clear();
    }

    public static void Register(ISaveable saveable)
    {
        if (saveable != null && !saveables.Contains(saveable))
            saveables.Add(saveable);
    }

    public static void Unregister(ISaveable saveable)
    {
        saveables.Remove(saveable);
    }

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

    /// <summary>
    /// 세이브 슬롯을 비동기 로드한다.
    /// 맵을 먼저 복원한 뒤(위치 복원의 선행 조건) 나머지 상태를 복원한다.
    /// </summary>
    public async Task<bool> LoadAsync(int slotIndex)
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

            // 1. 맵 먼저 비동기 복원 (플레이어 위치 복원의 선행 조건)
            await RestoreMapAsync();

            // 2. 맵 로드 완료 후 나머지 상태 동기 복원
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
        foreach (var saveable in saveables)
        {
            saveable.CaptureState(currentSaveData);
        }
    }

    private void RestoreAllStates()
    {
        foreach (var saveable in saveables)
        {
            saveable.RestoreState(currentSaveData);
        }
    }

    /// <summary>
    /// 저장된 mapId가 현재 맵과 다를 때만 Addressables 맵을 전환한다.
    /// </summary>
    private async Task RestoreMapAsync()
    {
        string mapId = currentSaveData.playerData.currentMapId;
        if (string.IsNullOrEmpty(mapId))
            return;

        var mapManager = MapSystem.Core.MapManager.Instance;
        if (mapManager == null)
            return;

        if (mapManager.CurrentMap != null && mapManager.CurrentMap.mapId == mapId)
            return;

        await mapManager.LoadMapByIdAsync(mapId);
    }

    private void UpdateMetadata(int slotIndex)
    {
        currentSaveData.metadata.saveName = $"Slot {slotIndex + 1}";
        currentSaveData.metadata.UpdateTimestamp();
        currentSaveData.metadata.playTime = Time.realtimeSinceStartup - sessionStartTime;
        currentSaveData.metadata.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
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
