using System;
using Raylib_cs;

namespace Balatro101.Game.Engine;

public enum UINodeType
{
    Button,
    Card,
    Joker,
    Consumable,
    BlindNav
}

public class UINode
{
    public string Id { get; set; }
    public Rectangle Bounds { get; set; }
    public UINodeType Type { get; set; }

    // Optional callbacks that are registered when creating the node hook
    public Action? OnSelect { get; set; }
    public Action? OnHover { get; set; }

    public float CenterX => Bounds.X + Bounds.Width / 2f;
    public float CenterY => Bounds.Y + Bounds.Height / 2f;

    public UINode(string id, Rectangle bounds, UINodeType type, Action? onSelect = null, Action? onHover = null)
    {
        Id = id;
        Bounds = bounds;
        Type = type;
        OnSelect = onSelect;
        OnHover = onHover;
    }
}
