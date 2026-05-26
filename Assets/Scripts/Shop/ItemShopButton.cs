using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemShopButton : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text tierText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;

    private ItemData itemData;
    private ItemInstance ownedInstance;
    private bool isUnlocked;

    public void Setup(ItemData data, ItemInstance owned, int index, List<ItemInstance> allOwned, ItemData[] allItems)
    {
        itemData = data;
        ownedInstance = owned;

        if (index == 0) isUnlocked = true;
        else
        {
            ItemInstance prev = allOwned.Find(x => x.data == allItems[index - 1]);
            isUnlocked = prev != null && !prev.CanUpgrade();
        }
        if (owned != null) isUnlocked = true;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (icon != null && itemData.icon != null) icon.sprite = itemData.icon;

        if (!isUnlocked)
        {
            if (tierText != null) tierText.text = "Locked";
            if (buyButton != null) buyButton.interactable = false;
            if (buyButtonText != null) buyButtonText.text = "Locked";
            return;
        }

        if (ownedInstance == null)
        {
            if (tierText != null) tierText.text = "Lv.0";
            if (buyButton != null) buyButton.interactable = true;
            if (buyButtonText != null) buyButtonText.text = $"Buy: {itemData.tiers[0].cost}";
        }
        else if (ownedInstance.CanUpgrade())
        {
            int next = ownedInstance.currentTier + 1;
            if (tierText != null) tierText.text = $"Lv.{ownedInstance.currentTier + 1}";
            if (buyButton != null) buyButton.interactable = true;
            if (buyButtonText != null) buyButtonText.text = $"Upgrade: {itemData.tiers[next].cost}";
        }
        else
        {
            if (tierText != null) tierText.text = $"Lv.{ownedInstance.currentTier + 1}";
            if (buyButton != null) buyButton.interactable = false;
            if (buyButtonText != null) buyButtonText.text = "Max";
        }
    }

    public void OnBuyClick()
    {
        int cost;
        if (ownedInstance == null)
        {
            cost = itemData.tiers[0].cost;
            if (WorldManager.Instance.player.totalFortunaPoints < cost) return;
            WorldManager.Instance.player.totalFortunaPoints -= cost;
            ownedInstance = new ItemInstance(itemData, 0);
            WorldManager.Instance.player.ownedItemsList.Add(ownedInstance);
        }
        else if (ownedInstance.CanUpgrade())
        {
            int next = ownedInstance.currentTier + 1;
            cost = itemData.tiers[next].cost;
            if (WorldManager.Instance.player.totalFortunaPoints < cost) return;
            WorldManager.Instance.player.totalFortunaPoints -= cost;
            ownedInstance.Upgrade();
        }
        else return;

        ShopManager shop = GetComponentInParent<ShopManager>();
        if (shop != null) shop.OnBuyComplete();
        UpdateUI();
    }

    public void ShowInfo()
    {
        ShopManager.Instance.ShowItemInfo(itemData);
    }
}