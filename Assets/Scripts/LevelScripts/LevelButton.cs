using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
	[SerializeField] private TMP_Text levelTitle;
	[SerializeField] private TMP_Text levelStatus;
	[SerializeField] private Image rewardIcon;
	[SerializeField] private LevelData levelData;

	public void UpdateLevelButton()
	{
		levelTitle.text = levelData.levelName;
		if (WorldManager.Instance.IsLevelCompleted(levelData))
		{
			levelTitle.color = Color.green;
			levelStatus.text = "Completed";

		}
		else if (WorldManager.Instance.GetSaveData(WorldManager.Instance.loadedSaveSlot).levelIndex == levelData.levelIndex)
		{
			levelTitle.color = Color.black;
			levelStatus.text = "Current Level";
		}
		else
		{
			levelTitle.color = Color.lightGray;
			levelStatus.text = "Locked";
		}
		rewardIcon.sprite = (WorldManager.Instance.IsLevelCompleted(levelData) ? levelData.unlockedRewardIcon : levelData.lockedRewardIcon);
	}

	public void SetLevelData(LevelData data)
	{
		levelData = data;
		UpdateLevelButton();
	}

	public void SetLevelIndex(int index)
	{
		levelData.levelIndex = index;
	}

}
