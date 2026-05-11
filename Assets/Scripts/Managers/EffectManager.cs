using System.Threading.Tasks;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    private void Awake()
    {
		if (Instance != null && Instance != this) Destroy(this.gameObject);
		else Instance = this;
	}
    public void ApplyEffect(string effectName, DiceInstance dice, float chance = 1)
    {
        switch (effectName)
        {
            case "Reroll":
                RerollEffect(dice, chance);
                break;
            default:
                Debug.LogWarning($"Effect '{effectName}' not recognized.");
                break;
		}
    }
    private void RerollEffect(DiceInstance dice, float chance){
        if(Random.value < chance){
			DiceAnimation.Instance.Roll(dice);
		}
	}
}