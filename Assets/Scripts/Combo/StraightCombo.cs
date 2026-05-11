using UnityEngine;
using System.Collections.Generic;

// Straight: all dice show consecutive values (e.g. 2,3,4,5 or 1,2,3,4,5).
// Requires at least 4 dice for the bonus to apply.
[CreateAssetMenu(menuName = "Dice/Combos/Straight")]
public class StraightCombo : DiceCombo
{
    [SerializeField] private int minLength = 4; // minimum run length to count

    public override List<int> GetMatchingIndices(List<int> values)
    {
        if (values.Count < minLength) return new List<int>();

        // Sort unique values and check for a consecutive run
        var sorted = new List<int>(values);
        sorted.Sort();

        // Find the longest consecutive run
        List<int> bestRun = new();
        List<int> currentRun = new() { 0 }; // store indices into sorted

        for (int i = 1; i < sorted.Count; i++)
        {
            if (sorted[i] == sorted[i - 1] + 1)
                currentRun.Add(i);
            else if (sorted[i] != sorted[i - 1]) // skip duplicates, break on gap
            {
                if (currentRun.Count > bestRun.Count) bestRun = new List<int>(currentRun);
                currentRun = new List<int> { i };
            }
        }
        if (currentRun.Count > bestRun.Count) bestRun = currentRun;

        if (bestRun.Count < minLength) return new List<int>();

        // Map sorted indices back to original value-list indices
        var used   = new HashSet<int>();
        var result = new List<int>();
        foreach (int si in bestRun)
        {
            int val = sorted[si];
            for (int oi = 0; oi < values.Count; oi++)
            {
                if (!used.Contains(oi) && values[oi] == val)
                {
                    used.Add(oi);
                    result.Add(oi);
                    break;
                }
            }
        }
        return result;
    }

    public override int GetBonus() => 20;
    public override string GetName() => "Straight!";
}
