using System;
using System.IO;
using Raylib_cs;

namespace Balatro101.Game.Engine;

public static class AudioEngine
{
    private static Sound blipSound;
    private static Sound scoreSound;
    private static Sound shuffleSound;
    private static Sound coinSound;
    private static Sound slideSound;

    private static Random rng = new Random();

    public static void Initialize()
    {
        Raylib.InitAudioDevice();

        blipSound = GenerateBlip();
        scoreSound = GenerateScoreSino();
        shuffleSound = GenerateShuffle();
        coinSound = GenerateCoin();
        slideSound = GenerateSlide();
    }

    public static void Close()
    {
        if (Raylib.IsAudioDeviceReady())
        {
            Raylib.UnloadSound(blipSound);
            Raylib.UnloadSound(scoreSound);
            Raylib.UnloadSound(shuffleSound);
            Raylib.UnloadSound(coinSound);
            Raylib.UnloadSound(slideSound);
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

    public static void PlayCoin()
    {
        if (!Raylib.IsAudioDeviceReady()) return;
        Raylib.SetSoundPitch(coinSound, 1.0f + (float)(rng.NextDouble() * 0.1));
        Raylib.PlaySound(coinSound);
    }

    public static void PlaySlide()
    {
        if (!Raylib.IsAudioDeviceReady()) return;
        Raylib.SetSoundPitch(slideSound, 1.0f + (float)(rng.NextDouble() * 0.2 - 0.1));
        Raylib.PlaySound(slideSound);
    }

    private static Sound GenerateSlide()
    {
        int sampleRate = 44100;
        int durationMs = 60;
        int samples = (sampleRate * durationMs) / 1000;
        short[] data = new short[samples];

        for (int i = 0; i < samples; i++)
        {
            float envelope = 1.0f - ((float)i / samples);
            envelope = (float)Math.Pow(envelope, 2);

            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);

            data[i] = (short)(noise * 6000 * envelope);
        }

        return CreateSoundFromShorts(data, sampleRate);
    }

    private static Sound GenerateBlip()
    {
        int sampleRate = 44100;
        int durationMs = 120;
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
        int durationMs = 250;
        int samples = (sampleRate * durationMs) / 1000;
        short[] data = new short[samples];

        float lastNoise = 0;
        for (int i = 0; i < samples; i++)
        {
            float progress = (float)i / samples;
            float envelope;

            // Multiple envelope peaks to simulate multiple cards falling
            float pulse = (float)Math.Abs(Math.Sin(progress * Math.PI * 6));
            envelope = 1.0f - progress;
            envelope *= pulse;

            float rawNoise = (float)(rng.NextDouble() * 2.0 - 1.0);

            // Simple low-pass filter (smoothing) to make paper "thicker"
            float filteredNoise = (rawNoise + lastNoise) * 0.5f;
            lastNoise = filteredNoise;

            data[i] = (short)(filteredNoise * 14000 * envelope);
        }

        return CreateSoundFromShorts(data, sampleRate);
    }

    private static Sound GenerateCoin()
    {
        int sampleRate = 44100;
        int durationMs = 300;
        int samples = (sampleRate * durationMs) / 1000;
        short[] data = new short[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq1 = 2000f; // High metallic pitch
            float freq2 = 2800f; // Dissonant metallic harmonic

            float envelope = 1.0f - ((float)i / samples);
            envelope = (float)Math.Pow(envelope, 6); // Extremely sharp ping

            float mix = (float)Math.Sin(2 * Math.PI * freq1 * t) + ((float)Math.Sin(2 * Math.PI * freq2 * t) * 0.5f);

            data[i] = (short)(mix * 8000 * envelope);
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
