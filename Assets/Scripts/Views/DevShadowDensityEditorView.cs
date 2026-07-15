using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Services.Dev;

namespace Game.Views
{
    public class DevShadowDensityEditorView : MonoBehaviour
    {
        private const int GridSize = 5;
        private const int TotalCells = GridSize * GridSize;

        [field: SerializeField] private Button _cellButtonPrefab;
        [field: SerializeField] private Transform _wallYZContent;
        [field: SerializeField] private Transform _wallXYContent;
        [field: SerializeField] private Color _activeColor = Color.white;
        [field: SerializeField] private Color _inactiveColor = new(0.2f, 0.2f, 0.2f, 0.8f);

        [Inject] private IShadowDensityService _densityService;
        [Inject] private ICellHoverService _cellHoverService;

        private readonly Button[] _wallYZButtons = new Button[TotalCells];
        private readonly Button[] _wallXYButtons = new Button[TotalCells];
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            InitializeGrid(_wallYZContent, _wallYZButtons, 0);
            InitializeGrid(_wallXYContent, _wallXYButtons, 1);
            SyncAllButtons();

            _densityService.OnDensityToggled
                .Subscribe(UpdateButton)
                .AddTo(_disposables);
        }

        private void InitializeGrid(Transform content, Button[] buttons, int wallIndex)
        {
            if (_cellButtonPrefab is null || content is null) return;

            for (var i = 0; i < TotalCells; i++)
            {
                var button = Instantiate(_cellButtonPrefab, content, false);
                var capturedIndex = i;

                button.onClick.AddListener(() => _densityService.ToggleDensity(wallIndex, capturedIndex));

                button.OnPointerEnterAsObservable()
                    .Subscribe(_ => _cellHoverService.NotifyHovered(wallIndex, capturedIndex))
                    .AddTo(_disposables);

                button.OnPointerExitAsObservable()
                    .Subscribe(_ => _cellHoverService.NotifyUnhovered())
                    .AddTo(_disposables);

                buttons[i] = button;
            }
        }

        private void SyncAllButtons()
        {
            for (var i = 0; i < TotalCells; i++)
            {
                UpdateButtonColor(_wallYZButtons[i], _densityService.IsDensityEnabled(0, i));
                UpdateButtonColor(_wallXYButtons[i], _densityService.IsDensityEnabled(1, i));
            }
        }

        private void UpdateButton((int WallIndex, int CellIndex, bool IsEnabled) data)
        {
            var buttons = data.WallIndex == 0 ? _wallYZButtons : _wallXYButtons;
            if (data.CellIndex >= 0 && data.CellIndex < TotalCells && buttons[data.CellIndex] is not null)
                UpdateButtonColor(buttons[data.CellIndex], data.IsEnabled);
        }

        private void UpdateButtonColor(Button button, bool isActive)
        {
            if (button is null) return;
            var colors = button.colors;
            colors.normalColor = isActive ? _activeColor : _inactiveColor;
            colors.highlightedColor = isActive ? _activeColor * 1.1f : _inactiveColor * 1.1f;
            colors.pressedColor = isActive ? _activeColor * 0.9f : _inactiveColor * 0.9f;
            button.colors = colors;
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}