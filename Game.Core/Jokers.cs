using System.Linq;

namespace Balatro101.Game.Core;

public class PairJoker() : AnimatableItem, IJoker
{
    public string Name => "Joker de Pares";
    public string Description => "+4 Mult se jogar um Par";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.Pair or HandType.TwoPair or HandType.FullHouse)
            scoreState.Multiplier += 4;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class HeartsJoker() : AnimatableItem, IJoker
{
    public string Name => "Joker de Copas";
    public string Description => "+10 Fichas por Copas";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.Suit == Suit.Hearts) scoreState.Chips += 10;
    }
}

public class FixedJoker() : AnimatableItem, IJoker
{
    public string Name => "Joker Fixo";
    public string Description => "+50 Fichas bases";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        scoreState.Chips += 50;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class FlushMultiplierJoker() : AnimatableItem, IJoker
{
    public string Name => "Joker Multiplicador";
    public string Description => "x2 Mult no Flush";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.Flush or HandType.StraightFlush)
            scoreState.Multiplier *= 2;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class SpadesJoker() : AnimatableItem, IJoker
{
    public string Name => "Joker de Espadas";
    public string Description => "+10 Fichas por Espadas";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.Suit == Suit.Spades) scoreState.Chips += 10;
    }
}

public class HighCardJoker() : AnimatableItem, IJoker
{
    public string Name => "Joker Alto";
    public string Description => "+1 Mult por carta 10+";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.GetValue() >= 10) scoreState.Multiplier += 1;
    }
}

public class CrazyJoker() : AnimatableItem, IJoker
{
    public string Name => "Joker Louco";
    public string Description => "+15 Mult por Trinca";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.ThreeOfAKind or HandType.FullHouse or HandType.FourOfAKind)
            scoreState.Multiplier += 15;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class LuckyJoker() : AnimatableItem, IJoker
{
    public string Name => "Joker da Sorte";
    public string Description => "+20 Fichas por Carta Alta";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type == HandType.HighCard)
            scoreState.Chips += 20;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}

public class RedJoker() : AnimatableItem, IJoker
{
    public string Name => "Joker Vermelho";
    public string Description => "+2 Mult se Copas/Ouros";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState) { }
    public void OnCardScored(Card card, ScoreState scoreState)
    {
        if (card.Suit is Suit.Hearts or Suit.Diamonds) scoreState.Multiplier += 2;
    }
}

public class SupremeJoker() : AnimatableItem, IJoker
{
    public string Name => "Joker Supremo";
    public string Description => "x3 Mult no Straight";

    public void OnHandEvaluated(HandResult handResult, ScoreState scoreState)
    {
        if (handResult.Type is HandType.Straight or HandType.StraightFlush)
            scoreState.Multiplier *= 3;
    }
    public void OnCardScored(Card card, ScoreState scoreState) { }
}
