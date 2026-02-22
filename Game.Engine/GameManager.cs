using System.Collections.Generic;
using System.Linq;
using Balatro101.Game.Core;
using Balatro101.Game.UI;

namespace Balatro101.Game.Engine;

public class GameManager
{
    public GameState CurrentState { get; set; } = GameState.MainMenu;

    public int Money { get; set; } = 4;
    public int CurrentAnte { get; set; } = 1;
    public int CurrentBlind { get; set; } = 1;
    public BlindType CurrentBlindType { get; set; } = BlindType.Small;
    public BossDebuffType CurrentBossDebuff { get; set; } = BossDebuffType.None;

    // Juice & Effects
    public float ShakeMagnitude { get; set; } = 0f;
    public float ShakeTimer { get; set; } = 0f;

    public int CurrentScore { get; set; }
    public int HandsLeft { get; set; }
    public int DiscardsLeft { get; set; }

    public Deck Deck { get; } = new();
    public List<Card> PlayerHand { get; } = new();
    public ScoringEngine ScoringEngine { get; } = new();

    public List<IJoker> AllJokers { get; }
    public List<IJoker> SelectedJokers { get; } = new();
    public List<IConsumable> Consumables { get; } = new();

    public string LastHandType { get; set; } = "";
    public int LastScorePlayed { get; set; }
    public HandResult? LastHandResult { get; set; }
    public ScoreState? LastScoreState { get; set; }

    public SortMode CurrentSortMode { get; private set; } = SortMode.None;

    public GameManager()
    {
        AllJokers = new List<IJoker>
        {
            new PairJoker(),
            new HeartsJoker(),
            new FixedJoker(),
            new FlushMultiplierJoker(),
            new SpadesJoker(),
            new HighCardJoker(),
            new CrazyJoker(),
            new LuckyJoker(),
            new RedJoker(),
            new SupremeJoker(),
            new ClubsJoker(),
            new DiamondsJoker(),
            new BaseMultiplierJoker(),
            new StraightJoker(),
            new TwoPairJoker(),
            new FaceCardJoker(),
            new BlackJoker(),
            new AcesJoker(),
            new FullHouseJoker(),
            new FourOfAKindJoker(),
            new EvenJoker()
        };
    }

    public void StartGame()
    {
        CurrentState = GameState.Shop;
        Money = 15; // Starting money for Ante 0 Build
        CurrentAnte = 1;
        CurrentBlind = 1;
        SelectedJokers.Clear();
        Consumables.Clear();
        ShopUI.InitializeShop(this); // Force shop population on start
    }

    public void RandomizeJokers()
    {
        var rnd = System.Random.Shared;
        for (int i = 0; i < AllJokers.Count; i++)
        {
            int swapIndex = rnd.Next(AllJokers.Count);
            (AllJokers[i], AllJokers[swapIndex]) = (AllJokers[swapIndex], AllJokers[i]);
        }
    }

    private float shuffleTimer = 0f;

    public void StartBlind()
    {
        CurrentState = GameState.Shuffling;
        shuffleTimer = 1.5f; // 1.5 seconds shuffle animation

        // Determine Blind Type
        CurrentBlindType = CurrentBlind switch
        {
            1 => BlindType.Small,
            2 => BlindType.Big,
            _ => BlindType.Boss
        };

        if (CurrentBlindType == BlindType.Boss)
        {
            // Pick a random debuff
            var debuffs = new[] { BossDebuffType.TheHook, BossDebuffType.TheClub, BossDebuffType.TheWindow, BossDebuffType.ThePillar };
            CurrentBossDebuff = debuffs[System.Random.Shared.Next(debuffs.Length)];
        }
        else
        {
            CurrentBossDebuff = BossDebuffType.None;
        }

        CurrentScore = 0;

        // Scale Target Score
        float mult = CurrentBlindType switch { BlindType.Small => 1.0f, BlindType.Big => 1.5f, BlindType.Boss => 2.0f, _ => 1.0f };
        GameConfig.TargetScore = (int)(300 * CurrentAnte * mult);

        HandsLeft = GameConfig.MaxHands;
        if (CurrentBossDebuff == BossDebuffType.ThePillar) HandsLeft = System.Math.Max(1, HandsLeft - 1);

        DiscardsLeft = GameConfig.MaxDiscards;
        if (CurrentBossDebuff == BossDebuffType.TheWindow) DiscardsLeft = System.Math.Max(0, DiscardsLeft / 2);

        ScoringEngine.ActiveJokers.Clear();
        ScoringEngine.ActiveJokers.AddRange(SelectedJokers);

        Deck.Initialize();

        PlayerHand.Clear();
    }

