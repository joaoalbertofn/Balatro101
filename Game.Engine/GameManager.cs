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
        var rnd = new System.Random();
        for (int i = 0; i < AllJokers.Count; i++)
        {
            int swapIndex = rnd.Next(AllJokers.Count);
            (AllJokers[i], AllJokers[swapIndex]) = (AllJokers[swapIndex], AllJokers[i]);
        }
    }

    public void StartBlind()
    {
        CurrentState = GameState.FaseGameplay;
        CurrentScore = 0;
        GameConfig.TargetScore = 300 * CurrentAnte; // Scale Difficulty
        HandsLeft = GameConfig.MaxHands;
        DiscardsLeft = GameConfig.MaxDiscards;

        ScoringEngine.ActiveJokers.Clear();
        ScoringEngine.ActiveJokers.AddRange(SelectedJokers);

        Deck.Initialize();
        PlayerHand.Clear();
        DrawCards();
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
        LastScorePlayed = ScoringEngine.CalculateScore(handResult, out var finalState);

        LastHandType = handResult.Type.ToString();
        LastHandResult = handResult;
        LastScoreState = finalState;

        // Remove played cards
        foreach (var card in selectedCards)
        {
            PlayerHand.Remove(card);
        }

        CurrentScore += LastScorePlayed;
        CurrentState = GameState.AnimacaoPontuacao;
    }

    public void SortHandByRank()
    {
        CurrentSortMode = SortMode.Rank;
        var sorted = PlayerHand.OrderByDescending(c => c.Rank).ThenBy(c => c.Suit).ToList();
        PlayerHand.Clear();
        PlayerHand.AddRange(sorted);
    }

    public void SortHandBySuit()
    {
        CurrentSortMode = SortMode.Suit;
        var sorted = PlayerHand.OrderBy(c => c.Suit).ThenByDescending(c => c.Rank).ToList();
        PlayerHand.Clear();
        PlayerHand.AddRange(sorted);
    }

    public void FinishScoreAnimation()
    {
        if (CurrentScore >= GameConfig.TargetScore)
        {
            CurrentState = GameState.ShopTransition;
            Money += 3; // Base win reward
            Money += HandsLeft; // +1$ per remaining hand
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
