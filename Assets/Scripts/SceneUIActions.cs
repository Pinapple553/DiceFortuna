using System;
using UnityEngine;

public class SceneUIActions : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.Instance.LoadScene(sceneName);
    }
    public void CloseScene()
    {
        SceneManager.Instance.CloseScene();
    }

    public void StartGame()
    {
        bool savesExist = WorldManager.Instance.GetSaveData(0) != null || WorldManager.Instance.GetSaveData(1) != null || WorldManager.Instance.GetSaveData(2) != null || WorldManager.Instance.GetSaveData(3) != null;
        if (savesExist)
        {
            int mostRecentSaveIndex = -1;
            for (int i = 0; i < 4; i++)
            {
                SaveFileData saveData = WorldManager.Instance.GetSaveData(i);
                if (saveData != null)
                {
                    if (mostRecentSaveIndex == -1 || DateTime.Parse(saveData.dateSaved) > DateTime.Parse(WorldManager.Instance.GetSaveData(mostRecentSaveIndex).dateSaved))
                    {
                        mostRecentSaveIndex = i;
                    }
                }
            }
            WorldManager.Instance.LoadSave(mostRecentSaveIndex);
        }
        else
        {
            WorldManager.Instance.NewSave(0);
        }
    }
}
