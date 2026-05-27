using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adds a flat bonus to the current roller's round score.
[CreateAssetMenu(menuName = "Item/Effects/BonusPoints")]
public class BonusPointsEffect : ItemEffect
{
    [SerializeField] private int bonusPerTier = 5; // base bonus; multiply by (tier+1) so it scales

    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice, ItemTier tier, List<int> targetIndices)
    {
        // diceTargets doubles as the flat bonus amount at this tier
        int bonus = tier.diceTargets;
        target.roundFortunaPoints += bonus;
        PointPopupGenerator.Instance.CreatePopUp($"+{bonus}");
        UIManager.Instance.ShowRollResults();
        yield break;
    }

    public override string GetDescription(ItemTier tier) =>
        $"Immediately add {tier.diceTargets} Fortuna Points to your score. ({tier.uses} uses per match)";
}
