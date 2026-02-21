using System;
using Balatro101.Game.Core;

namespace Balatro101.Game.Engine;

public static class TweenEngine
{
    // Spring physics configuration
    private const float SpringConstant = 250f;
    private const float DampingRatio = 15f;

    /// <summary>
    /// Applies spring physics to move a value towards a target.
    /// </summary>
    public static void ApplySpring(ref float current, ref float velocity, float target, float dt)
    {
        // Simple Hooke's law with damping: F = -k*x - c*v
        float displacement = current - target;
        float force = -SpringConstant * displacement - DampingRatio * velocity;

        velocity += force * dt;
        current += velocity * dt;
    }

    /// <summary>
    /// Updates all physics properties of a card.
    /// </summary>
    public static void UpdateCardPhysics(Card card, float dt)
    {
        float x = card.X;
        float vx = card.VelocityX;
        ApplySpring(ref x, ref vx, card.TargetX, dt);
        card.X = x;
        card.VelocityX = vx;

        float y = card.Y;
        float vy = card.VelocityY;
        ApplySpring(ref y, ref vy, card.TargetY, dt);
        card.Y = y;
        card.VelocityY = vy;

        float rot = card.Rotation;
        float vrot = card.VelocityRotation;
        ApplySpring(ref rot, ref vrot, card.TargetRotation, dt);
        card.Rotation = rot;
        card.VelocityRotation = vrot;

        float scale = card.Scale;
        float vscale = card.VelocityScale;
        ApplySpring(ref scale, ref vscale, card.TargetScale, dt);
        card.Scale = scale;
        card.VelocityScale = vscale;
    }
}
