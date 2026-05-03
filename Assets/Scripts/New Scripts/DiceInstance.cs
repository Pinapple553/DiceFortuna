public class DiceInstance
{
    public DiceData data;
    public int currentSideIndex = 0;

    public int instanceId;
    private static int nextId = 0;

    public DiceInstance(DiceData data)
    {
        this.data = data;
        this.instanceId = nextId++;
    }
}