using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SaveFileButton : MonoBehaviour
{
	[SerializeField] private Button deleteButton;
	[SerializeField] private TMP_Text pointText;
	[SerializeField] private TMP_Text dateText;
	[SerializeField] private Image saveFileImage;
	[SerializeField] private TMP_Text saveFileButtonText;
	[SerializeField] private Sprite defaultSprite;

	public SaveFileData saveFileData;
	public int saveSlot;
	private void Start()
	{
		UpdateSaveFileButton();
	}
	public void UpdateSaveFileButton()
	{
		if (saveFileData == null)
		{
			deleteButton.gameObject.SetActive(false);
			pointText.text = "Empty";
			dateText.text = "";
			saveFileImage.sprite = defaultSprite;
			saveFileButtonText.text = "New";
		}
		else
		{
			deleteButton.gameObject.SetActive(true);
			pointText.text = saveFileData.fortunaPoints.ToString();
			dateText.text = saveFileData.dateSaved.ToString();
			//saveFileImage.sprite = WorldManager.Instance.levels[saveFileData.levelIndex].levelImage;
			saveFileButtonText.text = "Load";
		}
	}

	public void OnSaveFileButtonClick()
	{
		if (saveFileData == null)
		{
			SaveFileManager.Instance.NewSave(saveSlot);
		}
		else
		{
			string json = File.ReadAllText($"{Application.persistentDataPath}/Saves/SaveSlot{saveSlot}.json");
			if (!string.IsNullOrEmpty(json))
			{
				saveFileData = JsonUtility.FromJson<SaveFileData>(json);
				WorldManager.Instance.LoadSave(saveSlot);
				UpdateSaveFileButton();
			}
		}
	}

	public void DeleteButtonClick()
	{
 		SaveFileManager.Instance.DeleteSave(saveSlot);
	}
}
