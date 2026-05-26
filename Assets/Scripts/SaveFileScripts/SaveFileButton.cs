using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveFileButton : MonoBehaviour
{
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_Text pointText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private Image saveFileImage;
    [SerializeField] private TMP_Text saveFileButtonText;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private GameObject lockIcon;

    public SaveFileData saveFileData;
    public int saveSlot;
    public bool isLocked;

    private void Start() => UpdateSaveFileButton();

    public void UpdateSaveFileButton()
    {
        if (saveFileData == null)
        {
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);
            if (pointText != null) pointText.text = "Empty";
            if (dateText != null) dateText.text = "";
            if (livesText != null) livesText.text = "";
            if (saveFileImage != null) saveFileImage.sprite = defaultSprite;
            if (saveFileButtonText != null) saveFileButtonText.text = "New";
            if (lockIcon != null) lockIcon.SetActive(false);
        }
        else if (isLocked)
        {
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);
            if (pointText != null) pointText.text = saveFileData.fortunaPoints.ToString();
            if (dateText != null) dateText.text = saveFileData.dateSaved;
            if (livesText != null) livesText.text = $"Lives: {saveFileData.lives}";
            if (saveFileImage != null) saveFileImage.sprite = WorldManager.Instance.levels[saveFileData.currentLevelIndex > 0 ? saveFileData.currentLevelIndex - 1 : 0].levelImage;
            if (saveFileButtonText != null) saveFileButtonText.text = "Completed";
            if (lockIcon != null) lockIcon.SetActive(true);
        }
        else
        {
            if (deleteButton != null) deleteButton.gameObject.SetActive(true);
            if (pointText != null) pointText.text = saveFileData.fortunaPoints.ToString();
            if (dateText != null) dateText.text = saveFileData.dateSaved;
            if (livesText != null) livesText.text = $"Lives: {saveFileData.lives}";
            if (saveFileImage != null) saveFileImage.sprite = WorldManager.Instance.levels[saveFileData.currentLevelIndex].levelImage;
            if (saveFileButtonText != null) saveFileButtonText.text = "Load";
            if (lockIcon != null) lockIcon.SetActive(false);
        }
    }

    public void OnSaveFileButtonClick()
    {
        if (isLocked) return;
        if (saveFileData == null)
        {
            SaveFileManager.Instance.NewSave(saveSlot);
        }
        else
        {
            saveFileData = WorldManager.Instance.GetSaveData(saveSlot);
            if (saveFileData != null)
            {
                WorldManager.Instance.LoadSave(saveSlot);
                UpdateSaveFileButton();
            }
        }
    }

    public void DeleteButtonClick()
    {
        SaveFileManager.Instance.DeleteSave(saveSlot);
    }
}