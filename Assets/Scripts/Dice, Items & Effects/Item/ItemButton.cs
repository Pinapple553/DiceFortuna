using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    public int instanceId;

    [Header("Item Info")]
    public ItemData itemData;
    public int amountOwned;

    private bool inItemPhase;
    private ItemInstance itemInstance;
    public void OnClick()
    {
        if (!inItemPhase || itemInstance == null) return;
        if (itemInstance.used) return;

        StartCoroutine(GameManager.Instance.PlayerUseItem(itemInstance));
    }
}
