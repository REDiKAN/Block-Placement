using UnityEngine;

namespace Game.Views
{
    public class BlockView : MonoBehaviour
    {
        private static readonly Vector3 Offset = new(0.5f, 0.5f, 0.5f);

        public void SetPosition(Vector3Int cell) => transform.position = cell + Offset;
    }
}