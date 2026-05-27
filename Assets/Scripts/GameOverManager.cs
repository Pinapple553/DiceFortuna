using TMPro;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text subText;

    private void Start()
    {
        SaveFileData save = WorldManager.Instance.GetSaveData(WorldManager.Instance.loadedSaveSlot);
        if (save == null) return;

        int totalLevels = save.levels.Length;
        int won = 0;
        int totalTries = 0;
        bool allFirstTry = true;

        foreach (LevelInstance lvl in save.levels)
        {
            if (lvl.status == "Won") won++;
            totalTries += lvl.triesTaken;
            if (lvl.triesTaken > 1) allFirstTry = false;
        }

        gameOverText.text = "Game Over!";

        string tries = allFirstTry ? "You cleared every level on the first try!"  : $"Total attempts across all levels: {totalTries}";

        subText.text =
        $"Levels won: {won}/{totalLevels}\n" +
        $"{tries}\n" +
        $"Lives remaining: {save.lives}\n" +
        $"Fortuna Points: {save.fortunaPoints}";
    }
}