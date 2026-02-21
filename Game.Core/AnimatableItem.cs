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
}
