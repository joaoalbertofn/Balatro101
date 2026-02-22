using Raylib_cs;
using Balatro101.Game.Core;
using Balatro101.Game.Engine;
using Balatro101.Game.UI;

namespace Balatro101;

static class Program
{
    static void Main(string[] args)
    {
        Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
        Raylib.InitWindow(GameConfig.WindowWidth, GameConfig.WindowHeight, "Balatro do J.A.");
        Raylib.SetTargetFPS(GameConfig.FPS);

        AssetManager.LoadAll();

        GameManager game = new GameManager();

        InputManager.Initialize(GameConfig.WindowWidth / 2, GameConfig.WindowHeight / 2);

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            InputManager.Update(dt);

            // Gamepad Navigation (using nodes from the previous frame)
            UINavigator.UpdateNavigation();

            // Clear nodes to capture new positions in the upcoming draw call
            UINavigator.ClearNodes();

            // Render and register nodes
            GameUI.UpdateAndDraw(game);
        }

        Raylib.CloseWindow();
    }
}
