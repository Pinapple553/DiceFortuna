using UnityEngine;

public abstract class DiceSideEffect : ScriptableObject
{
    public abstract void ApplyEffect(DiceInstance dice);
}