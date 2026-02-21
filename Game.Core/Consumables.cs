using System.Collections.Generic;

namespace Balatro101.Game.Core;

public enum ConsumableType
{
    Tarot,
    Planet
}

public interface IConsumable
{
    string Name { get; }
    string Description { get; }
    int Cost { get; }
    ConsumableType Type { get; }
}

public interface ITarot : IConsumable
{
    void ApplyToCard(Card card);
}

public interface IPlanet : IConsumable
{
    void ApplyToHand(HandType handType, ref int baseChips, ref int baseMult);
    HandType TargetHand { get; }
}

// Basic Implementations
public class TheFool : AnimatableItem, ITarot
{
    public string Name => "The Fool";
    public string Description => "Enhances card to give +10 Chips.";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Tarot;

    public void ApplyToCard(Card card)
    {
        // Simple mock of enhancement: we just set a property, but we don't have enhanced cards yet.
        // For now, in Balatro101, we might just need to add a Bonus property to Card.
    }
}

public class Mercury : AnimatableItem, IPlanet
{
    public string Name => "Mercury";
    public string Description => "Level up Pair (+15 Chips, +1 Mult)";
    public int Cost => 2;
    public ConsumableType Type => ConsumableType.Planet;
    public HandType TargetHand => HandType.Pair;

    public void ApplyToHand(HandType handType, ref int baseChips, ref int baseMult)
    {
        if (handType == TargetHand)
        {
            baseChips += 15;
            baseMult += 1;
        }
    }
}
