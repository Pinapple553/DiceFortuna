using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceAnimation : MonoBehaviour
{
	public static DiceAnimation Instance;

    private Dictionary<DiceInstance, Coroutine> activeRolls = new();
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
	public void Roll(DiceInstance dice)
	{
		if (activeRolls.ContainsKey(dice))
		{
			return;
        }
		Coroutine coroutine = StartCoroutine(RollCoroutine(dice));
		activeRolls[dice] = coroutine;

		dice.isRolling = true;
    }

    private IEnumerator RollCoroutine(DiceInstance dice)
    {
        yield return RollAnimation(dice);
        FinishRoll(dice);
    }
    private void FinishRoll(DiceInstance dice)
    {
        int index = Random.Range(0, dice.data.sides.Length);
        dice.currentSideIndex = index;

        var result = dice.data.sides[index];
        result.effect?.Apply(dice, result.effectChance);

        dice.isRolling = false;
        activeRolls.Remove(dice);
    }
	private IEnumerator RollAnimation(DiceInstance dice)
	{
		float delay = 0.05f;
		int rollCount = Random.Range(10, 14);
		for (int i = 0; i < rollCount; i++)
		{
            int index = Random.Range(0, dice.data.sides.Length);
			dice.currentSideIndex = index;

			UIManager.Instance.UpdateUI();
			yield return new WaitForSeconds(delay);

			delay *= 1.2f;
		}
    }
    public void SkipAll()
    {
        var rollsCopy = new List<KeyValuePair<DiceInstance, Coroutine>>(activeRolls);

        foreach (var pair in rollsCopy)
        {
            StopCoroutine(pair.Value);
            FinishRoll(pair.Key);
        }
        activeRolls.Clear();
    }
}
