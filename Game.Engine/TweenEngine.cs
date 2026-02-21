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
    public static void UpdatePhysics(AnimatableItem item, float dt)
    {
        float x = item.X;
        float vx = item.VelocityX;
        ApplySpring(ref x, ref vx, item.TargetX, dt);
        item.X = x;
        item.VelocityX = vx;

        float y = item.Y;
        float vy = item.VelocityY;
        ApplySpring(ref y, ref vy, item.TargetY, dt);
        item.Y = y;
        item.VelocityY = vy;

        float rot = item.Rotation;
        float vrot = item.VelocityRotation;
        ApplySpring(ref rot, ref vrot, item.TargetRotation, dt);
        item.Rotation = rot;
        item.VelocityRotation = vrot;

        float scale = item.Scale;
        float vscale = item.VelocityScale;
        ApplySpring(ref scale, ref vscale, item.TargetScale, dt);
        item.Scale = scale;
        item.VelocityScale = vscale;
    }
}
