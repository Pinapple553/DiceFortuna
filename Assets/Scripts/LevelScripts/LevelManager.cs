using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
	[SerializeField] GameObject levelContainer;
	[SerializeField] LevelButton levelButtonPrefab;

	private void Start()
	{
		UpdateLevelButtons();
	}

	public void UpdateLevelButtons()
	{

		foreach (Transform child in levelContainer.transform)
		{
			Destroy(child.gameObject);
		}
		for (int i = 0; i < WorldManager.Instance.levels.Length; i++)
		{
			LevelButton button = Instantiate(levelButtonPrefab, levelContainer.transform);
			button.SetLevelData(WorldManager.Instance.levels[i]);
			button.SetLevelIndex(i);
			button.UpdateLevelButton();
		}

	}

	public void SaveExitButtonClick()
	{
		WorldManager.Instance.SavePlayerData();
		WorldManager.Instance.SaveShop();
		SceneManager.Instance.LoadScene("SaveLoad");
	}

}
