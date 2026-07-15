using UnityEngine;

namespace Game.Views
{
    public class FloorCellView : MonoBehaviour
    {
        [field: SerializeField] public int Index { get; set; }
        [field: SerializeField] public Material HoverMaterial { get; private set; }

        private Renderer _renderer;
        private Material _defaultMaterial;
        private Material _currentBaseMaterial;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();

            if (_renderer is not null)
            {
                _defaultMaterial = _renderer.material;
                _currentBaseMaterial = _defaultMaterial;
            }
        }

        public void SetVisible(bool isVisible) => gameObject.SetActive(isVisible);

        public void SetHover(bool isHovered)
        {
            if (_renderer is null) return;

            if (isHovered && HoverMaterial is not null)
                _renderer.material = HoverMaterial;
            else if (_currentBaseMaterial is not null)
                _renderer.material = _currentBaseMaterial;
        }
    }
}