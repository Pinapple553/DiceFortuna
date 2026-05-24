using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [Header("Item Info")]
    public ItemInstance item;

    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text usesText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image selectionHighlight;
    public void OnClick()
    {
        GameManager.Instance.PlayerClickItem(item);
    }
    public void UpdateButtonUI()
    {
        icon.sprite = item.data.icon;
        usesText.text = item.usesRemainingThisMatch.ToString();
        levelText.text = "LV." + item.currentTier;
        
        //disabled
        bool canUse = item.CanUse() && GameManager.Instance.isPlayerItemTurn;
        icon.color = canUse ? Color.white : new Color(1f, 1f, 1f, 0.4f);
        
        //selected
        bool isSelected = GameManager.Instance.pendingItem == item;
        if (selectionHighlight != null) selectionHighlight.enabled = isSelected;
    }
}