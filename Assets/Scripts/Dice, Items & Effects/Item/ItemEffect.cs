using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemEffect : ScriptableObject
{
    public abstract IEnumerator Apply( PlayerBase target, List<DiceInstance> dice,ItemTier tier, List<int> targetIndices);
    public abstract string GetDescription(ItemTier tier);
}