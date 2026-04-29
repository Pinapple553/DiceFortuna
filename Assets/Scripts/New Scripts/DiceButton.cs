using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DiceButton : MonoBehaviour
{
	[Header("Dice Info")]
	public DiceData dice;
	public int amountOwned;
	int amountAdded;

	[Header("UI")]
	[SerializeField] private Image icon;
	[SerializeField] private TMP_Text amountText;
	public void AddDice(){
		if(amountOwned-amountAdded>0)
		{
			UIManager.Instance.AddDice(dice);
			amountAdded +=1;
		}
		UpdateButtonUI();
	}
	public void RemoveDice(){
		UIManager.Instance.RemoveDice(dice);
		amountAdded -=1;
	}

	public void UpdateButtonUI() { 
		amountText.text = (amountOwned - amountAdded).ToString();
		icon.sprite = dice.sides[0].sprite;
	}
	public void SetIcon(Sprite sprite)
	{
		icon.sprite = sprite;
	}
}
