using System.Collections.Generic;
using UnityEngine;

public interface ISaveSystem
{
    public void SaveData();
    public void LoadData();
}


public class SaveManager : MonoBehaviour
{
    private static SaveManager instance = null;
    private List<ISaveSystem> _saveSystems = new List<ISaveSystem>();

    public static SaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("SaveManager");
                instance = obj.AddComponent<SaveManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadAllData();
    }

    public void RegisterSaveSystem(ISaveSystem saveSystem)
    {
        if (!_saveSystems.Contains(saveSystem))
        {
            _saveSystems.Add(saveSystem);
        }
    }

    public void UnregisterSaveSystem(ISaveSystem saveSystem)
    {
        _saveSystems.Remove(saveSystem);
    }

    void OnApplicationQuit()
    {
        SaveAllData();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllData();
        }
    }

    public void SaveAllData()
    {
        foreach (ISaveSystem saveSystem in _saveSystems)
        {
            saveSystem.SaveData();
        }
        PlayerPrefs.Save();
    }

    public void LoadAllData()
    {
        foreach (ISaveSystem saveSystem in _saveSystems)
        {
            saveSystem.LoadData();
        }
    }

}
