using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Create asset: Right-click → Create → Item → Effects → Add Bonus
// Directly adds a flat point bonus to the current round result.
[CreateAssetMenu(menuName = "Item/Effects/Add Bonus")]
public class AddBonusEffect : ItemEffect
{
    [SerializeField] private int bonusAmount = 5;

    public override IEnumerator Apply(PlayerBase target, List<DiceInstance> dice)
    {
        DiceManager.Instance.currentResult += bonusAmount;
        PointPopupGenerator.Instance.CreatePopUp($"+{bonusAmount}");
        UIManager.Instance.ShowRollResults();
        yield return null;
    }

    public override string GetDescription() => $"Add +{bonusAmount} to your current score.";
}
