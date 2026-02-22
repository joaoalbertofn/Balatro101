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

        string[] suits = { "clubs", "diamonds", "hearts", "spades" };
        string[] ranks = { "02", "03", "04", "05", "06", "07", "08", "09", "10", "J", "Q", "K", "A" };

        foreach (var suit in suits)
        {
            foreach (var rank in ranks)
            {
                LoadTexture($"card_{suit}_{rank}", $"Assets/Cards/card_{suit}_{rank}.png");
            }
        }
        LoadTexture("card_back", "Assets/Cards/card_back.png");

        for (int i = 1; i <= 6; i++)
        {
            LoadTexture($"Tarot_0{i}", $"Assets/Tarot_0{i}.png");
            LoadTexture($"Planetas_0{i}", $"Assets/Planetas_0{i}.png");
        }

        string[] jokerFiles = {
            "Jokers 2_01", "Jokers 2_03", "Jokers 2_04", "Jokers 2_08", "Jokers 2_11", "Jokers 2_12", "Jokers 2_13", "Jokers 2_15",
            "Jokers 3_01", "Jokers 3_06", "Jokers 3_07", "Jokers 3_08", "Jokers 3_09", "Jokers 3_10", "Jokers 3_13", "Jokers 3_14",
            "Jokers_08", "Jokers_11", "Jokers_12", "Jokers_13", "Jokers_15",
            "Especiais_01", "Especiais_02", "Especiais_03", "Especiais_04", "Especiais_05", "Especiais_06", "Especiais_07", "Especiais_08"
        };
        foreach (var joker in jokerFiles)
        {
            LoadTexture(joker, $"Assets/{joker}.png");
        }

        _initialized = true;
    }

    private static void LoadTexture(string key, string path)
    {
        string[] possiblePaths = {
            path,
            System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../", path)
        };

        foreach (var p in possiblePaths)
        {
            if (System.IO.File.Exists(p))
            {
                Image img = Raylib.LoadImage(p);
                int minX = img.Width, minY = img.Height, maxX = 0, maxY = 0;
                unsafe
                {
                    Color* pixels = (Color*)Raylib.LoadImageColors(img);
                    for (int y = 0; y < img.Height; y++)
                    {
                        for (int x = 0; x < img.Width; x++)
                        {
                            if (pixels[y * img.Width + x].A > 10)
                            {
                                if (x < minX) minX = x;
                                if (x > maxX) maxX = x;
                                if (y < minY) minY = y;
                                if (y > maxY) maxY = y;
                            }
                        }
                    }
                    Raylib.UnloadImageColors(pixels);
                }

                if (minX <= maxX && minY <= maxY)
                {
                    Rectangle cropRec = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
                    Raylib.ImageCrop(ref img, cropRec);
                }

                Texture2D tex = Raylib.LoadTextureFromImage(img);
                Raylib.UnloadImage(img);

                _textures[key] = tex;
                return;
            }
        }

        System.Console.WriteLine($"[WARNING] AssetManager could not find texture: {path}");
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
