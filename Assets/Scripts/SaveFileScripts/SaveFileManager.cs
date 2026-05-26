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
        foreach (Transform child in saveFileContainer.transform) Destroy(child.gameObject);

        for (int i = 1; i <= numberOfSaves; i++)
        {
            SaveFileButton button = Instantiate(saveFileButtonPrefab, saveFileContainer.transform);
            string path = $"{Application.persistentDataPath}/Saves/SaveSlot{i}.json";
            string json = File.Exists(path) ? File.ReadAllText(path) : null;

            button.saveSlot = i;
            button.saveFileData = !string.IsNullOrEmpty(json) ? JsonUtility.FromJson<SaveFileData>(json) : null;

            button.isLocked = button.saveFileData != null && button.saveFileData.isCompleted;
            button.UpdateSaveFileButton();
        }
    }
    public void NewSave(int saveSlot)
    {
        SaveFileData existing = WorldManager.Instance.GetSaveData(saveSlot);
        if (existing != null && existing.isCompleted) return;

        WorldManager.Instance.NewSave(saveSlot);
        UpdateSaveFileButtons();
    }

    public void DeleteSave(int saveSlot)
    {
        WorldManager.Instance.DeleteSave(saveSlot);
        UpdateSaveFileButtons();
    }
}
