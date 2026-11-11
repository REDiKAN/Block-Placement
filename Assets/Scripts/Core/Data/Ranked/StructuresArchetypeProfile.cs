using System;
using UnityEngine;

namespace Game.Data.Ranked
{
    [Serializable]
    public class StructuresArchetypeProfile : IRankedArchetypeProfile
    {
        [field: SerializeField] public Archetype Archetype { get; private set; }
        [field: SerializeField] public int MinMmr { get; private set; }
        [field: SerializeField] public int MaxMmr { get; private set; }
        [field: SerializeField] public int BaseDifficulty { get; private set; }
        [field: SerializeField] public float Verticality { get; private set; }
        [field: SerializeField] public float OverhangRatio { get; private set; }
        [field: SerializeField] public float Ambiguity { get; private set; }
        [field: SerializeField] public float DensityComplexity { get; private set; }
        [field: SerializeField] public float Asymmetry { get; private set; }
        [field: SerializeField] public float Compactness { get; private set; }
        [field: SerializeField] public int MinBlockCount { get; private set; }
        [field: SerializeField] public int MaxBlockCount { get; private set; }
        [field: SerializeField] public bool IsBlockLimitEnabled { get; private set; }
        [field: SerializeField] public bool IsRotationRequired { get; private set; }
        [field: SerializeField] public int BlockLimitPadding { get; private set; }

        [field: SerializeField] public bool UseDecoyStructures { get; private set; }
        [field: SerializeField] public int MinDecoyCount { get; private set; }
        [field: SerializeField] public int MaxDecoyCount { get; private set; }
        [field: SerializeField] public int MinStructureTypes { get; private set; } = 2;
        [field: SerializeField] public int MaxStructureTypes { get; private set; } = 4;

        public RankedDifficultyProfile ToProfile() =>
            new(
                Verticality,
                OverhangRatio,
                Ambiguity,
                DensityComplexity,
                Asymmetry,
                Compactness,
                UnityEngine.Random.Range(MinBlockCount, MaxBlockCount + 1));
    }
}