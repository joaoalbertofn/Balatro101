using Raylib_cs;
using System.Numerics;
using Balatro101.Game.Core;
using Balatro101.Game.Engine;

namespace Balatro101.Game.UI;

public static class RendererUtils
{
    public const int CardWidth = 130;
    public const int CardHeight = 195;

    public static string? QueuedTooltipTitle;
    public static string? QueuedTooltipDesc;

    private static string? _cachedTooltipDesc;
    private static string[] _cachedTooltipWords = Array.Empty<string>();

    public static void DrawTooltip()
    {
        if (string.IsNullOrEmpty(QueuedTooltipTitle)) return;

        Vector2 mousePos = InputManager.VirtualCursor;
        int width = 200;
        int padding = 10;

        // Cache Word Wrap arrays to eliminate GC pressure
        if (QueuedTooltipDesc != _cachedTooltipDesc)
        {
            _cachedTooltipDesc = QueuedTooltipDesc;
            _cachedTooltipWords = QueuedTooltipDesc?.Split(' ') ?? Array.Empty<string>();
        }

        string[] words = _cachedTooltipWords;
        int fontSizeDesc = 15;
        int maxW = width - padding * 2;

        int lines = 1;
        string currentLine = "";
        foreach (var w in words)
        {
            string test = currentLine.Length == 0 ? w : currentLine + " " + w;
            if (Raylib.MeasureText(test, fontSizeDesc) < maxW)
            {
                currentLine = test;
            }
            else
            {
                lines++;
                currentLine = w;
            }
        }

        int height = 50 + lines * 20;

        float x = mousePos.X + 15;
        float y = mousePos.Y + 15;

        // Keep it on screen
        if (x + width > GameConfig.WindowWidth) x = GameConfig.WindowWidth - width - 5;
        if (y + height > GameConfig.WindowHeight) y = GameConfig.WindowHeight - height - 5;

        Raylib.DrawRectangleRounded(new Rectangle(x, y, width, height), 0.1f, 10, new Color(30, 30, 35, 255));
        Raylib.DrawRectangleRoundedLines(new Rectangle(x, y, width, height), 0.1f, 10, Color.Black);

        int titleW = Raylib.MeasureText(QueuedTooltipTitle, 20);
        Raylib.DrawText(QueuedTooltipTitle, (int)x + (width - titleW) / 2, (int)y + padding, 20, Color.Gold);

        currentLine = "";
        float currY = y + 40;
        foreach (var word in words)
        {
            string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
            if (Raylib.MeasureText(testLine, fontSizeDesc) < maxW)
            {
                currentLine = testLine;
            }
            else
            {
                int lineW = Raylib.MeasureText(currentLine, fontSizeDesc);
                Raylib.DrawText(currentLine, (int)x + (width - lineW) / 2, (int)currY, fontSizeDesc, Color.White);
                currentLine = word;
                currY += 20;
            }
        }
        if (currentLine.Length > 0)
        {
            int lineW = Raylib.MeasureText(currentLine, fontSizeDesc);
            Raylib.DrawText(currentLine, (int)x + (width - lineW) / 2, (int)currY, fontSizeDesc, Color.White);
        }

        QueuedTooltipTitle = null;
        QueuedTooltipDesc = null;
    }

