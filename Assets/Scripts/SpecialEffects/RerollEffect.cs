using UnityEngine;

[CreateAssetMenu(menuName = "Dice/Effects/Reroll")]
public class RerollEffect : DiceSideEffect
{
    public override void ApplyEffect(Dice dice)
    {
        dice.RotateToFace(Random.Range(1, 7));
    }
}