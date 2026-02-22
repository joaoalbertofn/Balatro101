namespace Balatro101.Game.Core;

public static class GameConfig
{
    // Gameplay Limits
    public static int TargetScore = 300;
    public static int MaxHands = 4;
    public static int MaxDiscards = 3;
    public static int DefaultHandSize = 8;
    public static int MaxSelectedCards = 5;
    public static int MaxConsumables = 2;

    // Rendering Parameters
    public const int WindowWidth = 1600;
    public const int WindowHeight = 900;
    public const int FPS = 60;
}
