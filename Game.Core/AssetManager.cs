using System.Collections.Generic;
using Raylib_cs;

namespace Balatro101.Game.Core;

public static class AssetManager
{
    private static readonly Dictionary<string, Texture2D> _textures = new();
    private static bool _initialized = false;

    public static void LoadAll()
    {
        if (_initialized) return;

        // Face Cards
        LoadTexture("jack", "Assets/jack.png");
        LoadTexture("queen", "Assets/queen.png");
        LoadTexture("king", "Assets/king.png");
        LoadTexture("ace", "Assets/ace.png");

        // Keep adding others below as we generate them...

        _initialized = true;
    }

    private static void LoadTexture(string key, string path)
    {
        if (System.IO.File.Exists(path))
        {
            Texture2D tex = Raylib.LoadTexture(path);
            _textures[key] = tex;
        }
        else
        {
            System.Console.WriteLine($"[WARNING] AssetManager could not find texture: {path}");
        }
    }

    public static Texture2D? GetTexture(string key)
    {
        if (_textures.TryGetValue(key, out var texture))
        {
            return texture;
        }
        return null;
    }

    public static void UnloadAll()
    {
        foreach (var tex in _textures.Values)
        {
            Raylib.UnloadTexture(tex);
        }
        _textures.Clear();
        _initialized = false;
    }
}
