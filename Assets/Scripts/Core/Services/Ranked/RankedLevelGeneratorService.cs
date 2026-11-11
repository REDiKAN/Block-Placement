using Game.Core;
using Game.Data;
using Game.Data.Ranked;
using Game.Services.Shadow;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Services.Ranked
{
    public class RankedLevelGeneratorService : IRankedLevelGeneratorService
    {
        private readonly RankedConfig _config;
        private readonly IArchetypeResolver _archetypeResolver;
        private readonly IProfileBuilder _profileBuilder;
        private readonly IShapeGenerator _shapeGenerator;
        private readonly IStructureShapeGenerator _structureShapeGenerator;
        private readonly IDecoyPlacementService _decoyPlacementService;
        private readonly IQualityValidator _qualityValidator;
        private readonly IShadowCalculationService _calculationService;

        public RankedLevelGeneratorService(
            RankedConfig config,
            IArchetypeResolver archetypeResolver,
            IProfileBuilder profileBuilder,
            IShapeGenerator shapeGenerator,
            IStructureShapeGenerator structureShapeGenerator,
            IDecoyPlacementService decoyPlacementService,
            IQualityValidator qualityValidator,
            IShadowCalculationService calculationService)
        {
            _config = config;
            _archetypeResolver = archetypeResolver;
            _profileBuilder = profileBuilder;
            _shapeGenerator = shapeGenerator;
            _structureShapeGenerator = structureShapeGenerator;
            _decoyPlacementService = decoyPlacementService;
            _qualityValidator = qualityValidator;
            _calculationService = calculationService;
        }

        public LevelConfig GenerateLevel(int mmr, int matchLevel, int matchPoints)
        {
            var archetype = _archetypeResolver.Resolve(mmr);
            var profile = _profileBuilder.Build(archetype, matchLevel, matchPoints);
            var isStructureMode = RankedContext.CurrentMode == GameMode.Structures;
            var profileData = _archetypeResolver.GetProfileData(archetype);

            for (var attempt = 0; attempt < _config.MaxGenerationAttempts; attempt++)
            {
                List<Vector3Int> shape;

                if (isStructureMode)
                {
                    shape = GenerateStructureShape(profile, profileData);
                    if (shape.Count == 0) continue;
                }
                else
                {
                    shape = _shapeGenerator.Generate(profile);
                    if (shape.Count == 0) continue;
                }

                var config = CreateLevelConfig(shape, profile, archetype, isStructureMode);
                var report = _qualityValidator.Validate(config, profile);

                if (_qualityValidator.IsAcceptable(report, profile))
                    return config;
            }

            var fallbackProfile = _profileBuilder.BuildFallback(archetype);
            var fallbackShape = isStructureMode
                ? GenerateStructureShape(fallbackProfile, profileData)
                : _shapeGenerator.Generate(fallbackProfile);

            return CreateLevelConfig(fallbackShape, fallbackProfile, archetype, isStructureMode);
        }

        private List<Vector3Int> GenerateStructureShape(RankedDifficultyProfile profile, ArchetypeProfileData profileData)
        {
            var allBlocks = new HashSet<Vector3Int>();
            var targetCount = Mathf.Max(3, Mathf.RoundToInt(profile.TargetBlockCount * 0.7f));
            var attempts = 0;

            while (allBlocks.Count < targetCount && attempts < 50)
            {
                attempts++;
                var structureBlocks = _structureShapeGenerator.GenerateStructure(profile);
                var canPlace = true;

                foreach (var block in structureBlocks)
                {
                    if (allBlocks.Contains(block))
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace)
                {
                    foreach (var block in structureBlocks)
                        allBlocks.Add(block);
                }
            }

            if (profileData is { UseDecoyStructures: true } && allBlocks.Count > 0)
            {
                var projection = _calculationService.Calculate(allBlocks.ToArray(), _config.GridSize);
                var decoyCount = UnityEngine.Random.Range(profileData.MinDecoyCount, profileData.MaxDecoyCount + 1);
                var decoys = _decoyPlacementService.GenerateDecoys(new List<Vector3Int>(allBlocks), projection.Wall1, projection.Wall2, decoyCount);

                foreach (var decoy in decoys)
                    allBlocks.Add(decoy);
            }

            return new List<Vector3Int>(allBlocks);
        }

        private LevelConfig CreateLevelConfig(List<Vector3Int> shape, RankedDifficultyProfile profile, Archetype archetype, bool isStructureMode)
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            var floor = GenerateFloor();
            var projection = _calculationService.Calculate(shape.ToArray(), _config.GridSize);

            var wallYZ = CreateWallData(projection.Wall1, shape, 0);
            var wallXY = CreateWallData(projection.Wall2, shape, 1);

            config.SetData(shape.ToArray(), floor, wallYZ, wallXY);
            config.SetMode(isStructureMode ? GameMode.Structures : GameMode.Blocks);

            if (isStructureMode && _config.AvailableStructures is { Length: > 0 })
            {
                var spawnData = new StructureSpawnData[_config.AvailableStructures.Length];
                for (var i = 0; i < _config.AvailableStructures.Length; i++)
                {
                    var maxCount = UnityEngine.Random.Range(2, 5);
                    spawnData[i] = new StructureSpawnData(_config.AvailableStructures[i], maxCount);
                }
                config.SetAvailableStructures(spawnData);
            }

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