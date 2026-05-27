using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private TMP_Text levelTitle;
    [SerializeField] private TMP_Text levelStatus;
    [SerializeField] private TMP_Text triesText;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private LevelData levelData;
    [SerializeField] private Button button;

    public void UpdateLevelButton()
    {
        if (levelData == null) return;
        SaveFileData save = WorldManager.Instance.GetSaveData(WorldManager.Instance.loadedSaveSlot);
        if (save == null) return;

        levelTitle.text = levelData.levelName;

        string status = save.levels[levelData.levelIndex].status;
        bool completed = status == "Won" || status == "Lost";
        bool isCurrent = status == "Current";

        if (triesText != null)
        {
            int tries = save.levels[levelData.levelIndex].triesTaken;
            triesText.text = tries > 0 ? $"Tries: {tries}" : "";
        }

        if (completed)
        {
            levelTitle.color = Color.black;
            levelStatus.text = status == "Won" ? "Victory" : "Defeat";
            if (button != null) button.interactable = false;
        }
        else if (isCurrent)
        {
            levelTitle.color = Color.black;
            levelStatus.text = "Current Level";
            if (button != null) button.interactable = true;
        }
        else
        {
            levelTitle.color = Color.black;
            levelStatus.text = "Locked";
            if (button != null) button.interactable = false;
        }

        if (rewardIcon != null) rewardIcon.sprite = completed ? levelData.unlockedRewardIcon : levelData.lockedRewardIcon;
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

    public void OnLevelButtonClick()
    {
        SaveFileData save = WorldManager.Instance.GetSaveData(WorldManager.Instance.loadedSaveSlot);
        if (save == null) return;
        if (save.currentLevelIndex != levelData.levelIndex) return;

        WorldManager.Instance.currentLevelIndex = levelData.levelIndex;
        SceneManager.Instance.LoadScene("DiceGame");
    }
}