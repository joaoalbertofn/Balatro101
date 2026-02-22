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

        var allConsumables = new List<IConsumable>
        {
            new TheHierophant(), new TheEmpress(), new TheMagician(), new TheLovers(), new TheChariot(), new Justice(),
            new Pluto(), new Mercury(), new Uranus(), new Venus(), new Saturn(), new Jupiter()
        };

        var rnd = new System.Random();
        for (int i = 0; i < 2; i++)
        {
            shopConsumables.Add(allConsumables[rnd.Next(allConsumables.Count)]);
        }

        // Add 2 random Jokers to the shop
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

        GameUI.DrawLeftPanel(game, true);

        Vector2 mousePos = Raylib.GetMousePosition();
        bool actionTaken = false;

        // Content area starts at X = 320 to avoiding Left Panel
        int shopContentX = 320;

        // 1. Draw Inventory (Top)
        Raylib.DrawText($"INVENTORY ({game.SelectedJokers.Count}/5 Jokers)", shopContentX, 30, 20, Color.LightGray);
        for (int i = 0; i < game.SelectedJokers.Count; i++)
        {
            var joker = game.SelectedJokers[i];
            float jx = shopContentX + i * 110;
            float jy = 60;
            // Draw joker in smaller scale for inventory
            RendererUtils.DrawJoker(joker, jx, jy, false, 0.7f);

            // Sell Button below inventory item
            if (!actionTaken && RendererUtils.DrawButton("SELL $2", jx, jy + 160, 90, 30))
            {
                game.Money += 2;
                game.SelectedJokers.RemoveAt(i);
                actionTaken = true;
                i--;
            }
        }

        // 1.b Draw Inventory Consumables
        Raylib.DrawText($"CONSUMABLES ({game.Consumables.Count}/{GameConfig.MaxConsumables})", shopContentX + 580, 30, 20, Color.LightGray);
        for (int i = 0; i < game.Consumables.Count; i++)
        {
            var item = game.Consumables[i];
            float ix = shopContentX + 580 + i * 120;
            float iy = 60;

            // Draw scale down consumable
            RendererUtils.DrawConsumable(item, ix, iy, false, 0.65f);

            // Sell Button below inventory item
            if (!actionTaken && RendererUtils.DrawButton("SELL $1", ix, iy + 165, 90, 30))
            {
                game.Money += 1;
                game.Consumables.RemoveAt(i);
                actionTaken = true;
                i--;
            }
        }

        // 2. Draw Shop Offerings (Bottom)
        Raylib.DrawText("FOR SALE", shopContentX, 320, 20, Color.LightGray);
        int startX = shopContentX;
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
        if (RendererUtils.DrawButton($"NEXT BLIND (ANTE {game.CurrentAnte})", GameConfig.WindowWidth - 300, GameConfig.WindowHeight - 80, 280, 60))
        {
            shopInitialized = false;
            game.StartBlind();
        }
    }
}
