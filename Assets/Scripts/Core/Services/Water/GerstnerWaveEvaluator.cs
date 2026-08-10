using UnityEngine;
using Game.Data;

namespace Game.Services.Water
{
    public static class GerstnerWaveEvaluator
    {
        private const float TwoPi = 6.2831853f;
        private const float MinWavelength = 0.01f;
        private static readonly Vector2 DirEpsilon = new(0.001f, 0.001f);

        public static void Evaluate(
            Vector2 xz,
            float time,
            in WaterShaderParameters parameters,
            out Vector3 displacement,
            out Vector3 normal)
        {
            displacement = Vector3.zero;
            normal = Vector3.up;

            var timeScaled = time * parameters.WaveSpeed;

            AddWave(xz, timeScaled, parameters.Wave1Dir + DirEpsilon, parameters.Wave1Amp, Mathf.Max(parameters.Wave1Len, MinWavelength), parameters.Wave1Steep, ref displacement, ref normal);
            AddWave(xz, timeScaled, parameters.Wave2Dir + DirEpsilon, parameters.Wave2Amp, Mathf.Max(parameters.Wave2Len, MinWavelength), parameters.Wave2Steep, ref displacement, ref normal);

            normal.Normalize();
        }

        private static void AddWave(
            Vector2 xz,
            float timeScaled,
            Vector2 rawDirection,
            float amplitude,
            float wavelength,
            float steepness,
            ref Vector3 displacement,
            ref Vector3 normal)
        {
            var direction = rawDirection.normalized;
            var k = TwoPi / wavelength;
            var phase = k * (Vector2.Dot(direction, xz) - timeScaled);
            var cos = Mathf.Cos(phase);
            var sin = Mathf.Sin(phase);

            displacement.x += steepness * amplitude * direction.x * cos;
            displacement.y += amplitude * sin;
            displacement.z += steepness * amplitude * direction.y * cos;

            normal.x += -direction.x * amplitude * k * cos;
            normal.y += -steepness * amplitude * k * sin;
            normal.z += -direction.y * amplitude * k * cos;
        }
    }
}