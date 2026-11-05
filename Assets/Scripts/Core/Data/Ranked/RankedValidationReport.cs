using System;

namespace Game.Data.Ranked
{
    [Serializable]
    public readonly struct RankedValidationReport
    {
        public bool IsSolvable { get; }
        public bool HasUniqueSolution { get; }
        public int SolverSteps { get; }
        public float BlockEfficiency { get; }
        public float MetricMatchScore { get; }
        public bool RequiresRotation { get; }
        public int MinimumBlocksRequired { get; }

        public bool IsAcceptable =>
            IsSolvable &&
            HasUniqueSolution &&
            MetricMatchScore > 0.7f;

        public RankedValidationReport(
            bool isSolvable,
            bool hasUniqueSolution,
            int solverSteps,
            float blockEfficiency,
            float metricMatchScore,
            bool requiresRotation,
            int minimumBlocksRequired)
        {
            IsSolvable = isSolvable;
            HasUniqueSolution = hasUniqueSolution;
            SolverSteps = solverSteps;
            BlockEfficiency = blockEfficiency;
            MetricMatchScore = metricMatchScore;
            RequiresRotation = requiresRotation;
            MinimumBlocksRequired = minimumBlocksRequired;
        }

        public static RankedValidationReport Failure =>
            new(false, false, 0, 0f, 0f, false, 0);
    }
}