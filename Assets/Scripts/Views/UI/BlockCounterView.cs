using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Services.Animation;
using Game.Services.Placement;

namespace Game.Views.UI
{
    public class BlockCounterView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI CounterText { get; set; }
        [field: SerializeField] private Transform ShakeTarget { get; set; }
        [field: SerializeField] private GameObject RootObject { get; set; }
        [field: SerializeField] private float ShakeDuration { get; set; } = 0.3f;
        [field: SerializeField] private float ShakeStrength { get; set; } = 0.25f;

        [Inject] private IBlockPlacementService _placementService;
        [Inject] private IShakeAnimationService _shakeService;

        private readonly CompositeDisposable _disposables = new();
        private bool _lastIsEnabled;

        private void Start()
        {
            _placementService.RemainingBlocks
                .Subscribe(UpdateCounter)
                .AddTo(_disposables);
        }

        private void UpdateCounter((bool IsEnabled, int Remaining) data)
        {
            if (RootObject is not null)
                RootObject.SetActive(data.IsEnabled);

            if (!data.IsEnabled) return;

            if (CounterText is not null)
                CounterText.text = data.Remaining.ToString();

            if (ShakeTarget is not null && _lastIsEnabled)
                _shakeService.Shake(ShakeTarget, ShakeDuration, ShakeStrength);

            _lastIsEnabled = data.IsEnabled;
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}