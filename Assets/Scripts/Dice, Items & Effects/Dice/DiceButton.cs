using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiceButton : MonoBehaviour
{
    public int instanceId;

    [Header("Dice Info")]
    public DiceData dice;
    public int amountOwned;
    public int amountSelected;

    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountOwnedText;
    [SerializeField] private Button amountSelectedButton;
    [SerializeField] private TMP_Text amountSelectedText;

    [HideInInspector] public int activeDiceIndex = -1;

    private static readonly Color highlightColor = new Color(0.5f, 0.8f, 1f, 1f);

    public void AddDice()
    {
        if (amountOwned - amountSelected > 0)
        {
            if (!DiceManager.Instance.AddDice(dice, GameManager.Instance.player)) return;
            amountSelected++;
        }
        UpdateButtonUI();
    }

    public void RemoveDice()
    {
        if (!DiceManager.Instance.RemoveDice(dice, GameManager.Instance.player)) return;
        amountSelected--;
        UpdateButtonUI();
    }

    public void OnDiceDisplayClick()
    {
        if (amountOwnedText != null) return; // this is a selector button, not a display button
        if (!GameManager.Instance.diceForItemSelection) return;
        if (activeDiceIndex < 0) return;
        GameManager.Instance.PlayerToggleDiceForItem(activeDiceIndex);
    }

    public void UpdateButtonUI()
    {
        if (amountOwnedText != null) amountOwnedText.text = amountOwned.ToString();
        if (amountSelectedButton != null) amountSelectedButton.gameObject.SetActive(amountSelected > 0);
        if (amountSelectedText != null) amountSelectedText.text = amountSelected.ToString();
        if (dice != null && icon != null) icon.sprite = dice.sides[0].sprite;
    }

    public void SetIcon(Sprite sprite)
    {
        if (icon != null) icon.sprite = sprite;
    }

    public void SetHighlight(bool on)
    {
        if (icon != null) icon.color = on ? highlightColor : Color.white;
    }

    public void OnPointerEnter() {
        if(dice!=null) UIManager.Instance.ShowDiceInfo(dice);
    }

    public void ShowDiceInfo()
    {
        UIManager.Instance.ShowDiceInfo(dice);
    }
}