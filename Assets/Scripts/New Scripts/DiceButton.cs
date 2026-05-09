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
	int amountAdded;

	[Header("UI")]
	[SerializeField] private Image icon;
	[SerializeField] private TMP_Text amountText;
	public void AddDice(){
		if (amountOwned-amountAdded>0)
		{
            if (!DiceManager.Instance.AddDice(dice)) return;
            amountAdded +=1;
		}
		UpdateButtonUI();
	}
	public void RemoveDice(){
        if (!DiceManager.Instance.RemoveDice(dice)) return;
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
