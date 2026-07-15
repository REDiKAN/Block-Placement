using UnityEngine;
using Game.Services.Shadow;

namespace Game.Views
{
    public class WallCellView : MonoBehaviour
    {
        [field: SerializeField] public Material MissingMaterial { get; private set; }
        [field: SerializeField] public Material CorrectMaterial { get; private set; }
        [field: SerializeField] public Material ExtraMaterial { get; private set; }
        [field: SerializeField] public GameObject[] DensityIndicators { get; private set; }

        private Renderer _renderer;
        private Material _defaultMaterial;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
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

        public void SetTargetDensity(int density, bool isDensityEnabled)
        {
            if (DensityIndicators is null) return;

            var targetIndex = isDensityEnabled && density > 0 ? density : -1;

            for (var i = 0; i < DensityIndicators.Length; i++)
            {
                if (DensityIndicators[i] is not null)
                    DensityIndicators[i].SetActive(i == targetIndex);
            }
        }
    }
}