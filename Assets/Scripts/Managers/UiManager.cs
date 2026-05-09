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
    [SerializeField] private TMP_Text startButtonText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text aiMoneyText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text diceResultText;
    [SerializeField] private TMP_Text betText;

    [SerializeField] private Sprite emptyDiceSlot;

    [Header("DiceDisplay")]
    [SerializeField] private DiceButton[] diceDisplayButtons;

    [Header("DiceSelector")]
    [SerializeField] private GridLayoutGroup diceSelectorGrid;
    [SerializeField] private DiceButton diceIconPrefab;

    private List<DiceInstance> inventoryDice = new List<DiceInstance>();

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
	}
    private void Start()
    {
        inventoryDice = Player.Instance.ownedDiceList.ConvertAll(d => new DiceInstance(d));
        UpdateUI();
    }
    public void UpdateUI()
    {
        resetResult();
        UpdateMoney();
        UpdateBetAmount();
        UpdateDiceSelector();
        UpdateDiceDisplay();
    }
    private void UpdateDiceDisplay(){
       
        for (int i = 0; i < diceDisplayButtons.Length; i++)
        {
            if (i >= DiceManager.Instance.activeDiceList.Count) {
                diceDisplayButtons[i].SetIcon(emptyDiceSlot);
                diceDisplayButtons[i].dice = null;
            }
            else
            {
                var instance = DiceManager.Instance.activeDiceList[i];
                diceDisplayButtons[i].SetIcon(instance.data.sides[instance.currentSideIndex].sprite);
                diceDisplayButtons[i].dice = instance.data;
                diceDisplayButtons[i].instanceId = instance.instanceId;
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
		foreach (DiceData dice in Player.Instance.ownedDiceList)
        { 
            if(uniqueDice.Contains(dice)) continue;
            uniqueDice.Add(dice);

			var icon = Instantiate(diceIconPrefab, diceSelectorGrid.transform);
            icon.dice = dice;
            icon.amountOwned = Player.Instance.GetAvailableAmount(dice);
            icon.UpdateButtonUI();
        }
	}
    public void SetBetType(string type)
    {
        betType = (BetType)System.Enum.Parse(typeof(BetType), type);
        ResetBetButtons();
        switch (betType)
        {
            case BetType.Odd:
                oddButtonImage.color = Color.red;
                break;
            case BetType.Even:
                evenButtonImage.color = Color.red;
                break;
            case BetType.High:
                highButtonImage.color = Color.red;
                break;
            case BetType.Low:
                lowButtonImage.color = Color.red;
                break;
        }
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
    public void changeAmount(int amount)
    {
        if (!(betAmount + amount > Player.Instance.money) && !(betAmount + amount < 0))
        {
            betAmount += amount;
        }
        UpdateUI();
    }
    public BetData GetBet()
    {
        return new BetData(betType, betAmount);
    }
    public void resetResult()
    {
        resultText.text ="";
    }
    public void ShowRollResults()
    {
        diceResultText.text = DiceManager.Instance.currentResult.ToString();
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

    public void StartRoundUI(bool start)
    {
        startButtonText.text = start ? "Roll" : "Start";
    }
}