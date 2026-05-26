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


    public static ShopManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }
    private void OnEnable() => LoadShop();

    public void LoadShop()
    {
        fortunaPointsText.text = WorldManager.Instance.player.totalFortunaPoints.ToString();

        foreach (Transform child in itemDisplay) Destroy(child.gameObject);
        foreach (Transform child in diceDisplay) Destroy(child.gameObject);

        // Items — ordered unlock/upgrade
        var allItems = WorldManager.Instance.allItemsInGame;
        var ownedItems = WorldManager.Instance.player.ownedItemsList;
        for (int i = 0; i < allItems.Length; i++)
        {
            ItemInstance owned = ownedItems.Find(x => x.data == allItems[i]);
            var btn = Instantiate(itemShopButtonPrefab, itemDisplay);
            btn.Setup(allItems[i], owned, i, ownedItems, allItems);
        }

        // Dice — one button per shop slot; bought tracked per-slot not per-dice-type
        SaveFileData save = WorldManager.Instance.GetSaveData(WorldManager.Instance.loadedSaveSlot);
        List<int> shopDiceIds = save?.shopDiceIds ?? new List<int>();
        List<int> boughtSlots = save?.shopBoughtSlots ?? new List<int>();
        var allDice = WorldManager.Instance.allDiceInGame;

        for (int slotIndex = 0; slotIndex < shopDiceIds.Count; slotIndex++)
        {
            int diceId = shopDiceIds[slotIndex];
            if (diceId < 0 || diceId >= allDice.Length) continue;

            var btn = Instantiate(diceShopButtonPrefab, diceDisplay);
            btn.dice = allDice[diceId];
            btn.price = allDice[diceId].shopPrice;
            btn.shopSlotIndex = slotIndex;
            btn.bought = boughtSlots.Contains(slotIndex);
            btn.UpdateButtonUI();
        }
    }

    public void OnBuyComplete(int boughtSlotIndex = -1)
    {
        fortunaPointsText.text = WorldManager.Instance.player.totalFortunaPoints.ToString();
        WorldManager.Instance.SaveShop(boughtSlotIndex);
    }

    public void showShop(bool show)
    {
        if (shopPanel != null) shopPanel.SetActive(show);
    }
}