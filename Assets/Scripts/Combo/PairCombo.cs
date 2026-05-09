using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dice/Combos/Pair")]
public class PairCombo : DiceCombo
{
    public override List<int> GetMatchingIndices(List<int> values)
    {
        Dictionary<int, List<int>> map = new();
        for (int i = 0; i < values.Count; i++)
        {
            int v = values[i];
            if (!map.ContainsKey(v))
                map[v] = new List<int>();

            map[v].Add(i);
        }
        foreach (var pair in map)
        {
            if (pair.Value.Count >= 2)
                return pair.Value;
        }
        return new List<int>();
    }
    public override int GetBonus() => 5;
    public override string GetName() => "Pair";
}