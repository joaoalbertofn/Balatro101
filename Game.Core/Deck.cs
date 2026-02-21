using System;
using System.Collections.Generic;
using System.Linq;

namespace Balatro101.Game.Core;

public class Deck
{
    private List<Card> _cards = new();
    private readonly Random _random = new();

    public Deck()
    {
        Initialize();
    }

    public void Initialize()
    {
        _cards.Clear();
        foreach (Suit suit in Enum.GetValues<Suit>())
        {
            foreach (Rank rank in Enum.GetValues<Rank>())
            {
                _cards.Add(new Card(suit, rank));
            }
        }
        Shuffle();
    }

    public void Shuffle()
    {
        int n = _cards.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            (_cards[k], _cards[n]) = (_cards[n], _cards[k]);
        }
    }

    public Card? Draw()
    {
        if (_cards.Count == 0) return null;
        var card = _cards[^1];
        _cards.RemoveAt(_cards.Count - 1);
        return card;
    }

    public int Count => _cards.Count;
}
