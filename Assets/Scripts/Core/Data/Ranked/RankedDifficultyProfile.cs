using System;

namespace Game.Data.Ranked
{
    [Serializable]
    public readonly struct RankedDifficultyProfile
    {
        public float Verticality { get; }
        public float OverhangRatio { get; }
        public float Ambiguity { get; }
        public float DensityComplexity { get; }
        public float Asymmetry { get; }
        public float Compactness { get; }
        public int TargetBlockCount { get; }

        public RankedDifficultyProfile(
            float verticality,
            float overhangRatio,
            float ambiguity,
            float densityComplexity,
            float asymmetry,
            float compactness,
            int targetBlockCount)
        {
            Verticality = verticality;
            OverhangRatio = overhangRatio;
            Ambiguity = ambiguity;
            DensityComplexity = densityComplexity;
            Asymmetry = asymmetry;
            Compactness = compactness;
            TargetBlockCount = targetBlockCount;
        }

        public static RankedDifficultyProfile Lerp(in RankedDifficultyProfile a, in RankedDifficultyProfile b, float t) =>
            new(
                UnityEngine.Mathf.Lerp(a.Verticality, b.Verticality, t),
                UnityEngine.Mathf.Lerp(a.OverhangRatio, b.OverhangRatio, t),
                UnityEngine.Mathf.Lerp(a.Ambiguity, b.Ambiguity, t),
                UnityEngine.Mathf.Lerp(a.DensityComplexity, b.DensityComplexity, t),
                UnityEngine.Mathf.Lerp(a.Asymmetry, b.Asymmetry, t),
                UnityEngine.Mathf.Lerp(a.Compactness, b.Compactness, t),
                UnityEngine.Mathf.RoundToInt(UnityEngine.Mathf.Lerp(a.TargetBlockCount, b.TargetBlockCount, t)));

        public RankedDifficultyProfile WithBonus(float verticalityBonus, float overhangBonus, float ambiguityBonus) =>
            new(
                UnityEngine.Mathf.Clamp01(Verticality + verticalityBonus),
                UnityEngine.Mathf.Clamp01(OverhangRatio + overhangBonus),
                UnityEngine.Mathf.Clamp01(Ambiguity + ambiguityBonus),
                DensityComplexity,
                Asymmetry,
                Compactness,
                TargetBlockCount);
    }
}