namespace Balatro101.Game.Core;

public abstract class AnimatableItem
{
    public float X { get; set; }
    public float TargetX { get; set; }
    public float VelocityX { get; set; }

    public float Y { get; set; }
    public float TargetY { get; set; }
    public float VelocityY { get; set; }

    public float Rotation { get; set; }
    public float TargetRotation { get; set; }
    public float VelocityRotation { get; set; }

    public float Scale { get; set; } = 1f;
    public float TargetScale { get; set; } = 1f;
    public float VelocityScale { get; set; }

    public bool IsHovered { get; set; }
    public bool IsSelected { get; set; }

    /// <summary>
    /// Instantly increases the scale so the spring physics can snap it back down.
    /// </summary>
    public void BumpScale(float amount = 1.3f)
    {
        Scale = amount;
    }

    /// <summary>
    /// Injects rotational velocity so the spring physics wobbles the item back to target rotation.
    /// </summary>
    public void Wobble(float velocity = 25f)
    {
        VelocityRotation = velocity;
    }
}
