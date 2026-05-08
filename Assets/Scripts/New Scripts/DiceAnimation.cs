using System.Collections;
using UnityEngine;

public class DiceAnimation : MonoBehaviour
{
	public static DiceAnimation Instance;
	public void Awake()
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
	public int Roll(DiceInstance dice)
	{
		StartCoroutine(RollCoroutine(dice));
		return dice.data.sides[dice.currentSideIndex].value;
	}
	private IEnumerator RollCoroutine(DiceInstance dice)
	{
		yield return RollAnimation(dice);
		int index = Random.Range(0, dice.data.sides.Length);
		dice.currentSideIndex = index;
		var result = dice.data.sides[index];
		result.effect?.Apply(dice, result.effectChance);
		yield break;
	}

	private IEnumerator RollAnimation(DiceInstance dice)
	{
		float delay = 0.05f;
		int rollCount = Random.Range(10, 20);
		for (int i = 0; i < rollCount; i++)
		{
			int index = Random.Range(0, dice.data.sides.Length);
			dice.currentSideIndex = index;

			UIManager.Instance.UpdateUI();
			yield return new WaitForSeconds(delay);

			delay *= 1.2f; // slows down
		}
	}
}
