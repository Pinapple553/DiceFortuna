using UnityEngine;

public class Dice : MonoBehaviour
{
	[SerializeField]
	private string diceName;
	[SerializeField]
	private DiceSide[] sides;
	[SerializeField]
	private RollEffect effect;
	public Dice(string name, DiceSide[] sides)
	{
		this.diceName = name;
		this.sides = sides;
	}
	public DiceSide[] GetSides()
	{
		return sides;
	}
	public int GetSideCount()
	{
		return sides.Length;
	}
}