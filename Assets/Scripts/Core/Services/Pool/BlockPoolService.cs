using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.Views;

namespace Game.Services.Pool
{
    public interface IBlockPoolService
    {
        BlockView Get();
        void Return(BlockView block);
    }

    public class BlockPoolService : IBlockPoolService, IInitializable
    {
        private const int PoolSize = 125;
        private readonly BlockView _prefab;
        private readonly Transform _parent;
        private readonly Queue<BlockView> _pool = new();

        public BlockPoolService([Inject(Id = "BlockPrefab")] BlockView prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        public void Initialize()
        {
            for (var i = 0; i < PoolSize; i++)
            {
                var block = Object.Instantiate(_prefab, _parent);
                block.gameObject.SetActive(false);
                _pool.Enqueue(block);
            }
        }

        public BlockView Get()
        {
            if (_pool.Count == 0) return null;
            var block = _pool.Dequeue();
            block.gameObject.SetActive(true);
            return block;
        }

        public void Return(BlockView block)
        {
            block.gameObject.SetActive(false);
            _pool.Enqueue(block);
        }
    }
}