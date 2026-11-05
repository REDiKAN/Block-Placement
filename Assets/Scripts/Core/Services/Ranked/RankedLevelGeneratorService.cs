using System;
using System.Collections.Generic;
using Game.Data;
using Game.Data.Ranked;
using Game.Services.Shadow;
using UnityEngine;

namespace Game.Services.Ranked
{
    public class RankedLevelGeneratorService : IRankedLevelGeneratorService
    {
        private readonly RankedConfig _config;
        private readonly IArchetypeResolver _archetypeResolver;
        private readonly IProfileBuilder _profileBuilder;
        private readonly IShapeGenerator _shapeGenerator;
        private readonly IQualityValidator _qualityValidator;
        private readonly IShadowCalculationService _calculationService;

        public RankedLevelGeneratorService(
            RankedConfig config,
            IArchetypeResolver archetypeResolver,
            IProfileBuilder profileBuilder,
            IShapeGenerator shapeGenerator,
            IQualityValidator qualityValidator,
            IShadowCalculationService calculationService)
        {
            _config = config;
            _archetypeResolver = archetypeResolver;
            _profileBuilder = profileBuilder;
            _shapeGenerator = shapeGenerator;
            _qualityValidator = qualityValidator;
            _calculationService = calculationService;
        }

        public LevelConfig GenerateLevel(int mmr, int matchLevel, int matchPoints)
        {
            var archetype = _archetypeResolver.Resolve(mmr);
            var profile = _profileBuilder.Build(archetype, matchLevel, matchPoints);

            for (var attempt = 0; attempt < _config.MaxGenerationAttempts; attempt++)
            {
                var shape = _shapeGenerator.Generate(profile);
                if (shape.Count == 0) continue;

                var config = CreateLevelConfig(shape, profile, archetype);
                var report = _qualityValidator.Validate(config, profile);

                if (_qualityValidator.IsAcceptable(report, profile))
                    return config;
            }

            var fallbackProfile = _profileBuilder.BuildFallback(archetype);
            var fallbackShape = _shapeGenerator.Generate(fallbackProfile);

            return CreateLevelConfig(fallbackShape, fallbackProfile, archetype);
        }

        private LevelConfig CreateLevelConfig(List<Vector3Int> shape, RankedDifficultyProfile profile, Archetype archetype)
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();

            var floor = GenerateFloor();
            var projection = _calculationService.Calculate(shape.ToArray(), _config.GridSize);

            var wallYZ = CreateWallData(projection.Wall1, shape, 0);
            var wallXY = CreateWallData(projection.Wall2, shape, 1);

            config.SetData(shape.ToArray(), floor, wallYZ, wallXY);
            config.SetMode(GameMode.Blocks);

            var timeLimit = _profileBuilder.CalculateTimeLimit(_archetypeResolver.GetBaseDifficulty(archetype));
            config.SetTimeLimit(true, timeLimit);

            if (_archetypeResolver.IsBlockLimitEnabled(archetype))
            {
                var padding = _archetypeResolver.GetBlockLimitPadding(archetype);
                config.SetBlockLimit(true, shape.Count + padding);
            }
            else
            {
                config.SetBlockLimit(false, -1);
            }

            return config;
        }

        private bool[] GenerateFloor()
        {
            var cellCount = _config.GridSize * _config.GridSize;
            var floor = new bool[cellCount];
            Array.Fill(floor, true);
            return floor;
        }

        private WallData CreateWallData(bool[] shadowMask, List<Vector3Int> shape, int wallIndex)
        {
            var densities = new WallCellDensityData[shadowMask.Length];

            for (var i = 0; i < shadowMask.Length; i++)
            {
                var density = CalculateDensity(shape, i, wallIndex);
                densities[i] = new WallCellDensityData(shadowMask[i], density);
            }

            var wallData = new WallData();
            wallData.SetDensities(densities);

            return wallData;
        }

        private int CalculateDensity(List<Vector3Int> shape, int cellIndex, int wallIndex)
        {
            var count = 0;
            var y = cellIndex / _config.GridSize;
            var z = cellIndex % _config.GridSize;
            var x = cellIndex / _config.GridSize;
            var y2 = cellIndex % _config.GridSize;

            foreach (var block in shape)
            {
                if (wallIndex == 0 && block.y == y && block.z == z) count++;
                if (wallIndex == 1 && block.x == x && block.y == y2) count++;
            }

            return count;
        }
    }
}