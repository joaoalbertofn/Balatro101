using Raylib_cs;
using Balatro101.Game.Core;
using Balatro101.Game.Engine;
using Balatro101.Game.UI;

namespace Balatro101;

class Program
{
    static void Main(string[] args)
    {
        Raylib.InitWindow(GameConfig.WindowWidth, GameConfig.WindowHeight, "Balatro 101 Prototype");
        Raylib.SetTargetFPS(GameConfig.FPS);

        AudioEngine.Initialize();

        GameManager game = new GameManager();

        while (!Raylib.WindowShouldClose())
        {
            GameUI.UpdateAndDraw(game);
        }

        AudioEngine.Close();
        Raylib.CloseWindow();
    }
}
