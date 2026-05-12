using System.Collections;
using UnityEngine;

public abstract class ItemEffect : ScriptableObject
{
	public abstract IEnumerator Apply(PlayerBase target, System.Collections.Generic.List<DiceInstance> dice);
	public abstract string GetDescription();
}