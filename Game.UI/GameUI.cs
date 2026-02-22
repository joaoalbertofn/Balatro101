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
        Raylib.DrawText("BALATRO do J.A.", GameConfig.WindowWidth / 2 - 250, 200, 60, Color.White);
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
        float startX = 280 + (GameConfig.WindowWidth - 280 - totalWidth) / 2; // Fixed spacing using 280 for left panel
        float baseY = GameConfig.WindowHeight - 250f;
        float dt = Raylib.GetFrameTime();

        // Find the topmost hovered card first
        int hoveredIndex = -1;
        for (int i = numCards - 1; i >= 0; i--)
        {
            var card = game.PlayerHand[i];
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
                hoveredIndex = i;
            }
        }

        // Apply hover state and sounds
        for (int i = 0; i < numCards; i++)
        {
            var card = game.PlayerHand[i];
            bool isNowHovered = (i == hoveredIndex);

            card.IsHovered = isNowHovered;

            if (isNowHovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                // If we are trying to select a new card, check the limit
                if (!card.IsSelected && game.PlayerHand.Count(c => c.IsSelected) >= GameConfig.MaxSelectedCards)
                {
                    // Play error sound or just ignore
                }
                else
                {
                    card.IsSelected = !card.IsSelected;
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

            TweenEngine.UpdatePhysics(card, dt);
        }
    }

    private static void DrawGameplay(GameManager game)
    {
        DrawLeftPanel(game, false);
        DrawTopRightComponents(game);

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

                int ExpectedScore = game.ScoringEngine.CalculateScore(prediction, out var tempState);

                int w = Raylib.MeasureText($"{handName}", 30);
                Raylib.DrawText($"{handName}", 280 + (GameConfig.WindowWidth - 280 - w) / 2, 380, 30, Color.Gold);

                string expectText = $"{tempState.Chips} X {tempState.Multiplier} = {ExpectedScore}";
                int hw = Raylib.MeasureText(expectText, 20);
                Raylib.DrawText(expectText, 280 + (GameConfig.WindowWidth - 280 - hw) / 2, 420, 20, Color.LightGray);
            }
            catch { }
        }

        foreach (var card in game.PlayerHand)
        {
            RendererUtils.DrawCard(card);
        }

        // Action and Sort Buttons positioned BELOW the cards
        int actionBtnY = GameConfig.WindowHeight - 70; // Just below the cards

        // Deck visual at bottom right
        Rectangle deckRect = new Rectangle(GameConfig.WindowWidth - 140, GameConfig.WindowHeight - 190, 110, 160);
        Raylib.DrawRectangleRounded(deckRect, 0.1f, 10, Color.LightGray);

        // Draw background panel for bottom HUD
        Raylib.DrawRectangle(280, GameConfig.WindowHeight - 80, GameConfig.WindowWidth - 280, 80, new Color(20, 40, 30, 255));
        Raylib.DrawLine(280, GameConfig.WindowHeight - 80, GameConfig.WindowWidth, GameConfig.WindowHeight - 80, Color.DarkGray);
        Raylib.DrawRectangleRoundedLines(deckRect, 0.1f, 10, Color.Black);
        Raylib.DrawText($"{game.Deck.Count}/52", GameConfig.WindowWidth - 105, GameConfig.WindowHeight - 25, 20, Color.White);

        int centerX = 280 + (GameConfig.WindowWidth - 280) / 2;

        int btnW = 160;
        int gap = 15;
        int totalW = (btnW * 4) + (gap * 3);
        int sx = centerX - (totalW / 2);

        if (RendererUtils.DrawButton("DESCARTAR", sx, actionBtnY, btnW, 45, new Color(200, 50, 50, 255), new Color(150, 30, 30, 255)))
            game.DiscardSelected();

        sx += btnW + gap;
        if (RendererUtils.DrawButton("POR RANK", sx, actionBtnY, btnW, 45))
            game.SortHandByRank();

        sx += btnW + gap;
        if (RendererUtils.DrawButton("POR NAIPE", sx, actionBtnY, btnW, 45))
            game.SortHandBySuit();

        sx += btnW + gap;
        if (RendererUtils.DrawButton("JOGAR MÃO", sx, actionBtnY, btnW, 45, new Color(50, 160, 80, 255), new Color(30, 110, 50, 255)))
            game.PlayHand();
    }

    public static void DrawLeftPanel(GameManager game, bool isShop)
    {
        // Dark Side Panel Background
        Raylib.DrawRectangle(0, 0, 280, GameConfig.WindowHeight, new Color(20, 30, 25, 255));
        Raylib.DrawLineEx(new Vector2(280, 0), new Vector2(280, GameConfig.WindowHeight), 4, new Color(40, 100, 120, 255));

        // Blind Info / Shop Header
        Raylib.DrawRectangle(10, 10, 260, 60, new Color(40, 60, 80, 255));
        if (isShop)
        {
            Raylib.DrawText("LOJA", 100, 25, 30, Color.Gold);
        }
        else
        {
            Raylib.DrawText("Small Blind", 60, 25, 30, Color.White);
            Raylib.DrawText($"Meta: {GameConfig.TargetScore}", 20, 80, 20, Color.Red);
            Raylib.DrawText("Recompensa: $$$", 20, 110, 20, Color.Gold);
        }

        // Round Score
        Raylib.DrawRectangle(10, 150, 260, 50, new Color(50, 50, 50, 255));
        Raylib.DrawText("Round Score", 20, 165, 20, Color.LightGray);
        Raylib.DrawText(game.CurrentScore.ToString(), 160, 160, 30, Color.White);

        if (!isShop)
        {
            // Run Info (Hands / Discards)
            Raylib.DrawRectangle(10, 400, 135, 120, new Color(60, 40, 40, 255));
            Raylib.DrawText("Run Info", 35, 410, 15, Color.LightGray);

            Raylib.DrawText($"Hands", 20, 440, 20, Color.SkyBlue);
            Raylib.DrawText($"{game.HandsLeft}", 105, 440, 20, Color.White);

            Raylib.DrawText($"Discards", 15, 480, 20, Color.Orange);
            Raylib.DrawText($"{game.DiscardsLeft}", 105, 480, 20, Color.White);
        }

        // Money
        Raylib.DrawRectangle(155, 400, 115, 60, new Color(40, 40, 40, 255));
        Raylib.DrawText($"${game.Money}", 170, 415, 30, Color.Gold);

        // Options
        if (RendererUtils.DrawButton("Opções", 10, GameConfig.WindowHeight - 160, 260, 40))
        {
            // Pause menu logic here later
        }

        // Ante / Round Info
        Raylib.DrawRectangle(10, GameConfig.WindowHeight - 100, 125, 60, new Color(30, 30, 30, 255));
        Raylib.DrawText("Ante", 50, GameConfig.WindowHeight - 95, 15, Color.LightGray);
        Raylib.DrawText($"{game.CurrentAnte}/8", 50, GameConfig.WindowHeight - 70, 20, Color.Gold);

        Raylib.DrawRectangle(145, GameConfig.WindowHeight - 100, 125, 60, new Color(30, 30, 30, 255));
        Raylib.DrawText("Round", 185, GameConfig.WindowHeight - 95, 15, Color.LightGray);
        Raylib.DrawText($"{game.CurrentBlind}", 195, GameConfig.WindowHeight - 70, 20, Color.Gold);
    }

    private static void DrawTopRightComponents(GameManager game)
    {
        // Jokers (Left to right near top, starting after left panel)
        int startX = 300;
        for (int i = 0; i < game.SelectedJokers.Count; i++)
        {
            RendererUtils.DrawJoker(game.SelectedJokers[i], startX + i * 110, 20, false, 0.75f);
        }

        // Consumables in top right
        int startConsumableX = GameConfig.WindowWidth - 280;
        for (int i = 0; i < game.Consumables.Count; i++)
        {
            var item = game.Consumables[i];
            float ix = startConsumableX + i * 120;
            float iy = 20;

            var rect = new Rectangle(ix, iy, 150 * 0.75f, 240 * 0.75f);
            bool isHovered = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), rect);

            RendererUtils.DrawConsumable(item, ix, iy, isHovered, 0.75f);

            if (RendererUtils.DrawButton("USE", ix + 5, iy + 190, 100, 30))
            {
                if (item.Type == ConsumableType.Tarot)
                {
                    // Action handled here
                }
                game.Consumables.RemoveAt(i);
                return;
            }
        }
    }

    private static void DrawScoreAnimation(GameManager game)
    {
        DrawGameplay(game); // Draw background behind it

        // Draw translucent dark panel overlaying only the game area (excluding left panel if possible, but let's just overlay all for focus)
        Raylib.DrawRectangle(0, 0, GameConfig.WindowWidth, GameConfig.WindowHeight, new Color(0, 0, 0, 200));

        // Let's draw a nice Cash Out looking box
        Rectangle box = new Rectangle(GameConfig.WindowWidth / 2 - 250, 150, 500, 300);
        Raylib.DrawRectangleRounded(box, 0.1f, 10, new Color(40, 40, 40, 255));
        Raylib.DrawRectangleRoundedLines(box, 0.1f, 10, Color.Gold);

        Raylib.DrawText(game.LastHandType, (int)box.X + 150, (int)box.Y + 30, 40, Color.Gold);

        string scoreText = $"{game.LastScoreState?.Chips} X {game.LastScoreState?.Multiplier}";
        Raylib.DrawText(scoreText, (int)box.X + 180, (int)box.Y + 100, 40, Color.Red);

        string totalText = $"+ {game.LastScorePlayed} PONTOS";
        Raylib.DrawText(totalText, (int)box.X + 130, (int)box.Y + 160, 40, Color.Green);

        if (RendererUtils.DrawButton("CONTINUAR", (int)box.X + 150, (int)box.Y + 230, 200, 50))
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
