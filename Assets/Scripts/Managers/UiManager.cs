using NUnit.Framework;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private AIPlayer ai;
    private MoneySystem money;

    [Header("Bet Buttons")]
    [SerializeField] private Image oddButtonImage;
    [SerializeField] private Image evenButtonImage;
    [SerializeField] private Image highButtonImage;
    [SerializeField] private Image lowButtonImage;


    [Header("GameObjects")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text aiMoneyText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text betText;

    [SerializeField] private Sprite emptyDiceSlot;

    [Header("DiceDisplay")]
    [SerializeField] private DiceButton[] diceDisplayButtons;

    [Header("DiceSelector")]
    [SerializeField] private GridLayoutGroup diceSelectorGrid;
    [SerializeField] private DiceButton diceIconPrefab;

	[System.Serializable]
    public class DiceIcon
    {
        public string diceType;
        public GameObject iconObject;
    }
    private Dictionary<string, GameObject> lookup;

    private int betAmount = 10;
    private BetType betType = BetType.Odd;

	public static UIManager Instance;
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			Instance = this;
		}
		UpdateDiceDisplay();
		UpdateDiceSelector();
	}

    public void Init(MoneySystem moneySystem)
    {
        money = moneySystem;
        UpdateUI();
    }
    public void UpdateUI()
    {
        resetResult();
        UpdateMoney();
        UpdateBetAmount();
        UpdateDiceSelector();
	}

    public void AddDice(DiceData dice)
    {
        if (!DiceManager.Instance.AddDice(dice)) return;
		UpdateDiceDisplay();
	}
    public void RemoveDice(DiceData dice)
    {
        if (!DiceManager.Instance.RemoveDice(dice)) return;
		UpdateDiceDisplay();

	}

    private void UpdateDiceDisplay(){
       
        for (int i = 0; i < diceDisplayButtons.Length; i++)
        {
            if (i >= DiceManager.Instance.diceList.Count){
				diceDisplayButtons[i].SetIcon(emptyDiceSlot);
                diceDisplayButtons[i].dice = null;
			}
            else
            {
				diceDisplayButtons[i].SetIcon(DiceManager.Instance.diceList[i].sides[0].sprite);
				diceDisplayButtons[i].dice = DiceManager.Instance.diceList[i];
			}
		}
    }

    private void UpdateDiceSelector()
    {
        foreach (Transform child in diceSelectorGrid.transform)
        {
            Destroy(child.gameObject);
        }
        List<DiceData> uniqueDice = new List<DiceData>();
		foreach (var dice in Player.Instance.ownedDiceList)
        { 
            if(uniqueDice.Contains(dice)) continue;
            uniqueDice.Add(dice);

			var icon = Instantiate(diceIconPrefab, diceSelectorGrid.transform);
            icon.dice = dice;
            icon.amountOwned = Player.Instance.GetDiceAmount(dice);
			icon.UpdateButtonUI();
        }
	}

	public void SetBetOdd()
    {
        betType = BetType.Odd;
        ResetBetButtons();
        oddButtonImage.color = Color.red;
    }
    public void SetBetEven()
    {
        betType = BetType.Even;
        ResetBetButtons();
        evenButtonImage.color = Color.red;
    }
    public void SetBetHigh()
    {
        betType = BetType.High;
        ResetBetButtons();
        highButtonImage.color = Color.red;
    }
    public void SetBetLow()
    {
        betType = BetType.Low;
        ResetBetButtons();
        lowButtonImage.color = Color.red;
    }
    private void ResetBetButtons()
    {
        oddButtonImage.color = Color.white;
        evenButtonImage.color = Color.white;
        highButtonImage.color = Color.white;
        lowButtonImage.color = Color.white;
    }
    private void UpdateBetAmount()
    {
        betText.text = $"Amount: {betAmount}";
    }
    public void IncreaseAmount(int amount)
    {
        if (!(betAmount + amount > Player.Instance.money))
        {
            betAmount += amount;
        }
        UpdateBetAmount();
    }

    public void DecreaseAmount(int amount)
    {
        if (betAmount - amount >= 0)
        {
            betAmount -= amount;
        }
        UpdateBetAmount();
    }

    public BetData GetBet()
    {
        return new BetData(betType, betAmount);
    }
    public void resetResult()
    {
        resultText.text ="...";
    }
    public void ShowResult(bool win, int amount)
    {
        if (win)
        {
            resultText.text = $"WIN +{amount}";
        }
        else
        {
            resultText.text = $"LOSE -{amount}";
        }
    }

    public void UpdateMoney()
    {
        moneyText.text = $"YOU: {Player.Instance.money}";
        aiMoneyText.text = $"OPONENT: {ai.AiMoney}";
    }

    public bool AllSelected()
    {
        if (betAmount > 0)
        {
            return true;
        }
        return false;
    }
}