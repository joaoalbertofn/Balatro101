using System.Collections.Generic;
using Balatro101.Game.Core;

namespace Balatro101.Game.Engine;

public class ScoringEngine
{
    public List<IJoker> ActiveJokers { get; } = new();

    public int CalculateScore(HandResult handResult, BossDebuffType debuff, out ScoreState finalState)
    {
        finalState = new ScoreState
        {
            Chips = handResult.BaseChips,
            Multiplier = handResult.BaseMultiplier
        };

        // First add base chips from the played scoring cards
        foreach (var card in handResult.ScoringCards)
        {
            if (debuff == BossDebuffType.TheClub && card.Suit == Suit.Clubs) continue;

            finalState.Chips += card.GetValue();

            // Trigger card specific jokers
            foreach (var joker in ActiveJokers)
            {
                joker.OnCardScored(card, finalState);
            }
        }

        // Trigger hand overall jokers
        foreach (var joker in ActiveJokers)
        {
            joker.OnHandEvaluated(handResult, finalState);
        }

        return finalState.Chips * finalState.Multiplier;
    }
}
