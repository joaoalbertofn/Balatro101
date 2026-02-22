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
    string TextureKey { get; }
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

// Tarots
public class TheHierophant : AnimatableItem, ITarot
{
    public string Name => "The Hierophant";
    public string Description => "Enhances card to give +30 Bonus Chips";
    public string TextureKey => "Tarot_01";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Tarot;

    public void ApplyToCard(Card card) => card.BonusChips += 30;
}

public class TheEmpress : AnimatableItem, ITarot
{
    public string Name => "The Empress";
    public string Description => "Enhances card to give +4 Multiplier";
    public string TextureKey => "Tarot_02";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Tarot;

    public void ApplyToCard(Card card) => card.BonusMult += 4;
}

public class TheMagician : AnimatableItem, ITarot
{
    public string Name => "The Magician";
    public string Description => "Enhances card to Foil (+50 Chips)";
    public string TextureKey => "Tarot_03";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Tarot;

    public void ApplyToCard(Card card) => card.IsFoil = true;
}

public class TheLovers : AnimatableItem, ITarot
{
    public string Name => "The Lovers";
    public string Description => "Enhances card to Holographic (+10 Mult)";
    public string TextureKey => "Tarot_04";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Tarot;

    public void ApplyToCard(Card card) => card.IsHolographic = true;
}

public class TheChariot : AnimatableItem, ITarot
{
    public string Name => "The Chariot";
    public string Description => "Enhances card to Polychrome (x1.5 Mult)";
    public string TextureKey => "Tarot_05";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Tarot;

    public void ApplyToCard(Card card) => card.IsPolychrome = true;
}

public class Justice : AnimatableItem, ITarot
{
    public string Name => "Justice";
    public string Description => "Enhances card to give +15 Chips, +3 Mult";
    public string TextureKey => "Tarot_06";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Tarot;

    public void ApplyToCard(Card card)
    {
        card.BonusChips += 15;
        card.BonusMult += 3;
    }
}

// Planets
public class Pluto : AnimatableItem, IPlanet
{
    public string Name => "Pluto";
    public string Description => "Level up High Card (+10 Chips, +1 Mult)";
    public string TextureKey => "Planetas_01";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Planet;
    public HandType TargetHand => HandType.HighCard;

    public void ApplyToHand(HandType handType, ref int baseChips, ref int baseMult)
    {
        if (handType == TargetHand) { baseChips += 10; baseMult += 1; }
    }
}

public class Mercury : AnimatableItem, IPlanet
{
    public string Name => "Mercury";
    public string Description => "Level up Pair (+15 Chips, +1 Mult)";
    public string TextureKey => "Planetas_02";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Planet;
    public HandType TargetHand => HandType.Pair;

    public void ApplyToHand(HandType handType, ref int baseChips, ref int baseMult)
    {
        if (handType == TargetHand) { baseChips += 15; baseMult += 1; }
    }
}

public class Uranus : AnimatableItem, IPlanet
{
    public string Name => "Uranus";
    public string Description => "Level up Two Pair (+20 Chips, +1 Mult)";
    public string TextureKey => "Planetas_03";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Planet;
    public HandType TargetHand => HandType.TwoPair;

    public void ApplyToHand(HandType handType, ref int baseChips, ref int baseMult)
    {
        if (handType == TargetHand) { baseChips += 20; baseMult += 1; }
    }
}

public class Venus : AnimatableItem, IPlanet
{
    public string Name => "Venus";
    public string Description => "Level up 3 of a Kind (+20 Chips, +2 Mult)";
    public string TextureKey => "Planetas_04";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Planet;
    public HandType TargetHand => HandType.ThreeOfAKind;

    public void ApplyToHand(HandType handType, ref int baseChips, ref int baseMult)
    {
        if (handType == TargetHand) { baseChips += 20; baseMult += 2; }
    }
}

public class Saturn : AnimatableItem, IPlanet
{
    public string Name => "Saturn";
    public string Description => "Level up Straight (+30 Chips, +2 Mult)";
    public string TextureKey => "Planetas_05";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Planet;
    public HandType TargetHand => HandType.Straight;

    public void ApplyToHand(HandType handType, ref int baseChips, ref int baseMult)
    {
        if (handType == TargetHand) { baseChips += 30; baseMult += 2; }
    }
}

public class Jupiter : AnimatableItem, IPlanet
{
    public string Name => "Jupiter";
    public string Description => "Level up Flush (+15 Chips, +2 Mult)";
    public string TextureKey => "Planetas_06";
    public int Cost => 3;
    public ConsumableType Type => ConsumableType.Planet;
    public HandType TargetHand => HandType.Flush;

    public void ApplyToHand(HandType handType, ref int baseChips, ref int baseMult)
    {
        if (handType == TargetHand) { baseChips += 15; baseMult += 2; }
    }
}
