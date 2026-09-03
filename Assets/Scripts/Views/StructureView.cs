using UnityEngine;

namespace Game.Views
{
    public class StructureView : MonoBehaviour
    {
        private static readonly Vector3 Offset = new(0.5f, 0.5f, 0.5f);
        private Collider[] _colliders;

        private void Awake() => _colliders = GetComponentsInChildren<Collider>();

        public void SetPosition(Vector3Int cell) => transform.position = cell + Offset;

        public void SetInteractionEnabled(bool enabled)
        {
            if (_colliders is null) return;
            foreach (var c in _colliders)
                if (c is not null) c.enabled = enabled;
        }
    }
}