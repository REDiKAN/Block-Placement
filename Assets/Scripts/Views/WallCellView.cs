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
        private Material _defaultMaterial;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer is not null)
                _defaultMaterial = _renderer.material;
        }

        public void SetState(ShadowCellState state)
        {
            if (_renderer is null) return;

            _renderer.enabled = true;

            _renderer.material = state switch
            {
                ShadowCellState.Empty => _defaultMaterial,
                ShadowCellState.Missing => MissingMaterial,
                ShadowCellState.Correct => CorrectMaterial,
                ShadowCellState.Extra => ExtraMaterial,
                _ => _renderer.material
            };
        }
    }
}