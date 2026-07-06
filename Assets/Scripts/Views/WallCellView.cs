using UnityEngine;
using Game.Services.Shadow;

namespace Game.Views
{
    public class WallCellView : MonoBehaviour
    {
        [field: SerializeField] public Material MissingMaterial { get; private set; }
        [field: SerializeField] public Material CorrectMaterial { get; private set; }
        [field: SerializeField] public Material ExtraMaterial { get; private set; }

        private Renderer _renderer;

        private void Awake() => _renderer = GetComponent<Renderer>();

        public void SetState(ShadowCellState state)
        {
            if (_renderer is null) return;

            if (state == ShadowCellState.Empty)
            {
                _renderer.enabled = false;
                return;
            }

            _renderer.enabled = true;
            _renderer.material = state switch
            {
                ShadowCellState.Missing => MissingMaterial,
                ShadowCellState.Correct => CorrectMaterial,
                ShadowCellState.Extra => ExtraMaterial,
                _ => _renderer.material
            };
        }
    }
}