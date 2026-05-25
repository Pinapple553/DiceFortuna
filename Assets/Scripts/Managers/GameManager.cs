using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public AIPlayer ai;
    public Player player;

    public static GameManager Instance;

    public bool roundRunning = false;
    public bool diceRolling = false;

    [HideInInspector] public bool isPlayerItemTurn = false;
    [HideInInspector] public bool playerPassedItems = false;
    [HideInInspector] public bool aiPassedItems = false;
    [HideInInspector] public ItemInstance pendingItem = null;
    [HideInInspector] public List<int> selectedDiceIndices = new List<int>();
    [HideInInspector] public bool diceForItemSelection = false;

    [HideInInspector] public bool playerChoseHeads = true;
    [HideInInspector] public bool playerGoesFirst = true;

    [HideInInspector] public bool waitingForPlayerRoll = false;
    private bool playerRolled = false;

    public int rounds = 2;
    public int maxRoundItems = 3;
    public int maxMatchItems = 6;

    private int currentLevelIndex;
    private bool itemExecuting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }

    private void Start()
    {
        currentLevelIndex = WorldManager.Instance.currentLevelIndex;
        LoadLevel();
        ResetItems();
        UIManager.Instance.CloseRoundInfo(false);
        WorldManager.Instance.IncrementLevelTries(currentLevelIndex);
    }

    private void LoadLevel()
    {
        LevelData level = WorldManager.Instance.levels[currentLevelIndex];
        if (level == null) return;
        rounds = level.rounds;
        maxRoundItems = level.maxRoundItems;
        maxMatchItems = level.maxMatchItems;
    }

    private void ResetItems()
    {
        foreach (ItemInstance i in player.ownedItemsList)
            i.ResetUses();
    }

    public void StartButtonClick()
    {
        if (!roundRunning) StartMatch();
        UIManager.Instance.CloseRoundInfo(true);
    }

    //handles: lock-in dice, roll, use item, pass
    public void SubmitButtonClick()
    {
        //select and lock in dice
        if (!DiceManager.Instance.diceSelected)
        {
            LockInDice();
            return;
        }

        //waiting for player to roll
        if (waitingForPlayerRoll)
        {
            playerRolled = true;
            return;
        }

        //item phase
        if (!isPlayerItemTurn) return;
        if (itemExecuting) return;

        if (pendingItem != null)
        {
            if (diceForItemSelection && selectedDiceIndices.Count < pendingItem.Tier.diceTargets) return;
            ConfirmItemUse();
            return;
        }

        //pass
        ClearItemSelection();
        playerPassedItems = true;
        UIManager.Instance.RefreshItemPhaseUI();
    }

    public void PlayerClickItem(ItemInstance item)
    {
        if (!isPlayerItemTurn) return;
        if (!item.CanUse()) return;
        if (itemExecuting) return;

        if (pendingItem == item)
        {
            ClearItemSelection();
            UIManager.Instance.RefreshItemPhaseUI();
            return;
        }

        ClearItemSelection();
        pendingItem = item;
        diceForItemSelection = item.data.playerSelectDice;
        UIManager.Instance.RefreshItemPhaseUI();
    }

    public void PlayerToggleDiceForItem(int diceIndex)
    {
        if (!diceForItemSelection || pendingItem == null) return;
        if (itemExecuting) return;

        int maxTargets = pendingItem.Tier.diceTargets;
        if (selectedDiceIndices.Contains(diceIndex))
            selectedDiceIndices.Remove(diceIndex);
        else
        {
            if (selectedDiceIndices.Count >= maxTargets) selectedDiceIndices.RemoveAt(0);
            selectedDiceIndices.Add(diceIndex);
        }

        UIManager.Instance.UpdateDiceHighlights();
        UIManager.Instance.UpdateSubmitText();
    }

    private void ConfirmItemUse()
    {
        if (pendingItem == null) return;
        List<int> targets = diceForItemSelection ? new List<int>(selectedDiceIndices) : AllDiceIndices();
        ItemInstance item = pendingItem;
        ClearItemSelection();
        StartCoroutine(ExecuteItemUse(item, targets));
    }

    private IEnumerator ExecuteItemUse(ItemInstance item, List<int> targetIndices)
    {
        if (!isPlayerItemTurn) yield break;
        if (!item.TryUse()) yield break;
        itemExecuting = true;

        player.roundItemsUsed++;
        player.matchItemsUsed++;

        yield return item.data.effect.Apply(player, DiceManager.Instance.activeDiceList, item.Tier, targetIndices);

        player.roundFortunaPoints = 0;
        yield return UIManager.Instance.CountAllDice(player);
        yield return DiceManager.Instance.CountAllBonuses(player);

        itemExecuting = false;
        UIManager.Instance.RefreshItemPhaseUI();

        if (!player.CanUseItem()) playerPassedItems = true;
    }

    public void ClearItemSelection()
    {
        pendingItem = null;
        selectedDiceIndices.Clear();
        diceForItemSelection = false;
    }

    private List<int> AllDiceIndices()
    {
        var list = new List<int>();
        for (int i = 0; i < DiceManager.Instance.activeDiceList.Count; i++) list.Add(i);
        return list;
    }

    public void StartMatch()
    {
        roundRunning = true;
        DiceManager.Instance.ResetMatch();
        UIManager.Instance.UpdateUI();
    }

    private void LockInDice()
    {
        AISelectDice();
        if (!DiceManager.Instance.SelectDice())
        {
            UIManager.Instance.ShowMessage("Select at least one die!");
            return;
        }
        UIManager.Instance.SelectorMode("item");
        StartCoroutine(CoinFlipRoutine());
    }

    private void AISelectDice()
    {
        ai.selectedDiceList.Clear();
        var pool = new List<DiceData>(ai.ownedDiceList);
        int count = Mathf.Min(ai.maxDice, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            ai.selectedDiceList.Add(new DiceInstance(pool[idx]));
            pool.RemoveAt(idx);
        }
    }

    public void PlayerChoseCoinSide(bool heads)
    {
        playerChoseHeads = heads;
        UIManager.Instance.coinFlipResolved = true;
    }

    private IEnumerator CoinFlipRoutine()
    {
        UIManager.Instance.coinFlipResolved = false;
        yield return StartCoroutine(UIManager.Instance.StartCoinAnimation());
        yield return new WaitUntil(() => UIManager.Instance.coinFlipResolved);

        bool coinIsHeads = Random.value >= 0.5f;
        playerGoesFirst = (coinIsHeads == playerChoseHeads);
        yield return StartCoroutine(UIManager.Instance.PlayCoinAnimation(coinIsHeads));
        yield return StartCoroutine(UIManager.Instance.ExitCoinAnimation());

        StartCoroutine(MatchRoutine());
    }

    private IEnumerator MatchRoutine()
    {
        for (int roll = 0; roll < rounds; roll++)
        {
            PlayerBase first = playerGoesFirst ? (PlayerBase)player : ai;
            PlayerBase second = playerGoesFirst ? (PlayerBase)ai : player;
            bool firstIsPlayer = playerGoesFirst;

            first.roundFortunaPoints = 0;
            second.roundFortunaPoints = 0;

            yield return StartCoroutine(DoRoll(first, isPlayerRoll: firstIsPlayer));
            yield return StartCoroutine(ItemPhase());

            yield return StartCoroutine(DoRoll(second, isPlayerRoll: !firstIsPlayer));
            yield return StartCoroutine(ItemPhase());

            yield return StartCoroutine(UIManager.Instance.AnimateMatchScore(player, ai));

            player.matchFortunaPoints += player.roundFortunaPoints;
            ai.matchFortunaPoints += ai.roundFortunaPoints;
            player.roundFortunaPoints = 0;
            ai.roundFortunaPoints = 0;
            UIManager.Instance.UpdateAllInfo();
        }

        yield return StartCoroutine(ResolveWinner());
    }

    private IEnumerator DoRoll(PlayerBase roller, bool isPlayerRoll)
    {
        diceRolling = true;
        UIManager.Instance.resetResult();
        UIManager.Instance.SetTurnHighlight(isPlayerRoll);

        if (isPlayerRoll)
        {
            //wait for player to press Roll button
            waitingForPlayerRoll = true;
            playerRolled = false;
            UIManager.Instance.UpdateSubmitText();
            yield return new WaitUntil(() => playerRolled);
            waitingForPlayerRoll = false;
        }
        else
        {
            UIManager.Instance.ShowMessage("Opponent rolling...");
            yield return new WaitForSeconds(1f);
        }

        foreach (var d in DiceManager.Instance.activeDiceList)
            DiceAnimation.Instance.Roll(d);

        yield return new WaitUntil(() => !AnyRolling(DiceManager.Instance.activeDiceList));

        yield return UIManager.Instance.CountAllDice(roller);
        yield return DiceManager.Instance.CountAllBonuses(roller);

        UIManager.Instance.ShowRollResults();
        UIManager.Instance.UpdateAllInfo();
        diceRolling = false;
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ItemPhase()
    {
        playerPassedItems = false;
        aiPassedItems = false;
        player.roundItemsUsed = 0;
        itemExecuting = false;

        for (int i = 0; i < 100; i++)
        {
            if (!playerPassedItems && player.CanUseItem())
            {
                isPlayerItemTurn = true;
                ClearItemSelection();
                UIManager.Instance.ShowItemPhase(true);
                UIManager.Instance.RefreshItemPhaseUI();
                yield return new WaitUntil(() => playerPassedItems || !player.CanUseItem());
                isPlayerItemTurn = false;
                ClearItemSelection();
            }
            else playerPassedItems = true;

            if (!aiPassedItems && ai.CanUseItem()) yield return StartCoroutine(AIUseItem());
            else aiPassedItems = true;

            if (playerPassedItems && aiPassedItems) break;
        }

        UIManager.Instance.ShowItemPhase(false);
        UIManager.Instance.SetTurnHighlight(false);
        UIManager.Instance.UpdateAllInfo();
    }

    private IEnumerator AIUseItem()
    {
        yield return new WaitForSeconds(0.8f);

        if (Random.value < 0f)
        {
            var available = ai.ownedItemsList.FindAll(i => i.CanUse());
            if (available.Count > 0)
            {
                var chosen = available[Random.Range(0, available.Count)];
                chosen.TryUse();
                ai.roundItemsUsed++;
                ai.matchItemsUsed++;
                int maxT = chosen.Tier.diceTargets;
                var targets = new List<int>();
                for (int i = 0; i < DiceManager.Instance.activeDiceList.Count && targets.Count < maxT; i++) targets.Add(i);
                yield return chosen.data.effect.Apply(ai, DiceManager.Instance.activeDiceList, chosen.Tier, targets);
                yield break;
            }
        }
        aiPassedItems = true;
    }

    private IEnumerator ResolveWinner()
    {
        int playerScore = player.matchFortunaPoints;
        int aiScore = ai.matchFortunaPoints;
        bool tie = playerScore == aiScore;
        bool playerWins = playerScore > aiScore;

        string resultStatus;
        if (tie) { UIManager.Instance.ShowMessage("TIE!"); resultStatus = "Tied"; }
        else if (playerWins) { UIManager.Instance.ShowMessage($"YOU WIN! {playerScore} vs {aiScore}"); resultStatus = "Won"; }
        else { UIManager.Instance.ShowMessage($"YOU LOSE {playerScore} vs {aiScore}"); resultStatus = "Lost"; }

        UIManager.Instance.UpdateAllInfo();
        roundRunning = false;

        yield return new WaitForSeconds(2f);
        UIManager.Instance.roundFinished = false;
        UIManager.Instance.ShowRoundEndPanel();
        yield return new WaitUntil(() => UIManager.Instance.roundFinished);

        if (resultStatus == "Won" || resultStatus == "Lost")
            WorldManager.Instance.Save(currentLevelIndex, player.matchFortunaPoints, resultStatus);

        bool isLastLevel = currentLevelIndex >= WorldManager.Instance.levels.Length - 1;
        if (resultStatus == "Won" && isLastLevel) SceneManager.Instance.LoadScene("GameOver");
        else SceneManager.Instance.LoadScene("LevelPicker");
    }

    public void ExitToMenu()
    {
        roundRunning = false;
        SceneManager.Instance.LoadScene("LevelPicker");
    }

    private bool AnyRolling(List<DiceInstance> list)
    {
        foreach (var d in list) if (d.isRolling) return true;
        return false;
    }
}