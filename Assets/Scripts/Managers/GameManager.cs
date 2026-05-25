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

    public int rounds = 2;
    public int maxRoundItems = 3;
    public int maxMatchItems = 6;

    private int currentLevelIndex;
    private bool itemExecuting = false;
    private bool playerRolled = false;

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
        WorldManager.Instance.player.ResetForNewMatch();
        WorldManager.Instance.levels[WorldManager.Instance.currentLevelIndex].opponent.ResetForNewMatch();
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

    public void SubmitButtonClick()
    {
        if (!DiceManager.Instance.diceSelected) { LockInDice(); return; }

        if (waitingForPlayerRoll)
        {
            playerRolled = true;
            return;
        }

        if (!isPlayerItemTurn) return;
        if (itemExecuting) return;

        if (pendingItem != null)
        {
            // Require at least 1 die selected when player-select mode is on
            if (diceForItemSelection && selectedDiceIndices.Count == 0) return;
            ConfirmItemUse();
            return;
        }

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

        UIManager.Instance.LogMessage($"Used: {item.data.itemName}");

        yield return item.data.effect.Apply(player, DiceManager.Instance.activeDiceList, item.Tier, targetIndices);
        
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => !AnyRolling(DiceManager.Instance.activeDiceList));

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
            UIManager.Instance.LogMessage("Select at least one die!");
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

        string coinResult = coinIsHeads ? "Heads" : "Tails";
        string firstTurn = playerGoesFirst ? "Your turn first" : "Opponent goes first";
        UIManager.Instance.LogMessage($"{coinResult} — {firstTurn}");

        yield return StartCoroutine(UIManager.Instance.PlayCoinAnimation(coinIsHeads));
        yield return StartCoroutine(UIManager.Instance.ExitCoinAnimation());

        StartCoroutine(MatchRoutine());
    }

    private IEnumerator MatchRoutine()
    {
        for (int roll = 0; roll < rounds; roll++)
        {
            UIManager.Instance.LogMessage($"── Round {roll + 1} ──");

            PlayerBase first = playerGoesFirst ? (PlayerBase)player : ai;
            PlayerBase second = playerGoesFirst ? (PlayerBase)ai : player;
            bool firstIsPlayer = playerGoesFirst;

            player.roundFortunaPoints = 0;
            ai.roundFortunaPoints = 0;
            player.roundItemsUsed = 0;
            ai.roundItemsUsed = 0;

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
            UIManager.Instance.LogMessage("Your turn — press Roll!");
            waitingForPlayerRoll = true;
            playerRolled = false;
            UIManager.Instance.UpdateSubmitText();
            yield return new WaitUntil(() => playerRolled);
            waitingForPlayerRoll = false;
        }
        else
        {
            UIManager.Instance.LogMessage("Opponent rolling...");
            yield return new WaitForSeconds(1f);
        }

        foreach (var d in DiceManager.Instance.activeDiceList)
            DiceAnimation.Instance.Roll(d);

        yield return new WaitUntil(() => !AnyRolling(DiceManager.Instance.activeDiceList));

        yield return UIManager.Instance.CountAllDice(roller);
        yield return DiceManager.Instance.CountAllBonuses(roller);

        int score = roller.roundFortunaPoints;
        if (isPlayerRoll) UIManager.Instance.LogMessage($"You scored {score}");
        else UIManager.Instance.LogMessage($"Opponent scored {score}");

        UIManager.Instance.ShowRollResults();
        UIManager.Instance.UpdateAllInfo();
        diceRolling = false;
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator ItemPhase()
    {
        playerPassedItems = false;
        aiPassedItems = false;
        itemExecuting = false;

        // Player and AI alternate using items until both pass or run out
        for (int i = 0; i < 100; i++)
        {
            // Player's turn to use an item
            if (!playerPassedItems)
            {
                if (player.CanUseItem())
                {
                    isPlayerItemTurn = true;
                    ClearItemSelection();
                    UIManager.Instance.ShowItemPhase(true);
                    UIManager.Instance.LogMessage("Your item turn — use an item or pass");
                    UIManager.Instance.RefreshItemPhaseUI();
                    yield return new WaitUntil(() => playerPassedItems || !player.CanUseItem());
                    isPlayerItemTurn = false;
                    ClearItemSelection();
                    UIManager.Instance.ShowItemPhase(false);
                }
                else
                {
                    playerPassedItems = true;
                }
            }

            // AI's turn to use an item
            if (!aiPassedItems)
            {
                if (ai.CanUseItem()) yield return StartCoroutine(AIUseItem());
                else aiPassedItems = true;
            }

            if (playerPassedItems && aiPassedItems) break;
        }

        UIManager.Instance.SetTurnHighlight(null);
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
                yield return new WaitUntil(() => !AnyRolling(DiceManager.Instance.activeDiceList));
                UIManager.Instance.LogMessage($"Opponent used {chosen.data.itemName}");
                yield break;
            }
        }
        aiPassedItems = true;
    }

    private IEnumerator ResolveWinner()
    {
        int playerScore = player.matchFortunaPoints;
        int aiScore = ai.matchFortunaPoints;
        bool playerWins = playerScore > aiScore;
        bool tie = playerScore == aiScore;

        string resultStatus;
        if (tie) { UIManager.Instance.LogMessage($"TIE! {playerScore} vs {aiScore}"); resultStatus = "Tied"; }
        else if (playerWins) { UIManager.Instance.LogMessage($"YOU WIN! {playerScore} vs {aiScore}"); resultStatus = "Won"; }
        else { UIManager.Instance.LogMessage($"YOU LOSE {playerScore} vs {aiScore}"); resultStatus = "Lost"; }

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