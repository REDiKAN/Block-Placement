using System;

namespace Game.Services.Ranked
{
    public readonly struct SolverResult
    {
        public int SolutionsFound { get; }
        public int SolverSteps { get; }
        public int MinimumBlocksRequired { get; }
        public bool IsSolvable => SolutionsFound > 0;
        public bool HasUniqueSolution => SolutionsFound == 1;

        public SolverResult(int solutionsFound, int solverSteps, int minimumBlocksRequired)
        {
            SolutionsFound = solutionsFound;
            SolverSteps = solverSteps;
            MinimumBlocksRequired = minimumBlocksRequired;
        }

        public static SolverResult Failure => new(0, 0, 0);
    }

    public interface ISolverService
    {
        SolverResult Solve(bool[] wall1, bool[] wall2, bool[] floor);
        SolverResult SolveWithoutRotation(bool[] wall1, bool[] wall2, bool[] floor);
        int CountSolutions(bool[] wall1, bool[] wall2, bool[] floor, int maxSolutions);
    }
}