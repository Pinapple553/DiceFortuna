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
    [HideInInspector] public bool animationSkippable = false;
    [HideInInspector] public bool waitingForConfirm = false;
    [HideInInspector] public string confirmButtonLabel = "Continue";

    public int rounds = 2;
    public int maxRoundItems = 3;
    public int maxMatchItems = 6;

    private int currentLevelIndex;
    private bool itemExecuting = false;
    private bool playerRolled = false;
    private bool skipRequested = false;
    private bool confirmPressed = false;
    public PlayerBase currentRoller = null;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }
    private void Start()
    {
        currentLevelIndex = WorldManager.Instance.currentLevelIndex;
        LoadLevel();
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
        UIManager.Instance.UpdateRoundNumber(1, rounds);
        UIManager.Instance.UpdateUI();
    }
    public void StartButtonClick()
    {
        if (!roundRunning) StartMatch();
        UIManager.Instance.CloseRoundInfo(true);
    }
    public void SubmitButtonClick()
    {
        if (!DiceManager.Instance.diceSelected) { LockInDice(); return; } //lock-in dice

        if (waitingForConfirm)
        {
            confirmPressed = true;
            return;
        }

        if (animationSkippable) //skip roll/count
        {
            skipRequested = true;
            return;
        }

        if (waitingForPlayerRoll)//roll
        {
            playerRolled = true;
            return;
        }

        if (itemExecuting) return;
        if (!isPlayerItemTurn) return;

        if (pendingItem != null) //confirm item use
        {
            if (diceForItemSelection && selectedDiceIndices.Count == 0) return;
            ConfirmItemUse();
            return;
        }
        //else pass 
        ClearItemSelection();
        playerPassedItems = true;
        UIManager.Instance.RefreshItemPhaseUI();
    }
    public void PlayerClickItem(ItemInstance item)
    {
        if (!isPlayerItemTurn) return;
        if (!item.CanUse()) return;
        if (itemExecuting) return;

        if (pendingItem == item) //just deselect dice if its same item
        {
            ClearItemSelection();
            UIManager.Instance.RefreshItemPhaseUI();
            return;
        }
        //else use new item
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
        if (selectedDiceIndices.Contains(diceIndex)) selectedDiceIndices.Remove(diceIndex); //remove if selected

        else //remove a dice and select a new one if over the max
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
        List<int> targets = diceForItemSelection ? new List<int>(selectedDiceIndices) : AllDiceIndices(); //either all dice or player selected dice depending on item need
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
        yield return null;

        //skip reroll
        animationSkippable = true;
        UIManager.Instance.UpdateSubmitText();

        yield return new WaitUntil(() =>
        {
            if (skipRequested) { DiceAnimation.Instance.SkipAll(); skipRequested = false; return true; }
            return !AnyRolling(DiceManager.Instance.activeDiceList);
        });

        animationSkippable = false;
        skipRequested = false;

        //recount
        PlayerBase recountTarget = currentRoller ?? player;
        recountTarget.roundFortunaPoints = 0;
        yield return SkippableCount(recountTarget);

        // Fully done — only NOW allow the item phase loop to see we're finished
        itemExecuting = false;
        UIManager.Instance.RefreshItemPhaseUI();

        if (!player.CanUseItem()) playerPassedItems = true;
    }

    private IEnumerator SkippableCount(PlayerBase targetPlayer)
    {
        animationSkippable = true;
        UIManager.Instance.UpdateSubmitText();
        yield return new WaitForSeconds(1);

        bool countDone = false;
        StartCoroutine(RunCount(targetPlayer, () => countDone = true));

        yield return new WaitUntil(() =>
        {
            if (skipRequested)
            {
                skipRequested = false;
                int total = 0;
                foreach (var d in DiceManager.Instance.activeDiceList) total += d.data.sides[d.currentSideIndex].value;
                targetPlayer.roundFortunaPoints = total;
                UIManager.Instance.ShowRollResults();
                return true;
            }
            return countDone;
        });

        animationSkippable = false;
        UIManager.Instance.UpdateSubmitText();
    }
    private IEnumerator RunCount(PlayerBase targetPlayer, System.Action onDone)
    {
        yield return UIManager.Instance.CountAllDice(targetPlayer);
        yield return DiceManager.Instance.CountAllBonuses(targetPlayer);
        UIManager.Instance.ShowRollResults();
        onDone?.Invoke();
    }
    public void ClearItemSelection()
    {
        pendingItem = null;
        selectedDiceIndices.Clear();
        diceForItemSelection = false;
    }

    private List<int> AllDiceIndices() //selects all dice in circle
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
        yield return StartCoroutine(UIManager.Instance.PlayCoinAnimation(coinIsHeads));

        string coinResult = coinIsHeads ? "Heads" : "Tails";
        string firstTurn = playerGoesFirst ? "Your turn first" : "Opponent goes first";
        UIManager.Instance.LogMessage($"{coinResult} — {firstTurn}");

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(UIManager.Instance.ExitCoinAnimation());
        yield return new WaitForSeconds(1f);

        StartCoroutine(MatchRoutine());
    }

    private IEnumerator MatchRoutine()
    {
        for (int roll = 0; roll < rounds; roll++)
        {
            UIManager.Instance.UpdateRoundNumber(roll + 1, rounds);
            UIManager.Instance.LogMessage($"── Round {roll + 1} ──");

            PlayerBase first = playerGoesFirst ? (PlayerBase)player : ai;
            PlayerBase second = playerGoesFirst ? (PlayerBase)ai : player;
            bool firstIsPlayer = playerGoesFirst;

            player.roundFortunaPoints = 0;
            ai.roundFortunaPoints = 0;
            player.roundItemsUsed = 0;
            ai.roundItemsUsed = 0;

            //first roller
            yield return StartCoroutine(DoRoll(first, isPlayerRoll: firstIsPlayer));
            yield return StartCoroutine(ItemPhase(nonRollerIsPlayer: !firstIsPlayer));
            yield return StartCoroutine(WaitForConfirm("End Turn"));

            //second roller
            yield return StartCoroutine(DoRoll(second, isPlayerRoll: !firstIsPlayer));
            yield return StartCoroutine(ItemPhase(nonRollerIsPlayer: firstIsPlayer));

            //animate and add points 
            yield return StartCoroutine(UIManager.Instance.AnimateMatchScore(player, ai));
            player.matchFortunaPoints += player.roundFortunaPoints;
            ai.matchFortunaPoints += ai.roundFortunaPoints;
            player.roundFortunaPoints = 0;
            ai.roundFortunaPoints = 0;
            UIManager.Instance.UpdateAllInfo();

            //refresh shop
            if (roll < rounds - 1)
            {
                WorldManager.Instance.RefreshShopDice();
                yield return StartCoroutine(WaitForConfirm("Next Round"));
            }
        }

        yield return StartCoroutine(ResolveWinner());
    }
    private IEnumerator DoRoll(PlayerBase roller, bool isPlayerRoll)
    {
        diceRolling = true;
        currentRoller = roller;
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

        //roll & wait
        foreach (var d in DiceManager.Instance.activeDiceList) DiceAnimation.Instance.Roll(d);
        yield return null;
        animationSkippable = true;
        UIManager.Instance.UpdateSubmitText();
        yield return new WaitUntil(() =>
        {
            if (skipRequested) { DiceAnimation.Instance.SkipAll(); skipRequested = false; return true; }
            return !AnyRolling(DiceManager.Instance.activeDiceList);
        });
        animationSkippable = false;
        skipRequested = false;

        //count
        roller.roundFortunaPoints = 0;
        yield return SkippableCount(roller);

        int score = roller.roundFortunaPoints;
        if (isPlayerRoll) UIManager.Instance.LogMessage($"You scored {score}");
        else UIManager.Instance.LogMessage($"Opponent scored {score}");

        UIManager.Instance.ShowRollResults();
        UIManager.Instance.UpdateAllInfo();
        diceRolling = false;

        yield return new WaitForSeconds(0.3f);
    }
    private IEnumerator ItemPhase(bool nonRollerIsPlayer)
    {
        playerPassedItems = false;
        aiPassedItems = false;
        itemExecuting = false;

        //alternate players
        for (int i = 0; i < 100; i++)
        {
            if (nonRollerIsPlayer)
            {
                //player first
                if (!playerPassedItems)
                {
                    if (player.CanUseItem())
                    {
                        isPlayerItemTurn = true;
                        ClearItemSelection();
                        UIManager.Instance.LogMessage("Your item turn — use an item or pass");
                        UIManager.Instance.RefreshItemPhaseUI();
                        yield return new WaitUntil(() => (playerPassedItems || !player.CanUseItem()) && !itemExecuting);
                        isPlayerItemTurn = false;
                        ClearItemSelection();
                    }
                    else playerPassedItems = true;
                }

                if (!aiPassedItems)
                {
                    if (ai.CanUseItem()) yield return StartCoroutine(AIUseItem());
                    else aiPassedItems = true;
                }
            }
            else
            {
                //ai first
                if (!aiPassedItems)
                {
                    if (ai.CanUseItem()) yield return StartCoroutine(AIUseItem());
                    else aiPassedItems = true;
                }

                if (!playerPassedItems)
                {
                    if (player.CanUseItem())
                    {
                        isPlayerItemTurn = true;
                        ClearItemSelection();
                        UIManager.Instance.LogMessage("Your item turn — use an item or pass");
                        UIManager.Instance.RefreshItemPhaseUI();
                        yield return new WaitUntil(() => (playerPassedItems || !player.CanUseItem()) && !itemExecuting);
                        isPlayerItemTurn = false;
                        ClearItemSelection();
                    }
                    else playerPassedItems = true;
                }
            }
            if (playerPassedItems && aiPassedItems) break;
        }
        UIManager.Instance.SetTurnHighlight(null);
        UIManager.Instance.UpdateAllInfo();
    }
    private IEnumerator AIUseItem()
    {
        yield return new WaitForSeconds(1f);
        if (Random.value < 0.4f)
        {
            var available = ai.ownedItemsList.FindAll(i => i.CanUse()); //get all items ai can use
            if (available.Count > 0)
            {
                var chosen = available[Random.Range(0, available.Count)];
                chosen.TryUse();
                ai.roundItemsUsed++;
                ai.matchItemsUsed++;
                int maxT = Random.Range(0, chosen.Tier.diceTargets) + 1; //dosent always select max dice
                var targets = new List<int>();
                while (targets.Count < maxT)
                {
                    targets.Add(Random.Range(0, DiceManager.Instance.activeDiceList.Count)); // random dice
                }
                yield return chosen.data.effect.Apply(ai, DiceManager.Instance.activeDiceList, chosen.Tier, targets);
                yield return null;

                animationSkippable = true;
                UIManager.Instance.UpdateSubmitText();
                yield return new WaitUntil(() =>
                {
                    if (skipRequested) { DiceAnimation.Instance.SkipAll(); skipRequested = false; return true; }
                    return !AnyRolling(DiceManager.Instance.activeDiceList);
                });
                animationSkippable = false;
                skipRequested = false;

                ai.roundFortunaPoints = 0;
                yield return SkippableCount(currentRoller ?? ai);
                UIManager.Instance.LogMessage($"Opponent used {chosen.data.itemName}");
                yield break;
            }
            else
            {
                UIManager.Instance.LogMessage($"Opponent passed");
            }
        }
        aiPassedItems = true;
    }
    private IEnumerator WaitForConfirm(string label)
    {
        confirmPressed = false;
        confirmButtonLabel = label;
        waitingForConfirm = true;
        UIManager.Instance.UpdateSubmitText();
        yield return new WaitUntil(() => confirmPressed);
        waitingForConfirm = false;
        confirmPressed = false;
        UIManager.Instance.UpdateSubmitText();
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

        if (resultStatus == "Won" || resultStatus == "Lost") WorldManager.Instance.Save(currentLevelIndex, player.matchFortunaPoints, resultStatus);

        bool isLastLevel = currentLevelIndex >= WorldManager.Instance.levels.Length - 1;
        if (resultStatus == "Won" && isLastLevel) SceneManager.Instance.LoadScene("GameOver");
        else SceneManager.Instance.LoadScene("LevelPicker");
    }
    public void ExitToMenu()
    {
        roundRunning = false;
        SceneManager.Instance.LoadScene("LevelPicker");
    }
    public void RetryLevel()
    {
        roundRunning = false;
        SceneManager.Instance.LoadScene("DiceGame");
    }
    private bool AnyRolling(List<DiceInstance> list)
    {
        foreach (var d in list) if (d.isRolling) return true;
        return false;
    }
}