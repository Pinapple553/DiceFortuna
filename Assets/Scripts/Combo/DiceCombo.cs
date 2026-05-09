using System.Collections.Generic;
using UnityEngine;

public abstract class DiceCombo : ScriptableObject
{
    public abstract List<int> GetMatchingIndices(List<int> values);
    public abstract int GetBonus();
    public abstract string GetName();
}