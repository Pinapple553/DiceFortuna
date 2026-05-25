using System;
using UnityEditor;
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
        int mostRecentSlot = -1;
        DateTime mostRecentDate = DateTime.MinValue;

        for (int i = 1; i <= 4; i++)
        {
            SaveFileData saveData = WorldManager.Instance.GetSaveData(i);
            if (saveData != null)
            {
                DateTime date = DateTime.Parse(saveData.dateSaved);
                if (date > mostRecentDate)
                {
                    mostRecentDate = date;
                    mostRecentSlot = i;
                }
            }
        }

        if (mostRecentSlot != -1)
            WorldManager.Instance.LoadSave(mostRecentSlot);
        else
        {
            WorldManager.Instance.NewSave(1);
            WorldManager.Instance.LoadSave(1);
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
        #else
                    Application.Quit();
        #endif
    }
}