using System;
using UniRx;
using UnityEngine;

namespace Game.Services.Registry
{
    public enum PlacedObjectType
    {
        Block,
        Floor
    }

    public readonly struct PlacedObjectData
    {
        public PlacedObjectType Type { get; }
        public Vector3Int Position { get; }
        public string Identifier { get; }

        public PlacedObjectData(PlacedObjectType type, Vector3Int position, string identifier)
        {
            Type = type;
            Position = position;
            Identifier = identifier;
        }
    }

    public interface IObjectRegistryService
    {
        ReactiveCollection<PlacedObjectData> Objects { get; }
        void Register(PlacedObjectData data);
        void Unregister(Vector3Int position, PlacedObjectType type);
    }

    public class ObjectRegistryService : IObjectRegistryService
    {
        public ReactiveCollection<PlacedObjectData> Objects { get; } = new();

        public void Register(PlacedObjectData data) => Objects.Add(data);

        public void Unregister(Vector3Int position, PlacedObjectType type)
        {
            for (var i = Objects.Count - 1; i >= 0; i--)
            {
                if (Objects[i].Position == position && Objects[i].Type == type)
                {
                    Objects.RemoveAt(i);
                    break;
                }
            }
        }
    }
}