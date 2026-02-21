using System;
using System.Collections.Generic;
using System.Linq;

namespace Balatro101.Game.Core;

public static class HandEvaluator
{
    public static HandResult Evaluate(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            throw new ArgumentException("Cannot evaluate empty hand");

        var sortedCards = cards.OrderByDescending(c => c.Rank).ToList();

        // Check Flush
        bool isFlush = sortedCards.Count >= 5 && sortedCards.GroupBy(c => c.Suit).Any(g => g.Count() >= 5);
        List<Card> flushCards = isFlush ? sortedCards.GroupBy(c => c.Suit).First(g => g.Count() >= 5).Take(5).ToList() : new();

        var straightCards = GetStraight(sortedCards);
        bool isStraight = straightCards != null;

        if (isFlush && isStraight)
        {
            var straightFlushCards = GetStraightFlush(sortedCards);
            if (straightFlushCards != null)
                return new HandResult(HandType.StraightFlush, 100, 8, straightFlushCards);
        }

        var groups = sortedCards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Key).ToList();

        if (groups[0].Count() == 4)
            return new HandResult(HandType.FourOfAKind, 60, 7, groups[0].Take(4).ToList());

        if (groups[0].Count() == 3 && groups.Count > 1 && groups[1].Count() >= 2)
        {
            var scoringCards = groups[0].Take(3).Concat(groups[1].Take(2)).ToList();
            return new HandResult(HandType.FullHouse, 40, 4, scoringCards);
        }

        if (isFlush)
            return new HandResult(HandType.Flush, 35, 4, flushCards);

        if (isStraight)
            return new HandResult(HandType.Straight, 30, 4, straightCards!);

        if (groups[0].Count() == 3)
            return new HandResult(HandType.ThreeOfAKind, 30, 3, groups[0].Take(3).ToList());

        if (groups[0].Count() == 2 && groups.Count > 1 && groups[1].Count() >= 2)
        {
            var scoringCards = groups[0].Take(2).Concat(groups[1].Take(2)).ToList();
            return new HandResult(HandType.TwoPair, 20, 2, scoringCards);
        }

        if (groups[0].Count() == 2)
            return new HandResult(HandType.Pair, 10, 2, groups[0].Take(2).ToList());

        return new HandResult(HandType.HighCard, 5, 1, new List<Card> { sortedCards[0] });
    }

    private static List<Card>? GetStraight(List<Card> cards)
    {
        var distinctRanks = cards.GroupBy(c => c.Rank).Select(g => g.First()).OrderByDescending(c => c.Rank).ToList();
        
        for (int i = 0; i <= distinctRanks.Count - 5; i++)
        {
            if (distinctRanks[i].Rank - distinctRanks[i + 4].Rank == 4)
            {
                return distinctRanks.Skip(i).Take(5).ToList();
            }
            if (distinctRanks[i].Rank == Rank.Ace &&
                distinctRanks.Any(c => c.Rank == Rank.Five) &&
                distinctRanks.Any(c => c.Rank == Rank.Four) &&
                distinctRanks.Any(c => c.Rank == Rank.Three) &&
                distinctRanks.Any(c => c.Rank == Rank.Two))
            {
                var lowStraight = new List<Card>
                {
                    distinctRanks.First(c => c.Rank == Rank.Five),
                    distinctRanks.First(c => c.Rank == Rank.Four),
                    distinctRanks.First(c => c.Rank == Rank.Three),
                    distinctRanks.First(c => c.Rank == Rank.Two),
                    distinctRanks.First(c => c.Rank == Rank.Ace)
                };
                return lowStraight;
            }
        }
        return null;
    }

    private static List<Card>? GetStraightFlush(List<Card> cards)
    {
        foreach (var suitGroup in cards.GroupBy(c => c.Suit).Where(g => g.Count() >= 5))
        {
            var straight = GetStraight(suitGroup.ToList());
            if (straight != null) return straight;
        }
        return null;
    }
}
