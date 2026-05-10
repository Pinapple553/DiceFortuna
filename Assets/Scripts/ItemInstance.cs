public class ItemInstance
{
    public ItemData data;
    public int currentSideIndex = 0;

    public int instanceId;
    private static int nextId = 0;

    public ItemInstance(ItemData data)
    {
        this.data = data;
        this.instanceId = nextId++;
    }
}