using System.IO;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
	public LevelData[] levels;
	public int loadedSaveSlot;
	

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
		if (GetSaveData(loadedSaveSlot).levelIndex > levelData.levelIndex) return true;
		else return false;	
	}
	public void NewSave(int saveSlot)
	{
		SaveFileData saveFileData = new SaveFileData();
		saveFileData.dateSaved = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		saveFileData.fortunaPoints = 0;
		saveFileData.levelIndex = 1;

		string json = JsonUtility.ToJson(saveFileData, true);
		File.WriteAllText($"{Application.persistentDataPath}/Saves/SaveSlot{saveSlot}.json", json);
	}
	public void Save(){

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