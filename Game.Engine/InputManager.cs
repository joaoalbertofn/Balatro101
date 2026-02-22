using System;
using System.Numerics;
using Raylib_cs;

namespace Balatro101.Game.Engine;

public static class InputManager
{
    // Ignora toques muito leves no analógico para evitar "drift" indesejado
    private const float DEADZONE = 0.15f;

    // Velocidade do cursor em pixels por segundo
    private const float CURSOR_SPEED = 900.0f;

    // O estado do seu Cursor Virtual
    public static Vector2 VirtualCursor { get; private set; }

    // Indica se o jogador está usando o controle ou o mouse
    public static bool IsGamepadModeActive { get; private set; } = false;

    public static void Initialize(int startX, int startY)
    {
        VirtualCursor = new Vector2(startX, startY);
    }

    public static void ForceVirtualCursor(Vector2 position)
    {
        VirtualCursor = position;
    }

    // Retorna o ID do primeiro controle conectado (0 a 7) ou -1 se nenhum
    public static int GetAvailableGamepad()
    {
        for (int i = 0; i < 8; i++)
        {
            if (Raylib.IsGamepadAvailable(i)) return i;
        }
        return -1;
    }

    public static Vector2 CurrentNavDirection { get; private set; }

    public static void Update(float deltaTime)
    {
        // Fallback para Mouse: Se o mouse se mover, ele assume o controle do cursor virtual
        Vector2 mouseDelta = Raylib.GetMouseDelta();
        if (mouseDelta.LengthSquared() > 0 || Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            IsGamepadModeActive = false;
        }

        // Update Nav Direction with Cooldown
        navCooldown -= deltaTime;

        Vector2 dir = Vector2.Zero;
        if (navCooldown <= 0)
        {
            for (int gid = 0; gid < 8; gid++)
            {
                if (!Raylib.IsGamepadAvailable(gid)) continue;

                if (Raylib.IsGamepadButtonPressed(gid, GamepadButton.LeftFaceUp)) dir.Y = -1;
                else if (Raylib.IsGamepadButtonPressed(gid, GamepadButton.LeftFaceDown)) dir.Y = 1;
                else if (Raylib.IsGamepadButtonPressed(gid, GamepadButton.LeftFaceLeft)) dir.X = -1;
                else if (Raylib.IsGamepadButtonPressed(gid, GamepadButton.LeftFaceRight)) dir.X = 1;

                if (dir != Vector2.Zero) break;

                // Stick Threshold Logic
                float axisX = Raylib.GetGamepadAxisMovement(gid, GamepadAxis.LeftX);
                float axisY = Raylib.GetGamepadAxisMovement(gid, GamepadAxis.LeftY);

                if (axisX < -0.6f) dir.X = -1;
                else if (axisX > 0.6f) dir.X = 1;
                else if (axisY < -0.6f) dir.Y = -1;
                else if (axisY > 0.6f) dir.Y = 1;

                if (dir != Vector2.Zero) break;
            }

            if (dir != Vector2.Zero)
            {
                navCooldown = 0.2f; // Prevent rapid-fire scrolling
            }
        }

        CurrentNavDirection = dir;

        // Se usar qualquer botão direcional, ativa o modo gamepad
        if (CurrentNavDirection != Vector2.Zero || IsSelectActionPressed(false))
        {
            IsGamepadModeActive = true;
        }

        if (!IsGamepadModeActive)
        {
            VirtualCursor = Raylib.GetMousePosition();
        }
    }

    private static float navCooldown = 0f;

    // --- ABSTRAÇÃO DE AÇÕES ---

    // Retorna TRUE no frame exato em que o botão foi pressionado (não se segurar)
    public static bool IsSelectActionPressed(bool includeMouse = true)
    {
        bool gamepad = false;
        for (int gid = 0; gid < 8; gid++)
        {
            if (!Raylib.IsGamepadAvailable(gid)) continue;
            // A button (bottom) or Keyboard.A
            if (Raylib.IsGamepadButtonPressed(gid, GamepadButton.RightFaceDown))
            {
                gamepad = true;
                break;
            }
        }

        if (Raylib.IsKeyPressed(KeyboardKey.A) || Raylib.IsKeyPressed(KeyboardKey.Space)) return true;

        if (!includeMouse) return gamepad;
        return gamepad || Raylib.IsMouseButtonPressed(MouseButton.Left);
    }

    public static bool IsSelectActionDown()
    {
        bool gamepad = false;
        for (int gid = 0; gid < 8; gid++)
        {
            if (!Raylib.IsGamepadAvailable(gid)) continue;
            if (Raylib.IsGamepadButtonDown(gid, GamepadButton.RightFaceDown))
            {
                gamepad = true;
                break;
            }
        }
        return gamepad || Raylib.IsMouseButtonDown(MouseButton.Left);
    }

    public static bool IsSelectActionReleased()
    {
        bool gamepad = false;
        for (int gid = 0; gid < 8; gid++)
        {
            if (!Raylib.IsGamepadAvailable(gid)) continue;
            if (Raylib.IsGamepadButtonReleased(gid, GamepadButton.RightFaceDown))
            {
                gamepad = true;
                break;
            }
        }
        return gamepad || Raylib.IsMouseButtonReleased(MouseButton.Left);
    }

    public static bool IsBackActionPressed()
    {
        bool gamepad = false;
        for (int gid = 0; gid < 8; gid++)
        {
            if (!Raylib.IsGamepadAvailable(gid)) continue;
            if (Raylib.IsGamepadButtonPressed(gid, GamepadButton.RightFaceRight))
            {
                gamepad = true;
                break;
            }
        }
        return gamepad || Raylib.IsKeyPressed(KeyboardKey.Escape) || Raylib.IsMouseButtonPressed(MouseButton.Right);
    }

    // Botão 'Y' para descartar cartas
    public static bool IsDiscardActionPressed()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Y)) return true;

        for (int gid = 0; gid < 8; gid++)
        {
            if (!Raylib.IsGamepadAvailable(gid)) continue;
            // Top Face button (Y)
            if (Raylib.IsGamepadButtonPressed(gid, GamepadButton.RightFaceUp))
            {
                return true;
            }
        }
        return false;
    }

    // Botão 'X' para Jogar a Mão
    public static bool IsPlayActionPressed()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.X)) return true;

        for (int gid = 0; gid < 8; gid++)
        {
            if (!Raylib.IsGamepadAvailable(gid)) continue;
            // Left Face button (X)
            if (Raylib.IsGamepadButtonPressed(gid, GamepadButton.RightFaceLeft))
            {
                return true;
            }
        }
        return false;
    }
}
