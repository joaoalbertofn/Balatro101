using System;
using System.IO;
using Raylib_cs;

namespace Balatro101.Game.Engine;

public static class AudioEngine
{
    private static Sound blipSound;
    private static Sound scoreSound;
    private static Sound shuffleSound;

    private static Random rng = new Random();

    public static void Initialize()
    {
        Raylib.InitAudioDevice();

        blipSound = GenerateBlip();
        scoreSound = GenerateScoreSino();
        shuffleSound = GenerateShuffle();
    }

    public static void Close()
    {
        if (Raylib.IsAudioDeviceReady())
        {
            Raylib.UnloadSound(blipSound);
            Raylib.UnloadSound(scoreSound);
            Raylib.UnloadSound(shuffleSound);
            Raylib.CloseAudioDevice();
        }
    }

    public static void PlayBlip()
    {
        if (!Raylib.IsAudioDeviceReady()) return;
        Raylib.SetSoundPitch(blipSound, 1.0f + (float)(rng.NextDouble() * 0.2 - 0.1));
        Raylib.PlaySound(blipSound);
    }

    public static void PlayScore(float pitchMultiplier = 1.0f)
    {
        if (!Raylib.IsAudioDeviceReady()) return;
        Raylib.SetSoundPitch(scoreSound, pitchMultiplier);
        Raylib.PlaySound(scoreSound);
    }

    public static void PlayShuffle()
    {
        if (!Raylib.IsAudioDeviceReady()) return;
        Raylib.SetSoundPitch(shuffleSound, 1.0f + (float)(rng.NextDouble() * 0.1 - 0.05));
        Raylib.PlaySound(shuffleSound);
    }

    private static Sound GenerateBlip()
    {
        int sampleRate = 44100;
        int durationMs = 80;
        int samples = (sampleRate * durationMs) / 1000;
        short[] data = new short[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = 300f; // low click
            float envelope = 1.0f - ((float)i / samples);
            envelope = (float)Math.Pow(envelope, 4); // very fast exponential decay
            data[i] = (short)(Math.Sin(2 * Math.PI * freq * t) * 20000 * envelope);
        }

        return CreateSoundFromShorts(data, sampleRate);
    }

    private static Sound GenerateScoreSino()
    {
        int sampleRate = 44100;
        int durationMs = 600;
        int samples = (sampleRate * durationMs) / 1000;
        short[] data = new short[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = 800f + (t * 200f); // slight pitch slide up
            float envelope = 1.0f - ((float)i / samples);
            envelope = (float)Math.Pow(envelope, 3);
            data[i] = (short)(Math.Sin(2 * Math.PI * freq * t) * 15000 * envelope);
        }

        return CreateSoundFromShorts(data, sampleRate);
    }

    private static Sound GenerateShuffle()
    {
        int sampleRate = 44100;
        int durationMs = 150;
        int samples = (sampleRate * durationMs) / 1000;
        short[] data = new short[samples];

        for (int i = 0; i < samples; i++)
        {
            float progress = (float)i / samples;
            float envelope;
            if (progress < 0.2f) envelope = progress / 0.2f;
            else envelope = 1.0f - ((progress - 0.2f) / 0.8f);

            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            data[i] = (short)(noise * 12000 * envelope);
        }

        return CreateSoundFromShorts(data, sampleRate);
    }

    private static Sound CreateSoundFromShorts(short[] samples, int sampleRate)
    {
        byte[] wavBytes = EmitWav(samples, sampleRate);
        Wave wave = Raylib.LoadWaveFromMemory(".wav", wavBytes);
        Sound sound = Raylib.LoadSoundFromWave(wave);
        Raylib.UnloadWave(wave);
        return sound;
    }

    private static byte[] EmitWav(short[] samples, int sampleRate)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        int dataSize = samples.Length * 2;
        int fileSize = 36 + dataSize;

        writer.Write(new char[] { 'R', 'I', 'F', 'F' });
        writer.Write(fileSize);
        writer.Write(new char[] { 'W', 'A', 'V', 'E' });
        writer.Write(new char[] { 'f', 'm', 't', ' ' });
        writer.Write(16); // format chunk size
        writer.Write((short)1); // PCM
        writer.Write((short)1); // Channels (Mono)
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2); // Byte rate
        writer.Write((short)2); // Block align
        writer.Write((short)16); // Bits per sample
        writer.Write(new char[] { 'd', 'a', 't', 'a' });
        writer.Write(dataSize);

        foreach (var sample in samples)
        {
            writer.Write(sample);
        }

        return ms.ToArray();
    }
}
