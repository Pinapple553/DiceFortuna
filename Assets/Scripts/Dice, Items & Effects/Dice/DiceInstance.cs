using UnityEngine;

public class DiceInstance
{
	public DiceData data;
	public int currentSideIndex = 0;
	public bool isRolling = false;
	public bool isLocked = false;   // set by LockDiceEffect — skipped on next roll, then auto-cleared
	public int instanceId;
	private static int nextId = 0;

	public DiceInstance(DiceData data)
	{
		this.data = data;
		this.instanceId = nextId++;
	}
}