using UnityEngine;

namespace Game.Data
{
    public readonly struct WaterShaderParameters
    {
        public float WaterDepth { get; }
        public Color ShallowWaterColor { get; }
        public Color DeepWater { get; }
        public float RefractionSpeed { get; }
        public float RefractionScale { get; }
        public float RefractionStrength { get; }
        public float FoamAmount { get; }
        public float FoamCutOff { get; }
        public float FoamSpeed { get; }
        public float FoamScale { get; }
        public Color FoamColor { get; }
        public Vector2 Wave1Dir { get; }
        public float Wave1Amp { get; }
        public float Wave1Len { get; }
        public float Wave1Steep { get; }
        public Vector2 Wave2Dir { get; }
        public float Wave2Amp { get; }
        public float Wave2Len { get; }
        public float Wave2Steep { get; }
        public float WaveSpeed { get; }

        public WaterShaderParameters(
            float waterDepth,
            Color shallowWaterColor,
            Color deepWater,
            float refractionSpeed,
            float refractionScale,
            float refractionStrength,
            float foamAmount,
            float foamCutOff,
            float foamSpeed,
            float foamScale,
            Color foamColor,
            Vector2 wave1Dir,
            float wave1Amp,
            float wave1Len,
            float wave1Steep,
            Vector2 wave2Dir,
            float wave2Amp,
            float wave2Len,
            float wave2Steep,
            float waveSpeed)
        {
            WaterDepth = waterDepth;
            ShallowWaterColor = shallowWaterColor;
            DeepWater = deepWater;
            RefractionSpeed = refractionSpeed;
            RefractionScale = refractionScale;
            RefractionStrength = refractionStrength;
            FoamAmount = foamAmount;
            FoamCutOff = foamCutOff;
            FoamSpeed = foamSpeed;
            FoamScale = foamScale;
            FoamColor = foamColor;
            Wave1Dir = wave1Dir;
            Wave1Amp = wave1Amp;
            Wave1Len = wave1Len;
            Wave1Steep = wave1Steep;
            Wave2Dir = wave2Dir;
            Wave2Amp = wave2Amp;
            Wave2Len = wave2Len;
            Wave2Steep = wave2Steep;
            WaveSpeed = waveSpeed;
        }

        public static WaterShaderParameters FromConfig(WaterShaderConfig config)
        {
            if (config is null)
                return default;

            return new WaterShaderParameters(
                config.WaterDepth,
                config.ShallowWaterColor,
                config.DeepWater,
                config.RefractionSpeed,
                config.RefractionScale,
                config.RefractionStrength,
                config.FoamAmount,
                config.FoamCutOff,
                config.FoamSpeed,
                config.FoamScale,
                config.FoamColor,
                config.Wave1Dir,
                config.Wave1Amp,
                config.Wave1Len,
                config.Wave1Steep,
                config.Wave2Dir,
                config.Wave2Amp,
                config.Wave2Len,
                config.Wave2Steep,
                config.WaveSpeed);
        }

        public static WaterShaderParameters Lerp(in WaterShaderParameters a, in WaterShaderParameters b, float t) =>
            new(
                Mathf.Lerp(a.WaterDepth, b.WaterDepth, t),
                Color.Lerp(a.ShallowWaterColor, b.ShallowWaterColor, t),
                Color.Lerp(a.DeepWater, b.DeepWater, t),
                Mathf.Lerp(a.RefractionSpeed, b.RefractionSpeed, t),
                Mathf.Lerp(a.RefractionScale, b.RefractionScale, t),
                Mathf.Lerp(a.RefractionStrength, b.RefractionStrength, t),
                Mathf.Lerp(a.FoamAmount, b.FoamAmount, t),
                Mathf.Lerp(a.FoamCutOff, b.FoamCutOff, t),
                Mathf.Lerp(a.FoamSpeed, b.FoamSpeed, t),
                Mathf.Lerp(a.FoamScale, b.FoamScale, t),
                Color.Lerp(a.FoamColor, b.FoamColor, t),
                Vector2.Lerp(a.Wave1Dir, b.Wave1Dir, t),
                Mathf.Lerp(a.Wave1Amp, b.Wave1Amp, t),
                Mathf.Lerp(a.Wave1Len, b.Wave1Len, t),
                Mathf.Lerp(a.Wave1Steep, b.Wave1Steep, t),
                Vector2.Lerp(a.Wave2Dir, b.Wave2Dir, t),
                Mathf.Lerp(a.Wave2Amp, b.Wave2Amp, t),
                Mathf.Lerp(a.Wave2Len, b.Wave2Len, t),
                Mathf.Lerp(a.Wave2Steep, b.Wave2Steep, t),
                Mathf.Lerp(a.WaveSpeed, b.WaveSpeed, t));
    }
}