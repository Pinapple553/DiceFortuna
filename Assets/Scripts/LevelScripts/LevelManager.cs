using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
	[SerializeField] GameObject levelContainer;
	[SerializeField] LevelButton levelButtonPrefab;
	[SerializeField] GameObject ShopButtonPrefab;

	[SerializeField] List<LevelData> LevelList;
	private void Start()
	{
		UpdateLevelButtons();
	}

	public void UpdateLevelButtons()
	{
		int currentLevelIndex = WorldManager.Instance.GetSaveData(WorldManager.Instance.loadedSaveSlot).levelIndex;
		foreach (Transform child in levelContainer.transform)
		{
			Destroy(child.gameObject);
		}
		for (int i = 0; i < LevelList.Count; i++)
		{
			LevelButton button = Instantiate(levelButtonPrefab, levelContainer.transform);
			button.SetLevelData(LevelList[i]);
			button.SetLevelIndex(i);
			button.UpdateLevelButton();
			if (i < LevelList.Count-1){
   				GameObject shopButton = Instantiate(ShopButtonPrefab, levelContainer.transform);
			}
		}

	}

}
