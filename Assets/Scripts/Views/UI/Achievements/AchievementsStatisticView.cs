using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Game.Services.Achievements;
using Game.Services.Menu;

namespace Game.Views.UI.Achievements
{
    public class AchievementsStatisticView : MonoBehaviour
    {
        [field: SerializeField] private Image ProgressFill { get; set; }
        [field: SerializeField] private TextMeshProUGUI PercentageText { get; set; }
        [field: SerializeField] private TextMeshProUGUI CompletedText { get; set; }
        [field: SerializeField] private TextMeshProUGUI ActivelyText { get; set; }

        [Inject] private IAchievementService _achievementService;
        [Inject] private IMenuNavigationService _navigationService;

        private readonly CompositeDisposable _disposables = new();

        private void Start()
        {
            if (ProgressFill is not null)
            {
                ProgressFill.type = Image.Type.Filled;
                ProgressFill.fillMethod = Image.FillMethod.Horizontal;
            }

            if (_navigationService is not null)
            {
                _navigationService.CurrentView
                    .Where(view => view == MenuView.Achievements)
                    .Subscribe(_ => UpdateStatistics())
                    .AddTo(_disposables);
            }
        }

        private void UpdateStatistics()
        {
            if (_achievementService is null) return;

            var total = _achievementService.Achievements.Count;
            if (total == 0)
            {
                if (ProgressFill is not null) ProgressFill.fillAmount = 0f;
                if (PercentageText is not null) PercentageText.text = "Game is 0% complete";
                if (CompletedText is not null) CompletedText.text = "Completed: 0";
                if (ActivelyText is not null) ActivelyText.text = "Actively: 0";
                return;
            }

            var completed = _achievementService.Achievements.Count(d => d.IsCompleted.Value);
            var actively = total - completed;
            var percentage = (float)completed / total * 100f;

            if (ProgressFill is not null) ProgressFill.fillAmount = percentage / 100f;
            if (PercentageText is not null) PercentageText.text = $"Game is {percentage:F0}% complete";
            if (CompletedText is not null) CompletedText.text = $"Completed: {completed}";
            if (ActivelyText is not null) ActivelyText.text = $"Actively: {actively}";
        }

        private void OnDestroy() => _disposables?.Dispose();
    }
}