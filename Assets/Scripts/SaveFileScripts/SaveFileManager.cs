using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class SaveFileManager : MonoBehaviour
{
	[SerializeField] private SaveFileButton saveFileButtonPrefab;
	[SerializeField] private GameObject saveFileContainer;

	private string saveFolder;
	private int numberOfSaves = 4;

	public static SaveFileManager Instance;
	private void Awake()
	{
		if (Instance != null && Instance != this) Destroy(this.gameObject);
		else Instance = this;

		saveFolder = Application.persistentDataPath + "/Saves/";
		if (!Directory.Exists(saveFolder))
		{
			Directory.CreateDirectory(saveFolder);
		}
	}

	private void Start()
	{
		UpdateSaveFileButtons();
	}

	private void UpdateSaveFileButtons()
	{
		foreach (Transform child in saveFileContainer.transform)
		{
			Destroy(child.gameObject);
		}
		for (int i = 1; i <= numberOfSaves; i++)
		{
			SaveFileButton button = Instantiate(saveFileButtonPrefab, saveFileContainer.transform);
			string json = null;
			if (File.Exists($"{Application.persistentDataPath}/Saves/SaveSlot{i}.json")){
				 json = File.ReadAllText($"{Application.persistentDataPath}/Saves/SaveSlot{i}.json");
			}
			button.saveSlot = i;
			if (!string.IsNullOrEmpty(json))
			{
				button.saveFileData = JsonUtility.FromJson<SaveFileData>(json);
			}
			else
			{
				button.saveFileData = null;
			}
			button.UpdateSaveFileButton();
		}
	}
	public void NewSave(int saveSlot)
	{
		WorldManager.Instance.NewSave(saveSlot);
		UpdateSaveFileButtons();
	}

	public void DeleteSave(int saveSlot)
	{
		WorldManager.Instance.DeleteSave(saveSlot);
		UpdateSaveFileButtons();
	}
}
