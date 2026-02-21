using System.Numerics;
using System.Linq;
using Raylib_cs;
using Balatro101.Game.Core;
using Balatro101.Game.Engine;

namespace Balatro101.Game.UI;

public static class GameUI
{
    public static void UpdateAndDraw(GameManager game)
    {
        // Smooth card animations
        if (game.CurrentState is GameState.FaseGameplay or GameState.AnimacaoPontuacao)
            UpdateCardsLogic(game);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(30, 60, 40, 255));

        switch (game.CurrentState)
        {
            case GameState.MainMenu:
                DrawMainMenu(game);
                break;
            case GameState.FaseGameplay:
                DrawGameplay(game);
                break;
            case GameState.AnimacaoPontuacao:
                DrawScoreAnimation(game);
                break;
            case GameState.ShopTransition:
                ShopUI.DrawShopTransition(game);
                break;
            case GameState.Shop:
                ShopUI.DrawShop(game);
                break;
            case GameState.GameOver:
                DrawGameOver(game);
                break;
        }

        Raylib.EndDrawing();
    }

    private static void DrawMainMenu(GameManager game)
    {
        Raylib.DrawText("BALATRO CLONE", GameConfig.WindowWidth / 2 - 250, 200, 60, Color.White);
        if (RendererUtils.DrawButton("INICIAR", GameConfig.WindowWidth / 2 - 100, 400, 200, 50))
        {
            game.StartGame();
        }
    }

    private static void UpdateCardsLogic(GameManager game)
    {
        if (game.CurrentState != GameState.FaseGameplay) return;

        Vector2 mousePos = Raylib.GetMousePosition();
        int numCards = game.PlayerHand.Count;
        float spacing = 110f;
        float totalWidth = numCards * spacing;
        float startX = (GameConfig.WindowWidth - totalWidth) / 2;
        float baseY = 500f;
        float dt = Raylib.GetFrameTime();

        // Reset hover, find the topmost hovered card if returning true backwards
        int hoveredIndex = -1;
        for (int i = numCards - 1; i >= 0; i--)
        {
            var card = game.PlayerHand[i];
            card.IsHovered = false;
            card.TargetX = startX + i * spacing;

            // Initialize X,Y if 0 so they don't fly from top-left on draw
            if (card.X == 0 && card.Y == 0)
            {
                card.X = GameConfig.WindowWidth + 200; // Draw from bottom right
                card.Y = baseY + 300;
            }

            var rect = new Rectangle(card.X, card.Y, RendererUtils.CardWidth * card.Scale, RendererUtils.CardHeight * card.Scale);
            if (hoveredIndex == -1 && Raylib.CheckCollisionPointRec(mousePos, rect))
            {
                card.IsHovered = true;
                hoveredIndex = i;
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    // If we are trying to select a new card, check the limit
                    if (!card.IsSelected && game.PlayerHand.Count(c => c.IsSelected) >= GameConfig.MaxSelectedCards)
                    {
                        // Play error sound or just ignore
                    }
                    else
                    {
                        AudioEngine.PlayBlip();
                        card.IsSelected = !card.IsSelected;
                    }
                }
            }
        }

        foreach (var card in game.PlayerHand)
        {
            if (card.IsSelected)
            {
                card.TargetY = baseY - 40;
                card.TargetScale = 1.1f;
                card.TargetRotation = 0f;
            }
            else if (card.IsHovered)
            {
                card.TargetY = baseY - 15;
                card.TargetScale = 1.05f;
                card.TargetRotation = (card.X - mousePos.X) * 0.05f; // Slight tilt based on mouse position
            }
            else
            {
                card.TargetY = baseY;
                card.TargetScale = 1.0f;
                card.TargetRotation = 0f;
            }

            TweenEngine.UpdateCardPhysics(card, dt);
        }
    }

    private static void DrawGameplay(GameManager game)
    {
        DrawHUD(game);

        var selectedCards = game.PlayerHand.Where(c => c.IsSelected).ToList();
        if (selectedCards.Count > 0 && selectedCards.Count <= GameConfig.MaxSelectedCards)
        {
            try
            {
                var prediction = HandEvaluator.Evaluate(selectedCards);
                string handName = prediction.Type switch
                {
                    HandType.HighCard => "Carta Alta",
                    HandType.Pair => "Par",
                    HandType.TwoPair => "Dois Pares",
                    HandType.ThreeOfAKind => "Trinca",
                    HandType.Straight => "Sequência",
                    HandType.Flush => "Flush",
                    HandType.FullHouse => "Full House",
                    HandType.FourOfAKind => "Quadra",
                    HandType.StraightFlush => "Straight Flush",
                    _ => prediction.Type.ToString()
                };

                int w = Raylib.MeasureText($"Mão Ativa: {handName}", 30);
                Raylib.DrawText($"Mão Ativa: {handName}", (GameConfig.WindowWidth - w) / 2, 410, 30, Color.Gold);
            }
            catch { }
        }

        foreach (var card in game.PlayerHand)
        {
            RendererUtils.DrawCard(card);
        }

        // Action and Sort Buttons positioned BELOW the cards
        int actionBtnY = 660; // Just below the cards (which sit around baseY=500, CardHeight=150 -> 650 is the bottom)

        // Draw background panel for bottom HUD
        Raylib.DrawRectangle(0, 650, GameConfig.WindowWidth, 70, new Color(20, 40, 30, 255));
        Raylib.DrawLine(0, 650, GameConfig.WindowWidth, 650, Color.DarkGray);

        if (RendererUtils.DrawButton("JOGAR MÃO", GameConfig.WindowWidth / 2 + 100, actionBtnY, 220, 45))
            game.PlayHand();

        if (RendererUtils.DrawButton("DESCARTAR", GameConfig.WindowWidth / 2 + 340, actionBtnY, 220, 45))
            game.DiscardSelected();

        if (RendererUtils.DrawButton("ORDENAR VALOR", GameConfig.WindowWidth / 2 - 300, actionBtnY, 200, 45))
            game.SortHandByRank();

        if (RendererUtils.DrawButton("ORDENAR NAIPE", GameConfig.WindowWidth / 2 - 520, actionBtnY, 200, 45))
            game.SortHandBySuit();
    }

    private static void DrawHUD(GameManager game)
    {
        // Current/Target Score
        Raylib.DrawText($"SCORE", 50, 50, 30, Color.White);
        Raylib.DrawText($"{game.CurrentScore} / {GameConfig.TargetScore}", 50, 80, 40, Color.Red);

        // Hands / Discards
        Raylib.DrawText($"MAOS: {game.HandsLeft}", 50, 200, 30, Color.Blue);
        Raylib.DrawText($"DESCARTES: {game.DiscardsLeft}", 50, 240, 30, Color.Orange);

        // Jokers
        int startX = 400;
        for (int i = 0; i < game.SelectedJokers.Count; i++)
        {
            RendererUtils.DrawJoker(game.SelectedJokers[i], startX + i * 140, 50);
        }

        // Consumables in top right
        int startConsumableX = GameConfig.WindowWidth - 300;
        for (int i = 0; i < game.Consumables.Count; i++)
        {
            var item = game.Consumables[i];
            float ix = startConsumableX + i * 120;
            float iy = 150;

            // Draw scale down consumable
            Rlgl.PushMatrix();
            Rlgl.Translatef(ix, iy, 0); // Translate to center
            Rlgl.Scalef(0.4f, 0.4f, 1f);

            var rectC = new Rectangle(-75, -120, 150, 240); // Draw from local center
            Raylib.DrawRectangleRounded(rectC, 0.1f, 10, item.Type == ConsumableType.Planet ? Color.SkyBlue : Color.Purple);
            Raylib.DrawRectangleLinesEx(rectC, 3f, Color.Black);
            Raylib.DrawText(item.Type.ToString().ToUpper(), -65, -110, 15, Color.White);
            Raylib.DrawText(item.Name, -65, -80, 20, Color.Yellow);
            Rlgl.PopMatrix();

            // Use button below consumable
            if (RendererUtils.DrawButton("USE", ix - 30, iy + 60, 60, 25))
            {
                // Play sound
                AudioEngine.PlayScore();

                // Discard the selected cards if it's a Tarot
                if (item.Type == ConsumableType.Tarot)
                {
                    // In lack of Enhanced Cards right now, we just "consume" the item and play sound.
                }

                // Remove item from inventory
                game.Consumables.RemoveAt(i);
                return; // Early return to avoid Collection Modified exception since we are inside drawing loop
            }
        }
    }

    private static void DrawScoreAnimation(GameManager game)
    {
        DrawGameplay(game); // Draw background behind it

        // Draw translucent dark panel
        Raylib.DrawRectangle(0, 0, GameConfig.WindowWidth, GameConfig.WindowHeight, new Color(0, 0, 0, 180));

        // Score display
        Raylib.DrawText(game.LastHandType, GameConfig.WindowWidth / 2 - 150, 200, 50, Color.Gold);

        string scoreText = $"{game.LastScoreState?.Chips} X {game.LastScoreState?.Multiplier}";
        Raylib.DrawText(scoreText, GameConfig.WindowWidth / 2 - 150, 300, 60, Color.Red);

        string totalText = $"+ {game.LastScorePlayed} PONTOS";
        Raylib.DrawText(totalText, GameConfig.WindowWidth / 2 - 200, 400, 50, Color.Green);

        if (RendererUtils.DrawButton("CONTINUAR", GameConfig.WindowWidth / 2 - 100, 550, 200, 50))
        {
            game.FinishScoreAnimation();
        }
    }

    private static void DrawGameOver(GameManager game)
    {
        if (game.CurrentScore >= GameConfig.TargetScore)
        {
            Raylib.DrawText("VITÓRIA!", GameConfig.WindowWidth / 2 - 200, 200, 80, Color.Gold);
        }
        else
        {
            Raylib.DrawText("GAME OVER!", GameConfig.WindowWidth / 2 - 250, 200, 80, Color.Red);
        }

        Raylib.DrawText($"Pontuação Final: {game.CurrentScore}", GameConfig.WindowWidth / 2 - 200, 350, 40, Color.White);

        if (RendererUtils.DrawButton("VOLTAR AO MENU", GameConfig.WindowWidth / 2 - 150, 500, 300, 50))
        {
            game.StartGame();
        }
    }
}
