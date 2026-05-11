using UnityEngine;
using System.Collections.Generic;

// Three of a kind. Returns exactly 3 indices.
[CreateAssetMenu(menuName = "Dice/Combos/Triple")]
public class TripleCombo : DiceCombo
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
        foreach (var kvp in map)
        {
            if (kvp.Value.Count >= 3)
                return new List<int> { kvp.Value[0], kvp.Value[1], kvp.Value[2] };
        }
        return new List<int>();
    }

    public override int GetBonus() => 15;
    public override string GetName() => "Triple!";
}
