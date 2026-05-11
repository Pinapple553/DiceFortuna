using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Create asset: Right-click → Create → Item → Effects → Reroll All
[CreateAssetMenu(menuName = "Item/Effects/Reroll All")]
public class RerollAllDiceEffect : ItemEffect
{
    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice)
    {
        foreach (var d in dice)
            DiceAnimation.Instance.Roll(d);

        // Wait for every die to stop rolling
        bool anyRolling = true;
        while (anyRolling)
        {
            anyRolling = false;
            foreach (var d in dice)
                if (d.isRolling) { anyRolling = true; break; }
            yield return null;
        }
    }

    public override string GetDescription() => "Reroll all dice.";
}
