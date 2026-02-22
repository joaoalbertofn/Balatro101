using System.Linq;

namespace Balatro101.Game.Core;

public class PairJoker() : AnimatableItem, IJoker
{
    public string Name => "Jolly Joker";
    public string Description => "+8 Mult if Pair";
    public string TextureKey => "Jokers 2_01";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.Pair or HandType.TwoPair or HandType.FullHouse)
            scoreState.Multiplier += 8;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class HeartsJoker() : AnimatableItem, IJoker
{
    public string Name => "Lusty Joker";
    public string Description => "+4 Mult for Hearts";
    public string TextureKey => "Jokers 2_03";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.Suit == Suit.Hearts) scoreState.Multiplier += 4;
    }
}

public class FixedJoker() : AnimatableItem, IJoker
{
    public string Name => "Sly Joker";
    public string Description => "+50 Chips if Pair";
    public string TextureKey => "Jokers 2_04";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.Pair or HandType.TwoPair or HandType.FullHouse)
            scoreState.Chips += 50;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class FlushMultiplierJoker() : AnimatableItem, IJoker
{
    public string Name => "Droll Joker";
    public string Description => "+10 Mult if Flush";
    public string TextureKey => "Jokers 2_08";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.Flush or HandType.StraightFlush)
            scoreState.Multiplier += 10;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class SpadesJoker() : AnimatableItem, IJoker
{
    public string Name => "Wrathful Joker";
    public string Description => "+4 Mult for Spades";
    public string TextureKey => "Jokers 2_11";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.Suit == Suit.Spades) scoreState.Multiplier += 4;
    }
}

public class HighCardJoker() : AnimatableItem, IJoker
{
    public string Name => "High Card Joker";
    public string Description => "+1 Mult for card 10+";
    public string TextureKey => "Jokers 2_12";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.GetValue() >= 10) scoreState.Multiplier += 1;
    }
}

public class CrazyJoker() : AnimatableItem, IJoker
{
    public string Name => "Zany Joker";
    public string Description => "+12 Mult if 3 of a Kind";
    public string TextureKey => "Jokers 2_13";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.ThreeOfAKind or HandType.FullHouse or HandType.FourOfAKind)
            scoreState.Multiplier += 12;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class LuckyJoker() : AnimatableItem, IJoker
{
    public string Name => "Lucky Joker";
    public string Description => "+20 Chips for High Card";
    public string TextureKey => "Jokers 2_15";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type == HandType.HighCard)
            scoreState.Chips += 20;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class RedJoker() : AnimatableItem, IJoker
{
    public string Name => "Blood Joker";
    public string Description => "+2 Mult if Hearts/Diamonds";
    public string TextureKey => "Jokers 3_01";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.Suit is Suit.Hearts or Suit.Diamonds) scoreState.Multiplier += 2;
    }
}

public class SupremeJoker() : AnimatableItem, IJoker
{
    public string Name => "Supreme Joker";
    public string Description => "x3 Mult if Straight";
    public string TextureKey => "Jokers 3_06";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.Straight or HandType.StraightFlush)
            scoreState.Multiplier *= 3;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class ClubsJoker() : AnimatableItem, IJoker
{
    public string Name => "Gluttonous Joker";
    public string Description => "+4 Mult for Clubs";
    public string TextureKey => "Jokers 3_07";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.Suit == Suit.Clubs) scoreState.Multiplier += 4;
    }
}

public class DiamondsJoker() : AnimatableItem, IJoker
{
    public string Name => "Greedy Joker";
    public string Description => "+4 Mult for Diamonds";
    public string TextureKey => "Jokers 3_08";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.Suit == Suit.Diamonds) scoreState.Multiplier += 4;
    }
}

public class BaseMultiplierJoker() : AnimatableItem, IJoker
{
    public string Name => "Standard Joker";
    public string Description => "+4 Mult base";
    public string TextureKey => "Jokers 3_09";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        scoreState.Multiplier += 4;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class StraightJoker() : AnimatableItem, IJoker
{
    public string Name => "Crazy Joker";
    public string Description => "+12 Mult if Straight";
    public string TextureKey => "Jokers 3_10";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.Straight or HandType.StraightFlush)
            scoreState.Multiplier += 12;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class TwoPairJoker() : AnimatableItem, IJoker
{
    public string Name => "Mad Joker";
    public string Description => "+10 Mult if Two Pair";
    public string TextureKey => "Jokers 3_13";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.TwoPair)
            scoreState.Multiplier += 10;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class FaceCardJoker() : AnimatableItem, IJoker
{
    public string Name => "Scary Face";
    public string Description => "+30 Chips for Face Cards";
    public string TextureKey => "Jokers 3_14";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.GetValue() == 10) scoreState.Chips += 30; // J, Q, K
    }
}

public class BlackJoker() : AnimatableItem, IJoker
{
    public string Name => "Onyx Joker";
    public string Description => "+2 Mult if Spades/Clubs";
    public string TextureKey => "Jokers_08";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.Suit is Suit.Spades or Suit.Clubs) scoreState.Multiplier += 2;
    }
}

public class AcesJoker() : AnimatableItem, IJoker
{
    public string Name => "Aces Joker";
    public string Description => "+10 Mult per Ace";
    public string TextureKey => "Jokers_11";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.Rank == Rank.Ace) scoreState.Multiplier += 10;
    }
}

public class FullHouseJoker() : AnimatableItem, IJoker
{
    public string Name => "House Joker";
    public string Description => "+20 Mult if Full House";
    public string TextureKey => "Jokers_12";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type == HandType.FullHouse)
            scoreState.Multiplier += 20;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class FourOfAKindJoker() : AnimatableItem, IJoker
{
    public string Name => "Four Joker";
    public string Description => "+30 Mult if 4 of a Kind";
    public string TextureKey => "Jokers_13";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type == HandType.FourOfAKind)
            scoreState.Multiplier += 30;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class EvenJoker() : AnimatableItem, IJoker
{
    public string Name => "Even Joker";
    public string Description => "+1 Mult for Even cards";
    public string TextureKey => "Jokers_15";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        int val = (int)card.Rank;
        if (val >= 2 && val <= 10 && val % 2 == 0) scoreState.Multiplier += 1;
    }
}
