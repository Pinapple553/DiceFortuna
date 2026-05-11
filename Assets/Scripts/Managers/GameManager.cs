using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public AIPlayer ai;
	public Player player;

	public static GameManager Instance;

	public bool roundRunning = false;
	public bool diceRolling = false;
	public int pendingItemTargetIndex = -1;

	[HideInInspector] public bool playerChoseHeads = true;
	[HideInInspector] public bool playerGoesFirst = true;
	[HideInInspector] public bool isPlayerItemTurn = false;
	[HideInInspector] public bool playerPassedItems = false;
	[HideInInspector] public bool aiPassedItems = false;

	[SerializeField] private int rollsPerMatch = 2;

	private void Awake()
	{
		if (Instance != null && Instance != this) Destroy(this.gameObject);
		else Instance = this;
	}

	private void Start()
	{
		UIManager.Instance.CloseRoundInfo(false);
	}
	public void StartButtonClick()
	{
		if (!roundRunning) StartRound();
		UIManager.Instance.CloseRoundInfo(true);
	}

	public void LockInDiceAndFlipCoin()
	{
		if (!DiceManager.Instance.SelectDice()) return;
		AISelectDice();
		StartCoroutine(CoinFlipRoutine());
	}
	public void PlayerChoseCoinSide(bool heads)
	{
		playerChoseHeads = heads;
		UIManager.Instance.coinFlipResolved = true;  // unblock the coroutine
	}
	public void PassItemsButton()
	{
		if (isPlayerItemTurn) playerPassedItems = true;
	}
	public void StartRound()
	{
		roundRunning = true;
		player.BuildItemHand();
		ai.BuildItemHand();
		DiceManager.Instance.ResetRound();
		UIManager.Instance.SetGameButtonText("Select Dice");
		UIManager.Instance.UpdateUI();
	}
	private void AISelectDice()
	{
		ai.selectedDiceList.Clear();
		var pool = new List<DiceData>(ai.ownedDiceList);
		int count = Mathf.Min(ai.maxDice, pool.Count); //in case ai owns fewer dice than max allowed
		for (int i = 0; i < count; i++)
		{
			int idx = Random.Range(0, pool.Count);
			ai.selectedDiceList.Add(new DiceInstance(pool[idx]));
			pool.RemoveAt(idx);
		}
	}
	private IEnumerator CoinFlipRoutine()
	{
		UIManager.Instance.coinFlipResolved = false;
		UIManager.Instance.ShowCoinFlip(true);
		yield return new WaitUntil(() => UIManager.Instance.coinFlipResolved);

		bool coinIsHeads = Random.value >= 0.5f;
		playerGoesFirst = (coinIsHeads == playerChoseHeads);

		UIManager.Instance.ShowCoinResult(coinIsHeads, playerGoesFirst);
		yield return new WaitForSeconds(2f);
		UIManager.Instance.ShowCoinFlip(false);

		StartCoroutine(MatchRoutine());
	}
	private IEnumerator MatchRoutine()
	{
		for (int roll = 0; roll < rollsPerMatch; roll++)
		{
			PlayerBase first = playerGoesFirst ? player : ai;
			PlayerBase second = playerGoesFirst ? ai : player;
			bool firstIsPlayer = playerGoesFirst;

			yield return StartCoroutine(DoRoll(first, firstIsPlayer, isPlayerRoll: firstIsPlayer));
			yield return StartCoroutine(ItemPhase(firstIsPlayer));

			yield return StartCoroutine(DoRoll(second, !firstIsPlayer, isPlayerRoll: !firstIsPlayer));
			yield return StartCoroutine(ItemPhase(!firstIsPlayer));
		}

		yield return StartCoroutine(ResolveWinner());
	}
	private IEnumerator DoRoll(PlayerBase roller, bool rollerIsPlayer, bool isPlayerRoll)
	{
		diceRolling = true;
		UIManager.Instance.resetResult();
		
		foreach (var d in roller.selectedDiceList)
		{
			DiceAnimation.Instance.Roll(d);
		}
		yield return new WaitUntil(() => !AnyRolling(roller.selectedDiceList));

		if (isPlayerRoll) //player
		{
			yield return UIManager.Instance.CountAllDice();
			yield return DiceManager.Instance.CountAllBonuses();
		}
		else //ai
		{
			foreach (var d in roller.selectedDiceList)
			{
				DiceManager.Instance.aiCurrentResult += d.data.sides[d.currentSideIndex].value;
			}
				
		}

		UIManager.Instance.ShowRollResults();
		diceRolling = false;
		yield return new WaitForSeconds(0.5f);
	}
	private IEnumerator ItemPhase(bool playerWentFirst)
	{
		playerPassedItems = false;
		aiPassedItems = false;

		//go until both pass or no more uses
		for (int i = 0; i < 100; i++)
		{
			//player
			if (!playerPassedItems && player.CanUseItem())
			{
				isPlayerItemTurn = true;
				UIManager.Instance.ShowItemPhase(true);
				yield return new WaitUntil(() => playerPassedItems || !player.CanUseItem());
				isPlayerItemTurn = false;
			}
			else playerPassedItems = true;

			//ai
			if (!aiPassedItems && ai.CanUseItem()) yield return StartCoroutine(AIUseItem());
			else aiPassedItems = true;

			if (playerPassedItems && aiPassedItems) break;
		}
		UIManager.Instance.ShowItemPhase(false);
	}

	private IEnumerator AIUseItem()
	{
		yield return new WaitForSeconds(0.8f);

		if (Random.value < 0.4f)
		{
			var unused = ai.itemHand.FindAll(item => !item.used);
			if (unused.Count > 0)
			{
				var chosen = unused[Random.Range(0, unused.Count)];
				ai.TryUseItem(chosen);
				pendingItemTargetIndex = Random.Range(0, ai.selectedDiceList.Count);
				yield return chosen.data.effect.Apply(ai, ai.selectedDiceList);
				UIManager.Instance.UpdateUI();
				yield break;
			}
		}
		aiPassedItems = true;
	}
	public IEnumerator PlayerUseItem(ItemInstance item)
	{
		if (!isPlayerItemTurn) yield break;
		if (!player.TryUseItem(item)) yield break;

		yield return item.data.effect.Apply(player, player.selectedDiceList);

		//recount 
		DiceManager.Instance.currentResult = 0;
		yield return UIManager.Instance.CountAllDice();
		yield return DiceManager.Instance.CountAllBonuses();
		UIManager.Instance.UpdateUI();
	}
	private IEnumerator ResolveWinner()
	{
		int playerScore = DiceManager.Instance.currentResult;
		int aiScore = DiceManager.Instance.aiCurrentResult;
		bool tie = (playerScore == aiScore);
		bool playerWins = (playerScore > aiScore);

		if (tie)
		{
			UIManager.Instance.ShowMessage("TIE! Token stays on the table.");
		}
		else if (playerWins)
		{
			UIManager.Instance.ShowMessage($"YOU WIN!  {playerScore} vs {aiScore}");
		}
		else
		{
			UIManager.Instance.ShowMessage($"YOU LOSE  {playerScore} vs {aiScore}");
			UIManager.Instance.LoseLife();
		}

		UIManager.Instance.UpdateMoney();
		roundRunning = false;

		yield return new WaitForSeconds(2f);
		UIManager.Instance.ShowRoundEndPanel();
	}
	private bool AnyRolling(List<DiceInstance> list)
	{
		foreach (var d in list)
		{ 
			if (d.isRolling) return true;
		}
		return false;
	}
}