    public static void DrawCard(Card card)
    {
        // Procedural Drop Shadow (dark rectangle offset by scale/height)
        float shadowOffset = 10f * card.Scale;
        var shadowRect = new Rectangle(card.X + shadowOffset, card.Y + shadowOffset, CardWidth * card.Scale, CardHeight * card.Scale);
        Rlgl.PushMatrix();
        // Move to shadow center for rotation
        Rlgl.Translatef(card.X + (CardWidth * card.Scale) / 2 + shadowOffset, card.Y + (CardHeight * card.Scale) / 2 + shadowOffset, 0);
        Rlgl.Rotatef(card.Rotation, 0, 0, 1);
        // Draw shadow from center
        Raylib.DrawRectangleRounded(new Rectangle(-(CardWidth * card.Scale) / 2, -(CardHeight * card.Scale) / 2, CardWidth * card.Scale, CardHeight * card.Scale), 0.1f, 10, new Color(10, 10, 10, 100));
        Rlgl.PopMatrix();

        Rlgl.PushMatrix();

        // Translate to card center
        Rlgl.Translatef(card.X + (CardWidth * card.Scale) / 2, card.Y + (CardHeight * card.Scale) / 2, 0);
        Rlgl.Rotatef(card.Rotation, 0, 0, 1);
        Rlgl.Scalef(card.Scale, card.Scale, 1f);

        // Now draw everything around local origin (0,0 is center of card)
        float localX = -CardWidth / 2f;
        float localY = -CardHeight / 2f;

        var tex = AssetManager.GetTexture(card.TextureKey);

        Color borderColor = card.IsSelected ? Color.Blue : Color.Black;
        float thickness = card.IsSelected ? 4f : (tex.HasValue ? 0f : 2f);

        if (tex.HasValue)
        {
            var sourceRec = new Rectangle(0, 0, tex.Value.Width, tex.Value.Height);
            float targetWidth = CardWidth;
            float targetHeight = targetWidth * ((float)tex.Value.Height / tex.Value.Width);
            float yOffset = (CardHeight - targetHeight) / 2f;
            var destRec = new Rectangle(localX, localY + yOffset, targetWidth, targetHeight);

            Raylib.DrawTexturePro(tex.Value, sourceRec, destRec, Vector2.Zero, 0f, Color.White);

            if (thickness > 0)
            {
                Raylib.DrawRectangleLinesEx(destRec, thickness, borderColor);
            }

            if (card.IsHovered)
            {
                QueuedTooltipTitle = $"{card.Rank} de {card.Suit}";
                QueuedTooltipDesc = $"Fichas (Chips): {card.GetValue() + card.BonusChips}\nMult: {card.BonusMult}";
                if (card.IsFoil) QueuedTooltipDesc += "\n(Foil)";
                if (card.IsHolographic) QueuedTooltipDesc += "\n(Holographic)";
                if (card.IsPolychrome) QueuedTooltipDesc += "\n(Polychrome)";
            }

            if (card.IsFoil || card.IsHolographic || card.IsPolychrome)
            {
                Color fxColor = card.IsPolychrome ? Color.SkyBlue : (card.IsHolographic ? Color.Purple : Color.LightGray);
                string fxText = card.IsPolychrome ? "POLY" : (card.IsHolographic ? "HOLO" : "FOIL");

                var tagRec = new Rectangle(destRec.X + destRec.Width - 45, destRec.Y - 5, 50, 20);
                Raylib.DrawRectangleRounded(tagRec, 0.5f, 5, fxColor);
                Raylib.DrawRectangleRoundedLines(tagRec, 0.5f, 5, Color.Black);

                int textW = Raylib.MeasureText(fxText, 10);
                Raylib.DrawText(fxText, (int)(tagRec.X + 25 - textW / 2), (int)(tagRec.Y + 5), 10, Color.Black);
            }
        }
        else
        {
            var rect = new Rectangle(localX, localY, CardWidth, CardHeight);
            Raylib.DrawRectangleRounded(rect, 0.1f, 10, Color.White);

            Raylib.DrawRectangleLinesEx(rect, thickness, borderColor);

            Color textColor = card.Suit switch
            {
                Suit.Hearts => Color.Red,
                Suit.Diamonds => Color.Blue,
                Suit.Clubs => new Color(0, 150, 0, 255), // Dark Green
                Suit.Spades => Color.Black,
                _ => Color.Black
            };

            string procRankStr = card.Rank switch
            {
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                Rank.Ace => "A",
                _ => ((int)card.Rank).ToString()
            };

            Raylib.DrawText(procRankStr, (int)localX + 10, (int)localY + 10, 20, textColor);
            int w = Raylib.MeasureText(procRankStr, 40);
            Raylib.DrawText(procRankStr, (int)localX + (CardWidth - w) / 2, (int)localY + 50, 40, textColor);

            switch (card.Suit)
            {
                case Suit.Hearts:
                    DrawHeart((int)localX + 10, (int)localY + 35, 10, textColor);
                    DrawHeart((int)localX + CardWidth / 2, (int)localY + 110, 20, textColor);
                    break;
                case Suit.Diamonds:
                    DrawDiamond((int)localX + 10, (int)localY + 35, 10, textColor);
                    DrawDiamond((int)localX + CardWidth / 2, (int)localY + 110, 20, textColor);
                    break;
                case Suit.Clubs:
                    DrawClub((int)localX + 10, (int)localY + 35, 10, textColor);
                    DrawClub((int)localX + CardWidth / 2, (int)localY + 110, 20, textColor);
                    break;
                case Suit.Spades:
                    DrawSpade((int)localX + 10, (int)localY + 35, 10, textColor);
                    DrawSpade((int)localX + CardWidth / 2, (int)localY + 110, 20, textColor);
                    break;
            }
        }

        Rlgl.PopMatrix();
    }

