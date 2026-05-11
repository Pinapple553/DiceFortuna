using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	[Header("Core Text")]
	[SerializeField] private TMP_Text gameButtonText;
	[SerializeField] private TMP_Text moneyText;
	[SerializeField] private TMP_Text aiMoneyText;
	[SerializeField] private TMP_Text pointsText;
	[SerializeField] private Sprite emptyDiceSlot;

	[Header("Panels")]
	[SerializeField] private GameObject roundInfoPanel;
	[SerializeField] private GameObject itemPhasePanel;
	[SerializeField] private GameObject messagePanel;
	[SerializeField] private TMP_Text messageText;
	[SerializeField] private GameObject roundEndPanel;

	[Header("CoinFlip")]
	[SerializeField] private GameObject coinFlipUI;
	[SerializeField] private Animator coinFlipAnimator;

	[Header("DiceDisplay")]
	[SerializeField] private DiceButton[] diceDisplayButtons;

	[Header("DiceSelector")]
	[SerializeField] private GridLayoutGroup diceSelectorGrid;
	[SerializeField] private DiceButton diceIconPrefab;

	[Header("DiceInfo")]
	[SerializeField] private Image diceInfoIcon;
	[SerializeField] private TMP_Text diceInfoName;
	[SerializeField] private TMP_Text diceInfoSides;
	[SerializeField] private TMP_Text diceInfoDescription;

	public static UIManager Instance;
	public bool coinFlipResolved = false;

	private bool selectorShowDice = true;

	//test
	private int currentLives = 3;

	private void Awake()
	{
		if (Instance != null && Instance != this) Destroy(this.gameObject);
		else Instance = this;
	}
	private void Start()
	{
		if (messagePanel != null) messagePanel.SetActive(false);
		if (coinFlipUI != null) coinFlipUI.SetActive(false);
		if (itemPhasePanel != null) itemPhasePanel.SetActive(false);
		if (roundEndPanel != null) roundEndPanel.SetActive(false);
		UpdateUI();
	}

	public void UpdateUI()
	{
		resetResult();
		UpdateMoney();
		UpdateDiceSelector();
		UpdateDiceDisplay();
	}

	public void UpdateMoney()
	{
		if (moneyText != null) moneyText.text = $"YOU: {GameManager.Instance.player.fortunaPoints}";
		if (aiMoneyText != null) aiMoneyText.text = $"OPONENT: {GameManager.Instance.ai.fortunaPoints}";
	}

	public void SetGameButtonText(string text)
	{
		if (gameButtonText != null) gameButtonText.text = text;
	}
	public void resetResult()
	{
		if (pointsText != null) pointsText.text = "";
	}
	public void ShowRollResults()
	{
		if (pointsText != null) pointsText.text = DiceManager.Instance.currentResult.ToString();
	}
	public void CloseRoundInfo(bool close)
	{
		if (roundInfoPanel != null) roundInfoPanel.SetActive(!close);
	}
	private void UpdateDiceDisplay()
	{
		for (int i = 0; i < diceDisplayButtons.Length; i++)
		{
			if (i >= DiceManager.Instance.activeDiceList.Count)
			{
				diceDisplayButtons[i].SetIcon(emptyDiceSlot);
				diceDisplayButtons[i].dice = null;
			}
			else
			{
				var inst = DiceManager.Instance.activeDiceList[i];
				diceDisplayButtons[i].SetIcon(inst.data.sides[inst.currentSideIndex].sprite);
				diceDisplayButtons[i].dice = inst.data;
				diceDisplayButtons[i].instanceId = inst.instanceId;
			}
		}
	}
	private void UpdateDiceSelector()
	{
		foreach (Transform child in diceSelectorGrid.transform)
		{
			Destroy(child.gameObject);
		}

		if (selectorShowDice)
		{
            var uniqueDice = new List<DiceData>();
            foreach (DiceData dice in GameManager.Instance.player.ownedDiceList)
            {
                if (uniqueDice.Contains(dice)) continue;
                uniqueDice.Add(dice);
                var icon = Instantiate(diceIconPrefab, diceSelectorGrid.transform);
                icon.dice = dice;
                icon.amountOwned = GameManager.Instance.player.GetAvailableAmount(dice);
                icon.UpdateButtonUI();
            }
        }
		else
		{
			foreach (var item in GameManager.Instance.player.ownedItemsList)
			{
				//display items 
			}
        }
    }

	public void SelectorMode(string mode)
	{
		if (mode == "dice")
		{
			selectorShowDice = true;
		}
		else
		{
			selectorShowDice = false;
		}
	}



    public void ShowDiceInfo(DiceData dice)
	{
		if (diceInfoIcon != null) diceInfoIcon.sprite = dice.sides[0].sprite;
		if (diceInfoName != null) diceInfoName.text = dice.diceName;
		if (diceInfoDescription != null) diceInfoDescription.text = dice.description;
		if (diceInfoSides != null) diceInfoSides.text = dice.sides.Length.ToString();
	}
	public IEnumerator CountAllDice()
	{
		int result = 0;
		for (int i = 0; i < diceDisplayButtons.Length; i++)
		{
			if (i >= DiceManager.Instance.activeDiceList.Count) break;

			yield return StartCoroutine(ScaleRoutine(diceDisplayButtons[i].transform, 1.5f));
			var instance = DiceManager.Instance.activeDiceList[i];
			int value = instance.data.sides[instance.currentSideIndex].value;
			PointPopupGenerator.Instance.CreatePopUp(value.ToString());

			for (int j = 0; j < value; j++)
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

	public IEnumerator ShowCombo(List<int> inDices, DiceCombo combo)
	{
		pointsText.text = combo.GetName();
		int bonus = combo.GetBonus();

		foreach (int i in inDices)
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
		foreach (int i in inDices)
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

	public void ShowItemPhase(bool show)
	{
		if (itemPhasePanel != null) itemPhasePanel.SetActive(show);
	}

	public void ShowMessage(string text)
	{
		if (messagePanel == null || messageText == null) return;
		messageText.text = text;
		messagePanel.SetActive(true);
		StartCoroutine(HideAfter(messagePanel, 3f));
	}

	private IEnumerator HideAfter(GameObject panel, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (panel != null) panel.SetActive(false);
	}

	public void ShowRoundEndPanel()
	{
		if (roundEndPanel != null) roundEndPanel.SetActive(true);
	}
	public void OnPlayAgainClicked()
	{
		if (roundEndPanel != null) roundEndPanel.SetActive(false);
		GameManager.Instance.StartButtonClick();
	}
	public void LoseLife()
	{
		if (currentLives <= 0) return;
		currentLives--;
		if (currentLives <= 0) ShowMessage("Out of straws! Game over.");
	}

	public IEnumerator StartCoinAnimation()
	{
        coinFlipAnimator.SetTrigger("Enter");
        yield return new WaitForSeconds(2f);
        coinFlipUI.SetActive(true);
    }
	public IEnumerator PlayCoinAnimation(bool heads)
	{
        coinFlipUI.SetActive(false);
        if (heads)
		{
            coinFlipAnimator.SetTrigger("Enter");
            yield return new WaitForSeconds(1f);
            yield break;
		}
		else
		{
            coinFlipAnimator.SetTrigger("Flip");
			yield return new WaitForSeconds(1f);
            yield break;
        }
	}

	public IEnumerator ExitCoinAnimation()
	{
		coinFlipAnimator.SetTrigger("Exit");
		yield return new WaitForSeconds(1f);
    } 
}