using NUnit.Framework;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
	public LevelData[] levels;
	public int loadedSaveSlot;
	public int currentLevelIndex;
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


	public bool IsLevelCompleted(LevelData levelData){
		if (GetSaveData(loadedSaveSlot).currentLevelIndex > levelData.levelIndex) return true;
		else return false;	
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
			levelInstance.status = "Locked";
			levelInstance.fortunaPointsEarned = 0;
			levelInstances[i] = levelInstance;
		}
		saveFileData.levels = levelInstances;

		string json = JsonUtility.ToJson(saveFileData, true);
		File.WriteAllText($"{Application.persistentDataPath}/Saves/SaveSlot{saveSlot}.json", json);
	}
	public void Save(int levelIndex, int roundFortunaPoints, string levelStatus){
		SaveFileData saveFileData = GetSaveData(loadedSaveSlot);
		if (saveFileData != null)
		{
			saveFileData.dateSaved = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			saveFileData.fortunaPoints += roundFortunaPoints;
			saveFileData.currentLevelIndex = levelIndex;

			saveFileData.levels[levelIndex].status = levelStatus;
			saveFileData.levels[levelIndex].fortunaPointsEarned = roundFortunaPoints;

			string json = JsonUtility.ToJson(saveFileData, true);
			File.WriteAllText($"{Application.persistentDataPath}/Saves/SaveSlot{loadedSaveSlot}.json", json);
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
}