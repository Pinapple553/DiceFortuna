using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Effects/Reroll")]
public class RerollDiceEffect : ItemEffect
{
    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice, ItemTier tier, List<int> targetIndices)
    {
        int count = Mathf.Min(targetIndices.Count, tier.diceTargets);
        var toReroll = new List<DiceInstance>();
        for (int i = 0; i < count; i++)
        {
            int idx = targetIndices[i];
            if (idx < 0 || idx >= dice.Count) continue;
            DiceAnimation.Instance.Roll(dice[idx]);
            toReroll.Add(dice[idx]);
        }
        while (true)
        {
            bool anyRolling = false;
            foreach (var d in toReroll) 
                if (d.isRolling) 
                { 
                    anyRolling = true; break; 
                }
            if (!anyRolling) break;
            yield return null;
        }
    }
    public override string GetDescription(ItemTier tier) => $"Reroll {tier.diceTargets} chosen {(tier.diceTargets == 1 ? "die" : "dice")}. ({tier.uses} use/round)";
}