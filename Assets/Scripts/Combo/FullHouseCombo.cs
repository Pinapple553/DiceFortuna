using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dice/Combos/Full House")]
public class FullHouseCombo : DiceCombo
{
    public override List<int> GetMatchingIndices(List<int> values)
    {
        var map = new Dictionary<int, List<int>>();
        for (int i = 0; i < values.Count; i++)
        {
            int v = values[i];
            if (!map.ContainsKey(v)) map[v] = new List<int>();
            map[v].Add(i);
        }

        List<int> tripleIndices = null;
        List<int> pairIndices   = null;

        foreach (var kvp in map)
        {
            if (kvp.Value.Count >= 3 && tripleIndices == null) tripleIndices = new List<int> { kvp.Value[0], kvp.Value[1], kvp.Value[2] };
            else if (kvp.Value.Count >= 2 && pairIndices == null) pairIndices = new List<int> { kvp.Value[0], kvp.Value[1] };
        }

        if (tripleIndices != null && pairIndices != null)
        {
            var result = new List<int>(tripleIndices);
            result.AddRange(pairIndices);
            return result;
        }
        return new List<int>();
    }

    public override int GetBonus() => 25;
    public override string GetName() => "Full House!";
}
