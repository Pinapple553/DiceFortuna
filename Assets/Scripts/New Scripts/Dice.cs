using UnityEngine;

public class Dice : MonoBehaviour
{
    [SerializeField] private DiceData data;

    public DiceSide Roll()
    {
        int index = Random.Range(0, data.sides.Length);
        return data.sides[index];
    }
	public DiceSide[] GetSides()
	{
		return data.sides;
	}
	public int GetSideCount()
	{
		return data.sides.Length;
	}
}