using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoCard : MonoBehaviour
{
    public TMP_Text PointText;
    public TMP_Text ItemText;
    [SerializeField] private Image panelBackground;
    [SerializeField] private Color activeColor = new Color(1f, 0.9f, 0.5f, 1f);
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.3f);

    public void SetActive(bool isActive)
    {
        if (panelBackground != null)
            panelBackground.color = isActive ? activeColor : inactiveColor;
    }
}