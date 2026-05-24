using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UIManager : MonoBehaviour
{
	[Header("Center")]
	[SerializeField] private TMP_Text rollPointsText;
	[SerializeField] private Button rollButton;
	[SerializeField] private TMP_Text rollButtonText;
    [SerializeField] private DiceButton[] diceDisplayButtons;
    [SerializeField] private Sprite emptyDiceSlot;

	[Header("ItemBar")]
	[SerializeField] private HorizontalLayoutGroup itemSelector;
    [SerializeField] private DiceButton diceButtonPrefab;
    [SerializeField] private ItemButton itemButtonPrefab;
	[SerializeField] private TMP_Text itemSelectButtonText;
    [HideInInspector] private bool selectorShowDice = true;

	[Header("RoundInfo")]
	[SerializeField] private TMP_Text currentRoundText;
	[SerializeField] private PlayerInfoCard playerRoundInfo;
    [SerializeField] private PlayerInfoCard NPCRoundInfo;

	[Header("MatchInfo")]
	[SerializeField] private VerticalLayoutGroup matchEffects;
	[SerializeField] private Button matchEffectButtonPrefab;
    [SerializeField] private PlayerInfoCard playerMatchInfo;
    [SerializeField] private PlayerInfoCard NPCMatchInfo;

    [Header("Panels")]
	[SerializeField] private GameObject roundInfoPanel;
	[SerializeField] private GameObject itemPhasePanel;
	[SerializeField] private GameObject messagePanel;
	[SerializeField] private TMP_Text messageText;
	[SerializeField] private GameObject roundEndPanel;

	[Header("CoinFlip")]
	[SerializeField] private GameObject coinFlipUI;
	[SerializeField] private Animator coinFlipAnimator;
    [HideInInspector] public bool coinFlipResolved = false;
    [HideInInspector] public bool roundFinished = false;

	
	[HideInInspector] public static UIManager Instance;

    //corutines
    private Coroutine hideMessageCoroutine;

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
        UpdateRoundInfo();
		UpdateDiceSelector();
		UpdateDiceDisplay();
		UpdateItemBarButton();
    }
	public void UpdateRoundInfo()
	{
		if (playerRoundInfo != null) playerRoundInfo.PointText.text = GameManager.Instance.player.roundFortunaPoints.ToString();
        if (NPCRoundInfo != null) NPCRoundInfo.PointText.text = GameManager.Instance.player.roundFortunaPoints.ToString();
    }

    private void UpdateItemBarButton()
    {
		if (!GameManager.Instance.diceSelected) itemSelectButtonText.text = "Lock in";
		else itemSelectButtonText.text = "Use";
    }
    public void resetResult()
	{
		if (rollPointsText != null) rollPointsText.text = "";
	}
	public void ShowRollResults()
	{
		if (rollPointsText != null) rollPointsText.text = DiceManager.Instance.currentResult.ToString();
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
		foreach (Transform child in itemSelector.transform)
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
				var icon = Instantiate(diceButtonPrefab, itemSelector.transform);
                icon.dice = dice;
                icon.amountOwned = GameManager.Instance.player.GetDiceAmount(dice);
				icon.amountSelected = GameManager.Instance.player.GetSelectedDiceAmout(dice);
                icon.UpdateButtonUI();
            }
        }
		else
		{
            var uniqueItems = new List<ItemData>();
            foreach (var item in GameManager.Instance.player.ownedItemsList)
			{
                if (uniqueItems.Contains(item)) continue;
                uniqueItems.Add(item);
                var icon = Instantiate(itemButtonPrefab, itemSelector.transform);
                /*icon.dice = item;
                icon.amountOwned = GameManager.Instance.player.GetAvailableItemAmount(item);
                icon.UpdateButtonUI();*/
            }
        }
    }
	public void SelectorMode(string mode) //switch between dice or item selector
	{
		if (mode == "dice") selectorShowDice = true;
		else selectorShowDice = false;
		UpdateDiceSelector();
	}
	public void UppdateItemBar()
	{
		if (!GameManager.Instance.diceSelected) itemSelectButtonText.text = "Select";
        else itemSelectButtonText.text = "Use";

	}

    /*public void ShowDiceInfo(DiceData dice)
	{
		if (diceInfoIcon != null) diceInfoIcon.sprite = dice.sides[0].sprite;
		if (diceInfoName != null) diceInfoName.text = dice.diceName;
		if (diceInfoDescription != null) diceInfoDescription.text = dice.description;
		if (diceInfoSides != null) diceInfoSides.text = dice.sides.Length.ToString();
	}*/
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
				rollPointsText.text = result.ToString();
				yield return new WaitForSeconds(0.05f);
			}

			yield return new WaitForSeconds(0.2f);
			diceDisplayButtons[i].transform.localScale = Vector3.one;
		}
	}
	public IEnumerator ShowCombo(List<int> inDices, DiceCombo combo)
	{
        rollPointsText.text = combo.GetName();
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
			rollPointsText.text = startValue.ToString();
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
		//Round info phase:Item ?
	}
	
	//popup and panel functions
	public void ShowRoundEndPanel()
	{
		if (roundEndPanel != null) roundEndPanel.SetActive(true);
	}
	public void OnFinishClicked()
	{
		if (roundEndPanel != null) roundEndPanel.SetActive(false);
		roundFinished = true;
	}
    public void ShowMessage(string text) //show message popup with text, disapears after 2 seconds 
    {
        if (hideMessageCoroutine != null) { StopCoroutine(hideMessageCoroutine); hideMessageCoroutine = null; }
        if (messagePanel == null || messageText == null) return;
        messageText.text = text;
        messagePanel.SetActive(true);
        if (hideMessageCoroutine == null) hideMessageCoroutine = StartCoroutine(HideAfter(messagePanel, 2f));
    }
    private IEnumerator HideAfter(GameObject panel, float delay) //sets object to disapear after set amount of time
    {
        yield return new WaitForSeconds(delay);
        if (panel != null) panel.SetActive(false);
        if (panel = messagePanel) hideMessageCoroutine = null;

    }
    //coin animation functions
    public IEnumerator StartCoinAnimation() //begins coin flip interaction
	{
        coinFlipAnimator.SetTrigger("Enter");
        yield return new WaitForSeconds(2f);
        coinFlipUI.SetActive(true); //shows heads or tails buttons
    }
	public IEnumerator PlayCoinAnimation(bool heads) //plas different animation depending if its heads or tails
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
	public IEnumerator ExitCoinAnimation() //coin flip hand exits screen
	{
		coinFlipAnimator.SetTrigger("Exit");
		yield return new WaitForSeconds(1f);
    } 
}