using System.Collections.Generic;

namespace Balatro101.Game.Core;

public class Card(Suit suit, Rank rank)
{
    public Suit Suit { get; } = suit;
    public Rank Rank { get; } = rank;
    // Enhancements (Tarot effects)
    public int BonusChips { get; set; }
    public int BonusMult { get; set; }
    public bool IsFoil { get; set; }
    public bool IsHolographic { get; set; }
    public bool IsPolychrome { get; set; }

    // UI properties
    public float X { get; set; }
    public float TargetX { get; set; }
    public float VelocityX { get; set; }

    public float Y { get; set; }
    public float TargetY { get; set; }
    public float VelocityY { get; set; }

    public float Rotation { get; set; }
    public float TargetRotation { get; set; }
    public float VelocityRotation { get; set; }

    public float Scale { get; set; } = 1f;
    public float TargetScale { get; set; } = 1f;
    public float VelocityScale { get; set; }

    public bool IsSelected { get; set; }
    public bool IsHovered { get; set; }

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
