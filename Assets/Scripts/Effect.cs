using UnityEngine;

[CreateAssetMenu(fileName = "NewEffect", menuName = "Dice/Effects")]
public class Effect : ScriptableObject
{
    [SerializeField] private string effectName;
	public void Apply(DiceInstance dice, float chance){
        EffectManager.Instance.ApplyEffect(effectName,dice,chance);
	}
}	