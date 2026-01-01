using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class Noise
{
    static float[] octaveFrequencies = new float[] { 1, 1.5f, 2, 2.5f };
    static float[] octaveAmplitudes = new float[] { 1, 0.9f, 0.7f, 0.3f };

    static Vector3[] GetSeedOffsets(int seed)
    {
        System.Random prng = new System.Random(seed);
        Vector3[] octaveOffsets = new Vector3[octaveFrequencies.Length];
        for (int i = 0; i < octaveFrequencies.Length; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            float offsetZ = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);
        }
        return octaveOffsets;
    }

    public static float Noise2D(Vector3 pos, float scale, int seed)
    {
        Vector3[] octaveOffsets = GetSeedOffsets(seed);

        float value = 0;
        for (int i = 0; i < octaveFrequencies.Length; i++)
        {
            float sampleX = octaveFrequencies[i] * pos.x / scale + octaveOffsets[i].x;
            float sampleZ = octaveFrequencies[i] * pos.z / scale + octaveOffsets[i].z;

            value += octaveAmplitudes[i] * Mathf.PerlinNoise(sampleX, sampleZ) * 2f - 0.5f;
        }
        return value;
    }

    public static float Noise3D(Vector3 pos, float scale, int seed)
    {
        Vector3[] octaveOffsets = GetSeedOffsets(seed);

        float value = 0;
        for (int i = 0; i < octaveFrequencies.Length; i++)
        {
            float sampleX = octaveFrequencies[i] * pos.x / scale + octaveOffsets[i].x;
            float sampleY = octaveFrequencies[i] * pos.y / scale + octaveOffsets[i].y;
            float sampleZ = octaveFrequencies[i] * pos.z / scale + octaveOffsets[i].z;

            value += octaveAmplitudes[i] * Perlin3D(sampleX, sampleY, sampleZ) * 2f - 0.5f;
        }
        return value;
    }

    static float Perlin3D(float x, float y, float z)
    {
        y += 1;
        z += 2;
        float xy = _perlin3DFixed(x, y);
        float xz = _perlin3DFixed(x, z);
        float yz = _perlin3DFixed(y, z);
        float yx = _perlin3DFixed(y, x);
        float zx = _perlin3DFixed(z, x);
        float zy = _perlin3DFixed(z, y);
        return xy * xz * yz * yx * zx * zy;
    }

    static float _perlin3DFixed(float a, float b)
    {
        return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a, b));
    }
}
