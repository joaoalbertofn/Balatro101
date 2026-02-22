using Raylib_cs;
using System.Numerics;
using Balatro101.Game.Core;

namespace Balatro101.Game.UI;

public static class RendererUtils
{
    public const int CardWidth = 100;
    public const int CardHeight = 150;

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

        string rankStr = card.Rank switch
        {
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ace => "A",
            _ => ((int)card.Rank).ToString("00")
        };
        string suitStr = card.Suit.ToString().ToLower();
        string texKey = $"card_{suitStr}_{rankStr}";

        var tex = AssetManager.GetTexture(texKey);

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
        var rect = new Rectangle(x, y, width, height);

        // Drop shadow
        Raylib.DrawRectangleRounded(new Rectangle(x + 5, y + 5, width, height), 0.1f, 10, new Color(0, 0, 0, 80));

        var tex = AssetManager.GetTexture(joker.TextureKey);

        if (tex.HasValue)
        {
            var sourceRec = new Rectangle(0, 0, tex.Value.Width, tex.Value.Height);
            float targetWidth = width;
            float targetHeight = targetWidth * ((float)tex.Value.Height / tex.Value.Width);
            var destRec = new Rectangle(x, y, targetWidth, targetHeight);

            Raylib.DrawTexturePro(tex.Value, sourceRec, destRec, Vector2.Zero, 0f, Color.White);

            if (isSelected)
            {
                Raylib.DrawRectangleLinesEx(destRec, 4f, Color.Green);
            }
        }
        else
        {
            // Card Base
            Raylib.DrawRectangleRounded(rect, 0.1f, 10, Color.LightGray);
            Color borderColor = isSelected ? Color.Green : Color.Black;
            Raylib.DrawRectangleLinesEx(rect, isSelected ? 4f : 2f, borderColor);

            // Name
            int titleFontSize = (int)(16 * scale);
            // Center name
            int nameW = Raylib.MeasureText(joker.Name, titleFontSize);
            Raylib.DrawText(joker.Name, (int)(x + width / 2 - nameW / 2), (int)(y + 15 * scale), titleFontSize, Color.Black);

            // Logo
            int logoFontSize = (int)(26 * scale);
            int logoW = Raylib.MeasureText("JOKER", logoFontSize);
            Raylib.DrawText("JOKER", (int)(x + width / 2 - logoW / 2), (int)(y + 70 * scale), logoFontSize, Color.DarkBlue);

            // Description word wrap (smarter)
            string[] words = joker.Description.Split(' ');
            float currentY = y + 130 * scale;
            string currentLine = "";
            int descFontSize = (int)(13 * scale);
            int maxLineWidth = (int)(width - 20 * scale); // 10 padding each side

            foreach (var word in words)
            {
                string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                int testWidth = Raylib.MeasureText(testLine, descFontSize);

                if (testWidth < maxLineWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    // Draw current line and move to next
                    int lineW = Raylib.MeasureText(currentLine, descFontSize);
                    Raylib.DrawText(currentLine, (int)(x + width / 2 - lineW / 2), (int)(currentY), descFontSize, Color.DarkGray);

                    currentLine = word;
                    currentY += 16 * scale;
                }
            }
            // Draw remainder
            if (currentLine.Length > 0)
            {
                int lineW = Raylib.MeasureText(currentLine, descFontSize);
                Raylib.DrawText(currentLine, (int)(x + width / 2 - lineW / 2), (int)(currentY), descFontSize, Color.DarkGray);
            }
        }
    }

    public static bool DrawButton(string text, float x, float y, float width, float height)
    {
        return DrawButton(text, x, y, width, height, Color.Gray, Color.DarkGray);
    }

    public static bool DrawButton(string text, float x, float y, float width, float height, Color baseColor, Color hoverColor)
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        Rectangle rect = new Rectangle(x, y, width, height);
        bool isHovered = Raylib.CheckCollisionPointRec(mousePos, rect);

        Raylib.DrawRectangleRec(rect, isHovered ? hoverColor : baseColor);
        Raylib.DrawRectangleLinesEx(rect, 2, Color.Black);

        int textW = Raylib.MeasureText(text, 20);
        Raylib.DrawText(text, (int)(x + width / 2 - textW / 2), (int)(y + height / 2 - 10), 20, Color.White);

        if (isHovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            return true;
        }

        return false;
    }

    public static void DrawConsumable(IConsumable item, float x, float y, bool isHovered, float scale = 1.0f)
    {
        float animScale = isHovered ? scale * 1.05f : scale;
        float width = 150 * animScale;
        float height = 240 * animScale;

        // Shadow
        Raylib.DrawRectangleRounded(new Rectangle(x + 5, y + 5, width, height), 0.1f, 10, new Color(0, 0, 0, 100));

        var rect = new Rectangle(x, y, width, height);
        var tex = AssetManager.GetTexture(item.TextureKey);

        if (tex.HasValue)
        {
            var sourceRec = new Rectangle(0, 0, tex.Value.Width, tex.Value.Height);
            float targetWidth = width;
            float targetHeight = targetWidth * ((float)tex.Value.Height / tex.Value.Width);
            var destRec = new Rectangle(x, y, targetWidth, targetHeight);

            Raylib.DrawTexturePro(tex.Value, sourceRec, destRec, Vector2.Zero, 0f, Color.White);

            if (isHovered)
            {
                Raylib.DrawRectangleLinesEx(destRec, 4f, Color.Yellow);
            }
        }
        else
        {
            Color bgColor = item.Type == ConsumableType.Planet ? Color.SkyBlue : Color.Purple;

            Raylib.DrawRectangleRounded(rect, 0.1f, 10, bgColor);
            Raylib.DrawRectangleLinesEx(rect, 3f, Color.Black);

            // Header Title
            int typeFontSize = (int)(15 * animScale);
            string typeStr = item.Type.ToString().ToUpper();
            int typeW = Raylib.MeasureText(typeStr, typeFontSize);
            Raylib.DrawText(typeStr, (int)(x + width / 2 - typeW / 2), (int)(y + 10 * animScale), typeFontSize, Color.White);

            // Name
            int nameFontSize = (int)(22 * animScale);
            int nameW = Raylib.MeasureText(item.Name, nameFontSize);
            Raylib.DrawText(item.Name, (int)(x + width / 2 - nameW / 2), (int)(y + 40 * animScale), nameFontSize, Color.Yellow);

            // Word wrap description
            string[] words = item.Description.Split(' ');
            float currentY = y + 100 * animScale;
            string currentLine = "";
            int descFontSize = (int)(15 * animScale);
            int maxLineWidth = (int)(width - 20 * animScale);

            foreach (var word in words)
            {
                string testLine = currentLine.Length == 0 ? word : currentLine + " " + word;
                int testWidth = Raylib.MeasureText(testLine, descFontSize);

                if (testWidth < maxLineWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    int lineW = Raylib.MeasureText(currentLine, descFontSize);
                    Raylib.DrawText(currentLine, (int)(x + width / 2 - lineW / 2), (int)(currentY), descFontSize, Color.White);
                    currentLine = word;
                    currentY += 20 * animScale;
                }
            }
            if (currentLine.Length > 0)
            {
                int lineW = Raylib.MeasureText(currentLine, descFontSize);
                Raylib.DrawText(currentLine, (int)(x + width / 2 - lineW / 2), (int)(currentY), descFontSize, Color.White);
            }
        }
    }
}
