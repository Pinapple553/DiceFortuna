using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TMP_Text fortunaPointsText;
    [SerializeField] private Transform itemDisplay;
    [SerializeField] private Transform diceDisplay;
    [SerializeField] private DiceShopButton diceShopButtonPrefab;
    [SerializeField] private ItemShopButton itemShopButtonPrefab;

    private void OnEnable() => LoadShop();

    public void LoadShop()
    {
        fortunaPointsText.text = WorldManager.Instance.player.totalFortunaPoints.ToString();

        foreach (Transform child in itemDisplay) Destroy(child.gameObject);
        foreach (Transform child in diceDisplay) Destroy(child.gameObject);

        var allItems = WorldManager.Instance.allItemsInGame;
        var ownedItems = WorldManager.Instance.player.ownedItemsList;

        for (int i = 0; i < allItems.Length; i++)
        {
            ItemInstance owned = ownedItems.Find(x => x.data == allItems[i]);
            var btn = Instantiate(itemShopButtonPrefab, itemDisplay);
            btn.Setup(allItems[i], owned, i, ownedItems, allItems);
        }

        SaveFileData save = WorldManager.Instance.GetSaveData(WorldManager.Instance.loadedSaveSlot);
        List<int> shopDiceIds = save?.shopDiceIds ?? new List<int>();
        List<int> boughtDiceIds = save?.ownedDiceIds ?? new List<int>();

        var allDice = WorldManager.Instance.allDiceInGame;
        foreach (int diceId in shopDiceIds)
        {
            if (diceId < 0 || diceId >= allDice.Length) continue;
            var btn = Instantiate(diceShopButtonPrefab, diceDisplay);
            btn.dice = allDice[diceId];
            btn.price = allDice[diceId].shopPrice;
            btn.bought = boughtDiceIds.Contains(diceId);
            btn.UpdateButtonUI();
        }
    }

    public void OnBuyComplete()
    {
        fortunaPointsText.text = WorldManager.Instance.player.totalFortunaPoints.ToString();
        WorldManager.Instance.SavePlayerData();
    }

    public void showShop(bool show)
    {
        shopPanel.SetActive(show);
    }
}