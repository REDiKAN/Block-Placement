using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Views;

namespace Game.Services.Pool
{
    public interface IStructurePoolService
    {
        StructureView Get(StructureConfig config);
        void Return(StructureView structure);
    }

    public class StructurePoolService : IStructurePoolService, IInitializable
    {
        private const int PoolSize = 50;
        private readonly StructureConfig[] _configs;
        private readonly Transform _parent;
        private readonly Dictionary<StructureConfig, Queue<StructureView>> _configPools = new();
        private readonly Dictionary<StructureView, StructureConfig> _activeStructures = new();

        public StructurePoolService(
            [InjectOptional] StructureConfig[] configs,
            Transform parent)
        {
            _configs = configs ?? Array.Empty<StructureConfig>();
            _parent = parent;
        }

        public void Initialize()
        {
            foreach (var config in _configs)
            {
                if (config?.Prefab is null) continue;
                var queue = new Queue<StructureView>();
                for (var i = 0; i < PoolSize; i++)
                {
                    var structure = UnityEngine.Object.Instantiate(config.Prefab, _parent);
                    structure.gameObject.SetActive(false);
                    queue.Enqueue(structure);
                }
                _configPools[config] = queue;
            }
        }

        public StructureView Get(StructureConfig config)
        {
            if (config is null || !_configPools.TryGetValue(config, out var pool) || pool.Count == 0) return null;
            var structure = pool.Dequeue();
            structure.gameObject.SetActive(true);
            _activeStructures[structure] = config;
            return structure;
        }

        public void Return(StructureView structure)
        {
            if (structure is null) return;
            structure.SetInteractionEnabled(true);
            structure.gameObject.SetActive(false);
            if (_activeStructures.TryGetValue(structure, out var config))
            {
                if (_configPools.TryGetValue(config, out var pool))
                    pool.Enqueue(structure);
                _activeStructures.Remove(structure);
            }
        }
    }
}