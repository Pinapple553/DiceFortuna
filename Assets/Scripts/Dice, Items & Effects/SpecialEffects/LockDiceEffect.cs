using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Create asset: Right-click → Create → Item → Effects → Lock Dice
// Locks the die at pendingItemTargetIndex — it keeps its current value for the next roll.
// DiceAnimation.Roll already checks isLocked and skips it (we add that flag to DiceInstance).
[CreateAssetMenu(menuName = "Item/Effects/Lock Dice")]
public class LockDiceEffect : ItemEffect
{
    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice)
    {
        int index = GameManager.Instance.pendingItemTargetIndex;
        if (index < 0 || index >= dice.Count) yield break;

        dice[index].isLocked = true;
        UIManager.Instance.UpdateUI();
        yield return null;
    }

    public override string GetDescription() => "Lock one die — it keeps its value next roll.";
}