    private static void DrawHeart(int cx, int cy, int size, Color color)
    {
        Raylib.DrawCircle(cx - size / 2, cy - size / 2, size / 2 + 1, color);
        Raylib.DrawCircle(cx + size / 2, cy - size / 2, size / 2 + 1, color);
        Raylib.DrawTriangle(
            new Vector2(cx - size, cy - size / 4),
            new Vector2(cx, cy + size),
            new Vector2(cx + size, cy - size / 4), color);
    }

    private static void DrawDiamond(int cx, int cy, int size, Color color)
    {
        Raylib.DrawTriangle(new Vector2(cx, cy - size), new Vector2(cx - size, cy), new Vector2(cx + size, cy), color);
        Raylib.DrawTriangle(new Vector2(cx - size, cy), new Vector2(cx, cy + size), new Vector2(cx + size, cy), color);
    }

    private static void DrawClub(int cx, int cy, int size, Color color)
    {
        Raylib.DrawCircle(cx, cy - size / 2, size / 2 + 1, color);
        Raylib.DrawCircle(cx - size / 2, cy, size / 2 + 1, color);
        Raylib.DrawCircle(cx + size / 2, cy, size / 2 + 1, color);
        Raylib.DrawRectangle(cx - size / 4, cy, size / 2, size, color);
    }

    private static void DrawSpade(int cx, int cy, int size, Color color)
    {
        Raylib.DrawTriangle(new Vector2(cx, cy - size), new Vector2(cx - size, cy), new Vector2(cx + size, cy), color);
        Raylib.DrawCircle(cx - size / 2, cy, size / 2 + 1, color);
        Raylib.DrawCircle(cx + size / 2, cy, size / 2 + 1, color);
        Raylib.DrawRectangle(cx - size / 4, cy, size / 2, size, color);
    }

