namespace Balatro101.Game.Core;

public interface IJoker
{
    string Name { get; }
    string Description { get; }
    string TextureKey { get; }

    void OnHandEvaluated(HandResult handResult, ScoreState scoreState);
    void OnCardScored(Card card, ScoreState scoreState);
}
