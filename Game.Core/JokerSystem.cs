namespace Balatro101.Game.Core;

public interface IJoker
{
    string Name { get; }
    string Description { get; }
    string TextureKey { get; }

    // Physics properties for ScoreAnimator
    float X { get; set; }
    float Y { get; set; }
    void BumpScale(float amount = 1.3f);
    void Wobble(float velocity = 25f);

    void OnHandEvaluated(HandResult handResult, ScoreState scoreState);
    void OnCardScored(Card card, ScoreState scoreState);
}
