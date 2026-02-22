namespace Balatro101.Game.Core;

public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public enum Rank
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14
}

public enum HandType
{
    HighCard,
    Pair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush
}

public enum SortMode
{
    None,
    Rank,
    Suit
}

public enum BlindType
{
    Small,
    Big,
    Boss
}

public enum BossDebuffType
{
    None,
    TheHook,   // Discards 2 random cards per hand played
    TheClub,   // All Club cards are debuffed
    TheWindow, // Halves the max discards
    ThePillar  // Reduces max hands by 1
}
