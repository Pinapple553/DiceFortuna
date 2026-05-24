using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "ScriptableObjects/LevelData", order = 1)]
public class LevelData : ScriptableObject
{
	public string levelName;
	public int levelIndex;
	public string levelDescription;
	public Sprite levelImage;
	public Sprite lockedRewardIcon;
	public Sprite unlockedRewardIcon;

	public AIPlayer opponent;
	public int maxRoundItems;
	public int maxMatchItems;
	public int rounds;
}
