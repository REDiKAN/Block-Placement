using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Data;
using Game.Views;

namespace Game.Services.Pool
{
    public interface IBlockPoolService
    {
        BlockView GetDefault();
        BlockView Get(BlockConfig config);
        void Return(BlockView block);
    }

    public class BlockPoolService : IBlockPoolService, IInitializable
    {
        private const int PoolSize = 125;
        private readonly BlockView _defaultPrefab;
        private readonly BlockConfig[] _configs;
        private readonly Transform _parent;

        private readonly Queue<BlockView> _defaultPool = new();
        private readonly Dictionary<BlockConfig, Queue<BlockView>> _configPools = new();
        private readonly HashSet<BlockView> _activeDefaultBlocks = new();
        private readonly Dictionary<BlockView, BlockConfig> _activeConfigBlocks = new();

        public BlockPoolService(
            [Inject(Id = "BlockPrefab")] BlockView defaultPrefab,
            [InjectOptional] BlockConfig[] configs,
            Transform parent)
        {
            _defaultPrefab = defaultPrefab;
            _configs = configs ?? Array.Empty<BlockConfig>();
            _parent = parent;
        }

        public void Initialize()
        {
            if (_defaultPrefab is not null)
            {
                for (var i = 0; i < PoolSize; i++)
                {
                    var block = UnityEngine.Object.Instantiate(_defaultPrefab, _parent);
                    block.gameObject.SetActive(false);
                    _defaultPool.Enqueue(block);
                }
            }

            foreach (var config in _configs)
            {
                if (config?.Prefab is null) continue;
                var queue = new Queue<BlockView>();
                for (var i = 0; i < PoolSize; i++)
                {
                    var block = UnityEngine.Object.Instantiate(config.Prefab, _parent);
                    block.gameObject.SetActive(false);
                    queue.Enqueue(block);
                }
                _configPools[config] = queue;
            }
        }

        public BlockView GetDefault()
        {
            if (_defaultPool.Count == 0) return null;
            var block = _defaultPool.Dequeue();
            block.gameObject.SetActive(true);
            _activeDefaultBlocks.Add(block);
            return block;
        }

        public BlockView Get(BlockConfig config)
        {
            if (config is null || !_configPools.TryGetValue(config, out var pool) || pool.Count == 0) return null;
            var block = pool.Dequeue();
            block.gameObject.SetActive(true);
            _activeConfigBlocks[block] = config;
            return block;
        }

        public void Return(BlockView block)
        {
            if (block is null) return;
            block.gameObject.SetActive(false);

            if (_activeDefaultBlocks.Remove(block))
            {
                _defaultPool.Enqueue(block);
                return;
            }

            if (_activeConfigBlocks.TryGetValue(block, out var config))
            {
                _configPools[config].Enqueue(block);
                _activeConfigBlocks.Remove(block);
            }
        }
    }
}