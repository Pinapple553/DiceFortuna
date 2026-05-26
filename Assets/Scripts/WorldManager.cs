using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public LevelData[] levels;
    public DiceData[] allDiceInGame;
    public ItemData[] allItemsInGame;

    public int loadedSaveSlot;
    public int currentLevelIndex = 0;
    public Player player;

    public static WorldManager Instance;
    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public bool IsLevelCompleted(LevelData levelData)
    {
        SaveFileData data = GetSaveData(loadedSaveSlot);
        return data != null && data.currentLevelIndex > levelData.levelIndex;
    }

    private string SavePath(int slot) => $"{Application.persistentDataPath}/Saves/SaveSlot{slot}.json";

    private void WriteSave(int slot, SaveFileData data)
    {
        string dir = $"{Application.persistentDataPath}/Saves";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(SavePath(slot), JsonUtility.ToJson(data, true));
    }

    public void NewSave(int saveSlot)
    {
        SaveFileData saveFileData = new SaveFileData();
        saveFileData.dateSaved = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        saveFileData.fortunaPoints = 0;
        saveFileData.lives = 3;

        LevelInstance[] levelInstances = new LevelInstance[levels.Length];
        for (int i = 0; i < levels.Length; i++)
        {
            LevelInstance li = new LevelInstance();
            li.levelIndex = i;
            li.status = i == 0 ? "Current" : "Locked";
            li.fortunaPointsEarned = 0;
            li.triesTaken = 0;
            levelInstances[i] = li;
        }
        saveFileData.levels = levelInstances;
        saveFileData.ownedDiceIds = new List<int>();
        saveFileData.ownedDiceIds.Add(0);
            
        saveFileData.ownedItemIds = new List<int>();
        saveFileData.ownedItemTiers = new List<int>();
        saveFileData.ownedItemIds.Add(0);
        saveFileData.ownedItemTiers.Add(0);

        saveFileData.shopSeed = Random.Range(0, 999999);
        saveFileData.shopDiceIds = GenerateShopDice(saveFileData.shopSeed);
        saveFileData.shopBoughtSlots = new List<int>();

        WriteSave(saveSlot, saveFileData);
    }

    public List<int> GenerateShopDice(int seed)
    {
        int levelIdx = Mathf.Max(0, currentLevelIndex - 1);
        int count = (levels != null && levelIdx < levels.Length && levels[levelIdx] != null)
            ? levels[levelIdx].shopDiceCount : 5;

        Random.State prevState = Random.state;
        Random.InitState(seed);

        List<int> result = new List<int>();
        for (int i = 0; i < count; i++)
            result.Add(Random.Range(0, allDiceInGame.Length));

        Random.state = prevState;
        return result;
    }

    public void RefreshShopDice()
    {
        SaveFileData save = GetSaveData(loadedSaveSlot);
        if (save == null) return;
        save.shopSeed = Random.Range(0, 999999);
        save.shopDiceIds = GenerateShopDice(save.shopSeed);
        save.shopBoughtSlots = new List<int>();
        WriteSave(loadedSaveSlot, save);
    }

    public void Save(int levelIndex, int roundFortunaPoints, string levelStatus)
    {
        SaveFileData saveFileData = GetSaveData(loadedSaveSlot);
        if (saveFileData == null) return;

        saveFileData.dateSaved = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        if (levelStatus == "Won")
        {
            player.totalFortunaPoints += roundFortunaPoints;
            if (levelIndex + 1 < levels.Length)
            {
                saveFileData.currentLevelIndex = levelIndex + 1;
                currentLevelIndex = saveFileData.currentLevelIndex;
                if (saveFileData.levels[levelIndex + 1].status == "Locked") saveFileData.levels[levelIndex + 1].status = "Current";
            }
            saveFileData.levels[levelIndex].status = "Won";
            saveFileData.levels[levelIndex].fortunaPointsEarned = roundFortunaPoints;
            RefreshShopDice();
            bool isLastLevel = levelIndex >= levels.Length - 1;
            if (isLastLevel) saveFileData.isCompleted = true;


        }
        else if (levelStatus == "Lost")
        {
            player.totalFortunaPoints += roundFortunaPoints / 2;
            saveFileData.lives = Mathf.Max(0, saveFileData.lives - 1);
            if (levelIndex + 1 < levels.Length)
            {
                saveFileData.currentLevelIndex = levelIndex + 1;
                currentLevelIndex = saveFileData.currentLevelIndex;
                if (saveFileData.levels[levelIndex + 1].status == "Locked")
                    saveFileData.levels[levelIndex + 1].status = "Current";
            }
            saveFileData.levels[levelIndex].status = "Lost";
            saveFileData.levels[levelIndex].fortunaPointsEarned = roundFortunaPoints / 2;
        }

        saveFileData.fortunaPoints = player.totalFortunaPoints;

        saveFileData.ownedDiceIds = new List<int>();
        foreach (DiceData d in player.ownedDiceList)
        {
            int id = System.Array.IndexOf(allDiceInGame, d);
            if (id >= 0) saveFileData.ownedDiceIds.Add(id);
        }

        saveFileData.ownedItemIds = new List<int>();
        saveFileData.ownedItemTiers = new List<int>();
        foreach (ItemInstance item in player.ownedItemsList)
        {
            int id = System.Array.IndexOf(allItemsInGame, item.data);
            if (id >= 0)
            {
                saveFileData.ownedItemIds.Add(id);
                saveFileData.ownedItemTiers.Add(item.currentTier);
            }
        }

        WriteSave(loadedSaveSlot, saveFileData);
    }
    public void IncrementLevelTries(int levelIndex)
    {
        SaveFileData save = GetSaveData(loadedSaveSlot);
        if (save == null) return;
        if (levelIndex < 0 || levelIndex >= save.levels.Length) return;
        save.levels[levelIndex].triesTaken++;
        WriteSave(loadedSaveSlot, save);
    }
    public void DeleteSave(int saveSlot)
    {
        string filePath = SavePath(saveSlot);
        if (File.Exists(filePath)) File.Delete(filePath);
    }
    public void LoadSave(int saveSlot)
    {
        SaveFileData saveFileData = GetSaveData(saveSlot);
        if (saveFileData != null)
        {
            loadedSaveSlot = saveSlot;
            currentLevelIndex = saveFileData.currentLevelIndex;
            LoadPlayerData();
            SceneManager.Instance.LoadScene("LevelPicker");
        }
    }
    public SaveFileData GetSaveData(int saveSlot)
    {
        string filePath = SavePath(saveSlot);
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            if (!string.IsNullOrEmpty(json)) return JsonUtility.FromJson<SaveFileData>(json);
        }
        return null;
    }
    public void LoadPlayerData()
    {
        SaveFileData save = GetSaveData(loadedSaveSlot);
        if (save == null) return;

        player.totalFortunaPoints = save.fortunaPoints;

        player.ownedDiceList = new List<DiceData>();
        if (save.ownedDiceIds != null)
        {
            foreach (int id in save.ownedDiceIds)
                if (id >= 0 && id < allDiceInGame.Length && allDiceInGame[id] != null)
                    player.ownedDiceList.Add(allDiceInGame[id]);
        }

        player.ownedItemsList = new List<ItemInstance>();
        if (save.ownedItemIds != null)
        {
            for (int i = 0; i < save.ownedItemIds.Count; i++)
            {
                int id = save.ownedItemIds[i];
                if (id < 0 || id >= allItemsInGame.Length || allItemsInGame[id] == null) continue;
                ItemData itemData = allItemsInGame[id];
                if (itemData.tiers == null || itemData.tiers.Length == 0) continue;
                int tier = (save.ownedItemTiers != null && i < save.ownedItemTiers.Count)
                    ? Mathf.Clamp(save.ownedItemTiers[i], 0, itemData.tiers.Length - 1)
                    : 0;
                player.ownedItemsList.Add(new ItemInstance(itemData, tier));
            }
        }
    }
    public void SaveShop(int boughtSlotIndex = -1)
    {
        SaveFileData save = GetSaveData(loadedSaveSlot);
        if (save == null) return;

        if (boughtSlotIndex >= 0)
        {
            if (save.shopBoughtSlots == null) save.shopBoughtSlots = new List<int>();
            if (!save.shopBoughtSlots.Contains(boughtSlotIndex)) save.shopBoughtSlots.Add(boughtSlotIndex);
        }

        save.fortunaPoints = player.totalFortunaPoints;

        save.ownedDiceIds = new List<int>();
        foreach (DiceData d in player.ownedDiceList)
        {
            int id = System.Array.IndexOf(allDiceInGame, d);
            if (id >= 0) save.ownedDiceIds.Add(id);
        }

        save.ownedItemIds = new List<int>();
        save.ownedItemTiers = new List<int>();
        foreach (ItemInstance item in player.ownedItemsList)
        {
            int id = System.Array.IndexOf(allItemsInGame, item.data);
            if (id >= 0)
            {
                save.ownedItemIds.Add(id);
                save.ownedItemTiers.Add(item.currentTier);
            }
        }

        WriteSave(loadedSaveSlot, save);
    }
    public void SavePlayerData()
    {
        SaveFileData save = GetSaveData(loadedSaveSlot);
        if (save == null) return;

        save.fortunaPoints = player.totalFortunaPoints;

        save.ownedDiceIds = new List<int>();
        foreach (DiceData d in player.ownedDiceList)
        {
            int id = System.Array.IndexOf(allDiceInGame, d);
            if (id >= 0) save.ownedDiceIds.Add(id);
        }

        save.ownedItemIds = new List<int>();
        save.ownedItemTiers = new List<int>();
        foreach (ItemInstance item in player.ownedItemsList)
        {
            int id = System.Array.IndexOf(allItemsInGame, item.data);
            if (id >= 0)
            {
                save.ownedItemIds.Add(id);
                save.ownedItemTiers.Add(item.currentTier);
            }
        }

        WriteSave(loadedSaveSlot, save);
    }
}