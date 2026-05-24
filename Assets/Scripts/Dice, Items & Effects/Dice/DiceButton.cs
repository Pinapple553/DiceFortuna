using TMPro;
using Unity.VisualScripting;
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
	public void SelectDice(){
		if (amountSelected >= amountOwned) return;
        if (!DiceManager.Instance.AddDice(dice, GameManager.Instance.player)) return;
		amountSelected++;
        UpdateButtonUI();
	}
	public void DeselectDice(){
        if (amountSelected <= 0) return;
        if (!DiceManager.Instance.RemoveDice(dice, GameManager.Instance.player)) return;
        amountSelected--;
        UpdateButtonUI();
    }

	public void UpdateButtonUI() {
		amountOwnedText.text = amountOwned.ToString();
		amountSelectedButton.gameObject.SetActive(amountSelected>0);
		amountSelectedText.text = amountSelected.ToString();
		icon.sprite = dice.sides[0].sprite;
	}
	public void SetIcon(Sprite sprite)
	{
		icon.sprite = sprite;
	}

	public void OnPointerEnter(){
		//UIManager.Instance.ShowDiceInfo(dice);
	}
}
