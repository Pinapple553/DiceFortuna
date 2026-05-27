using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [SerializeField] GameObject levelContainer;
    [SerializeField] LevelButton levelButtonPrefab;
    [SerializeField] TMP_Text livesText;
    private void Start()
    {
        UpdateLevelButtons();
        UpdateLivesDisplay();

    }

    public void UpdateLivesDisplay()
    {
        if (livesText == null) return;
        SaveFileData save = WorldManager.Instance.GetSaveData(WorldManager.Instance.loadedSaveSlot);
        livesText.text = save != null ? $"{save.lives}/3" : "?/3";
    }
    public void UpdateLevelButtons()
    {
        foreach (Transform child in levelContainer.transform) Destroy(child.gameObject);

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