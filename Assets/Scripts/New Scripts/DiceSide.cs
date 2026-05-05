using UnityEngine;

[System.Serializable]
public class DiceSide
{
    public string name;
    public int value =0;
	public Sprite sprite;
    public Effect effect;

    public void Apply()
    {
        effect?.Apply();
    }
}