using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Game.Services.Ranked;
using UnityEngine.SceneManagement;

namespace Game.Views.UI.Ranked
{
    public class RankedResultView : MonoBehaviour
    {
        [field: SerializeField] private TextMeshProUGUI LevelsPassedText { get; set; }
        [field: SerializeField] private TextMeshProUGUI PointsEarnedText { get; set; }
        [field: SerializeField] private TextMeshProUGUI Level5BonusText { get; set; }
        [field: SerializeField] private TextMeshProUGUI PerfectRunText { get; set; }
        [field: SerializeField] private TextMeshProUGUI OldMmrText { get; set; }
        [field: SerializeField] private TextMeshProUGUI NewMmrText { get; set; }
        [field: SerializeField] private TextMeshProUGUI MmrDeltaText { get; set; }
        [field: SerializeField] private Button ContinueButton { get; set; }
        [field: SerializeField] private GameObject RootObject { get; set; }

        [Inject] private readonly IRankedModeService _rankedService;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (RootObject is not null)
                RootObject.SetActive(false);

            _rankedService.OnMatchEnded
                .Subscribe(ShowResults)
                .AddTo(_disposables);

            if (ContinueButton is not null)
            {
                ContinueButton.OnClickAsObservable()
                    .Subscribe(_ => ReturnToMenu())
                    .AddTo(_disposables);
            }
        }

        private void ShowResults(RankedMatchResult result)
        {
            if (RootObject is not null)
                RootObject.SetActive(true);

            if (LevelsPassedText is not null)
                LevelsPassedText.text = $"Levels Passed: {result.LevelsCompleted} / 5";

            if (PointsEarnedText is not null)
                PointsEarnedText.text = $"Points Earned: {(result.MatchPoints >= 0 ? "+" : "")}{result.MatchPoints}";

            if (Level5BonusText is not null)
                Level5BonusText.text = $"Level 5 Bonus: +{result.Level5Bonus}";

            if (PerfectRunText is not null)
                PerfectRunText.text = result.IsPerfectRun ? "Perfect Run: +50" : "Perfect Run: ?";

            if (OldMmrText is not null)
                OldMmrText.text = $"Old MMR: {result.OldMmr}";

            if (NewMmrText is not null)
                NewMmrText.text = $"New MMR: {result.NewMmr}";

            if (MmrDeltaText is not null)
            {
                var delta = result.MmrDelta;
                var arrow = delta >= 0 ? "?" : "?";
                MmrDeltaText.text = $"Change: {(delta >= 0 ? "+" : "")}{delta} {arrow}";
            }
        }

        private void ReturnToMenu()
        {
            if (RootObject is not null)
                RootObject.SetActive(false);

            SceneManager.LoadScene("MenuScene");
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}