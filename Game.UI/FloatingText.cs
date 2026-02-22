using System;
using System.Numerics;
using Raylib_cs;

namespace Balatro101.Game.UI;

public class FloatingText
{
    public string Text { get; }
    public Color TextColor { get; }
    public Vector2 StartPos { get; }
    public Vector2 EndPos { get; }

    public float Lifetime { get; }
    public float Age { get; private set; }

    public bool IsDead => Age >= Lifetime;

    // Bezier control point offset to make it fly in an arc
    private Vector2 _controlPoint;

    // Optional scale bump on impact
    public bool HitTarget { get; private set; }

    public FloatingText(string text, Color color, Vector2 start, Vector2 end, float lifetime = 0.6f)
    {
        Text = text;
        TextColor = color;
        StartPos = start;
        EndPos = end;
        Lifetime = lifetime;
        Age = 0f;
        HitTarget = false;

        // Create an arc by shifting the control point upwards perpendicular to the path
        Vector2 midPoint = (start + end) / 2f;
        Vector2 direct = end - start;
        Vector2 perp = new Vector2(-direct.Y, direct.X);
        if (perp.LengthSquared() > 0)
            perp = Vector2.Normalize(perp);

        // Control point goes up by 150 pixels to ensure a nice arc
        _controlPoint = midPoint + perp * 150f;
        // If it goes too high out of screen, invert it
        if (_controlPoint.Y < 0)
        {
            _controlPoint = midPoint - perp * 150f;
        }
    }

    public void Update(float dt)
    {
        if (IsDead) return;
        Age += dt;
        if (Age >= Lifetime)
        {
            Age = Lifetime;
            HitTarget = true;
        }
    }

    public void Draw()
    {
        if (IsDead) return;

        float t = Age / Lifetime; // 0.0 to 1.0

        // Ease Out Cubic: fast start, slow end
        float easeT = 1f - MathF.Pow(1f - t, 3f);

        // Quadratic Bezier interpolation
        Vector2 p0 = StartPos;
        Vector2 p1 = _controlPoint;
        Vector2 p2 = EndPos;

        Vector2 q0 = Vector2.Lerp(p0, p1, easeT);
        Vector2 q1 = Vector2.Lerp(p1, p2, easeT);
        Vector2 currentPos = Vector2.Lerp(q0, q1, easeT);

        // Fade out slightly at the very end to prevent a harsh pop
        float alpha = 1f;
        if (t > 0.8f)
        {
            alpha = 1f - ((t - 0.8f) / 0.2f); // fade 1 to 0 over the last 20%
        }

        Color drawColor = new Color((int)TextColor.R, (int)TextColor.G, (int)TextColor.B, (int)(255 * alpha));

        // Draw shadow
        Raylib.DrawText(Text, (int)currentPos.X + 2, (int)currentPos.Y + 2, 24, new Color(0, 0, 0, (int)(150 * alpha)));
        // Draw text
        Raylib.DrawText(Text, (int)currentPos.X, (int)currentPos.Y, 24, drawColor);
    }
}
