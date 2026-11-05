using TMPro;
using UniRx;
using UnityEngine;
using Zenject;
using Game.Services.Ranked;

namespace Game.Views.UI.Ranked
{
    public class RankedHUDView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI LevelText { get; set; }
        [field: SerializeField] private TextMeshProUGUI MmrText { get; set; }
        [field: SerializeField] private TextMeshProUGUI PointsText { get; set; }
        [field: SerializeField] private GameObject RootObject { get; set; }

        [Inject] private readonly IRankedModeService _rankedService;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            _rankedService.IsMatchActive
                .Subscribe(isActive =>
                {
                    if (RootObject is not null)
                        RootObject.SetActive(isActive);
                })
                .AddTo(_disposables);

            _rankedService.CurrentMatchLevel
                .Subscribe(UpdateLevel)
                .AddTo(_disposables);

            _rankedService.CurrentMmr
                .Subscribe(UpdateMmr)
                .AddTo(_disposables);

            _rankedService.MatchPoints
                .Subscribe(UpdatePoints)
                .AddTo(_disposables);
        }

        private void UpdateLevel(int level)
        {
            if (LevelText is not null)
                LevelText.text = $"Level {level}/5";
        }

        private void UpdateMmr(int mmr)
        {
            if (MmrText is not null)
                MmrText.text = $"MMR: {mmr}";
        }

        private void UpdatePoints(int points)
        {
            if (PointsText is not null)
            {
                var prefix = points >= 0 ? "+" : "";
                PointsText.text = $"Points: {prefix}{points}";
            }
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}