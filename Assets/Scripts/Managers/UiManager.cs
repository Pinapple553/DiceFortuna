using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Center")]
    [SerializeField] private TMP_Text rollPointsText;
    [SerializeField] private Button rollButton;
    [SerializeField] private TMP_Text rollButtonText;
    [SerializeField] private DiceButton[] diceDisplayButtons;
    [SerializeField] private Sprite emptyDiceSlot;

    [Header("ItemBar")]
    [SerializeField] private HorizontalLayoutGroup itemSelector;
    [SerializeField] private DiceButton diceButtonPrefab;
    [SerializeField] private ItemButton itemButtonPrefab;
    [SerializeField] private TMP_Text submitButtonText;

    [HideInInspector] private bool selectorShowDice = true;

    [Header("RoundInfo")]
    [SerializeField] private TMP_Text currentRoundText;
    [SerializeField] private PlayerInfoCard playerRoundInfo;
    [SerializeField] private PlayerInfoCard NPCRoundInfo;

    [Header("MatchInfo")]
    [SerializeField] private VerticalLayoutGroup matchEffects;
    [SerializeField] private PlayerInfoCard playerMatchInfo;
    [SerializeField] private PlayerInfoCard NPCMatchInfo;

    [Header("Panels")]
    [SerializeField] private GameObject roundInfoPanel;
    [SerializeField] private GameObject itemPhasePanel;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject roundEndPanel;

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
    private Coroutine hideMessageCoroutine;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }

    private void Start()
    {
        if (messagePanel != null) messagePanel.SetActive(false);
        if (coinFlipUI != null) coinFlipUI.SetActive(false);
        if (itemPhasePanel != null) itemPhasePanel.SetActive(false);
        if (roundEndPanel != null) roundEndPanel.SetActive(false);
        UpdateUI();
    }
    public void UpdateUI()
    {
        resetResult();
        UpdateRoundInfo();
        UpdateDiceSelector();
        UpdateDiceDisplay();
        UpdateSubmitButton();
    }
    private void UpdateSubmitButton()
    {
        if (submitButtonText == null) return;

        if (!DiceManager.Instance.diceSelected)
        {
            submitButtonText.text = "Lock In";
            return;
        }

        if (!GameManager.Instance.isPlayerItemTurn)
        {
            submitButtonText.text = "—";
            return;
        }

        if (GameManager.Instance.pendingItem != null)
        {
            if (GameManager.Instance.diceForItemSelection)
            {
                int needed = GameManager.Instance.pendingItem.Tier.diceTargets;
                int chosen = GameManager.Instance.selectedDiceIndices.Count;
                submitButtonText.text = chosen >= needed ? "Use Item" : $"Pick Dice ({chosen}/{needed})";
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
    public void UpdateDiceHighlights()
    {
        var selected = GameManager.Instance.selectedDiceIndices;
        for (int i = 0; i < diceDisplayButtons.Length; i++)
        {
            bool highlighted = selected.Contains(i);
            diceDisplayButtons[i].SetHighlight(highlighted);
        }
        UpdateSubmitButton();
    }

    public void ClearDiceHighlights()
    {
        for (int i = 0; i < diceDisplayButtons.Length; i++)
            diceDisplayButtons[i].SetHighlight(false);
    }
    private void UpdateDiceDisplay()
    {
        ClearDiceHighlights();
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
        if (GameManager.Instance.diceForItemSelection) UpdateDiceHighlights();
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
    public void UpdateRoundInfo()
    {
        if (playerRoundInfo != null)
            playerRoundInfo.PointText.text = GameManager.Instance.player.roundFortunaPoints.ToString();
        if (NPCRoundInfo != null)
            NPCRoundInfo.PointText.text = GameManager.Instance.ai.roundFortunaPoints.ToString();
    }

    public void UpdateMatchInfo()
    {
        if (playerMatchInfo != null)
            playerMatchInfo.PointText.text = GameManager.Instance.player.matchFortunaPoints.ToString();
        if (NPCMatchInfo != null)
            NPCMatchInfo.PointText.text = GameManager.Instance.ai.matchFortunaPoints.ToString();
    }
    public void resetResult()
    {
        if (rollPointsText != null) rollPointsText.text = "";
    }

    public void ShowRollResults()
    {
        if (rollPointsText != null) rollPointsText.text = GameManager.Instance.player.roundFortunaPoints.ToString();
    }
    public void CloseRoundInfo(bool close)
    {
        if (roundInfoPanel != null) roundInfoPanel.SetActive(!close);
    }

    public void ShowItemPhase(bool show)
    {
        if (itemPhasePanel != null) itemPhasePanel.SetActive(show);
        if (show) UpdateDiceSelector();
        UpdateSubmitButton();
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
    public void ShowMessage(string text)
    {
        if (hideMessageCoroutine != null) { StopCoroutine(hideMessageCoroutine); hideMessageCoroutine = null; }
        if (messagePanel == null || messageText == null) return;
        messageText.text = text;
        messagePanel.SetActive(true);
        hideMessageCoroutine = StartCoroutine(HideAfter(messagePanel, 2f));
    }

    private IEnumerator HideAfter(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null) panel.SetActive(false);
        hideMessageCoroutine = null;
    }
    public void ShowDiceInfo(DiceData dice)
    {
        if (diceInfoIcon != null) diceInfoIcon.sprite = dice.sides[0].sprite;
        if (diceInfoName != null) diceInfoName.text = dice.diceName;
        if (diceInfoDescription != null) diceInfoDescription.text = dice.description;
    }
    public IEnumerator CountAllDice()
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
                GameManager.Instance.player.roundFortunaPoints = result;
                rollPointsText.text = result.ToString();
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(0.2f);
            diceDisplayButtons[i].transform.localScale = Vector3.one;
        }
    }
    public IEnumerator ShowCombo(List<int> inDices, DiceCombo combo)
    {
        rollPointsText.text = combo.GetName();
        int bonus = combo.GetBonus();

        foreach (int i in inDices)
            yield return StartCoroutine(ScaleRoutine(diceDisplayButtons[i].transform, 1.5f));

        yield return new WaitForSeconds(0.3f);
        PointPopupGenerator.Instance.CreatePopUp(combo.GetName());

        int startValue = GameManager.Instance.player.roundFortunaPoints;
        for (int i = 0; i < bonus; i++)
        {
            startValue++;
            rollPointsText.text = startValue.ToString();
            yield return new WaitForSeconds(0.05f);
        }

        foreach (int i in inDices)
            diceDisplayButtons[i].transform.localScale = Vector3.one;

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator ScaleRoutine(Transform target, float scale)
    {
        target.localScale = Vector3.one * scale;
        yield return new WaitForSeconds(0.2f);
        target.localScale = Vector3.one;
    }
    //coinflip
    public IEnumerator StartCoinAnimation()
    {
        coinFlipAnimator.SetTrigger("Enter");
        yield return new WaitForSeconds(2f);
        coinFlipUI.SetActive(true);
    }

    public IEnumerator PlayCoinAnimation(bool heads)
    {
        coinFlipUI.SetActive(false);
        coinFlipAnimator.SetTrigger(heads ? "Enter" : "Flip");
        yield return new WaitForSeconds(1f);
    }

    public IEnumerator ExitCoinAnimation()
    {
        coinFlipAnimator.SetTrigger("Exit");
        yield return new WaitForSeconds(1f);
    }
}