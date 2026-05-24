using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
	public LevelData[] levels;
    public DiceData[] allDiceInGame;
    public ItemData[] allItemsInGame;

    public int loadedSaveSlot;
	public int currentLevelIndex = 1;
	public Player player;
	

	public static WorldManager Instance;
	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else Destroy(gameObject);
	}

    public bool IsLevelCompleted(LevelData levelData)
    {
        SaveFileData data = GetSaveData(loadedSaveSlot);
        return data != null && data.currentLevelIndex > levelData.levelIndex;
    }
    private string SavePath(int slot) { return $"{Application.persistentDataPath}/Saves/SaveSlot{slot}.json"; }
    private void WriteSave(int slot, SaveFileData data)
    {
        string dir = $"{Application.persistentDataPath}/Saves";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(SavePath(slot), JsonUtility.ToJson(data, true));
    }
    public void NewSave(int saveSlot)
	{
		SaveFileData saveFileData = new SaveFileData();
		saveFileData.dateSaved = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		saveFileData.fortunaPoints = 0;

		LevelInstance[] levelInstances = new LevelInstance[levels.Length];
		for (int i = 0; i < levels.Length; i++)
		{
			LevelInstance levelInstance = new LevelInstance();
			levelInstance.levelIndex = i;
            if (i == 0) levelInstance.status = "Current";
            else levelInstance.status = "Locked";
            levelInstance.fortunaPointsEarned = 0;
			levelInstances[i] = levelInstance;
		}
		saveFileData.levels = levelInstances;

        if (!Directory.Exists($"{Application.persistentDataPath}/Saves"))
        {
            Directory.CreateDirectory($"{Application.persistentDataPath}/Saves");
        }
        string json = JsonUtility.ToJson(saveFileData, true);
		File.WriteAllText($"{Application.persistentDataPath}/Saves/SaveSlot{saveSlot}.json", json);
	}
	public void Save(int levelIndex, int roundFortunaPoints, string levelStatus){
		SaveFileData saveFileData = GetSaveData(loadedSaveSlot);
		if (saveFileData != null)
		{
			saveFileData.dateSaved = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (levelStatus == "Won")
            {
                player.totalFortunaPoints += roundFortunaPoints;
            }
            saveFileData.fortunaPoints = player.totalFortunaPoints;

            if (levelIndex + 1 < levels.Length)
            {
                saveFileData.currentLevelIndex = levelIndex + 1;
                currentLevelIndex = saveFileData.currentLevelIndex;
            }

            saveFileData.levels[levelIndex].status = levelStatus;
			saveFileData.levels[levelIndex].fortunaPointsEarned = roundFortunaPoints;

            saveFileData.ownedDiceIds = new List<int>();
            foreach (DiceData d in player.ownedDiceList)
            {
                int id = System.Array.IndexOf(allDiceInGame, d);
                if (id >= 0) saveFileData.ownedDiceIds.Add(id);
            }
            saveFileData.ownedItemIds = new List<int>();
            foreach (ItemData item in player.ownedItemsList)
            {
                int id = System.Array.IndexOf(allItemsInGame, item);
                if (id >= 0) saveFileData.ownedItemIds.Add(id);
            }

            WriteSave(loadedSaveSlot, saveFileData);
        }
	}
	public void DeleteSave(int saveSlot)
	{
		string filePath = $"{Application.persistentDataPath}/Saves/SaveSlot{saveSlot}.json";
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}
	public void LoadSave(int saveSlot)
	{
		SaveFileData saveFileData = GetSaveData(saveSlot);
		if (saveFileData != null)
		{
			loadedSaveSlot = saveSlot;
			currentLevelIndex = saveFileData.currentLevelIndex;
			SceneManager.Instance.LoadScene("LevelPicker");
		}
	}
	public SaveFileData GetSaveData(int saveSlot)
	{
		string filePath = $"{Application.persistentDataPath}/Saves/SaveSlot{saveSlot}.json";
		if (File.Exists(filePath))
		{
			string json = File.ReadAllText(filePath);
			if (!string.IsNullOrEmpty(json))
			{
				return JsonUtility.FromJson<SaveFileData>(json);
			}
		}
		return null;
	}
    public void LoadPlayerData()
    {
        SaveFileData save = GetSaveData(loadedSaveSlot);
        if (save == null) return;

        player.totalFortunaPoints = save.fortunaPoints;

        player.ownedDiceList = new List<DiceData>();
        if (save.ownedDiceIds != null)
        {
            foreach (int id in save.ownedDiceIds)
            {
                if (id >= 0 && id < allDiceInGame.Length)
                    player.ownedDiceList.Add(allDiceInGame[id]);
            }
        }

        player.ownedItemsList = new List<ItemData>();
        if (save.ownedItemIds != null)
        {
            foreach (int id in save.ownedItemIds)
            {
                if (id >= 0 && id < allItemsInGame.Length)
                    player.ownedItemsList.Add(allItemsInGame[id]);
            }
        }
    }
    public void SavePlayerData()
    {
        SaveFileData save = GetSaveData(loadedSaveSlot);
        if (save == null) return;

        save.fortunaPoints = player.totalFortunaPoints;

        save.ownedDiceIds = new List<int>();
        foreach (DiceData d in player.ownedDiceList)
        {
            int id = System.Array.IndexOf(allDiceInGame, d);
            if (id >= 0) save.ownedDiceIds.Add(id);
        }

        save.ownedItemIds = new List<int>();
        foreach (ItemData item in player.ownedItemsList)
        {
            int id = System.Array.IndexOf(allItemsInGame, item);
            if (id >= 0) save.ownedItemIds.Add(id);
        }

        WriteSave(loadedSaveSlot, save);
    }
}