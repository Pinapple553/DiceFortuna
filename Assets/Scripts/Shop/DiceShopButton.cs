using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceShopButton : MonoBehaviour
{
    public DiceData dice;
    public int price;
    public int shopSlotIndex; 
    public bool bought = false;

    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountOwnedText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;

    public void Buy()
    {
        if (bought) return;
        if (WorldManager.Instance.player.totalFortunaPoints < price) return;

        WorldManager.Instance.player.totalFortunaPoints -= price;
        WorldManager.Instance.player.ownedDiceList.Add(dice);
        bought = true;

        ShopManager.Instance.OnBuyComplete(shopSlotIndex);

        UpdateButtonUI();
    }

    public void UpdateButtonUI()
    {
        if (amountOwnedText != null)
        {
            int count = 0;
            foreach (var d in WorldManager.Instance.player.ownedDiceList)
                if (d == dice) count++;
            amountOwnedText.text = count.ToString();
        }
        if (buyButton != null) buyButton.interactable = !bought;
        if (buyButtonText != null) buyButtonText.text = bought ? "Bought" : $"Buy: {price}";
        if (icon != null && dice != null) icon.sprite = dice.sides[0].sprite;
    }
}