using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
	public static DiceManager Instance;
	private PlayerController controller;

	[SerializeField] private List<DiceCombo> combos;

	public List<DiceInstance> activeDiceList = new List<DiceInstance>();

	public bool diceSelected = false;

	private void Awake()
	{
		if (Instance != null && Instance != this) Destroy(this.gameObject);
		else Instance = this;
		controller = new PlayerController();
		controller.Dice.Roll.performed += ctx => ThrowDice();
	}
	private void OnEnable() { controller.Dice.Enable(); }
	private void OnDisable() { controller.Dice.Disable(); }

	public void ResetMatch()
	{
		activeDiceList.Clear();
		diceSelected = false;
	}

	public bool AddDice(DiceData dice, PlayerBase player)
	{
		if (!GameManager.Instance.roundRunning) return false;
		if (GameManager.Instance.diceRolling) return false;
		if (player.selectedDiceList.Count >= player.maxDice) return false;
		player.selectedDiceList.Add(new DiceInstance(dice));
		UIManager.Instance.UpdateUI();
		return true;
	}

	public bool RemoveDice(DiceData dice, PlayerBase player)
	{
		if (!GameManager.Instance.roundRunning) return false;
		if (GameManager.Instance.diceRolling) return false;
		for (int i = player.selectedDiceList.Count - 1; i >= 0; i--)
		{
			if (player.selectedDiceList[i].data == dice)
			{
				player.selectedDiceList.RemoveAt(i);
				UIManager.Instance.UpdateUI();
				return true;
			}
		}
		UIManager.Instance.UpdateUI();
		return false;
	}

	public bool SelectDice()
	{
		if (!GameManager.Instance.roundRunning) return false;
		if (diceSelected) return false;
		if (GameManager.Instance.player.selectedDiceList.Count == 0) return false;
		activeDiceList = new List<DiceInstance>(GameManager.Instance.player.selectedDiceList);
		activeDiceList.AddRange(GameManager.Instance.ai.selectedDiceList);
		diceSelected = true;
		UIManager.Instance.UpdateUI();
		return true;
	}

	public void ThrowDice()
	{
		foreach (var d in activeDiceList)
		{ 
			DiceAnimation.Instance.Roll(d); 
		}
	}

	public bool IsAnyDiceRolling()
	{
		foreach (var d in activeDiceList)
		{
			if (d.isRolling) return true;
		}
		return false;
	}

	public List<int> GetAllValues()
	{
		var values = new List<int>();
		foreach (var dice in activeDiceList)
		{
			values.Add(dice.data.sides[dice.currentSideIndex].value);
		}
		return values;
	}

	public IEnumerator CountAllBonuses(PlayerBase targetPlayer)
	{
		List<int> values = GetAllValues();
		int totalBonus = 0;

		foreach (var combo in combos)
		{
			var remaining = new List<int>(values);

			while (true)
			{
				List<int> localMatch = combo.GetMatchingIndices(remaining);
				if (localMatch.Count == 0) break;

				List<int> originalIndices = MapToOriginal(localMatch, remaining, values);

				yield return UIManager.Instance.ShowCombo(originalIndices, combo);
				totalBonus += combo.GetBonus();

				localMatch.Sort((a, b) => b.CompareTo(a));
				foreach (int idx in localMatch)
				{
					remaining.RemoveAt(idx);
				}
			}
		}
        targetPlayer.roundFortunaPoints += totalBonus;
	}

	private List<int> MapToOriginal(List<int> localIndices, List<int> remaining, List<int> original)//map back to original indices for UI display
	{
		var result = new List<int>();
		var claimed = new HashSet<int>();

		foreach (int li in localIndices)
		{
			int targetVal = remaining[li];
			for (int oi = 0; oi < original.Count; oi++)
			{
				if (!claimed.Contains(oi) && original[oi] == targetVal)
				{
					claimed.Add(oi);
					result.Add(oi);
					break;
				}
			}
		}
		return result;
	}
}