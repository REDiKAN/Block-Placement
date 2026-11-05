using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Ranked;
using Game.Services.Shadow;
using UnityEngine;

namespace Game.Services.Ranked
{
    public class QualityValidator : IQualityValidator
    {
        private readonly RankedConfig _config;
        private readonly ISolverService _solverService;
        private readonly IShadowCalculationService _calculationService;

        public QualityValidator(
            RankedConfig config,
            ISolverService solverService,
            IShadowCalculationService calculationService)
        {
            _config = config;
            _solverService = solverService;
            _calculationService = calculationService;
        }

        public RankedValidationReport Validate(LevelConfig config, RankedDifficultyProfile targetProfile)
        {
            if (config?.InitialBlocks is null || config.InitialBlocks.Length == 0)
                return RankedValidationReport.Failure;

            var projection = _calculationService.Calculate(config.InitialBlocks, _config.GridSize);
            var floor = config.FloorMatrix ?? GetDefaultFloor();

            var solverResult = _solverService.Solve(projection.Wall1, projection.Wall2, floor);

            if (!solverResult.IsSolvable || !solverResult.HasUniqueSolution)
                return RankedValidationReport.Failure;

            var actualProfile = CalculateActualProfile(config.InitialBlocks);
            var metricScore = CalculateMetricMatchScore(actualProfile, targetProfile);

            if (metricScore < _config.MetricMatchThreshold)
                return RankedValidationReport.Failure;

            var blockEfficiency = solverResult.MinimumBlocksRequired > 0
                ? (float)config.InitialBlocks.Length / solverResult.MinimumBlocksRequired
                : 0f;

            var requiresRotation = actualProfile.Asymmetry > 0.5f;

            return new RankedValidationReport(
                true,
                true,
                solverResult.SolverSteps,
                blockEfficiency,
                metricScore,
                requiresRotation,
                solverResult.MinimumBlocksRequired);
        }

        public bool IsAcceptable(RankedValidationReport report, RankedDifficultyProfile targetProfile) =>
            report.IsAcceptable;

        private bool[] GetDefaultFloor()
        {
            var floor = new bool[_config.GridSize * _config.GridSize];
            Array.Fill(floor, true);
            return floor;
        }

        private RankedDifficultyProfile CalculateActualProfile(Vector3Int[] blocks)
        {
            var maxY = 0;
            var overhangs = 0;
            var minX = _config.GridSize;
            var maxX = 0;
            var minZ = _config.GridSize;
            var maxZ = 0;

            var blockSet = new HashSet<Vector3Int>(blocks);

            foreach (var block in blocks)
            {
                if (block.y > maxY) maxY = block.y;

                if (block.y > 0 && !blockSet.Contains(new Vector3Int(block.x, block.y - 1, block.z)))
                    overhangs++;

                if (block.x < minX) minX = block.x;
                if (block.x > maxX) maxX = block.x;
                if (block.z < minZ) minZ = block.z;
                if (block.z > maxZ) maxZ = block.z;
            }

            var verticality = maxY / 4f;
            var overhangRatio = blocks.Length > 0 ? (float)overhangs / blocks.Length : 0f;

            var projection = _calculationService.Calculate(blocks, _config.GridSize);
            var ambiguity = CalculateAmbiguity(projection.Wall1, projection.Wall2);
            var densityComplexity = CalculateDensityVariance(blocks);
            var asymmetry = CalculateAsymmetry(blocks);

            var volume = (maxX - minX + 1) * (maxY + 1) * (maxZ - minZ + 1);
            var compactness = volume > 0 ? (float)blocks.Length / volume : 0f;

            return new RankedDifficultyProfile(
                verticality,
                overhangRatio,
                ambiguity,
                densityComplexity,
                asymmetry,
                compactness,
                blocks.Length);
        }

        private float CalculateAmbiguity(bool[] wall1, bool[] wall2)
        {
            var w1Count = 0;
            var w2Count = 0;
            var cellCount = _config.GridSize * _config.GridSize;

            for (var i = 0; i < cellCount; i++)
            {
                if (wall1[i]) w1Count++;
                if (wall2[i]) w2Count++;
            }

            var ratio = Mathf.Abs(w1Count - w2Count) / (float)cellCount;
            return Mathf.Clamp01(ratio * 2f);
        }

        private float CalculateDensityVariance(Vector3Int[] blocks)
        {
            var cellCount = _config.GridSize * _config.GridSize;
            var densities = new int[cellCount];

            foreach (var block in blocks)
            {
                densities[block.y * _config.GridSize + block.z]++;
            }

            var sum = 0f;
            var count = 0;

            for (var i = 0; i < cellCount; i++)
            {
                if (densities[i] > 0)
                {
                    sum += densities[i];
                    count++;
                }
            }

            if (count == 0) return 0f;

            var mean = sum / count;
            var variance = 0f;

            for (var i = 0; i < cellCount; i++)
            {
                if (densities[i] > 0)
                {
                    variance += (densities[i] - mean) * (densities[i] - mean);
                }
            }

            return Mathf.Clamp01(variance / count / 5f);
        }

        private float CalculateAsymmetry(Vector3Int[] blocks)
        {
            var leftCount = 0;
            var rightCount = 0;

            foreach (var block in blocks)
            {
                if (block.x < 2) leftCount++;
                else if (block.x > 2) rightCount++;
            }

            var total = leftCount + rightCount;
            if (total == 0) return 0f;

            return Mathf.Abs(leftCount - rightCount) / (float)total;
        }

        private float CalculateMetricMatchScore(RankedDifficultyProfile actual, RankedDifficultyProfile target)
        {
            var diff = 0f;
            diff += Mathf.Abs(actual.Verticality - target.Verticality);
            diff += Mathf.Abs(actual.OverhangRatio - target.OverhangRatio);
            diff += Mathf.Abs(actual.Ambiguity - target.Ambiguity);
            diff += Mathf.Abs(actual.DensityComplexity - target.DensityComplexity);
            diff += Mathf.Abs(actual.Asymmetry - target.Asymmetry);
            diff += Mathf.Abs(actual.Compactness - target.Compactness);

            return Mathf.Clamp01(1f - (diff / 6f));
        }
    }
}