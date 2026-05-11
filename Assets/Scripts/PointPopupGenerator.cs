using System.Collections;
using TMPro;
using UnityEngine;

public class PointPopupGenerator : MonoBehaviour
{
    public static PointPopupGenerator Instance;
    [SerializeField] GameObject PopupContainer;
    [SerializeField] TMP_Text textPrefab;

    private void Awake()
    {
        Instance = this;
    }


    public void CreatePopUp(string text)
    {
        TMP_Text popUp = Instantiate(textPrefab, PopupContainer.transform);
        popUp.text = text;
        RectTransform rect = popUp.rectTransform;

        Vector2 randomOffset = new Vector2(Random.Range(-300f, 300f), Random.Range(-200f, 200f));
        Vector2 basePos = popUp.rectTransform.anchoredPosition;
        rect.anchoredPosition = basePos + randomOffset;

        StartCoroutine(AnimatePopup(popUp));
    }

    private IEnumerator AnimatePopup(TMP_Text popUp)
    {
        RectTransform rect = popUp.rectTransform;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 randomDir = new Vector2(Random.Range(-1f, 1f),Random.Range(0.8f, 1.5f)).normalized;

        float duration = 1f;
        float time = 0f;
        float startScale = 1f;
        float peakScale = 1.4f;

        while (time < duration)
        {
            float t = time / duration;

            float height = Mathf.Sin(t * Mathf.PI) * 80f;
            rect.anchoredPosition = startPos + randomDir * height;

            float scale;
            if (t < 0.2f)
            {
                scale = Mathf.Lerp(startScale, peakScale, t / 0.2f);
            }
            else
            {
                scale = Mathf.Lerp(peakScale, 0f, (t - 0.2f) / 0.8f);
            }
            rect.localScale = Vector3.one * scale;

            popUp.alpha = 1f - t;

            time += Time.deltaTime;
            yield return null;
        }

        Destroy(popUp.gameObject);
    }
}
