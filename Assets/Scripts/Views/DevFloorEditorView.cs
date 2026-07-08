using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Services.Grid;

namespace Game.Views
{
    public class DevFloorEditorView : MonoBehaviour
    {
        private const int GridSize = 5;
        private const int TotalCells = GridSize * GridSize;

        [field: SerializeField] private Button _cellButtonPrefab;
        [field: SerializeField] private Transform _gridContent;
        [field: SerializeField] private Color _activeColor = Color.white;
        [field: SerializeField] private Color _inactiveColor = new(0.2f, 0.2f, 0.2f, 0.8f);

        [Inject] private IGridService _gridService;
        private readonly Button[] _buttons = new Button[TotalCells];
        private readonly Vector2Int[] _coords = new Vector2Int[TotalCells];
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            InitializeGrid();
            SyncAllButtons();
            _gridService.OnFloorCellChanged
                .Subscribe(UpdateButton)
                .AddTo(_disposables);
        }

        private void InitializeGrid()
        {
            if (_cellButtonPrefab is null || _gridContent is null) return;

            for (var i = 0; i < TotalCells; i++)
            {
                _coords[i] = ButtonIndexToCoord(i);
                var button = Instantiate(_cellButtonPrefab, _gridContent, false);
                var capturedCoord = _coords[i];
                button.onClick.AddListener(() => ToggleCell(capturedCoord));
                _buttons[i] = button;
            }
        }

        private void SyncAllButtons()
        {
            for (var i = 0; i < TotalCells; i++)
                UpdateButtonColor(i, _gridService.IsFloorExists(_coords[i]));
        }

        private void UpdateButton(Vector2Int coord)
        {
            var index = CoordToButtonIndex(coord);
            if (index >= 0 && index < TotalCells)
                UpdateButtonColor(index, _gridService.IsFloorExists(coord));
        }

        private void UpdateButtonColor(int index, bool isActive)
        {
            if (_buttons[index] is null) return;
            var colors = _buttons[index].colors;
            colors.normalColor = isActive ? _activeColor : _inactiveColor;
            colors.highlightedColor = isActive ? _activeColor * 1.1f : _inactiveColor * 1.1f;
            colors.pressedColor = isActive ? _activeColor * 0.9f : _inactiveColor * 0.9f;
            _buttons[index].colors = colors;
        }

        private void ToggleCell(Vector2Int coord) =>
            _gridService.SetFloorExists(coord, !_gridService.IsFloorExists(coord));

        private static Vector2Int ButtonIndexToCoord(int i) =>
            new(i / GridSize, i % GridSize);

        private static int CoordToButtonIndex(Vector2Int coord) =>
            coord.x * GridSize + coord.y;

        private void OnDestroy() => _disposables?.Dispose();
    }
}