    public static void DrawJoker(IJoker joker, float x, float y, bool isSelected = false, float scale = 1.0f)
    {
        float width = 140 * scale;
        float height = 210 * scale;
        var baseRect = new Rectangle(x, y, width, height);
        bool isHovered = Raylib.CheckCollisionPointRec(InputManager.VirtualCursor, baseRect);

        // 1. Scale Tween
        float animScale = isHovered ? scale * 1.05f : scale;
        width = 140 * animScale;
        height = 210 * animScale;

        // 2. Floating Wave
        float time = (float)Raylib.GetTime();
        float waveY = MathF.Sin(time * 2f + x * 0.01f) * 5f;
        y += waveY;

        // 3. Dynamic Shadows
        float shadowOffsetX = (x - GameConfig.WindowWidth / 2f) * 0.03f;
        Raylib.DrawRectangleRounded(new Rectangle(x + shadowOffsetX, y + 15, width, height), 0.1f, 10, new Color(0, 0, 0, 80));

        // 4. Fake 3D Rotation on hover
        float rot = 0f;
        if (isHovered)
        {
            float centerX = x + width / 2;
            float mouseDistanceX = InputManager.VirtualCursor.X - centerX;
            rot = mouseDistanceX * 0.05f;
        }

        Rlgl.PushMatrix();
        Rlgl.Translatef(x + width / 2, y + height / 2, 0);
        Rlgl.Rotatef(rot, 0, 0, 1);

        float localX = -width / 2;
        float localY = -height / 2;

        var tex = AssetManager.GetTexture(joker.TextureKey);

        if (tex.HasValue)
        {
            var sourceRec = new Rectangle(0, 0, tex.Value.Width, tex.Value.Height);
            float targetWidth = width;
            float targetHeight = targetWidth * ((float)tex.Value.Height / tex.Value.Width);
            var destRec = new Rectangle(localX, localY, targetWidth, targetHeight);

            Raylib.DrawTexturePro(tex.Value, sourceRec, destRec, Vector2.Zero, 0f, Color.White);

            if (isSelected) Raylib.DrawRectangleLinesEx(destRec, 4f, Color.Green);

            if (isHovered)
            {
                Raylib.DrawRectangleLinesEx(destRec, 3f, Color.Yellow);
                QueuedTooltipTitle = joker.Name;
                QueuedTooltipDesc = joker.Description;
            }
        }
        else
        {
            var localRect = new Rectangle(localX, localY, width, height);
            Raylib.DrawRectangleRounded(localRect, 0.1f, 10, Color.LightGray);
            Color borderColor = isSelected ? Color.Green : (isHovered ? Color.Yellow : Color.Black);
            Raylib.DrawRectangleLinesEx(localRect, isSelected ? 4f : 2f, borderColor);

            int titleFontSize = (int)(16 * animScale);
            int nameW = Raylib.MeasureText(joker.Name, titleFontSize);
            Raylib.DrawText(joker.Name, (int)(localX + width / 2 - nameW / 2), (int)(localY + 15 * animScale), titleFontSize, Color.Black);

            int logoFontSize = (int)(26 * animScale);
            int logoW = Raylib.MeasureText("JOKER", logoFontSize);
            Raylib.DrawText("JOKER", (int)(localX + width / 2 - logoW / 2), (int)(localY + 70 * animScale), logoFontSize, Color.DarkBlue);

            string[] words = joker.Description.Split(' ');
            float currentY = localY + 130 * animScale;
            string currentLine = "";
            int descFontSize = (int)(13 * animScale);
            int maxLineWidth = (int)(width - 20 * animScale);

            foreach (var word in words)
            {
                string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                if (Raylib.MeasureText(testLine, descFontSize) < maxLineWidth) currentLine = testLine;
                else
                {
                    int lineW = Raylib.MeasureText(currentLine, descFontSize);
                    Raylib.DrawText(currentLine, (int)(localX + width / 2 - lineW / 2), (int)(currentY), descFontSize, Color.DarkGray);
                    currentLine = word;
                    currentY += 16 * animScale;
                }
            }
            if (currentLine.Length > 0)
            {
                int lineW = Raylib.MeasureText(currentLine, descFontSize);
                Raylib.DrawText(currentLine, (int)(localX + width / 2 - lineW / 2), (int)(currentY), descFontSize, Color.DarkGray);
            }
        }

        Rlgl.PopMatrix();
    }

    public static bool DrawButton(string text, float x, float y, float width, float height)
    {
        return DrawButton(text, x, y, width, height, Color.Gray, Color.DarkGray);
    }

    public static bool DrawButton(string text, float x, float y, float width, float height, Color baseColor, Color hoverColor)
    {
        Vector2 mousePos = InputManager.VirtualCursor;
        Rectangle rect = new Rectangle(x, y, width, height);
        bool isHovered = Raylib.CheckCollisionPointRec(mousePos, rect);

        // Register Node
        UINavigator.RegisterNode(new UINode($"btn_{text}_{x}_{y}", rect, UINodeType.Button));

        Raylib.DrawRectangleRec(rect, isHovered ? hoverColor : baseColor);
        Raylib.DrawRectangleLinesEx(rect, 2, Color.Black);

        int textW = Raylib.MeasureText(text, 20);
        Raylib.DrawText(text, (int)(x + width / 2 - textW / 2), (int)(y + height / 2 - 10), 20, Color.White);

        if (isHovered && InputManager.IsSelectActionPressed())
        {
            return true;
        }

        return false;
    }

