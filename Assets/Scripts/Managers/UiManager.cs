using NUnit.Framework;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private DiceManager diceManager;
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
    [SerializeField] private GameObject activeDiceContainer;
    [SerializeField] private GameObject blankDiceIcon;
    [SerializeField] private List<DiceIcon> diceIcons;

    [System.Serializable]
    public class DiceIcon
    {
        public string diceType;
        public GameObject iconObject;
    }
    private Dictionary<string, GameObject> lookup;

    private int betAmount = 10;
    private BetType betType = BetType.Odd;

    private void Awake()
    {
        //Dictionary to link diceType name to the icon
        lookup = new Dictionary<string, GameObject>();
        foreach (var icon in diceIcons)
        {
            lookup[icon.diceType] = icon.iconObject;
        }
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

    }

    public void AddDice(Dice dice)
    {
        if (!diceManager.AddDice(dice)) return;

        if (lookup.TryGetValue(dice.name, out var diceIcon))
        {
            GameObject newDiceIcon = Instantiate(diceIcon);
            newDiceIcon.name = dice.name;
            newDiceIcon.transform.SetParent(activeDiceContainer.transform);
        }
        else
        {
            blankDiceIcon.transform.parent = activeDiceContainer.transform;
        }
    }
    public void RemoveDice(Dice dice)
    {
        if (!diceManager.RemoveDice(dice)) return;

        for (int i = 0; i < activeDiceContainer.transform.childCount; i++)
        {
            Transform child = activeDiceContainer.transform.GetChild(i);

            if (child.name == dice.diceType)
            {
                Destroy(child.gameObject);
                return;
            }
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
        if (!(betAmount + amount > money.PlayerMoney))
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
        moneyText.text = $"YOU: {money.PlayerMoney}";
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