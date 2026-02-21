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
            float jx = 50 + i * 110;
            float jy = 105;
            // Draw joker in smaller scale for inventory
            RendererUtils.DrawJoker(joker, jx, jy, false, 0.7f);

            // Sell Button below inventory item
            if (!actionTaken && RendererUtils.DrawButton("SELL $2", jx, jy + 160, 90, 30))
            {
                AudioEngine.PlayCoin();
                game.Money += 2;
                game.SelectedJokers.RemoveAt(i);
                actionTaken = true;
                i--;
            }
        }

        // 1.b Draw Inventory Consumables
        Raylib.DrawText($"CONSUMABLES ({game.Consumables.Count}/{GameConfig.MaxConsumables})", 700, 75, 20, Color.LightGray);
        for (int i = 0; i < game.Consumables.Count; i++)
        {
            var item = game.Consumables[i];
            float ix = 700 + i * 120;
            float iy = 105;

            // Draw scale down consumable
            RendererUtils.DrawConsumable(item, ix, iy, false, 0.65f);

            // Sell Button below inventory item
            if (!actionTaken && RendererUtils.DrawButton("SELL $1", ix, iy + 165, 90, 30))
            {
                AudioEngine.PlayCoin();
                game.Money += 1;
                game.Consumables.RemoveAt(i);
                actionTaken = true;
                i--;
            }
        }

        // 2. Draw Shop Offerings (Bottom)
        Raylib.DrawText("FOR SALE", 50, 320, 20, Color.LightGray);
        int startX = 50;
        int y = 350;

        // Draw Consumables
        for (int i = 0; i < shopConsumables.Count; i++)
        {
            var item = shopConsumables[i];
            float itemX = startX + i * 220; // More spacing for larger scale

            // Determine hover zone for normal 1.0f sized consumable
            var rect = new Rectangle(itemX, y, 150, 240);
            bool isHovered = Raylib.CheckCollisionPointRec(mousePos, rect);

            RendererUtils.DrawConsumable(item, itemX, y, isHovered, 1.0f);

            if (!actionTaken && RendererUtils.DrawButton($"BUY ${item.Cost}", itemX, y + 260, 150, 40))
            {
                if (game.Money >= item.Cost && game.Consumables.Count < GameConfig.MaxConsumables)
                {
                    AudioEngine.PlayCoin();
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
                    AudioEngine.PlayCoin();
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
}
