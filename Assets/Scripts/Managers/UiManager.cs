using NUnit.Framework;
using System.Collections;
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

    [Header("GameObjects")]
    [SerializeField] private TMP_Text startButtonText;
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text aiMoneyText;
    [SerializeField] private TMP_Text pointsText;

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
    public IEnumerator CountAllDice() 
    {
        int result = 0;
        for (int i = 0; i < diceDisplayButtons.Length; i++)
        {
            if (i >= DiceManager.Instance.activeDiceList.Count)
            {
                break;
            }
            else
            {
                yield return StartCoroutine(ScaleRoutine(diceDisplayButtons[i].transform, 1.5f));
                int currentValue = DiceManager.Instance.activeDiceList[i].data.sides[DiceManager.Instance.activeDiceList[i].currentSideIndex].value;
                PointPopupGenerator.Instance.CreatePopUp(currentValue.ToString());
                for (int j = 0; j < currentValue; j++)
                {
                    result++;
                    DiceManager.Instance.currentResult = result;
                    pointsText.text = result.ToString();
                    yield return new WaitForSeconds(0.05f);
                }
                yield return new WaitForSeconds(0.2f);
                diceDisplayButtons[i].transform.localScale = Vector3.one;
            }
        }
    }
    public IEnumerator ShowCombo(List<int> indices, DiceCombo combo)
    {
        pointsText.text = combo.GetName();
        int bonus = combo.GetBonus();
        foreach (int i in indices)
        {   
            yield return StartCoroutine(ScaleRoutine(diceDisplayButtons[i].transform, 1.5f));
        }
        yield return new WaitForSeconds(0.3f);

        PointPopupGenerator.Instance.CreatePopUp(combo.GetName());

        int startValue = DiceManager.Instance.currentResult;
        for (int i = 0; i < bonus; i++)
        {
            startValue++;
            pointsText.text = startValue.ToString();
            yield return new WaitForSeconds(0.05f);
        }

        foreach (int i in indices)
        {
            diceDisplayButtons[i].transform.localScale = Vector3.one;
        }

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator ScaleRoutine(Transform target, float scale)
    {
        target.localScale = Vector3.one * scale;
        yield return new WaitForSeconds(0.2f);
        target.localScale = Vector3.one;
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
        pointsText.text ="";
    }
    public void ShowRollResults()
    {
        pointsText.text = DiceManager.Instance.currentResult.ToString();
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