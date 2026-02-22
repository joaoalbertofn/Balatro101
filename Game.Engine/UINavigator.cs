using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace Balatro101.Game.Engine;

public static class UINavigator
{
    private static List<UINode> currentNodes = new();
    private static string? focusedNodeId = null;

    // Registers a node for the current frame
    public static void RegisterNode(UINode node)
    {
        currentNodes.Add(node);
    }

    // Call this at the Start of UpdateAndDraw to clear the map
    public static void ClearNodes()
    {
        currentNodes.Clear();
    }

    // Call this before processing InputManager virtual cursor output
    public static void UpdateNavigation()
    {
        if (currentNodes.Count == 0) return;

        // Auto-focus if nothing is focused but we are in Gamepad Mode
        if (focusedNodeId == null || GetFocusedNode() == null)
        {
            focusedNodeId = currentNodes[0].Id;
        }

        // Handle Snapping Directions
        var padDir = InputManager.CurrentNavDirection;
        if (padDir != Vector2.Zero)
        {
            SnapToNextNode(padDir);
        }

        // Apply Focus to VirtualCursor
        var focus = GetFocusedNode();
        if (focus != null && InputManager.IsGamepadModeActive)
        {
            // Override the virtual cursor so checking hover bounds still "just works"
            InputManager.ForceVirtualCursor(new Vector2(focus.CenterX, focus.CenterY));

            if (focus.OnHover != null) focus.OnHover();

            // Intercept Actions
            if (InputManager.IsSelectActionPressed() && focus.OnSelect != null)
            {
                focus.OnSelect();
            }
        }
        else if (padDir != Vector2.Zero && !InputManager.IsGamepadModeActive)
        {
            // Ignored padDir because IsGamepadModeActive is false.
        }
    }

    private static UINode? GetFocusedNode()
    {
        return currentNodes.Find(n => n.Id == focusedNodeId);
    }

    private static void SnapToNextNode(Vector2 direction)
    {
        var current = GetFocusedNode();
        if (current == null) return;

        UINode? nearest = null;
        float shortestDistance = float.MaxValue;

        foreach (var node in currentNodes)
        {
            if (node.Id == current.Id) continue;

            Vector2 toNode = new Vector2(node.CenterX - current.CenterX, node.CenterY - current.CenterY);

            // Check if node is roughly in the desired direction using dot product
            float dot = Vector2.Dot(Vector2.Normalize(toNode), Vector2.Normalize(direction));

            if (dot > 0.5f) // Threshold to ensure it's "mostly" in that direction
            {
                float dist = toNode.LengthSquared();
                if (dist < shortestDistance)
                {
                    shortestDistance = dist;
                    nearest = node;
                }
            }
        }

        if (nearest != null)
        {
            focusedNodeId = nearest.Id;
        }
    }
}
