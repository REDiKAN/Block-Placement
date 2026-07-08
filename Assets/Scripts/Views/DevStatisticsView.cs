using UniRx;
using TMPro;
using UnityEngine;
using Zenject;
using Game.Services.Registry;

namespace Game.Views
{
    public class DevStatisticsView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI _blockCountText;
        [field: SerializeField] private TextMeshProUGUI _floorCountText;

        [Inject] private IObjectRegistryService _registryService;
        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            _registryService.Objects.ObserveCountChanged()
                .Subscribe(_ => UpdateStats())
                .AddTo(_disposables);

            _registryService.Objects.ObserveReplace()
                .Subscribe(_ => UpdateStats())
                .AddTo(_disposables);

            UpdateStats();
        }

        private void UpdateStats()
        {
            var blocks = 0;
            var floors = 0;
            foreach (var obj in _registryService.Objects)
            {
                if (obj.Type == PlacedObjectType.Block) blocks++;
                else floors++;
            }

            _blockCountText.text = $"Blocks: {blocks}";
            _floorCountText.text = $"Floors: {floors}";
        }

        private void OnDestroy() => _disposables.Dispose();
    }
}