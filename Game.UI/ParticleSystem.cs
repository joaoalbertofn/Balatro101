using System;
using System.Collections.Generic;
using Raylib_cs;
using System.Numerics;

namespace Balatro101.Game.UI;

public class Particle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Color Color;
    public float Life;
    public float MaxLife;
    public float Size;
    public float InitialSize;
    public float Rotation;
    public float RotSpeed;
}

public static class ParticleSystem
{
    private static readonly List<Particle> particles = new();
    private static readonly Random rng = new();

    public static void Emit(Vector2 position, int count, Color baseColor)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)rng.NextDouble() * MathF.PI * 2f;
            float speed = (float)rng.NextDouble() * 300f + 100f; // Explosion speed

            float life = (float)rng.NextDouble() * 1.5f + 0.5f;
            float size = (float)rng.NextDouble() * 8f + 4f;

            Color randColor = baseColor;
            if (rng.Next(0, 3) == 0) randColor = Color.White; // Add some spark variation

            particles.Add(new Particle
            {
                Position = position,
                Velocity = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed - 100f), // Slight upward bias
                Color = randColor,
                Life = life,
                MaxLife = life,
                Size = size,
                InitialSize = size,
                Rotation = (float)rng.NextDouble() * 360f,
                RotSpeed = (float)rng.NextDouble() * 400f - 200f
            });
        }
    }

    public static void EmitConfetti(Vector2 position, int count)
    {
        Color[] colors = { Color.Red, Color.Gold, Color.SkyBlue, Color.Green, Color.Purple, Color.Pink };
        for (int i = 0; i < count; i++)
        {
            Emit(position, 1, colors[rng.Next(colors.Length)]);
        }
    }

    public static void EmitScoreSpark(Vector2 position, int count, bool isMultiplier)
    {
        Color[] colors = isMultiplier
            ? new Color[] { Color.Red, new Color(255, 100, 100, 255), Color.Gold }
            : new Color[] { new Color(41, 128, 185, 255), Color.SkyBlue, Color.White };

        for (int i = 0; i < count; i++)
        {
            float angle = (float)rng.NextDouble() * MathF.PI * 2f;
            float speed = (float)rng.NextDouble() * 400f + 100f; // More explosive

            float life = (float)rng.NextDouble() * 2.0f + 0.5f;
            float size = (float)rng.NextDouble() * 12f + 6f;

            Color randColor = colors[rng.Next(colors.Length)];

            particles.Add(new Particle
            {
                Position = position,
                Velocity = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed - 150f), // Upward bias
                Color = randColor,
                Life = life,
                MaxLife = life,
                Size = size,
                InitialSize = size,
                Rotation = (float)rng.NextDouble() * 360f,
                RotSpeed = (float)rng.NextDouble() * 500f - 250f
            });
        }
    }

    public static void UpdateAndDraw(float dt)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            p.Life -= dt;

            if (p.Life <= 0)
            {
                particles.RemoveAt(i);
                continue;
            }

            // Gravity 
            p.Velocity.Y += 800f * dt;

            // Air friction
            p.Velocity.X *= MathF.Pow(0.8f, dt * 10f);

            p.Position += p.Velocity * dt;
            p.Rotation += p.RotSpeed * dt;

            // Shrink over time
            p.Size = p.InitialSize * (p.Life / p.MaxLife);

            // Fade alpha
            byte alpha = (byte)(255 * (p.Life / p.MaxLife));
            Color c = new Color(p.Color.R, p.Color.G, p.Color.B, alpha);

            Rlgl.PushMatrix();
            Rlgl.Translatef(p.Position.X, p.Position.Y, 0);
            Rlgl.Rotatef(p.Rotation, 0, 0, 1);
            Raylib.DrawRectangleRec(new Rectangle(-p.Size / 2, -p.Size / 2, p.Size, p.Size), c);
            Rlgl.PopMatrix();
        }
    }
}
