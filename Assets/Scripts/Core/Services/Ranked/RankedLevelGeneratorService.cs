using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Data;
using Game.Data.Ranked;
using Game.Services.Shadow;
using UnityEngine;

namespace Game.Services.Ranked
{
    public class RankedLevelGeneratorService : IRankedLevelGeneratorService
    {
        private readonly RankedCoreConfig _coreConfig;
        private readonly RankedBlocksConfig _blocksConfig;
        private readonly RankedStructuresConfig _structuresConfig;
        private readonly IArchetypeResolver _archetypeResolver;
        private readonly IProfileBuilder _profileBuilder;
        private readonly IShapeGenerator _shapeGenerator;
        private readonly IQualityValidator _qualityValidator;
        private readonly IShadowCalculationService _calculationService;

        private static readonly int[] ValidAngles = { 0, 90, 180, 270 };

        public RankedLevelGeneratorService(
            RankedCoreConfig coreConfig,
            RankedBlocksConfig blocksConfig,
            RankedStructuresConfig structuresConfig,
            IArchetypeResolver archetypeResolver,
            IProfileBuilder profileBuilder,
            IShapeGenerator shapeGenerator,
            IQualityValidator qualityValidator,
            IShadowCalculationService calculationService)
        {
            _coreConfig = coreConfig;
            _blocksConfig = blocksConfig;
            _structuresConfig = structuresConfig;
            _archetypeResolver = archetypeResolver;
            _profileBuilder = profileBuilder;
            _shapeGenerator = shapeGenerator;
            _qualityValidator = qualityValidator;
            _calculationService = calculationService;
        }

        public LevelConfig GenerateLevel(int mmr, int matchLevel, int matchPoints)
        {
            var archetypeProfile = _archetypeResolver.Resolve(mmr);
            if (archetypeProfile is null) return null;

            var archetype = archetypeProfile.Archetype;
            var profile = _profileBuilder.Build(archetype, matchLevel, matchPoints);
            var isStructureMode = RankedContext.CurrentMode == GameMode.Structures;

            for (var attempt = 0; attempt < _coreConfig.MaxGenerationAttempts; attempt++)
            {
                List<Vector3Int> shape;
                List<StructureSpawnData> inventory = null;

                if (isStructureMode)
                {
                    var result = GenerateStructureShape(profile);
                    shape = result.Blocks;
                    inventory = result.Inventory;
                    if (shape.Count == 0) continue;
                }
                else
                {
                    shape = _shapeGenerator.Generate(profile);
                    if (shape.Count == 0) continue;
                }

                var config = CreateLevelConfig(shape, archetype, isStructureMode, inventory);
                var report = _qualityValidator.Validate(config, profile);

                if (_qualityValidator.IsAcceptable(report, profile))
                    return config;
            }

            var fallbackProfile = _profileBuilder.BuildFallback(archetype);
            List<Vector3Int> fallbackShape;
            List<StructureSpawnData> fallbackInventory = null;

            if (isStructureMode)
            {
                var fallbackResult = GenerateStructureShape(fallbackProfile);
                fallbackShape = fallbackResult.Blocks;
                fallbackInventory = fallbackResult.Inventory;
            }
            else
            {
                fallbackShape = _shapeGenerator.Generate(fallbackProfile);
            }

            return CreateLevelConfig(fallbackShape, archetype, isStructureMode, fallbackInventory);
        }

