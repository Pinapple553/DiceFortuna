using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Create asset: Right-click → Create → Item → Effects → Reroll Odd
// Rerolls every die whose current face value is odd.
[CreateAssetMenu(menuName = "Item/Effects/Reroll Odd")]
public class RerollOddDiceEffect : ItemEffect
{
    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice)
    {
        var toReroll = new List<DiceInstance>();
        foreach (var d in dice)
        {
            int val = d.data.sides[d.currentSideIndex].value;
            if (val % 2 != 0)
            {
                DiceAnimation.Instance.Roll(d);
                toReroll.Add(d);
            }
        }

        // Wait for all of them to finish
        bool anyRolling = true;
        while (anyRolling)
        {
            anyRolling = false;
            foreach (var d in toReroll)
                if (d.isRolling) { anyRolling = true; break; }
            yield return null;
        }
    }

    public override string GetDescription() => "Reroll all dice showing an odd number.";
}