    public void UpdateShuffling(float dt)
    {
        shuffleTimer -= dt;
        if (shuffleTimer <= 0)
        {
            CurrentState = GameState.FaseGameplay;
            DrawCards();
        }
    }

    public void DrawCards()
    {
        bool drewAny = false;
        while (PlayerHand.Count < GameConfig.DefaultHandSize)
        {
            var card = Deck.Draw();
            if (card != null)
            {
                PlayerHand.Add(card);
                drewAny = true;
            }
            else
                break;
        }

        if (drewAny)
        {
            // sound removed
        }

        if (CurrentSortMode == SortMode.Rank) SortHandByRank();
        else if (CurrentSortMode == SortMode.Suit) SortHandBySuit();
    }

    public void DiscardSelected()
    {
        if (DiscardsLeft <= 0) return;

        var selectedCards = PlayerHand.Where(c => c.IsSelected).ToList();
        if (selectedCards.Count == 0 || selectedCards.Count > GameConfig.MaxSelectedCards) return;

        foreach (var card in selectedCards)
        {
            PlayerHand.Remove(card);
        }

        DiscardsLeft--;
        DrawCards();
    }

    public void PlayHand()
    {
        if (HandsLeft <= 0) return;

        var selectedCards = PlayerHand.Where(c => c.IsSelected).ToList();
        if (selectedCards.Count == 0 || selectedCards.Count > GameConfig.MaxSelectedCards) return;

        HandsLeft--;

        var handResult = HandEvaluator.Evaluate(selectedCards);
        LastScorePlayed = ScoringEngine.CalculateScore(handResult, CurrentBossDebuff, out var finalState);

        LastHandType = handResult.Type.ToString();
        LastHandResult = handResult;
        LastScoreState = finalState;

        if (CurrentBossDebuff == BossDebuffType.TheHook)
        {
            var rnd = System.Random.Shared;
            var remainingHand = PlayerHand.Where(c => !c.IsSelected).ToList();
            for (int i = 0; i < 2 && remainingHand.Count > 0; i++)
            {
                var target = remainingHand[rnd.Next(remainingHand.Count)];
                PlayerHand.Remove(target);
                remainingHand.Remove(target);
            }
        }

        CurrentState = GameState.AnimacaoPontuacao;
        ScoreAnimator.StartSequence(this, handResult, finalState, LastScorePlayed);
    }

    public void SortHandByRank()
    {
        CurrentSortMode = SortMode.Rank;
        PlayerHand.Sort((a, b) =>
        {
            int rankCmp = b.Rank.CompareTo(a.Rank);
            if (rankCmp != 0) return rankCmp;
            return a.Suit.CompareTo(b.Suit);
        });
    }

    public void SortHandBySuit()
    {
        CurrentSortMode = SortMode.Suit;
        PlayerHand.Sort((a, b) =>
        {
            int suitCmp = a.Suit.CompareTo(b.Suit);
            if (suitCmp != 0) return suitCmp;
            return b.Rank.CompareTo(a.Rank);
        });
    }

    public void FinishScoreAnimation()
    {
        if (CurrentScore >= GameConfig.TargetScore)
        {
            CurrentState = GameState.ShopTransition;
            Money += CurrentBlindType == BlindType.Boss ? 5 : (CurrentBlindType == BlindType.Big ? 4 : 3); // Base win reward
            Money += HandsLeft; // +1$ per remaining hand

            CurrentBlind++;
            if (CurrentBlind > 3)
            {
                CurrentBlind = 1;
                CurrentAnte++;
            }
        }
        else if (HandsLeft <= 0)
        {
            CurrentState = GameState.GameOver;
        }
        else
        {
            CurrentState = GameState.FaseGameplay;
            DrawCards();
        }
    }
}
