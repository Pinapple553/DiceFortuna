using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveFileData
{	
	public int fortunaPoints;
	public string dateSaved;
	public LevelInstance[] levels;
	public int currentLevelIndex;
	public List<int> ownedDiceIds;
	public List<int> ownedItemIds;
}