    public static void DrawConsumable(IConsumable item, float x, float y, bool isHovered, float scale = 1.0f)
    {
        // 1. Scale Tween
        float animScale = isHovered ? scale * 1.05f : scale;
        float width = 140 * animScale;
        float height = 210 * animScale;

        UINavigator.RegisterNode(new UINode($"item_{item.Name}_{x}_{y}", new Rectangle(x, y, 140 * scale, 210 * scale), UINodeType.Consumable));

        // 2. Floating Wave
        float time = (float)Raylib.GetTime();
        float waveY = MathF.Sin(time * 2f + x * 0.01f) * 5f;
        y += waveY;

        // 3. Dynamic Shadows
        float shadowOffsetX = (x - GameConfig.WindowWidth / 2f) * 0.03f;
        Raylib.DrawRectangleRounded(new Rectangle(x + shadowOffsetX, y + 15, width, height), 0.1f, 10, new Color(0, 0, 0, 80));

        // 4. Fake 3D Rotation on hover
        float rot = 0f;
        if (isHovered)
        {
            float centerX = x + width / 2;
            float mouseDistanceX = InputManager.VirtualCursor.X - centerX;
            rot = mouseDistanceX * 0.05f;
        }

        Rlgl.PushMatrix();
        Rlgl.Translatef(x + width / 2, y + height / 2, 0);
        Rlgl.Rotatef(rot, 0, 0, 1);

        float localX = -width / 2;
        float localY = -height / 2;

        var tex = AssetManager.GetTexture(item.TextureKey);

        if (tex.HasValue)
        {
            var sourceRec = new Rectangle(0, 0, tex.Value.Width, tex.Value.Height);
            float targetWidth = width;
            float targetHeight = targetWidth * ((float)tex.Value.Height / tex.Value.Width);
            var destRec = new Rectangle(localX, localY, targetWidth, targetHeight);

            Raylib.DrawTexturePro(tex.Value, sourceRec, destRec, Vector2.Zero, 0f, Color.White);

            if (isHovered)
            {
                Raylib.DrawRectangleLinesEx(destRec, 4f, Color.Yellow);
                QueuedTooltipTitle = item.Name;
                QueuedTooltipDesc = item.Description;
            }
        }
        else
        {
            var localRect = new Rectangle(localX, localY, width, height);
            Color bgColor = item.Type == ConsumableType.Planet ? Color.SkyBlue : Color.Purple;

            Raylib.DrawRectangleRounded(localRect, 0.1f, 10, bgColor);
            Raylib.DrawRectangleLinesEx(localRect, 3f, Color.Black);

            int typeFontSize = (int)(15 * animScale);
            string typeStr = item.Type.ToString().ToUpper();
            int typeW = Raylib.MeasureText(typeStr, typeFontSize);
            Raylib.DrawText(typeStr, (int)(localX + width / 2 - typeW / 2), (int)(localY + 10 * animScale), typeFontSize, Color.White);

            int nameFontSize = (int)(22 * animScale);
            int nameW = Raylib.MeasureText(item.Name, nameFontSize);
            Raylib.DrawText(item.Name, (int)(localX + width / 2 - nameW / 2), (int)(localY + 40 * animScale), nameFontSize, Color.Yellow);

            string[] words = item.Description.Split(' ');
            float currentY = localY + 100 * animScale;
            string currentLine = "";
            int descFontSize = (int)(15 * animScale);
            int maxLineWidth = (int)(width - 20 * animScale);

            foreach (var word in words)
            {
                string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                if (Raylib.MeasureText(testLine, descFontSize) < maxLineWidth) currentLine = testLine;
                else
                {
                    int lineW = Raylib.MeasureText(currentLine, descFontSize);
                    Raylib.DrawText(currentLine, (int)(localX + width / 2 - lineW / 2), (int)(currentY), descFontSize, Color.White);
                    currentLine = word;
                    currentY += 20 * animScale;
                }
            }
            if (currentLine.Length > 0)
            {
                int lineW = Raylib.MeasureText(currentLine, descFontSize);
                Raylib.DrawText(currentLine, (int)(localX + width / 2 - lineW / 2), (int)(currentY), descFontSize, Color.White);
            }
        }

        Rlgl.PopMatrix();
    }
}
