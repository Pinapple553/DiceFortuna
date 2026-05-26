using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Center")]
    [SerializeField] private TMP_Text rollPointsText;
    [SerializeField] private DiceButton[] diceDisplayButtons;
    [SerializeField] private Sprite emptyDiceSlot;

    [Header("ItemBar")]
    [SerializeField] private HorizontalLayoutGroup itemSelector;
    [SerializeField] private DiceButton diceButtonPrefab;
    [SerializeField] private ItemButton itemButtonPrefab;
    [SerializeField] private TMP_Text submitButtonText;

    private bool selectorShowDice = true;

    [Header("RoundInfo")]
    [SerializeField] private PlayerInfoCard playerRoundInfo;
    [SerializeField] private PlayerInfoCard NPCRoundInfo;

    [Header("MatchInfo")]
    [SerializeField] private PlayerInfoCard playerMatchInfo;
    [SerializeField] private PlayerInfoCard NPCMatchInfo;

    [Header("Panels")]
    [SerializeField] private GameObject roundInfoPanel;
    [SerializeField] private GameObject itemPhasePanel;
    [SerializeField] private GameObject roundEndPanel;

    [Header("Message Log")]
    [SerializeField] private ScrollRect messageScrollRect;
    [SerializeField] private Transform messageLogContent;
    [SerializeField] private TMP_Text messageLogEntryPrefab;
    [SerializeField] private int maxLogEntries = 50;

    [Header("CoinFlip")]
    [SerializeField] private GameObject coinFlipUI;
    [SerializeField] private Animator coinFlipAnimator;

    [Header("DiceInfo")]
    [SerializeField] private Image diceInfoIcon;
    [SerializeField] private TMP_Text diceInfoName;
    [SerializeField] private TMP_Text diceInfoDescription;

    [HideInInspector] public bool coinFlipResolved = false;
    [HideInInspector] public bool roundFinished = false;

    public static UIManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (coinFlipUI != null) coinFlipUI.SetActive(false);
        if (itemPhasePanel != null) itemPhasePanel.SetActive(false);
        if (roundEndPanel != null) roundEndPanel.SetActive(false);
        UpdateUI();
    }

    public void UpdateUI()
    {
        resetResult();
        UpdateAllInfo();
        UpdateDiceSelector();
        RebuildDiceDisplay();
        UpdateSubmitText();
    }
    public void RefreshItemPhaseUI()
    {
        UpdateAllInfo();
        RefreshDiceIcons();

        if (GameManager.Instance.diceForItemSelection) UpdateDiceHighlights();
        else ClearDiceHighlights();
       
        RefreshItemButtons();
        UpdateSubmitText();
    }
    public void RefreshDiceIcons()
    {
        for (int i = 0; i < diceDisplayButtons.Length; i++)
        {
            if (i >= DiceManager.Instance.activeDiceList.Count)
            {
                diceDisplayButtons[i].SetIcon(emptyDiceSlot);
                diceDisplayButtons[i].dice = null;
                diceDisplayButtons[i].activeDiceIndex = -1;
            }
            else
            {
                var inst = DiceManager.Instance.activeDiceList[i];
                diceDisplayButtons[i].SetIcon(inst.data.sides[inst.currentSideIndex].sprite);
                diceDisplayButtons[i].dice = inst.data;
                diceDisplayButtons[i].instanceId = inst.instanceId;
                diceDisplayButtons[i].activeDiceIndex = i;
            }
        }
    }

    //submit button
    public void UpdateSubmitText()
    {
        if (submitButtonText == null) return;

        if (!DiceManager.Instance.diceSelected)
        {
            submitButtonText.text = "Lock In";
            return;
        }
        if (GameManager.Instance.waitingForPlayerRoll)
        {
            submitButtonText.text = "Roll!";
            return;
        }
        if (!GameManager.Instance.isPlayerItemTurn)
        {
            submitButtonText.text = "...";
            return;
        }
        if (GameManager.Instance.pendingItem != null)
        {
            if (GameManager.Instance.diceForItemSelection)
            {
                int max = GameManager.Instance.pendingItem.Tier.diceTargets;
                int chosen = GameManager.Instance.selectedDiceIndices.Count;
                submitButtonText.text = chosen > 0 ? $"Use Item ({chosen}/{max})" : $"Pick Dice (0/{max})";
            }
            else
            {
                submitButtonText.text = "Use Item";
            }
        }
        else
        {
            submitButtonText.text = "Pass";
        }
    }
    //highlights
    public void UpdateDiceHighlights()
    {
        var selected = GameManager.Instance.selectedDiceIndices;
        for (int i = 0; i < diceDisplayButtons.Length; i++)
            diceDisplayButtons[i].SetHighlight(selected.Contains(i));
        UpdateSubmitText();
    }

    public void ClearDiceHighlights()
    {
        for (int i = 0; i < diceDisplayButtons.Length; i++)
            diceDisplayButtons[i].SetHighlight(false);
    }
    private void RebuildDiceDisplay()
    {
        ClearDiceHighlights();
        RefreshDiceIcons();
    }

    //item selector
    private void RefreshItemButtons()
    {
        foreach (Transform child in itemSelector.transform)
        {
            var btn = child.GetComponent<ItemButton>();
            if (btn != null) btn.UpdateButtonUI();
        }
    }

    private void UpdateDiceSelector()
    {
        foreach (Transform child in itemSelector.transform)
            Destroy(child.gameObject);

        if (selectorShowDice)
        {
            var uniqueDice = new List<DiceData>();
            foreach (DiceData dice in GameManager.Instance.player.ownedDiceList)
            {
                if (uniqueDice.Contains(dice)) continue;
                uniqueDice.Add(dice);
                var btn = Instantiate(diceButtonPrefab, itemSelector.transform);
                btn.dice = dice;
                btn.amountOwned = GameManager.Instance.player.GetDiceAmount(dice);
                btn.amountSelected = GameManager.Instance.player.GetSelectedDiceAmout(dice);
                btn.UpdateButtonUI();
            }
        }
        else
        {
            foreach (var item in GameManager.Instance.player.ownedItemsList)
            {
                var btn = Instantiate(itemButtonPrefab, itemSelector.transform);
                btn.item = item;
                btn.UpdateButtonUI();
            }
        }
    }

    public void SelectorMode(string mode)
    {
        selectorShowDice = (mode == "dice");
        UpdateDiceSelector();
    }
    public void UpdateAllInfo()
    {
        if (playerRoundInfo != null)
        {
            playerRoundInfo.PointText.text = GameManager.Instance.player.roundFortunaPoints.ToString();
            playerRoundInfo.ItemText.text = $"{GameManager.Instance.player.roundItemsUsed}/{GameManager.Instance.maxRoundItems}";
        }
        if (playerMatchInfo != null)
        {
            playerMatchInfo.PointText.text = GameManager.Instance.player.matchFortunaPoints.ToString();
            playerMatchInfo.ItemText.text = $"{GameManager.Instance.player.matchItemsUsed}/{GameManager.Instance.maxMatchItems}";
        }
        if (NPCRoundInfo != null)
        {
            NPCRoundInfo.PointText.text = GameManager.Instance.ai.roundFortunaPoints.ToString();
            NPCRoundInfo.ItemText.text = $"{GameManager.Instance.ai.roundItemsUsed}/{GameManager.Instance.maxRoundItems}";
        }
        if (NPCMatchInfo != null)
        {
            NPCMatchInfo.PointText.text = GameManager.Instance.ai.matchFortunaPoints.ToString();
            NPCMatchInfo.ItemText.text = $"{GameManager.Instance.ai.matchItemsUsed}/{GameManager.Instance.maxMatchItems}";
        }
    }

    public void SetTurnHighlight(bool? playerTurn)
    {
        bool p = playerTurn == true;
        bool a = playerTurn == false;
        if (playerRoundInfo != null) playerRoundInfo.SetActive(p);
        if (NPCRoundInfo != null) NPCRoundInfo.SetActive(a);
        if (playerMatchInfo != null) playerMatchInfo.SetActive(p);
        if (NPCMatchInfo != null) NPCMatchInfo.SetActive(a);
    }

    public void LogMessage(string text)
    {
        if (messageLogContent == null || messageLogEntryPrefab == null) return;

        while (messageLogContent.childCount >= maxLogEntries) Destroy(messageLogContent.GetChild(0).gameObject);

        var entry = Instantiate(messageLogEntryPrefab, messageLogContent);
        entry.text = text;

        StartCoroutine(ScrollToBottom());
    }
    private IEnumerator ScrollToBottom()
    {
        yield return null;
        if (messageScrollRect != null) messageScrollRect.verticalNormalizedPosition = 0f;
    }

    public void resetResult()
    {
        if (rollPointsText != null) rollPointsText.text = "";
    }

    public void ShowRollResults()
    {
        if (rollPointsText != null) rollPointsText.text = GameManager.Instance.currentRoller.roundFortunaPoints.ToString();
    }

    public IEnumerator AnimateMatchScore(PlayerBase p, PlayerBase ai)
    {
        int pStart = p.matchFortunaPoints;
        int aStart = ai.matchFortunaPoints;
        int steps = Mathf.Max(p.roundFortunaPoints, ai.roundFortunaPoints);

        for (int i = 1; i <= steps; i++)
        {
            int pVal = pStart + Mathf.Min(i, p.roundFortunaPoints);
            int aVal = aStart + Mathf.Min(i, ai.roundFortunaPoints);
            if (playerMatchInfo != null) playerMatchInfo.PointText.text = pVal.ToString();
            if (NPCMatchInfo != null) NPCMatchInfo.PointText.text = aVal.ToString();
            yield return new WaitForSeconds(0.04f);
        }

        if (playerMatchInfo != null) playerMatchInfo.PointText.text = (pStart + p.roundFortunaPoints).ToString();
        if (NPCMatchInfo != null) NPCMatchInfo.PointText.text = (aStart + ai.roundFortunaPoints).ToString();
        yield return new WaitForSeconds(0.3f);
    }

    public IEnumerator CountAllDice(PlayerBase targetPlayer)
    {
        int result = 0;
        for (int i = 0; i < diceDisplayButtons.Length; i++)
        {
            if (i >= DiceManager.Instance.activeDiceList.Count) break;

            yield return StartCoroutine(ScaleRoutine(diceDisplayButtons[i].transform, 1.5f));
            var instance = DiceManager.Instance.activeDiceList[i];
            int value = instance.data.sides[instance.currentSideIndex].value;
            PointPopupGenerator.Instance.CreatePopUp(value.ToString());

            for (int j = 0; j < value; j++)
            {
                result++;
                targetPlayer.roundFortunaPoints = result;
                if (rollPointsText != null) rollPointsText.text = result.ToString();
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(0.2f);
            diceDisplayButtons[i].transform.localScale = Vector3.one;
        }
    }

    public IEnumerator ShowCombo(List<int> inDices, DiceCombo combo)
    {
        if (rollPointsText != null) rollPointsText.text = combo.GetName();
        int bonus = combo.GetBonus();

        foreach (int i in inDices)
            yield return StartCoroutine(ScaleRoutine(diceDisplayButtons[i].transform, 1.5f));

        yield return new WaitForSeconds(0.3f);
        PointPopupGenerator.Instance.CreatePopUp(combo.GetName());

        int startValue = GameManager.Instance.currentRoller.roundFortunaPoints;
        for (int i = 0; i < bonus; i++)
        {
            startValue++;
            if (rollPointsText != null) rollPointsText.text = startValue.ToString();
            yield return new WaitForSeconds(0.05f);
        }

        foreach (int i in inDices) diceDisplayButtons[i].transform.localScale = Vector3.one;

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator ScaleRoutine(Transform target, float scale)
    {
        target.localScale = Vector3.one * scale;
        yield return new WaitForSeconds(0.2f);
        target.localScale = Vector3.one;
    }

    //panels
    public void CloseRoundInfo(bool close)
    {
        if (roundInfoPanel != null) roundInfoPanel.SetActive(!close);
    }
    public void ShowItemPhase(bool show)
    {
        if (itemPhasePanel != null) itemPhasePanel.SetActive(show);
        if (show)
        {
            selectorShowDice = false;
            UpdateDiceSelector();
        }
        UpdateSubmitText();
    }
    public void ShowRoundEndPanel()
    {
        if (roundEndPanel != null) roundEndPanel.SetActive(true);
    }
    public void OnFinishClicked()
    {
        if (roundEndPanel != null) roundEndPanel.SetActive(false);
        roundFinished = true;
    }
    public void ShowDiceInfo(DiceData dice)
    {
        if (diceInfoIcon != null) diceInfoIcon.sprite = dice.sides[0].sprite;
        if (diceInfoName != null) diceInfoName.text = dice.diceName;
        if (diceInfoDescription != null) diceInfoDescription.text = dice.description;
    }
    //coin flip
    public IEnumerator StartCoinAnimation()
    {
        coinFlipAnimator.SetTrigger("Enter");
        yield return new WaitForSeconds(1f);
        coinFlipUI.SetActive(true);
    }
    public IEnumerator PlayCoinAnimation(bool heads)
    {
        coinFlipUI.SetActive(false);
        coinFlipAnimator.SetTrigger(heads ? "Enter" : "Flip"); //change to heads & tails animations!!!!
        yield return new WaitForSeconds(1f);
    }
    public IEnumerator ExitCoinAnimation()
    {
        coinFlipAnimator.SetTrigger("Exit");
        yield return new WaitForSeconds(1f);
    }
}