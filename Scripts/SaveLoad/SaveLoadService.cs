using Zenject;

public class SaveLoadService
{
    private readonly SaveManager _saveManager;

    public SaveLoadService(SaveManager saveManager)
    {
        _saveManager = saveManager;
    }

    public void SaveData<T>(T data) where T : ISaveable
    {
        _saveManager.Save(data);
    }

    public T LoadData<T>() where T : class, ISaveable
    {
        return _saveManager.Load<T>();
    }

    public bool ContainsKey<T>() where T : ISaveable
    {
        return _saveManager.ContainsKey<T>();
    }
}