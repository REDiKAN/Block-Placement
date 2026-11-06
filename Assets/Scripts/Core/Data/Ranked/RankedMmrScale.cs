using UnityEngine;

namespace Game.Data.Ranked
{
    public readonly struct RankedMmrRange
    {
        public int Min { get; }
        public int Max { get; }
        public bool IsInfinity { get; }

        public RankedMmrRange(int min, int max, bool isInfinity)
        {
            Min = min;
            Max = max;
            IsInfinity = isInfinity;
        }

        public float CalculateFill(int mmr) =>
            Max <= Min ? 0f : Mathf.Clamp01((float)(mmr - Min) / (Max - Min));

        public string FormatMax() => IsInfinity ? $"Infinity ({Max})" : Max.ToString();
    }

    public static class RankedMmrScale
    {
        public const int TierStep = 500;
        public const int InfinityThreshold = 3000;
        public const int InfinityStep = 1000;

        public static RankedMmrRange GetRange(int mmr)
        {
            var clamped = Mathf.Max(0, mmr);

            if (clamped >= InfinityThreshold)
            {
                var infinityMax = (clamped / InfinityStep + 1) * InfinityStep;
                return new RankedMmrRange(InfinityThreshold, infinityMax, true);
            }

            var tier = clamped / TierStep;
            return new RankedMmrRange(tier * TierStep, (tier + 1) * TierStep, false);
        }

        public static RankedMmrRange GetSegmentRange(int mmr, int direction) =>
            direction < 0 ? GetRange(Mathf.Max(0, mmr - 1)) : GetRange(mmr);

        public static bool TryGetBreakpoint(int mmr, int direction, out int breakpoint)
        {
            if (direction > 0)
            {
                breakpoint = mmr < InfinityThreshold
                    ? Mathf.Min((mmr / TierStep + 1) * TierStep, InfinityThreshold)
                    : (mmr / InfinityStep + 1) * InfinityStep;
                return true;
            }

            if (mmr <= 0)
            {
                breakpoint = 0;
                return false;
            }

            breakpoint = mmr <= InfinityThreshold
                ? (mmr - 1) / TierStep * TierStep
                : Mathf.Max((mmr - 1) / InfinityStep * InfinityStep, InfinityThreshold);
            return true;
        }
    }
}