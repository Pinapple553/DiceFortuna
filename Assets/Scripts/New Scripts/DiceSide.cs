using UnityEngine;

[System.Serializable]
public class DiceSide
{
    public string name;
    public Sprite sprite;
    public RollEffect effect;

    public void Apply()
    {
        effect?.Apply();
    }
}