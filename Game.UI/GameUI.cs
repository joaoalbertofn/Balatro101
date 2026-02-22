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
        float dt = Raylib.GetFrameTime();

        // Screen Shake Logic
        float shakeOffsetX = 0f;
        float shakeOffsetY = 0f;
        if (game.ShakeTimer > 0)
        {
            game.ShakeTimer -= dt;
            float mag = game.ShakeMagnitude * (game.ShakeTimer / 0.5f); // Fade out shake
            shakeOffsetX = (float)(new Random().NextDouble() * 2f - 1f) * mag;
            shakeOffsetY = (float)(new Random().NextDouble() * 2f - 1f) * mag;
        }

        // Smooth card animations
        if (game.CurrentState is GameState.FaseGameplay or GameState.AnimacaoPontuacao)
            UpdateCardsLogic(game);

        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(30, 60, 40, 255));

        if (game.ShakeTimer > 0)
        {
            Rlgl.PushMatrix();
            Rlgl.Translatef(shakeOffsetX, shakeOffsetY, 0);
        }

        switch (game.CurrentState)
        {
            case GameState.MainMenu:
                DrawMainMenu(game);
                break;
            case GameState.FaseGameplay:
                DrawGameplay(game);
                break;
            case GameState.Shuffling:
                game.UpdateShuffling(dt);
                DrawShuffling(game);
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

        if (game.ShakeTimer > 0)
        {
            Rlgl.PopMatrix();
        }

        RendererUtils.DrawTooltip();

        // Draw the virtual cursor
        Raylib.DrawCircleV(InputManager.VirtualCursor, 8f, new Color(255, 100, 100, 200));
        Raylib.DrawCircleLines((int)InputManager.VirtualCursor.X, (int)InputManager.VirtualCursor.Y, 8f, Color.White);

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

        Vector2 mousePos = InputManager.VirtualCursor;
        int numCards = game.PlayerHand.Count;
        float spacing = 110f;
        float totalWidth = numCards * spacing;
        float startX = 280 + (GameConfig.WindowWidth - 280 - totalWidth) / 2; // Fixed spacing using 280 for left panel
        float baseY = GameConfig.WindowHeight - 290f; // Raised to avoid action bar clipping
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

            UINavigator.RegisterNode(new UINode($"hand_card_{i}", rect, UINodeType.Card));

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

            if (isNowHovered && InputManager.IsSelectActionPressed())
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

        // Prediction box moved to left panel

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

    private static void DrawShuffling(GameManager game)
    {
        // Background and UI
        Raylib.DrawRectangle(0, 0, GameConfig.WindowWidth, GameConfig.WindowHeight, new Color(42, 88, 55, 255));
        DrawLeftPanel(game, false);
        DrawTopRightComponents(game);

        float t = (float)Raylib.GetTime() * 15f;
        for (int i = 0; i < 52; i++)
        {
            float offsetX = MathF.Sin(t + i) * 6f;
            float offsetY = MathF.Cos(t + i * 2f) * 4f;
            float rot = MathF.Sin(t * 0.5f + i) * 3f;

            float x = GameConfig.WindowWidth - 150 + offsetX;
            float y = GameConfig.WindowHeight - 200 - i * 0.5f + offsetY;

            Rlgl.PushMatrix();
            Rlgl.Translatef(x, y, 0);
            Rlgl.Rotatef(rot, 0, 0, 1);

            Raylib.DrawRectangleRounded(new Rectangle(-RendererUtils.CardWidth / 2, -RendererUtils.CardHeight / 2, RendererUtils.CardWidth, RendererUtils.CardHeight), 0.05f, 10, Color.White);
            Raylib.DrawRectangleRoundedLines(new Rectangle(-RendererUtils.CardWidth / 2, -RendererUtils.CardHeight / 2, RendererUtils.CardWidth, RendererUtils.CardHeight), 0.05f, 10, Color.DarkGray);
            Raylib.DrawRectangle((int)(-RendererUtils.CardWidth / 2 + 10), (int)(-RendererUtils.CardHeight / 2 + 10), RendererUtils.CardWidth - 20, RendererUtils.CardHeight - 20, new Color(69, 143, 89, 255));

            Rlgl.PopMatrix();
        }
    }

    public static void DrawLeftPanel(GameManager game, bool isShop)
    {
        // Base Left Panel
        Raylib.DrawRectangle(0, 0, 270, GameConfig.WindowHeight, new Color(45, 52, 54, 255));
        Raylib.DrawRectangle(268, 0, 4, GameConfig.WindowHeight, new Color(41, 128, 185, 255));

        // Blind Header (Blue Pill or Red if Boss)
        Color headerColor = game.CurrentBlindType == BlindType.Boss ? new Color(180, 40, 40, 255) : new Color(41, 128, 185, 255);
        Raylib.DrawRectangleRounded(new Rectangle(15, 15, 240, 40), 0.5f, 10, headerColor);

        string blindName = game.CurrentBlindType == BlindType.Boss ? $"BOSS: {game.CurrentBossDebuff}" : $"{game.CurrentBlindType} Blind";
        int fontSize = game.CurrentBlindType == BlindType.Boss ? 20 : 26;
        int blindW = Raylib.MeasureText(blindName, fontSize);
        Raylib.DrawText(blindName, 15 + (240 - blindW) / 2, 15 + (40 - fontSize) / 2, fontSize, Color.White);

        // Score at Least (Dark Gray Box)
        Raylib.DrawRectangleRounded(new Rectangle(15, 65, 240, 75), 0.2f, 10, new Color(30, 30, 30, 255));
        string sAtLeast = "Score at least";
        Raylib.DrawText(sAtLeast, 15 + (240 - Raylib.MeasureText(sAtLeast, 18)) / 2, 75, 18, Color.LightGray);
        string targScore = GameConfig.TargetScore.ToString();
        Raylib.DrawText(targScore, 15 + (240 - Raylib.MeasureText(targScore, 30)) / 2, 95, 30, new Color(230, 60, 60, 255));
        string rwrd = "Reward: $$$";
        Raylib.DrawText(rwrd, 15 + (240 - Raylib.MeasureText(rwrd, 18)) / 2, 120, 18, Color.Gold);

        // Round Score
        Raylib.DrawRectangleRounded(new Rectangle(15, 150, 240, 50), 0.2f, 10, new Color(50, 50, 50, 255));
        Raylib.DrawText("Round score", 25, 165, 20, Color.LightGray);
        string cScoreStr = game.CurrentScore.ToString();
        int cw = Raylib.MeasureText(cScoreStr, 28);
        Raylib.DrawText(cScoreStr, 245 - cw, 160, 28, Color.White);

        if (!isShop)
        {
            var selectedCards = game.PlayerHand.Where(c => c.IsSelected).ToList();
            if (selectedCards.Count > 0 && selectedCards.Count <= GameConfig.MaxSelectedCards)
            {
                try
                {
                    var prediction = HandEvaluator.Evaluate(selectedCards);
                    string handName = prediction.Type.ToString(); // Keep simple switch logic
                    int ExpectedScore = game.ScoringEngine.CalculateScore(prediction, game.CurrentBossDebuff, out var tempState);

                    int predStartY = 215;
                    Raylib.DrawRectangleRounded(new Rectangle(15, predStartY, 240, 90), 0.2f, 10, new Color(30, 30, 30, 255));

                    int handNameW = Raylib.MeasureText(handName, 20);
                    Raylib.DrawText(handName, 15 + (240 - handNameW) / 2, predStartY + 10, 20, Color.White);

                    // Chips Pill (Blue)
                    Raylib.DrawRectangleRounded(new Rectangle(25, predStartY + 40, 95, 35), 0.5f, 10, new Color(41, 128, 185, 255));
                    string chipsStr = tempState.Chips.ToString();
                    Raylib.DrawText(chipsStr, 25 + (95 - Raylib.MeasureText(chipsStr, 22)) / 2, predStartY + 48, 22, Color.White);

                    // Mult Pill (Red)
                    Raylib.DrawRectangleRounded(new Rectangle(150, predStartY + 40, 95, 35), 0.5f, 10, new Color(230, 60, 60, 255));
                    string multStr = tempState.Multiplier.ToString();
                    Raylib.DrawText(multStr, 150 + (95 - Raylib.MeasureText(multStr, 22)) / 2, predStartY + 48, 22, Color.White);

                    Raylib.DrawText("X", 128, predStartY + 48, 20, Color.LightGray);
                }
                catch { }
            }

            // Run Info Complex Grouping
            int infoStartY = 350;
            // Red Run Info block
            Raylib.DrawRectangleRounded(new Rectangle(15, infoStartY, 70, 90), 0.2f, 10, new Color(230, 60, 60, 255));
            Raylib.DrawText("Run", 30, infoStartY + 25, 20, Color.White);
            Raylib.DrawText("Info", 28, infoStartY + 45, 20, Color.White);

            // Hands Box
            Raylib.DrawRectangleRounded(new Rectangle(95, infoStartY, 75, 40), 0.2f, 10, new Color(50, 50, 50, 255));
            Raylib.DrawText("Hands", 108, infoStartY + 3, 16, Color.SkyBlue);
            Raylib.DrawText(game.HandsLeft.ToString(), 125, infoStartY + 20, 22, Color.White);

            // Discards Box
            Raylib.DrawRectangleRounded(new Rectangle(180, infoStartY, 75, 40), 0.2f, 10, new Color(50, 50, 50, 255));
            Raylib.DrawText("Discards", 185, infoStartY + 3, 16, Color.Orange);
            Raylib.DrawText(game.DiscardsLeft.ToString(), 210, infoStartY + 20, 22, Color.White);

            // Money Box
            Raylib.DrawRectangleRounded(new Rectangle(95, infoStartY + 50, 160, 40), 0.2f, 10, new Color(50, 50, 50, 255));
            string cMoney = $"${game.Money}";
            Raylib.DrawText(cMoney, 95 + (160 - Raylib.MeasureText(cMoney, 26)) / 2, infoStartY + 58, 26, Color.Gold);
        }

        // Bottom Pillars
        int botY = GameConfig.WindowHeight - 80;

        // Options Yellow Pill
        Raylib.DrawRectangleRounded(new Rectangle(15, botY, 90, 60), 0.2f, 10, new Color(241, 169, 59, 255));
        if (RendererUtils.DrawButton("Opções", 20, botY + 15, 80, 30)) { } // empty for now

        // Ante Box
        Raylib.DrawRectangleRounded(new Rectangle(115, botY, 65, 60), 0.2f, 10, new Color(30, 30, 30, 255));
        Raylib.DrawText("Ante", 130, botY + 10, 15, Color.LightGray);
        Raylib.DrawText($"{game.CurrentAnte}/8", 125, botY + 30, 20, Color.Gold);

        // Round Box
        Raylib.DrawRectangleRounded(new Rectangle(190, botY, 65, 60), 0.2f, 10, new Color(30, 30, 30, 255));
        Raylib.DrawText("Round", 200, botY + 10, 15, Color.LightGray);
        Raylib.DrawText($"{game.CurrentBlind}", 215, botY + 30, 20, Color.White);
    }

    private static void DrawTopRightComponents(GameManager game)
    {
        // Jokers (Left to right near top, starting after left panel)
        int startX = 300;
        for (int i = 0; i < game.SelectedJokers.Count; i++)
        {
            RendererUtils.DrawJoker(game.SelectedJokers[i], startX + i * 125, 20, false, 0.85f);
        }

        // Consumables in top right
        int startConsumableX = GameConfig.WindowWidth - 300;
        for (int i = 0; i < game.Consumables.Count; i++)
        {
            var item = game.Consumables[i];
            float ix = startConsumableX + i * 125;
            float iy = 20;

            var rect = new Rectangle(ix, iy, 140 * 0.85f, 210 * 0.85f);
            bool isHovered = Raylib.CheckCollisionPointRec(InputManager.VirtualCursor, rect);

            RendererUtils.DrawConsumable(item, ix, iy, isHovered, 0.85f);

            if (RendererUtils.DrawButton("USE", ix + 5, iy + 190, 100, 30))
            {
                if (item.Type == ConsumableType.Tarot)
                {
                    var selectedCards = game.PlayerHand.Where(c => c.IsSelected).ToList();

                    if (selectedCards.Count > 0)
                    {
                        var tarot = (ITarot)item;
                        foreach (var c in selectedCards)
                        {
                            tarot.ApplyToCard(c);
                            c.IsSelected = false;

                            // Pop animation for visual feedback
                            c.Scale = 1.6f;
                            c.TargetScale = 1.0f; // TweenEngine returns it to normal
                            c.TargetRotation = 0f;
                            c.Rotation = (float)new Random().NextDouble() * 40f - 20f; // Spin
                            c.Y -= 50; // Jump up
                        }
                    }
                    else
                    {
                        // Can't use Tarot without selecting a card
                        return;
                    }
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

        // Explode particles when this state begins initially (hacky way: do it if timer just started)
        if (game.ShakeTimer > 0.48f) // Just triggered
        {
            // Emit chips on the left (blue) and mults on the right (red)
            ParticleSystem.EmitScoreSpark(new Vector2(box.X + 150, box.Y + 200), 30, false);
            ParticleSystem.EmitScoreSpark(new Vector2(box.X + 350, box.Y + 200), 30, true);
        }

        Raylib.DrawText(game.LastHandType, (int)box.X + 150, (int)box.Y + 30, 40, Color.Gold);

        string scoreText = $"{game.LastScoreState?.Chips} X {game.LastScoreState?.Multiplier}";
        Raylib.DrawText(scoreText, (int)box.X + 180, (int)box.Y + 100, 40, Color.Red);

        string totalText = $"+ {game.LastScorePlayed} PONTOS";
        Raylib.DrawText(totalText, (int)box.X + 130, (int)box.Y + 160, 40, Color.Green);

        ParticleSystem.UpdateAndDraw(Raylib.GetFrameTime());

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
