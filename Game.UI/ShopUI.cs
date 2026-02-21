using Raylib_cs;
using System.Collections.Generic;
using System.Numerics;
using Balatro101.Game.Core;
using Balatro101.Game.Engine;

namespace Balatro101.Game.UI;

public static class ShopUI
{
    private static List<IConsumable> shopConsumables = new();
    private static List<IJoker> shopJokers = new();
    private static bool shopInitialized = false;

    public static void InitializeShop(GameManager game)
    {
        shopConsumables.Clear();
        shopJokers.Clear();

        shopConsumables.Add(new TheFool());
        shopConsumables.Add(new Mercury());

        // Add 2 random Jokers to the shop
        var rnd = new System.Random();
        for (int i = 0; i < 2; i++)
        {
            var randomJoker = game.AllJokers[rnd.Next(game.AllJokers.Count)];
            shopJokers.Add(randomJoker);
        }

        shopInitialized = true;
    }

    public static void DrawShopTransition(GameManager game)
    {
        Raylib.DrawRectangle(0, 0, GameConfig.WindowWidth, GameConfig.WindowHeight, new Color(10, 20, 15, 230));

        Raylib.DrawText("BLIND DEFEATED!", GameConfig.WindowWidth / 2 - 250, 250, 60, Color.Gold);
        Raylib.DrawText($"Reward: ${game.Money}", GameConfig.WindowWidth / 2 - 120, 350, 40, Color.Green);

        if (RendererUtils.DrawButton("ENTER SHOP", GameConfig.WindowWidth / 2 - 150, 500, 300, 50))
        {
            if (!shopInitialized) InitializeShop(game);
            game.CurrentState = GameState.Shop;
        }
    }

