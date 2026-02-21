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

        var rect = new Rectangle(localX, localY, CardWidth, CardHeight);
        Raylib.DrawRectangleRounded(rect, 0.1f, 10, Color.White);

        Color borderColor = card.IsSelected ? Color.Blue : Color.Black;
        float thickness = card.IsSelected ? 4f : 2f;
        Raylib.DrawRectangleLinesEx(rect, thickness, borderColor);

        Color textColor = card.Suit switch
        {
            Suit.Hearts => Color.Red,
            Suit.Diamonds => Color.Blue,
            Suit.Clubs => new Color(0, 150, 0, 255), // Dark Green
            Suit.Spades => Color.Black,
            _ => Color.Black
        };

        string rankStr = card.Rank switch
        {
            Rank.Jack => "J",
            Rank.Queen => "Q",
            Rank.King => "K",
            Rank.Ace => "A",
            _ => ((int)card.Rank).ToString()
        };

        Raylib.DrawText(rankStr, (int)localX + 10, (int)localY + 10, 20, textColor);
        int w = Raylib.MeasureText(rankStr, 40);
        Raylib.DrawText(rankStr, (int)localX + (CardWidth - w) / 2, (int)localY + 50, 40, textColor);

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

    public static void DrawJoker(IJoker joker, float x, float y, bool isSelected = false)
    {
        var rect = new Rectangle(x, y, 120, 160);
        Raylib.DrawRectangleRounded(rect, 0.1f, 10, Color.LightGray);

        Color borderColor = isSelected ? Color.Green : Color.Black;
        Raylib.DrawRectangleLinesEx(rect, isSelected ? 4f : 2f, borderColor);

        Raylib.DrawText(joker.Name, (int)x + 5, (int)y + 8, 12, Color.Black);
        Raylib.DrawText("JOKER", (int)x + 30, (int)y + 60, 20, Color.DarkBlue);

        string[] words = joker.Description.Split(' ');
        string line1 = "";
        string line2 = "";
        string line3 = "";
        foreach (var w in words)
        {
            if (line1.Length + w.Length < 16) line1 += w + " ";
            else if (line2.Length + w.Length < 16) line2 += w + " ";
            else line3 += w + " ";
        }
        Raylib.DrawText(line1.Trim(), (int)x + 5, (int)y + 100, 10, Color.DarkGray);
        if (line2.Length > 0)
            Raylib.DrawText(line2.Trim(), (int)x + 5, (int)y + 115, 10, Color.DarkGray);
        if (line3.Length > 0)
            Raylib.DrawText(line3.Trim(), (int)x + 5, (int)y + 130, 10, Color.DarkGray);
    }

    public static bool DrawButton(string text, float x, float y, float width, float height)
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        Rectangle rect = new Rectangle(x, y, width, height);
        bool isHovered = Raylib.CheckCollisionPointRec(mousePos, rect);

        Raylib.DrawRectangleRec(rect, isHovered ? Color.DarkGray : Color.Gray);
        Raylib.DrawRectangleLinesEx(rect, 2, Color.Black);

        int textW = Raylib.MeasureText(text, 20);
        Raylib.DrawText(text, (int)(x + width / 2 - textW / 2), (int)(y + height / 2 - 10), 20, Color.White);

        if (isHovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Balatro101.Game.Engine.AudioEngine.PlayBlip();
            return true;
        }

        return false;
    }
}
