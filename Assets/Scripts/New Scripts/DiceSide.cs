using UnityEngine;

public class DiceSide : MonoBehaviour
{
	[SerializeField]
    private string sideName;
	[SerializeField]
	private int value;
	[SerializeField]
	private RollEffect effect;

	public DiceSide(string name, int value)
	{
		this.sideName = name;
		this.value = value;
	}
}