    public static void DrawShop(GameManager game)
    {
        Raylib.DrawRectangle(0, 0, GameConfig.WindowWidth, GameConfig.WindowHeight, new Color(40, 30, 20, 255));

        Raylib.DrawText("SHOP", 50, 30, 40, Color.White);
        Raylib.DrawText($"Money: ${game.Money}", GameConfig.WindowWidth - 250, 30, 40, Color.Gold);

        Vector2 mousePos = Raylib.GetMousePosition();
        bool actionTaken = false;

        // 1. Draw Inventory (Top Left)
        Raylib.DrawText($"INVENTORY ({game.SelectedJokers.Count}/5 Jokers)", 50, 75, 20, Color.LightGray);
        for (int i = 0; i < game.SelectedJokers.Count; i++)
        {
            var joker = game.SelectedJokers[i];
            float jx = 50 + i * 140;
            float jy = 105;
            RendererUtils.DrawJoker(joker, jx, jy);

            // Sell Button below inventory item
            if (!actionTaken && RendererUtils.DrawButton("SELL $2", jx, jy + 170, 120, 30))
            {
                game.Money += 2;
                game.SelectedJokers.RemoveAt(i);
                actionTaken = true;
                i--;
            }
        }

        // 1.b Draw Inventory Consumables
        Raylib.DrawText($"CONSUMABLES ({game.Consumables.Count}/{GameConfig.MaxConsumables})", 800, 75, 20, Color.LightGray);
        for (int i = 0; i < game.Consumables.Count; i++)
        {
            var item = game.Consumables[i];
            float ix = 800 + i * 160;
            float iy = 210;

            // Draw scale down consumable
            Rlgl.PushMatrix();
            Rlgl.Translatef(ix, iy, 0); // Translate to center
            Rlgl.Scalef(0.6f, 0.6f, 1f);

            var rectC = new Rectangle(-75, -120, 150, 240); // Draw from local center
            Raylib.DrawRectangleRounded(rectC, 0.1f, 10, item.Type == ConsumableType.Planet ? Color.SkyBlue : Color.Purple);
            Raylib.DrawRectangleLinesEx(rectC, 3f, Color.Black);
            Raylib.DrawText(item.Type.ToString().ToUpper(), -65, -110, 15, Color.White);
            Raylib.DrawText(item.Name, -65, -80, 20, Color.Yellow);
            Rlgl.PopMatrix();

            // Sell Button below inventory item
            if (!actionTaken && RendererUtils.DrawButton("SELL $1", ix - 35, iy + 90, 70, 30))
            {
                game.Money += 1;
                game.Consumables.RemoveAt(i);
                actionTaken = true;
                i--;
            }
        }

        // 2. Draw Shop Offerings (Bottom)
        Raylib.DrawText("FOR SALE", 50, 350, 20, Color.LightGray);
        int startX = 50;
        int y = 380;

        // Draw Consumables
        for (int i = 0; i < shopConsumables.Count; i++)
        {
            var item = shopConsumables[i];
            float itemX = startX + i * 200;
            var rect = new Rectangle(itemX, y, 150, 240);
            bool isHovered = Raylib.CheckCollisionPointRec(mousePos, rect);

            DrawConsumable(item, itemX, y, isHovered);

            if (!actionTaken && RendererUtils.DrawButton($"BUY ${item.Cost}", itemX, y + 260, 150, 40))
            {
                if (game.Money >= item.Cost && game.Consumables.Count < GameConfig.MaxConsumables)
                {
                    game.Money -= item.Cost;
                    game.Consumables.Add(item);
                    shopConsumables.RemoveAt(i);
                    actionTaken = true;
                    i--;
                }
            }
        }

        // Draw Jokers
        int jokerStartX = startX + (shopConsumables.Count * 200) + 50;
        for (int i = 0; i < shopJokers.Count; i++)
        {
            var joker = shopJokers[i];
            float itemX = jokerStartX + i * 140;
            int cost = 5;

            RendererUtils.DrawJoker(joker, itemX, y);

            if (!actionTaken && RendererUtils.DrawButton($"BUY ${cost}", itemX, y + 170, 120, 40))
            {
                if (game.Money >= cost && game.SelectedJokers.Count < 5)
                {
                    game.Money -= cost;
                    game.SelectedJokers.Add(joker);
                    shopJokers.RemoveAt(i);
                    actionTaken = true;
                    i--;
                }
            }
        }

        // NEXT BLIND BUTTON
        if (RendererUtils.DrawButton($"NEXT BLIND (ANTE {game.CurrentAnte})", GameConfig.WindowWidth - 350, GameConfig.WindowHeight - 100, 300, 50))
        {
            shopInitialized = false;
            game.StartBlind();
        }
    }

    private static void DrawConsumable(IConsumable item, float x, float y, bool isHovered)
    {
        float scale = isHovered ? 1.05f : 1.0f;

        Rlgl.PushMatrix();
        Rlgl.Translatef(x + 75, y + 120, 0); // Translate to center
        Rlgl.Scalef(scale, scale, 1f);

        var rect = new Rectangle(-75, -120, 150, 240); // Draw from local center

        // Shadow
        Raylib.DrawRectangleRounded(new Rectangle(-75 + 10, -120 + 10, 150, 240), 0.1f, 10, new Color(0, 0, 0, 100));

        Color bgColor = item.Type == ConsumableType.Planet ? Color.SkyBlue : Color.Purple;

        Raylib.DrawRectangleRounded(rect, 0.1f, 10, bgColor);
        Raylib.DrawRectangleLinesEx(rect, 3f, Color.Black);

        Raylib.DrawText(item.Type.ToString().ToUpper(), -65, -110, 15, Color.White);
        Raylib.DrawText(item.Name, -65, -80, 20, Color.Yellow);

        // Word wrap description
        string[] words = item.Description.Split(' ');
        int lineY = -15;
        string currentLine = "";

        foreach (var word in words)
        {
            if (currentLine.Length + word.Length < 16)
            {
                currentLine += word + " ";
            }
            else
            {
                Raylib.DrawText(currentLine, -65, lineY, 15, Color.White);
                currentLine = word + " ";
                lineY += 20;
            }
        }
        if (currentLine.Length > 0)
        {
            Raylib.DrawText(currentLine, -65, lineY, 15, Color.White);
        }

        Rlgl.PopMatrix();
    }
}
