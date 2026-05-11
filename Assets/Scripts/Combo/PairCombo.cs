using UnityEngine;
using System.Collections.Generic;

// Returns the first PAIR found in the list (exactly 2 indices).
// DiceManager calls this repeatedly, removing matched values each time,
// so rolling [3,3,5,5] correctly gives TWO pair bonuses.
[CreateAssetMenu(menuName = "Dice/Combos/Pair")]
public class PairCombo : DiceCombo
{
	public override List<int> GetMatchingIndices(List<int> values)
	{
		// Build a map of value → list of indices
		var map = new Dictionary<int, List<int>>();
		for (int i = 0; i < values.Count; i++)
		{
			int v = values[i];
			if (!map.ContainsKey(v)) map[v] = new List<int>();
			map[v].Add(i);
		}
		foreach (var kvp in map)
		{
			if (kvp.Value.Count >= 2)
			{
				// Return exactly 2 so triples don't get consumed here
				return new List<int> { kvp.Value[0], kvp.Value[1] };
			}
		}
		return new List<int>();
	}

	public override int GetBonus() => 5;
	public override string GetName() => "Pair";
}