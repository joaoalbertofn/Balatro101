using System.Collections.Generic;

namespace Balatro101.Game.Core;

public class Card(Suit suit, Rank rank) : AnimatableItem
{
    public Suit Suit { get; } = suit;
    public Rank Rank { get; } = rank;
    // Enhancements (Tarot effects)
    public int BonusChips { get; set; }
    public int BonusMult { get; set; }
    public bool IsFoil { get; set; }
    public bool IsHolographic { get; set; }
    public bool IsPolychrome { get; set; }

    public int GetValue() => Rank switch
    {
        Rank.Jack or Rank.Queen or Rank.King => 10,
        Rank.Ace => 11,
        _ => (int)Rank
    };

    public override string ToString() => $"{Rank} of {Suit}";
}

public record HandResult(HandType Type, int BaseChips, int BaseMultiplier, List<Card> ScoringCards);

public class ScoreState
{
    public int Chips { get; set; }
    public int Multiplier { get; set; }
}
