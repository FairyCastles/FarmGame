namespace Farm.Save
{
    public interface ISaveable
    {
        GameSaveData GenerateSaveData();
        void RestoreData(GameSaveData saveData);

        void RegisterSaveable()
        {
            SaveLoadManager.Instance.RegisterSaveable(this);
        }

        string GUID { get; }
    }
}