        private StructureGenerationResult GenerateStructureShape(RankedDifficultyProfile profile)
        {
            var allBlocks = new HashSet<Vector3Int>();
            var inventoryMap = new Dictionary<StructureConfig, int>();
            var availablePool = _structuresConfig?.AvailableStructures;

            if (availablePool is null || availablePool.Length == 0)
                return new StructureGenerationResult(new List<Vector3Int>(), new List<StructureSpawnData>());

            var targetBlockCount = profile.TargetBlockCount;
            var attempts = 0;
            var gridSize = _coreConfig.GridSize;

            while (allBlocks.Count < targetBlockCount && attempts < 100)
            {
                attempts++;
                var config = availablePool[UnityEngine.Random.Range(0, availablePool.Length)];
                if (config?.LocalCoordinates is null || config.LocalCoordinates.Length == 0) continue;

                var angle = GetRandomAngle();
                var rotatedCoords = GetRotatedLocalCoordinates(config.LocalCoordinates, angle);

                var origin = new Vector3Int(
                    UnityEngine.Random.Range(0, gridSize), 0, UnityEngine.Random.Range(0, gridSize));

                var canPlace = true;
                var newBlocks = new List<Vector3Int>();

                foreach (var local in rotatedCoords)
                {
                    var world = origin + local;
                    if (world.x < 0 || world.x >= gridSize ||
                        world.y < 0 || world.y >= gridSize ||
                        world.z < 0 || world.z >= gridSize)
                    {
                        canPlace = false;
                        break;
                    }
                    if (allBlocks.Contains(world))
                    {
                        canPlace = false;
                        break;
                    }
                    newBlocks.Add(world);
                }

                if (canPlace && newBlocks.Count > 0)
                {
                    foreach (var b in newBlocks) allBlocks.Add(b);
                    if (!inventoryMap.ContainsKey(config)) inventoryMap[config] = 0;
                    inventoryMap[config]++;
                }
            }

            var archetypeProfile = _archetypeResolver.Resolve(RankedContext.PlayerMmr);
            if (archetypeProfile is StructuresArchetypeProfile structProfile && structProfile.UseDecoyStructures && allBlocks.Count > 0)
            {
                var decoyCount = UnityEngine.Random.Range(structProfile.MinDecoyCount, structProfile.MaxDecoyCount + 1);
                var decoyAttempts = 0;
                var decoysAdded = 0;

                while (decoysAdded < decoyCount && decoyAttempts < 50)
                {
                    decoyAttempts++;
                    var config = availablePool[UnityEngine.Random.Range(0, availablePool.Length)];
                    if (config?.LocalCoordinates is null || config.LocalCoordinates.Length == 0) continue;

                    var angle = GetRandomAngle();
                    var rotatedCoords = GetRotatedLocalCoordinates(config.LocalCoordinates, angle);
                    var origin = new Vector3Int(UnityEngine.Random.Range(0, gridSize), 0, UnityEngine.Random.Range(0, gridSize));

                    var canPlace = true;
                    var newBlocks = new List<Vector3Int>();

                    foreach (var local in rotatedCoords)
                    {
                        var world = origin + local;
                        if (world.x < 0 || world.x >= gridSize || world.y < 0 || world.y >= gridSize || world.z < 0 || world.z >= gridSize)
                        {
                            canPlace = false; break;
                        }
                        if (allBlocks.Contains(world))
                        {
                            canPlace = false; break;
                        }
                        newBlocks.Add(world);
                    }

                    if (!canPlace || newBlocks.Count == 0) continue;

                    var currentShadowYZ = new HashSet<int>();
                    var currentShadowXY = new HashSet<int>();
                    foreach (var b in allBlocks)
                    {
                        currentShadowYZ.Add(b.y * gridSize + b.z);
                        currentShadowXY.Add(b.x * gridSize + b.y);
                    }

                    var createsNewShadow = false;
                    foreach (var b in newBlocks)
                    {
                        var yz = b.y * gridSize + b.z;
                        var xy = b.x * gridSize + b.y;
                        if (!currentShadowYZ.Contains(yz) || !currentShadowXY.Contains(xy))
                        {
                            createsNewShadow = true; break;
                        }
                    }

                    if (!createsNewShadow)
                    {
                        foreach (var b in newBlocks) allBlocks.Add(b);
                        if (!inventoryMap.ContainsKey(config)) inventoryMap[config] = 0;
                        inventoryMap[config]++;
                        decoysAdded++;
                    }
                }
            }

            var inventory = inventoryMap.Select(kvp => new StructureSpawnData(kvp.Key, kvp.Value)).ToList();
            return new StructureGenerationResult(new List<Vector3Int>(allBlocks), inventory);
        }

        private LevelConfig CreateLevelConfig(List<Vector3Int> shape, Archetype archetype, bool isStructureMode, List<StructureSpawnData> inventory)
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            var floor = GenerateFloor();
            var projection = _calculationService.Calculate(shape.ToArray(), _coreConfig.GridSize);

            var wallYZ = CreateWallData(projection.Wall1, shape, 0);
            var wallXY = CreateWallData(projection.Wall2, shape, 1);

            config.SetData(shape.ToArray(), floor, wallYZ, wallXY);
            config.SetMode(isStructureMode ? GameMode.Structures : GameMode.Blocks);

            if (isStructureMode && inventory is { Count: > 0 })
                config.SetAvailableStructures(inventory.ToArray());

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
            var cellCount = _coreConfig.GridSize * _coreConfig.GridSize;
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
            var gridSize = _coreConfig.GridSize;
            var y = cellIndex / gridSize;
            var z = cellIndex % gridSize;
            var x = cellIndex / gridSize;
            var y2 = cellIndex % gridSize;

            foreach (var block in shape)
            {
                if (wallIndex == 0 && block.y == y && block.z == z) count++;
                if (wallIndex == 1 && block.x == x && block.y == y2) count++;
            }

            return count;
        }

        private static int GetRandomAngle() => ValidAngles[UnityEngine.Random.Range(0, ValidAngles.Length)];

        private static Vector3Int[] GetRotatedLocalCoordinates(Vector3Int[] localCoords, int angle)
        {
            if (angle == 0) return localCoords;
            var rotated = new Vector3Int[localCoords.Length];
            for (var i = 0; i < localCoords.Length; i++)
            {
                var local = localCoords[i];
                rotated[i] = angle switch
                {
                    90 => new Vector3Int(local.z, local.y, -local.x),
                    180 => new Vector3Int(-local.x, local.y, -local.z),
                    270 => new Vector3Int(-local.z, local.y, local.x),
                    _ => local
                };
            }
            return rotated;
        }

        private readonly struct StructureGenerationResult
        {
            public List<Vector3Int> Blocks { get; }
            public List<StructureSpawnData> Inventory { get; }

            public StructureGenerationResult(List<Vector3Int> blocks, List<StructureSpawnData> inventory)
            {
                Blocks = blocks;
                Inventory = inventory;
            }
        }
    }
}