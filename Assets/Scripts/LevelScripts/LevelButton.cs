using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
	[SerializeField] private TMP_Text levelTitle;
	[SerializeField] private TMP_Text levelStatus;
	[SerializeField] private Image rewardIcon;
	[SerializeField] private LevelData levelData;

	private void Start()
	{
		UpdateLevelButton();
	}

	private void UpdateLevelButton()
	{
		levelTitle.text = levelData.levelName;
		levelStatus.text = WorldManager.Instance.IsLevelCompleted(levelData) ? "Completed" : "Not Completed";
		rewardIcon.sprite = (WorldManager.Instance.IsLevelCompleted(levelData) ? levelData.unlockedRewardIcon : levelData.lockedRewardIcon);
	}
}
